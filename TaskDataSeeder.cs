using System.Collections.Generic;
using WinPurifyPro.Models;
using WinPurifyPro.Services;

namespace WinPurifyPro
{
    /// <summary>
    /// 통합 JSON 매니페스트(modules.manifest.json) 및 사용자 정의 스튜디오 모듈 기반 태스크 로더
    /// (기존 1,850줄 하드코딩 중복을 제거하고 WPF/Electron 간 단일 진실 공급원 유지)
    /// </summary>
    public static class TaskDataSeeder
    {
        public static List<OptimizationTask> GetAllTasks()
        {
            var list = CustomModuleService.LoadBuiltInManifestTasks();

            // 사용자 정의 스튜디오 커스텀 모듈 병합 (%LocalAppData%\WinPurifyPro\custom-modules.json)
            var customModules = CustomModuleService.LoadCustomModules();
            foreach (var custom in customModules)
            {
                if (!list.Exists(t => t.Id == custom.Id))
                {
                    list.Add(custom);
                }
            }

            // 매니페스트 유실 시 최소 안전 핵심 모듈 폴백 보장
            if (list.Count == 0)
            {
                list.AddRange(GetEmergencyFallbackTasks());
            }

            for (int i = 0; i < list.Count; i++)
            {
                list[i].SequenceNumber = i + 1;
            }

            return list;
        }

        private static List<OptimizationTask> GetEmergencyFallbackTasks()
        {
            return new List<OptimizationTask>
            {
                new OptimizationTask
                {
                    Id = "sys-temp-cleanup",
                    Name = "시스템 임시 파일(Temp) 정화",
                    Description = "Windows 및 사용자 Temp 폴더 잔여 캐시 소거",
                    Category = TaskCategory.Storage,
                    Risk = RiskLevel.Safe,
                    ScanTargetFolder = @"$env:TEMP",
                    FastCliCommand = @"del /f /q /s ""%TEMP%\*""",
                    PowerShellCommand = @"Remove-Item ""$env:TEMP\*"" -Recurse -Force -ErrorAction SilentlyContinue",
                    NativeActionType = 3,
                    TimeoutSeconds = 30,
                    IsSelected = true
                },
                new OptimizationTask
                {
                    Id = "sys-dns-flush",
                    Name = "DNS 캐시 플러시 및 소켓 갱신",
                    Description = "로컬 DNS 리졸버 캐시를 초기화하여 네트워크 응답성 복원",
                    Category = TaskCategory.System,
                    Risk = RiskLevel.Safe,
                    FastCliCommand = @"ipconfig /flushdns",
                    PowerShellCommand = @"Clear-DnsClientCache",
                    NativeActionType = 2,
                    TimeoutSeconds = 15,
                    IsSelected = true
                }
            };
        }
    }
}
