using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WinPurifyPro.Services
{
    public sealed class ApiDispatchResponse
    {
        public int StatusCode { get; set; } = 200;
        public string JsonBody { get; set; } = "{}";
        public Dictionary<string, string> ExtraHeaders { get; } = new(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Zero-Config 소규모 작업실용 내장 TLS 1.2 / 1.3 HTTPS 서버 (TcpListener + SslStream)
    /// Windows HttpListener(http.sys)와 달리 netsh sslcert 관리자 바인딩 없이도
    /// ECDSA P-256 자체 서명 인증서로 모든 인터페이스에서 즉시 암호화 리스닝을 제공합니다.
    /// </summary>
    public sealed class RemoteTlsServer : IDisposable
    {
        private readonly int _port;
        private readonly X509Certificate2 _certificate;
        private readonly string _thumbprint;
        private readonly Func<string, string, NameValueCollection, string, string, bool, Task<ApiDispatchResponse>> _handler;
        private TcpListener? _listener;
        private CancellationTokenSource? _cts;

        public bool IsListening { get; private set; }
        public int Port => _port;
        public string Thumbprint => _thumbprint;

        public RemoteTlsServer(
            int port,
            X509Certificate2 certificate,
            Func<string, string, NameValueCollection, string, string, bool, Task<ApiDispatchResponse>> handler)
        {
            _port = port;
            _certificate = certificate;
            _thumbprint = ClusterSecurityVault.ComputeSha256Thumbprint(certificate);
            _handler = handler;
        }

        public void Start(CancellationToken externalToken)
        {
            if (IsListening) return;

            _cts = CancellationTokenSource.CreateLinkedTokenSource(externalToken);
            _listener = new TcpListener(IPAddress.Any, _port);
            _listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _listener.Start();
            IsListening = true;

            _ = Task.Run(() => AcceptLoopAsync(_cts.Token));
        }

        public void Stop()
        {
            IsListening = false;
            try { _cts?.Cancel(); } catch { }
            try { _listener?.Stop(); } catch { }
        }

        private async Task AcceptLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested && _listener != null)
            {
                try
                {
                    TcpClient client = await _listener.AcceptTcpClientAsync(token);
                    _ = Task.Run(() => HandleClientAsync(client, token), token);
                }
                catch (OperationCanceledException) { break; }
                catch (SocketException) { if (token.IsCancellationRequested) break; }
                catch { if (token.IsCancellationRequested) break; }
            }
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken token)
        {
            using (client)
            {
                client.ReceiveTimeout = 15000;
                client.SendTimeout = 15000;

                string clientIp = "127.0.0.1";
                if (client.Client.RemoteEndPoint is IPEndPoint ep)
                {
                    IPAddress addr = ep.Address.IsIPv4MappedToIPv6 ? ep.Address.MapToIPv4() : ep.Address;
                    clientIp = addr.ToString();
                }

                try
                {
                    using var sslStream = new SslStream(client.GetStream(), false);
                    await sslStream.AuthenticateAsServerAsync(
                        new SslServerAuthenticationOptions
                        {
                            ServerCertificate = _certificate,
                            ClientCertificateRequired = false,
                            EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
                            CertificateRevocationCheckMode = X509RevocationMode.NoCheck
                        },
                        token);

                    var (method, rawTarget, headers, body) = await ReadHttpRequestAsync(sslStream, token);
                    if (string.IsNullOrEmpty(method)) return;

                    string path = rawTarget.Split('?')[0].TrimEnd('/').ToLowerInvariant();
                    if (string.IsNullOrEmpty(path)) path = "/";

                    if (method.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
                    {
                        await WriteHttpResponseAsync(sslStream, new ApiDispatchResponse { StatusCode = 200, JsonBody = string.Empty }, headers["Origin"], token);
                        return;
                    }

                    var response = await _handler(method, path, headers, body, clientIp, true);
                    await WriteHttpResponseAsync(sslStream, response, headers["Origin"], token);
                }
                catch
                {
                    // Client disconnected or TLS handshake aborted cleanly
                }
            }
        }

        private static async Task<(string Method, string Target, NameValueCollection Headers, string Body)> ReadHttpRequestAsync(Stream stream, CancellationToken token)
        {
            var headers = new NameValueCollection(StringComparer.OrdinalIgnoreCase);
            var buffer = new byte[8192];
            using var headerMem = new MemoryStream();

            int headerEndIndex = -1;
            int totalRead = 0;

            while (totalRead < 65536)
            {
                int read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), token);
                if (read <= 0) break;

                headerMem.Write(buffer, 0, read);
                totalRead += read;

                byte[] current = headerMem.ToArray();
                headerEndIndex = FindHeaderTerminator(current);
                if (headerEndIndex >= 0) break;
            }

            if (headerEndIndex < 0)
            {
                return (string.Empty, string.Empty, headers, string.Empty);
            }

            byte[] allInitial = headerMem.ToArray();
            string headerText = Encoding.ASCII.GetString(allInitial, 0, headerEndIndex);
            string[] lines = headerText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            if (lines.Length == 0) return (string.Empty, string.Empty, headers, string.Empty);

            string[] requestLine = lines[0].Split(' ', 3);
            if (requestLine.Length < 2) return (string.Empty, string.Empty, headers, string.Empty);

            string method = requestLine[0].Trim().ToUpperInvariant();
            string target = requestLine[1].Trim();

            int contentLength = 0;
            for (int i = 1; i < lines.Length; i++)
            {
                int colon = lines[i].IndexOf(':');
                if (colon > 0)
                {
                    string key = lines[i].Substring(0, colon).Trim();
                    string val = lines[i].Substring(colon + 1).Trim();
                    headers[key] = val;
                    if (key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase))
                    {
                        int.TryParse(val, out contentLength);
                    }
                }
            }

            int bodyOffset = headerEndIndex + 4;
            if (allInitial.Length >= headerEndIndex + 2 && allInitial[headerEndIndex] == (byte)'\n')
            {
                bodyOffset = headerEndIndex + 2;
            }

            string body = string.Empty;
            if (contentLength > 0 && contentLength <= 2 * 1024 * 1024)
            {
                byte[] bodyBytes = new byte[contentLength];
                int alreadyCopied = Math.Min(contentLength, Math.Max(0, allInitial.Length - bodyOffset));
                if (alreadyCopied > 0)
                {
                    Buffer.BlockCopy(allInitial, bodyOffset, bodyBytes, 0, alreadyCopied);
                }

                int remaining = contentLength - alreadyCopied;
                int offset = alreadyCopied;
                while (remaining > 0)
                {
                    int r = await stream.ReadAsync(bodyBytes.AsMemory(offset, remaining), token);
                    if (r <= 0) break;
                    offset += r;
                    remaining -= r;
                }

                body = Encoding.UTF8.GetString(bodyBytes, 0, offset);
            }

            return (method, target, headers, body);
        }

        private static int FindHeaderTerminator(byte[] data)
        {
            for (int i = 0; i <= data.Length - 4; i++)
            {
                if (data[i] == '\r' && data[i + 1] == '\n' && data[i + 2] == '\r' && data[i + 3] == '\n')
                {
                    return i;
                }
            }
            return -1;
        }

        private async Task WriteHttpResponseAsync(Stream stream, ApiDispatchResponse resp, string? origin, CancellationToken token)
        {
            byte[] bodyBytes = string.IsNullOrEmpty(resp.JsonBody) ? Array.Empty<byte>() : Encoding.UTF8.GetBytes(resp.JsonBody);
            string reason = resp.StatusCode switch
            {
                200 => "OK",
                400 => "Bad Request",
                401 => "Unauthorized",
                403 => "Forbidden",
                404 => "Not Found",
                _ => "Internal Server Error"
            };

            var sb = new StringBuilder();
            sb.Append($"HTTP/1.1 {resp.StatusCode} {reason}\r\n");
            sb.Append("Content-Type: application/json; charset=utf-8\r\n");
            sb.Append($"Content-Length: {bodyBytes.Length}\r\n");
            sb.Append("Connection: close\r\n");
            sb.Append("X-Content-Type-Options: nosniff\r\n");
            sb.Append("X-Frame-Options: DENY\r\n");
            sb.Append("X-WinPurify-Tls: true\r\n");
            sb.Append($"X-WinPurify-Cert-Thumbprint: {_thumbprint}\r\n");

            if (!string.IsNullOrEmpty(origin) && (origin.Contains("localhost") || origin.Contains("127.0.0.1")))
            {
                sb.Append($"Access-Control-Allow-Origin: {origin}\r\n");
            }
            sb.Append("Access-Control-Allow-Methods: GET, POST, OPTIONS\r\n");
            sb.Append("Access-Control-Allow-Headers: Content-Type, X-WinPurify-Signature, X-WinPurify-Timestamp, X-WinPurify-Nonce, X-WinPurify-Challenge-Nonce, X-WinPurify-Challenge-Signature\r\n");

            foreach (var kv in resp.ExtraHeaders)
            {
                sb.Append($"{kv.Key}: {kv.Value}\r\n");
            }

            sb.Append("\r\n");

            byte[] headerBytes = Encoding.ASCII.GetBytes(sb.ToString());
            await stream.WriteAsync(headerBytes, token);
            if (bodyBytes.Length > 0)
            {
                await stream.WriteAsync(bodyBytes, token);
            }
            await stream.FlushAsync(token);
        }

        public void Dispose()
        {
            Stop();
            _cts?.Dispose();
        }
    }
}
