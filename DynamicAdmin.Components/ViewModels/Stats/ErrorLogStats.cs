namespace DynamicAdmin.Components.ViewModels.Stats;

public class ErrorLogStats
{
    public int ErrorLogCount { get; set; }
    public Dictionary<string, int> ErrorTypeCounts { get; set; } = new Dictionary<string, int>();
    public int RecentErrorCount { get; set; }
    public Dictionary<string, int> MostCommonErrors { get; set; } = new Dictionary<string, int>();
}
