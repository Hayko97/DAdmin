namespace DAdmin.Components.Components.Charts.ViewModels;

public class ChartData
{
    public string[] Labels { get; set; }
    public List<ChartDataset> Datasets { get; set; }
}

public class ChartDataset
{
    public string Label { get; set; }
    public double[] Data { get; set; }
    public string BackgroundColor { get; set; }

    public string BorderColor { get; set; }
}

public class ChartValue
{
    public string Label { get; set; }
    public double Value { get; set; }
}