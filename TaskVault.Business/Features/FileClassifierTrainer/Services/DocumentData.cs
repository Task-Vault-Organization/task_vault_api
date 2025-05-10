using Microsoft.ML.Data;
public class DocumentData
{
    [LoadColumn(0)]
    public string Text { get; set; }

    [LoadColumn(1), ColumnName("Label")]
    public bool IsCV { get; set; }
}

public class DocumentPrediction
{
    [ColumnName("PredictedLabel")]
    public bool IsCV { get; set; }

    public float Probability { get; set; }
    
    public float Score { get; set; }
}

public class ModelMetricsDto
{
    public double Accuracy { get; set; }
    public double Auc { get; set; }
    public double F1Score { get; set; }
}