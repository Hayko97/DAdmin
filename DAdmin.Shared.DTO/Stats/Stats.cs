namespace DAdmin.Shared.DTO.Stats;

public class Stats
{
    public int TotalTables { get; set; }
    public Dictionary<string, int> TableRecordCounts { get; set; } = new Dictionary<string, int>();

    public Dictionary<string, Dictionary<DateTime, int>> ActivityPerTableOverTime { get; set; } = new();

    public int? RecentActivityCount { get; set; }

    public SalesStats? SalesStats { get; set; }

    public ErrorLogStats? ErrorLogStats { get; set; }
}