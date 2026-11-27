using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WinPurifyPro.Models;
using WinPurifyPro.Services;

namespace WinPurifyPro.ViewModels
{
    public partial class MainViewModel
    {
        private void LoadTasks()
        {
            var tasks = TaskDataSeeder.GetAllTasks();
            AllTasks.Clear();
            foreach (var t in tasks)
            {
                t.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(OptimizationTask.IsSelected))
                    {
                        if (t.Id == "spc-sync-default-user-profile" && t.IsSelected && !_suppressDefaultProfileWarning)
                        {
                            _suppressDefaultProfileWarning = true;
                            t.IsSelected = false;
                            _suppressDefaultProfileWarning = false;
                            IsDefaultProfileWarningModalOpen = true;
                            return;
                        }
                        OnPropertyChanged(nameof(SelectedCount));
                        CalculateTotalReclaimable();
                    }
                };
                AllTasks.Add(t);
            }
            ApplyFilter();
            OnPropertyChanged(nameof(TotalTasksCount));
            OnPropertyChanged(nameof(SpecialSubCategoryCountAll));
            OnPropertyChanged(nameof(SpecialSubCategoryCountAccount));
            OnPropertyChanged(nameof(SpecialSubCategoryCountFactoryReset));
            OnPropertyChanged(nameof(SpecialSubCategoryCountProfile));
            OnPropertyChanged(nameof(SpecialSubCategoryCountForensics));
            AddLog($"[엔진 초기화] v{AppEnvironment.AppVersion} 동적 JSON 카탈로그 엔진 가동 - {AllTasks.Count}개 모듈 로드 완료.");
            AddLog($"[인스턴스 식별] 고유 프로세스 ID: {ProcessInstanceId} (다중 인스턴스 및 Central Commander 동시 실행 전면 허용)");
        }

        public bool AddCustomStudioModule(string name, string category, string targetFolder, string fastCliCommand, string powerShellCommand, RiskLevel risk = RiskLevel.Safe, int timeoutSeconds = 30, bool requiresAdmin = false)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;

            var dto = new ModuleManifestDto
            {
                Id = $"custom-{Guid.NewGuid():N}".Substring(0, 15),
                Name = name.Trim(),
                Description = $"사용자 정의 작업실 모듈 ({(string.IsNullOrWhiteSpace(targetFolder) ? "직접 명령" : targetFolder)})",
                Category = category,
                Risk = risk.ToString(),
                ScanTargetFolder = targetFolder.Trim(),
                FastCliCommand = fastCliCommand.Trim(),
                PowerShellCommand = powerShellCommand.Trim(),
                NativeActionType = 0,
                IsSelected = true,
                TimeoutSeconds = Math.Clamp(timeoutSeconds, 10, 300),
                RequiresAdmin = requiresAdmin
            };

            if (!CustomModuleService.SaveCustomModule(dto)) return false;
            LoadTasks();
            AddLog($"[커스텀 모듈 추가] '{dto.Name}' 작업실 모듈이 custom-modules.json에 등록되었습니다.");
            return true;
        }

        public bool RemoveCustomStudioModule(string moduleId)
        {
            if (!CustomModuleService.RemoveCustomModule(moduleId)) return false;
            LoadTasks();
            AddLog($"[커스텀 모듈 삭제] 모듈({moduleId})이 목록에서 제거되었습니다.");
            return true;
        }

        public void ApplyFilter()
        {
            FilteredTasks.Clear();
            var query = AllTasks.AsEnumerable();

            if (SelectedCategory.HasValue)
            {
                query = query.Where(t => t.Category == SelectedCategory.Value);

                if (SelectedCategory.Value == TaskCategory.Special &&
                    !string.IsNullOrEmpty(SelectedSpecialSubCategory) &&
                    !string.Equals(SelectedSpecialSubCategory, "All", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(t => string.Equals(t.SubCategory, SelectedSpecialSubCategory, StringComparison.OrdinalIgnoreCase));
                }
            }

            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                string lower = SearchQuery.ToLower();
                query = query.Where(t => t.Name.ToLower().Contains(lower) || t.Description.ToLower().Contains(lower) || t.Id.ToLower().Contains(lower));
            }

            int seq = 1;
            foreach (var item in query)
            {
                item.SequenceNumber = seq++;
                FilteredTasks.Add(item);
            }
            OnPropertyChanged(nameof(SelectedCount));
        }

        private void SetSelection(bool select)
        {
            foreach (var t in FilteredTasks)
            {
                if (select && t.Category == TaskCategory.Special && SelectedCategory != TaskCategory.Special)
                {
                    continue;
                }
                t.IsSelected = select;
            }
            CalculateTotalReclaimable();
            OnPropertyChanged(nameof(SelectedCount));
        }

        private void ApplyPreset(RiskLevel maxRisk)
        {
            foreach (var t in AllTasks)
            {
                if (t.Category == TaskCategory.Special)
                {
                    t.IsSelected = false;
                    continue;
                }
                t.IsSelected = (t.Risk <= maxRisk);
            }
            CalculateTotalReclaimable();
            OnPropertyChanged(nameof(SelectedCount));
            AddLog($"[프리셋] {maxRisk} 등급 이하 항목 자동 선택 완료 (선택: {SelectedCount}개)");
        }

        private void ApplyGamingProfile()
        {
            foreach (var t in AllTasks)
            {
                if (t.Category == TaskCategory.Special)
                {
                    t.IsSelected = false;
                    continue;
                }
                t.IsSelected = (t.Category == TaskCategory.System || t.Category == TaskCategory.Storage || t.Category == TaskCategory.Update) && (t.Risk <= RiskLevel.Deep);
            }
            CalculateTotalReclaimable();
            OnPropertyChanged(nameof(SelectedCount));
            AddLog($"[프로필 적용] Gaming Boost Profile 활성화 (선택: {SelectedCount}개 모듈)");
        }

        private void ApplyPrivacyProfile()
        {
            foreach (var t in AllTasks)
            {
                if (t.Category == TaskCategory.Special)
                {
                    t.IsSelected = false;
                    continue;
                }
                t.IsSelected = (t.Category == TaskCategory.Privacy || t.Category == TaskCategory.Browser || t.Category == TaskCategory.Security);
            }
            CalculateTotalReclaimable();
            OnPropertyChanged(nameof(SelectedCount));
            AddLog($"[프로필 적용] Deep Privacy Profile 활성화 (선택: {SelectedCount}개 모듈)");
        }

        private void ApplyZeroTraceProfile()
        {
            foreach (var t in AllTasks)
            {
                if (t.Category == TaskCategory.Special)
                {
                    t.IsSelected = false;
                    continue;
                }

                if (t.Category == TaskCategory.Privacy || t.Category == TaskCategory.Browser || t.Category == TaskCategory.Security)
                {
                    t.IsSelected = true;
                    continue;
                }

                if (t.Category == TaskCategory.Storage)
                {
                    bool isZeroTraceStorage = t.Id.Contains("recycle-bin") ||
                                              t.Id.Contains("crash-memory") ||
                                              t.Id.Contains("wer-reports") ||
                                              t.Id.Contains("temp") ||
                                              t.Id.Contains("hiberfil") ||
                                              t.Id.Contains("wsearch-edb") ||
                                              t.Id.Contains("prefetch");
                    t.IsSelected = isZeroTraceStorage;
                    continue;
                }

                t.IsSelected = false;
            }
            CalculateTotalReclaimable();
            OnPropertyChanged(nameof(SelectedCount));
            AddLog($"[프로필 적용] Zero-Trace 안티포렌식 초기화 프로필 활성화 ({SelectedCount}개 사용자 흔적/자격증명/세션 모듈 집중 선별)");
        }

        public async Task ScanAllAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            ProgressValue = 0;
            TotalProgressMax = AllTasks.Count;
            StatusText = "초고속 I/O 용량 분석 중 (Robocopy / Win32 API)...";
            AddLog($"[스캔 시작] {AllTasks.Count}개 모듈 대상 Robocopy 초고속 가상 탐색 개시");

            long totalBytes = 0;
            int scanned = 0;

            await Task.Run(async () =>
            {
                foreach (var task in AllTasks)
                {
                    task.Status = Models.TaskStatus.Scanning;
                    if (!string.IsNullOrWhiteSpace(task.ScanTargetFolder))
                    {
                        long size = await RobocopyScannerService.GetFolderSizeFastAsync(task.ScanTargetFolder);
                        task.EstimatedBytes = size;
                        if (task.IsSelected) totalBytes += size;
                    }
                    task.Status = Models.TaskStatus.Scanned;
                    scanned++;

                    System.Windows.Application.Current?.Dispatcher?.Invoke(() =>
                    {
                        ProgressValue = scanned;
                        TotalReclaimableBytes = totalBytes;
                    });
                }
            });

            IsBusy = false;
            StatusText = $"스캔 완료 - 확보 가능 공간: {FormattedTotalSize}";
            AddLog($"[스캔 완료] 총 {AllTasks.Count}개 모듈 분석 완료. 예상 절감 용량: {FormattedTotalSize}");
            SystemTrayService.Instance.ShowTrayNotification("🔍 전체 스캔 완료", $"총 {AllTasks.Count}개 모듈 분석 완료. 예상 절감 용량: {FormattedTotalSize}", TrayNotificationType.Info);
            SystemTrayService.Instance.UpdateTooltip($"WinPurify Pro v{AppVersion}\n스캔 완료 (확보 가능: {FormattedTotalSize})\n상태: 준비 완료");
        }

        public async Task OptimizeSelectedAsync()
        {
            var targets = AllTasks.Where(t => t.IsSelected).ToList();
            if (!targets.Any()) return;

            IsBusy = true;
            ProgressValue = 0;
            TotalProgressMax = targets.Count;
            StatusText = "하이브리드 다중화 엔진 최적화 진행 중...";

            SystemTrayService.Instance.ResetProgressMilestone();
            SystemTrayService.Instance.ShowTrayNotification("🚀 시스템 최적화 시작", $"{targets.Count}개 모듈의 초고속 정화 작업을 시작합니다.", TrayNotificationType.Info);

            AddLog("[Safepoint] 최적화 전 레지스트리 백업 및 무결성 복원 지점 생성 중...");
            LastSafepoint = await LiveSafepointService.CreateSafepointAsync(targets, TotalReclaimableBytes);
            AddLog($"[Safepoint 완료] 복원 스냅샷 저장됨 ({LastSafepoint.BackupRegFiles.Count}개 레지스트리 키 백업 완료)");

            int completed = 0;
            int successCount = 0;

            foreach (var task in targets)
            {
                task.Status = Models.TaskStatus.Optimizing;
                task.LastFailureDiagnosis = string.Empty;
                var res = await MultiEngineDispatcher.ExecuteTaskAsync(task);

                string traceTrail = res.Traces.Count > 0
                    ? string.Join(" → ", res.Traces.Select(tr => $"{tr.EngineName}({(tr.Succeeded ? "OK" : $"E:{tr.ExitCode}")},{tr.DurationMs}ms)"))
                    : res.UsedMode.ToString();

                if (res.Success)
                {
                    task.Status = Models.TaskStatus.Completed;
                    task.StatusMessage = $"{res.UsedMode} ({res.ExecutionTimeMs}ms)";
                    successCount++;
                    AddLog($"[성공] {task.Name} -> [{traceTrail}] ({res.ExecutionTimeMs}ms)");
                }
                else
                {
                    task.Status = Models.TaskStatus.Failed;
                    task.LastFailureDiagnosis = res.FailureDiagnosis;
                    task.StatusMessage = string.IsNullOrWhiteSpace(res.FailureDiagnosis)
                        ? "실행 실패"
                        : $"실패: {res.FailureDiagnosis}";
                    AddLog($"[오류] {task.Name} -> [{traceTrail}] | 진단: {res.OutputMessage}");
                }

                completed++;
                ProgressValue = completed;
                SystemTrayService.Instance.UpdateProgress(completed, targets.Count, task.Name);
            }

            IsBusy = false;
            StatusText = $"최적화 완료 ({successCount}/{targets.Count} 모듈 성공)";
            AddLog($"[작업 완료] {successCount}개 모듈 최적화 성공. (실패/건너뜀: {targets.Count - successCount}개)");
            SystemTrayService.Instance.ShowTrayNotification("✨ 최적화 완료", $"총 {successCount}/{targets.Count}개 모듈 최적화 성공! 리소스가 회수되었습니다.", TrayNotificationType.Success);
            SystemTrayService.Instance.UpdateTooltip($"WinPurify Pro v{AppVersion}\n최적화 완료 ({successCount}/{targets.Count} 성공)\n상태: 준비 완료");
        }

        public async Task RollbackSafepointAsync()
        {
            if (LastSafepoint == null || IsBusy) return;
            IsBusy = true;
            StatusText = "세이프포인트 복원(Undo Rollback) 실행 중...";
            AddLog($"[Rollback] 스냅샷({LastSafepoint.Id.Substring(0, 8)}) 복원 개시...");

            bool success = await LiveSafepointService.RollbackSafepointAsync(LastSafepoint);

            IsBusy = false;
            if (success)
            {
                StatusText = "세이프포인트 복원 성공 (1-Click Undo 완료)";
                AddLog("[Rollback 완료] 백업된 레지스트리 및 시스템 설정이 원래대로 복원되었습니다.");
                SystemTrayService.Instance.ShowTrayNotification("↩️ 세이프포인트 복원 성공", "레지스트리 및 시스템 설정이 복원되었습니다.", TrayNotificationType.Success);
            }
            else
            {
                StatusText = "세이프포인트 복원 중 일부 경고 발생";
                AddLog("[Rollback 경고] 일부 레지스트리 항목 복원 중 권한 확인이 필요합니다.");
                SystemTrayService.Instance.ShowTrayNotification("⚠️ 세이프포인트 복원 경고", "일부 레지스트리 복원 중 관리자 권한 확인이 필요합니다.", TrayNotificationType.Warning);
            }
        }

        public async Task CompressRamAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            StatusText = "물리 RAM 커널 압축 및 NT 대기 메모리(Standby List) 완전 정화 중...";
            AddLog("[RAM 최적화] 커널 특권 획득 후 전체 프로세스 Working Set 압축 및 Standby List 정화 개시");

            int count = await Task.Run(() =>
            {
                NativeEngineService.TryPurgeStandbyList(4);
                return NativeEngineService.CompressPhysicalRAM();
            });

            IsBusy = false;
            StatusText = $"RAM 압축 및 대기 메모리 정화 완료 ({count}개 프로세스 메모리 회수)";
            AddLog($"[RAM 최적화 완료] {count}개 활성 프로세스 Working Set Trim 및 NT 커널 대기 메모리 완전 정화 완료.");
            SystemTrayService.Instance.ShowTrayNotification("🧠 RAM & 대기 메모리 정화 완료", $"{count}개 프로세스 Trim 및 커널 Standby List 정화 완료. 가용 메모리가 즉시 확보되었습니다.", TrayNotificationType.Success);
        }

        public async Task ApplyGameBoostAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            StatusText = "하이퍼 게이밍 가속 모드 활성화 중...";
            AddLog("[Game Boost] 게임 프로세스 우선순위 HIGH 격상, 대기 메모리 정화 및 네트워크 스택 최적화 개시");

            await Task.Run(() =>
            {
                NativeEngineService.TrySetKernelGameBoost(true);
                NativeEngineService.TryOptimizeNetwork(1);
                NativeEngineService.ExecuteQuickMaintenance(2);
                NativeEngineService.TryPurgeStandbyList(4);
                NativeEngineService.CompressPhysicalRAM();
            });

            IsBusy = false;
            StatusText = "게이밍 가속 적용 완료 (Low-Latency Mode)";
            AddLog("[Game Boost 완료] 대기 메모리 정화, 지연시간 단축 및 프레임 드랍 방지 가속 설정 적용됨.");
        }

        private void CalculateTotalReclaimable()
        {
            TotalReclaimableBytes = AllTasks.Where(t => t.IsSelected).Sum(t => t.EstimatedBytes);
        }
    }
}
