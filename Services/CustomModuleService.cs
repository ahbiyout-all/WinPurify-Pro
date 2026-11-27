using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using WinPurifyPro.Models;

namespace WinPurifyPro.Services
{
    public class ModuleManifestRoot
    {
        [JsonPropertyName("schemaVersion")]
        public string SchemaVersion { get; set; } = "2.0";

        [JsonPropertyName("moduleCount")]
        public int ModuleCount { get; set; }

        [JsonPropertyName("modules")]
        public List<ModuleManifestDto> Modules { get; set; } = new();
    }

    public class ModuleManifestDto
    {
        [JsonPropertyName("seq")]
        public int Seq { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("category")]
        public string Category { get; set; } = "System";

        [JsonPropertyName("subCategory")]
        public string? SubCategory { get; set; }

        [JsonPropertyName("risk")]
        public string Risk { get; set; } = "Safe";

        [JsonPropertyName("scanTargetFolder")]
        public string ScanTargetFolder { get; set; } = string.Empty;

        [JsonPropertyName("fastCliCommand")]
        public string FastCliCommand { get; set; } = string.Empty;

        [JsonPropertyName("powerShellCommand")]
        public string PowerShellCommand { get; set; } = string.Empty;

        [JsonPropertyName("nativeActionType")]
        public int NativeActionType { get; set; }

        [JsonPropertyName("isSelected")]
        public bool IsSelected { get; set; } = true;

        [JsonPropertyName("estimatedKb")]
        public long EstimatedKb { get; set; }

        [JsonPropertyName("timeoutSeconds")]
        public int TimeoutSeconds { get; set; } = 15;

        [JsonPropertyName("requiresAdmin")]
        public bool RequiresAdmin { get; set; }
    }

    /// <summary>
    /// 통합 JSON 매니페스트(modules.manifest.json) 및 사용자 정의 작업실 커스텀 모듈 로더 서비스
    /// - WPF 데스크톱 앱과 Electron 커맨더 간 단일 진실 공급원(Single Source of Truth) 제공
    /// - 스튜디오 특화 캐시(Premiere, Blender, Unreal, AutoCAD 등) 런타임 추가/관리 지원
    /// </summary>
    public static class CustomModuleService
    {
        public static string CustomModulesFilePath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WinPurifyPro",
            "custom-modules.json"
        );

        public static List<OptimizationTask> LoadAllModules()
        {
            var tasks = LoadBuiltInManifestTasks();
            var custom = LoadCustomModules();
            if (custom.Count > 0)
            {
                int nextSeq = tasks.Count + 1;
                foreach (var c in custom)
                {
                    c.SequenceNumber = nextSeq++;
                    tasks.Add(c);
                }
            }
            return tasks;
        }

        public static List<OptimizationTask> LoadBuiltInManifestTasks()
        {
            string? json = TryReadManifestJson();
            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<OptimizationTask>();
            }

            try
            {
                var root = JsonSerializer.Deserialize<ModuleManifestRoot>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (root?.Modules == null || root.Modules.Count == 0)
                {
                    return new List<OptimizationTask>();
                }

                return root.Modules.Select(dto => MapDtoToTask(dto, isCustom: false)).ToList();
            }
            catch
            {
                return new List<OptimizationTask>();
            }
        }

        public static List<OptimizationTask> LoadCustomModules()
        {
            try
            {
                if (!File.Exists(CustomModulesFilePath))
                {
                    return new List<OptimizationTask>();
                }

                string json = File.ReadAllText(CustomModulesFilePath);
                var dtos = JsonSerializer.Deserialize<List<ModuleManifestDto>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (dtos == null) return new List<OptimizationTask>();

                return dtos.Select(dto => MapDtoToTask(dto, isCustom: true)).ToList();
            }
            catch
            {
                return new List<OptimizationTask>();
            }
        }

        public static bool SaveCustomModule(ModuleManifestDto newModule)
        {
            try
            {
                string? dir = Path.GetDirectoryName(CustomModulesFilePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                var existing = new List<ModuleManifestDto>();
                if (File.Exists(CustomModulesFilePath))
                {
                    string raw = File.ReadAllText(CustomModulesFilePath);
                    existing = JsonSerializer.Deserialize<List<ModuleManifestDto>>(raw, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<ModuleManifestDto>();
                }

                if (string.IsNullOrWhiteSpace(newModule.Id))
                {
                    newModule.Id = $"custom-{Guid.NewGuid():N}".Substring(0, 15);
                }

                existing.RemoveAll(m => string.Equals(m.Id, newModule.Id, StringComparison.OrdinalIgnoreCase));
                existing.Add(newModule);

                string updatedJson = JsonSerializer.Serialize(existing, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(CustomModulesFilePath, updatedJson);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool RemoveCustomModule(string id)
        {
            try
            {
                if (!File.Exists(CustomModulesFilePath)) return false;
                string raw = File.ReadAllText(CustomModulesFilePath);
                var existing = JsonSerializer.Deserialize<List<ModuleManifestDto>>(raw, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<ModuleManifestDto>();

                int removed = existing.RemoveAll(m => string.Equals(m.Id, id, StringComparison.OrdinalIgnoreCase));
                if (removed > 0)
                {
                    File.WriteAllText(CustomModulesFilePath, JsonSerializer.Serialize(existing, new JsonSerializerOptions { WriteIndented = true }));
                }
                return removed > 0;
            }
            catch
            {
                return false;
            }
        }

        private static string? TryReadManifestJson()
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string[] candidatePaths =
                {
                    Path.Combine(baseDir, "modules.manifest.json"),
                    Path.Combine(Environment.CurrentDirectory, "modules.manifest.json")
                };

                foreach (var p in candidatePaths)
                {
                    if (File.Exists(p))
                    {
                        return File.ReadAllText(p);
                    }
                }

                var asm = Assembly.GetExecutingAssembly();
                string? resName = asm.GetManifestResourceNames()
                    .FirstOrDefault(n => n.EndsWith("modules.manifest.json", StringComparison.OrdinalIgnoreCase));

                if (!string.IsNullOrEmpty(resName))
                {
                    using var stream = asm.GetManifestResourceStream(resName);
                    if (stream != null)
                    {
                        using var reader = new StreamReader(stream);
                        return reader.ReadToEnd();
                    }
                }
            }
            catch
            {
                // Fallback handled by caller
            }

            return null;
        }

        private static OptimizationTask MapDtoToTask(ModuleManifestDto dto, bool isCustom)
        {
            if (!Enum.TryParse<TaskCategory>(dto.Category, true, out var cat))
            {
                cat = TaskCategory.System;
            }

            if (!Enum.TryParse<RiskLevel>(dto.Risk, true, out var risk))
            {
                risk = RiskLevel.Safe;
            }

            return new OptimizationTask
            {
                SequenceNumber = dto.Seq,
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description,
                Category = cat,
                SubCategory = dto.SubCategory ?? string.Empty,
                Risk = risk,
                ScanTargetFolder = dto.ScanTargetFolder ?? string.Empty,
                FastCliCommand = dto.FastCliCommand ?? string.Empty,
                PowerShellCommand = dto.PowerShellCommand ?? string.Empty,
                NativeActionType = dto.NativeActionType,
                IsSelected = dto.IsSelected,
                EstimatedBytes = dto.EstimatedKb > 0 ? dto.EstimatedKb * 1024L : 0L,
                TimeoutSeconds = dto.TimeoutSeconds > 0 ? dto.TimeoutSeconds : 15,
                RequiresAdmin = dto.RequiresAdmin,
                IsCustomModule = isCustom
            };
        }
    }
}
