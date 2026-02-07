using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MoreAutomation.UI.Modules.DailyTasks
{
    public partial class DailyTasksViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool _dailyCheckIn;

        [ObservableProperty]
        private bool _hourlyHarvest;

        [ObservableProperty]
        private bool _dailyWater;

        [ObservableProperty]
        private bool _autoCompose;

        [ObservableProperty]
        private string _checkInTime = "08:00";

        [ObservableProperty]
        private int _harvestIntervalMinutes = 60;

        [ObservableProperty]
        private string _waterTimes = "08:00,12:00,20:00";

        public DailyTasksViewModel()
        {
            DailyCheckIn = true;
            HourlyHarvest = true;
            DailyWater = true;
            AutoCompose = false;
        }

        [RelayCommand]
        private void SaveConfig()
        {
            // TODO: 持久化任务配置到 AppConfig
        }

        [RelayCommand]
        private void ResetToDefault()
        {
            CheckInTime = "08:00";
            HarvestIntervalMinutes = 60;
            WaterTimes = "08:00,12:00,20:00";
            DailyCheckIn = true;
            HourlyHarvest = true;
            DailyWater = true;
            AutoCompose = false;
        }
    }
}
