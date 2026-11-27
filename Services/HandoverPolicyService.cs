using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Win32;
using WinPurifyPro.Models;

namespace WinPurifyPro.Services
{
    /// <summary>
    /// Clean Handover 보존 예외(Whitelist), 재부팅 자동 삭제 예약(PendingFileRenameOperations),
    /// 및 오프라인 HTML/JSON 인수인계 검증 인증서 생성을 담당하는 정책 서비스
    /// </summary>
    public static class HandoverPolicyService
    {
        private static readonly object SyncLock = new();

        private static string GetExclusionsFilePath()
        {
            string baseDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "WinPurifyPro");
            return Path.Combine(baseDir, "handover_exclusions.json");
        }

        public static List<HandoverExclusionRule> LoadExclusions()
        {
            lock (SyncLock)
            {
                try
                {
                    string file = GetExclusionsFilePath();
                    if (!File.Exists(file)) return new List<HandoverExclusionRule>();
                    string json = File.ReadAllText(file, Encoding.UTF8);
                    return JsonSerializer.Deserialize<List<HandoverExclusionRule>>(json) ?? new List<HandoverExclusionRule>();
                }
                catch
                {
                    return new List<HandoverExclusionRule>();
                }
            }
        }

        public static void SaveExclusions(IEnumerable<HandoverExclusionRule> rules)
        {
            lock (SyncLock)
            {
                try
                {
                    string file = GetExclusionsFilePath();
                    string? dir = Path.GetDirectoryName(file);
                    if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                    string json = JsonSerializer.Serialize(rules.ToList(), new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(file, json, Encoding.UTF8);
                }
                catch { }
            }
        }

        public static List<HandoverExclusionRule> AddExclusion(string ruleType, string rawValue, string label)
        {
            var list = LoadExclusions();
            if (string.IsNullOrWhiteSpace(rawValue)) return list;

            string cleanVal = ruleType.Equals("PathPrefix", StringComparison.OrdinalIgnoreCase)
                ? NormalizePathSafe(rawValue)
                : rawValue.Trim();

            if (string.IsNullOrWhiteSpace(cleanVal)) return list;

            bool exists = list.Any(r =>
                r.RuleType.Equals(ruleType, StringComparison.OrdinalIgnoreCase) &&
                r.Value.Equals(cleanVal, StringComparison.OrdinalIgnoreCase));

            if (!exists)
            {
                list.Add(new HandoverExclusionRule
                {
                    RuleType = ruleType,
                    Value = cleanVal,
                    Label = string.IsNullOrWhiteSpace(label) ? cleanVal : label.Trim(),
                    CreatedAt = DateTime.Now
                });
                SaveExclusions(list);
            }
            return list;
        }

        public static List<HandoverExclusionRule> RemoveExclusion(string ruleId)
        {
            var list = LoadExclusions();
            list.RemoveAll(r => r.Id.Equals(ruleId, StringComparison.OrdinalIgnoreCase));
            SaveExclusions(list);
            return list;
        }

        public static bool IsArtifactExcluded(DetectedSessionArtifact artifact, IReadOnlyCollection<HandoverExclusionRule> rules, out string reason)
        {
            reason = string.Empty;
            if (rules.Count == 0) return false;

            foreach (var r in rules)
            {
                if (r.RuleType.Equals("ArtifactId", StringComparison.OrdinalIgnoreCase) &&
                    (artifact.ArtifactId.Equals(r.Value, StringComparison.OrdinalIgnoreCase) ||
                     artifact.ArtifactId.StartsWith(r.Value + "::", StringComparison.OrdinalIgnoreCase)))
                {
                    reason = $"보존 예외: {r.Label}";
                    return true;
                }
            }

            if (artifact.MatchedPaths.Count > 0 && artifact.MatchedPaths.All(p => IsPathExcluded(p, rules, out _)))
            {
                reason = "모든 대상 경로가 보존 예외에 포함됨";
                return true;
            }

            return false;
        }

        public static bool IsPathExcluded(string candidatePath, IReadOnlyCollection<HandoverExclusionRule> rules, out string reason)
        {
            reason = string.Empty;
            if (string.IsNullOrWhiteSpace(candidatePath) || rules.Count == 0) return false;

            string norm = NormalizePathSafe(candidatePath);
            if (string.IsNullOrEmpty(norm)) return false;

            foreach (var r in rules)
            {
                if (!r.RuleType.Equals("PathPrefix", StringComparison.OrdinalIgnoreCase)) continue;
                string ex = NormalizePathSafe(r.Value);
                if (string.IsNullOrEmpty(ex)) continue;

                if (norm.Equals(ex, StringComparison.OrdinalIgnoreCase) ||
                    norm.StartsWith(ex + "\\", StringComparison.OrdinalIgnoreCase))
                {
                    reason = $"경로 보존 예외: {r.Label}";
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 잠긴 파일 또는 비어 있지 않은 폴더를 다음 Windows 부팅 초기(smss.exe)에 자동 삭제되도록 안전 예약합니다.
        /// (OS 보호 경로, WinPurify 자체 경로, 심볼릭 링크/재파스 포인트는 절대 예약하지 않습니다.)
        /// </summary>
        public static bool SchedulePendingFileDelete(string targetPath, out string normalizedTarget)
        {
            normalizedTarget = string.Empty;
            if (string.IsNullOrWhiteSpace(targetPath)) return false;

            try
            {
                string fullPath = Path.GetFullPath(targetPath).TrimEnd('\\');
                if (FreshHandoverService.IsPathProtected(fullPath)) return false;

                if (File.Exists(fullPath))
                {
                    if ((File.GetAttributes(fullPath) & FileAttributes.ReparsePoint) != 0) return false;
                }
                else if (Directory.Exists(fullPath))
                {
                    if ((new DirectoryInfo(fullPath).Attributes & FileAttributes.ReparsePoint) != 0) return false;
                }
                else
                {
                    return false;
                }

                normalizedTarget = fullPath;
                string ntPath = $@"\??\{fullPath}";

                lock (SyncLock)
                {
                    using var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                    using var sessionMgr = hklm.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager", writable: true);
                    if (sessionMgr == null) return false;

                    var existing = sessionMgr.GetValue("PendingFileRenameOperations") as string[] ?? Array.Empty<string>();
                    var list = new List<string>(existing);

                    for (int i = 0; i + 1 < list.Count; i += 2)
                    {
                        if (string.Equals(list[i], ntPath, StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(list[i + 1]))
                        {
                            return true; // Already queued
                        }
                    }

                    list.Add(ntPath);
                    list.Add(string.Empty);
                    sessionMgr.SetValue("PendingFileRenameOperations", list.ToArray(), RegistryValueKind.MultiString);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public static string BuildVerificationCertificateJson(HandoverVerificationSummary summary)
        {
            var doc = new
            {
                product = "WinPurify Pro",
                version = AppEnvironment.DisplayVersion,
                documentType = "CleanHandoverVerificationCertificate",
                generatedAt = summary.CompletedAt.ToString("o"),
                machineName = summary.MachineName,
                operatorUser = summary.OperatorUser,
                presetUsed = summary.PresetUsed,
                isCleanHandoverVerified = summary.IsCleanHandoverVerified,
                purgedProfilesCount = summary.PurgedProfilesCount,
                purgedSessionsCount = summary.PurgedSessionsCount,
                reclaimedBytes = summary.ReclaimedBytes,
                reclaimedFormatted = summary.FormattedReclaimedSize,
                remainingPreviousProfiles = summary.RemainingPreviousProfiles,
                remainingActiveSessions = summary.RemainingActiveSessions,
                excludedItemsCount = summary.ExcludedItemsCount,
                pendingRebootCount = summary.PendingRebootCount,
                pendingRebootPaths = summary.PendingRebootPaths,
                lockedProcessHints = summary.LockedProcessHints,
                summaryMessage = summary.SummaryMessage,
                purgedItems = summary.PurgedItemsSnapshot
            };

            return JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true });
        }

        public static string BuildVerificationCertificateHtml(HandoverVerificationSummary summary)
        {
            static string E(string? s) => WebUtility.HtmlEncode(s ?? string.Empty);

            string statusColor = summary.IsCleanHandoverVerified
                ? "#15803D"
                : (summary.HasPendingReboot ? "#B45309" : "#BE123C");
            string statusBg = summary.IsCleanHandoverVerified
                ? "#ECFDF5"
                : (summary.HasPendingReboot ? "#FFFBEB" : "#FFF1F2");
            string statusLabel = summary.IsCleanHandoverVerified
                ? "클린 인수인계 2차 무결성 검증 통과"
                : (summary.HasPendingReboot
                    ? $"재부팅 자동 삭제 예약 완료 ({summary.PendingRebootCount}건 대기)"
                    : "일부 잠금 항목 확인 필요");

            var rowsSb = new StringBuilder();
            foreach (var item in summary.PurgedItemsSnapshot)
            {
                rowsSb.AppendLine(
                    $"<tr>" +
                    $"<td>{E(item.Title)}</td>" +
                    $"<td>{E(item.Category)}</td>" +
                    $"<td class=\"mono num\">{E(DetectedUserProfile.FormatBytes(item.EstimatedBytes))}</td>" +
                    $"<td>{E(item.Status)}</td>" +
                    $"<td class=\"mono\">{E(item.PathsSummary)}</td>" +
                    $"</tr>");
            }

            if (summary.PurgedItemsSnapshot.Count == 0)
            {
                rowsSb.AppendLine("<tr><td colspan=\"5\" style=\"text-align:center;color:#64748B;\">기록된 소거 항목 상세 내역이 없습니다.</td></tr>");
            }

            var pendingSb = new StringBuilder();
            if (summary.PendingRebootPaths.Count > 0)
            {
                pendingSb.AppendLine("<h3>재부팅 시 자동 소거 예약 목록 (PendingFileRenameOperations)</h3><ul>");
                foreach (var p in summary.PendingRebootPaths)
                {
                    pendingSb.AppendLine($"<li class=\"mono\">{E(p)}</li>");
                }
                pendingSb.AppendLine("</ul>");
            }

            return $@"<!DOCTYPE html>
<html lang=""ko"">
<head>
<meta charset=""utf-8"" />
<title>WinPurify Pro - Clean Handover Verification Certificate</title>
<style>
  body {{ font-family: 'Segoe UI', system-ui, sans-serif; background:#F8FAFC; color:#0F172A; margin:32px; line-height:1.5; }}
  .sheet {{ max-width:960px; margin:0 auto; background:#FFFFFF; border:1px solid #CBD5E1; border-radius:10px; padding:32px; }}
  .header {{ display:flex; justify-content:space-between; align-items:flex-start; border-bottom:1px solid #E2E8F0; padding-bottom:18px; margin-bottom:20px; }}
  .title {{ font-size:22px; font-weight:700; margin:0; }}
  .sub {{ font-size:13px; color:#475569; margin-top:4px; }}
  .status {{ display:inline-block; padding:8px 14px; border-radius:6px; border:1px solid {statusColor}; background:{statusBg}; color:{statusColor}; font-weight:700; font-size:13px; }}
  .grid {{ display:grid; grid-template-columns:repeat(4,1fr); gap:12px; margin:20px 0; }}
  .metric {{ border:1px solid #E2E8F0; border-radius:8px; padding:12px 14px; background:#F8FAFC; }}
  .metric-label {{ font-size:11.5px; color:#64748B; }}
  .metric-val {{ font-size:18px; font-weight:700; margin-top:4px; font-variant-numeric:tabular-nums; }}
  table {{ width:100%; border-collapse:collapse; margin-top:14px; font-size:12.5px; }}
  th, td {{ border-bottom:1px solid #E2E8F0; padding:9px 10px; text-align:left; vertical-align:top; }}
  th {{ background:#F1F5F9; color:#334155; font-weight:600; }}
  .mono {{ font-family: Consolas, 'JetBrains Mono', monospace; font-size:11.5px; word-break:break-all; }}
  .num {{ text-align:right; white-space:nowrap; font-variant-numeric:tabular-nums; }}
  .footer {{ margin-top:24px; padding-top:14px; border-top:1px solid #E2E8F0; font-size:11.5px; color:#64748B; display:flex; justify-content:space-between; }}
</style>
</head>
<body>
  <div class=""sheet"">
    <div class=""header"">
      <div>
        <h1 class=""title"">WinPurify Pro · 클린 환경 인수인계 검증 인증서</h1>
        <div class=""sub"">PC 명: {E(summary.MachineName)} · 실행 계정: {E(summary.OperatorUser)} · 정책: {E(summary.PresetUsed)} · 검증 시각: {E(summary.CompletedAt.ToString("yyyy-MM-dd HH:mm:ss"))}</div>
      </div>
      <div class=""status"">{E(statusLabel)}</div>
    </div>

    <p style=""font-size:13.5px;margin:0 0 12px 0;"">{E(summary.SummaryMessage)}</p>

    <div class=""grid"">
      <div class=""metric""><div class=""metric-label"">제거된 이전 프로필</div><div class=""metric-val"">{summary.PurgedProfilesCount}개</div></div>
      <div class=""metric""><div class=""metric-label"">초기화된 앱/세션</div><div class=""metric-val"">{summary.PurgedSessionsCount}영역</div></div>
      <div class=""metric""><div class=""metric-label"">회수된 디스크 공간</div><div class=""metric-val"">{E(summary.FormattedReclaimedSize)}</div></div>
      <div class=""metric""><div class=""metric-label"">재부팅 예약 / 예외</div><div class=""metric-val"">{summary.PendingRebootCount}건 / {summary.ExcludedItemsCount}건</div></div>
    </div>

    <h3>소거 및 검증 상세 내역</h3>
    <table>
      <thead>
        <tr>
          <th>대상 영역</th>
          <th>분류</th>
          <th class=""num"">회수 크기</th>
          <th>상태</th>
          <th>경로 요약</th>
        </tr>
      </thead>
      <tbody>
        {rowsSb}
      </tbody>
    </table>

    {pendingSb}

    <div class=""footer"">
      <span>Generated offline by WinPurify Pro {E(AppEnvironment.DisplayVersion)} (Zero-Telemetry Verified)</span>
      <span>2차 잔여 프로필: {summary.RemainingPreviousProfiles}개 · 잔여 활성 세션: {summary.RemainingActiveSessions}건</span>
    </div>
  </div>
</body>
</html>";
        }

        private static string NormalizePathSafe(string raw)
        {
            try
            {
                string expanded = Environment.ExpandEnvironmentVariables(raw.Trim());
                return Path.GetFullPath(expanded).TrimEnd('\\');
            }
            catch
            {
                return raw.Trim().TrimEnd('\\');
            }
        }
    }
}
