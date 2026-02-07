namespace MoreAutomation.Contracts.Configuration
{
    public class AppConfig
    {
        public string ClientPath { get; set; } = string.Empty;
        public bool IsAgreed { get; set; } = false;
        public string LastMode { get; set; } = "Normal";
    }
}