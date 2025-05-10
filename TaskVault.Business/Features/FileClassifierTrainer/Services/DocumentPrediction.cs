using Microsoft.ML.Data;

namespace FileCategoryModelBuilder;

public class DocumentPrediction
{
    [ColumnName("PredictedLabel")]
    public bool IsCV { get; set; }

    public float Probability { get; set; }
    public float Score { get; set; }
}