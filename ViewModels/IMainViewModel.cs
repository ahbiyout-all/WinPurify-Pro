using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WinPurifyPro.ViewModels
{
    /// <summary>
    /// 시스템 트레이 서비스 및 UI 컴포넌트 연동을 위한 메인 뷰모델 인터페이스
    /// MVVM 아키텍처에 따라 Service 계층과 ViewModel 계층 간의 결합도를 낮추고
    /// WinPurifyPro 및 WinPurifyCommander 다중 프로젝트 빌드 시 CS0246 참조 오류를 방지합니다.
    /// </summary>
    public interface IMainViewModel : INotifyPropertyChanged
    {
        string StatusText { get; }
        double CpuUsage { get; }
        double RamUsagePercent { get; }
        int SelectedCount { get; }
        bool IsBusy { get; }
        bool IsTraySettingsModalOpen { get; set; }
        ICommand OpenScheduleModalCommand { get; }
        ICommand OpenLogFolderCommand { get; }
        ICommand OpenUpdateModalCommand { get; }
        Task OptimizeSelectedAsync();
        Task ScanAllAsync();
        Task CompressRamAsync();
    }
}
