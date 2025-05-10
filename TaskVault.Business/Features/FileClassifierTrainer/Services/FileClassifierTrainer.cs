using FileCategoryModelBuilder;
using Microsoft.ML;
using Microsoft.ML.Data;
using System.IO.Compression;
using System.Net.Http.Json;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using TaskVault.Contracts.Features.FileClassifierTrainer.Abstractions;
using TaskVault.Contracts.Features.FileClassifierTrainer.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.ML.Transforms.Text;
using Newtonsoft.Json;
using OpenAI;
using OpenAI.Chat;
using TaskVault.Contracts.Shared.Abstractions.Services;
using JsonElement = System.Text.Json.JsonElement;
using OpenAI;
using OpenAI.Chat;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace TaskVault.Business.Features.FileClassifierTrainer.Services;

public class FileClassifierTrainer : IFileClassifierTrainer
{
    private readonly IExceptionHandlingService _exceptionHandlingService;
    private readonly string _modelPath = @"C:\Users\balan\Desktop\models\cv_model.zip";

    public FileClassifierTrainer(IExceptionHandlingService exceptionHandlingService)
    {
        _exceptionHandlingService = exceptionHandlingService;
    }

    public async Task<TrainClassifierResponseDto> TrainFileClassifierModelOnFiles(string userEmail, TrainClassifierDto trainClassifierDto)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var extractedDocs = new List<DocumentData>();
            var tempDir = await ExtractFilesFromArchive(trainClassifierDto);

            foreach (var file in Directory.GetFiles(tempDir, "*.pdf", SearchOption.AllDirectories))
            {
                string text = ExtractTextFromPdf(file);

                extractedDocs.Add(new DocumentData
                {
                    Text = text,
                    IsCV = true
                });
            }

            var mlContext = new MLContext();
            var trainData = mlContext.Data.LoadFromEnumerable(extractedDocs);
            var trainedModel = TrainModel(trainData, mlContext);

            Directory.CreateDirectory(Path.GetDirectoryName(_modelPath)!);
            mlContext.Model.Save(trainedModel, trainData.Schema, _modelPath);

            var predictor = mlContext.Model.CreatePredictionEngine<DocumentData, DocumentPrediction>(trainedModel);

            var sample = new DocumentData { Text = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa" };
            var prediction = predictor.Predict(sample);
            Console.WriteLine($"Predicted IsCV: {prediction.IsCV}, Probability: {prediction.Probability:P2}");

            return new TrainClassifierResponseDto
            {
                Message = $"Model trained and saved to: {_modelPath}"
            };
        }, "");
    }
    
    public async Task<TrainClassifierResponseDto> RetrainModelWithAdditionalData(string userEmail, TrainClassifierDto positiveData, TrainClassifierDto negativeData)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var allDocs = new List<DocumentData>();

            var posDir = await ExtractFilesFromArchive(positiveData);
            foreach (var file in Directory.GetFiles(posDir, "*.pdf", SearchOption.AllDirectories))
            {
                string text = ExtractTextFromPdf(file);
                allDocs.Add(new DocumentData { Text = text, IsCV = true });
            }

            var negDir = await ExtractFilesFromArchive(negativeData);
            foreach (var file in Directory.GetFiles(negDir, "*.pdf", SearchOption.AllDirectories))
            {
                string text = ExtractTextFromPdf(file);
                allDocs.Add(new DocumentData { Text = text, IsCV = false });
            }

            var mlContext = new MLContext();
            var combinedData = mlContext.Data.LoadFromEnumerable(allDocs);

            var newModel = TrainModel(combinedData, mlContext);

            Directory.CreateDirectory(Path.GetDirectoryName(_modelPath)!);
            mlContext.Model.Save(newModel, combinedData.Schema, _modelPath);

            return new TrainClassifierResponseDto
            {
                Message = $"Model retrained with positive and negative examples and saved to: {_modelPath}"
            };
        }, "");
    }


    private ITransformer TrainModel(IDataView trainData, MLContext mlContext)
    {
        var textOptions = new TextFeaturizingEstimator.Options
        {
            WordFeatureExtractor = new WordBagEstimator.Options
            {
                NgramLength = 1,
                UseAllLengths = false
            },
            CharFeatureExtractor = null,
            KeepPunctuations = false,
            KeepDiacritics = false,
            KeepNumbers = false,
            CaseMode = TextNormalizingEstimator.CaseMode.Lower
        };

        var pipeline = mlContext.Transforms.Text
            .FeaturizeText("Features", nameof(DocumentData.Text))
            .Append(mlContext.BinaryClassification.Trainers
                .SdcaLogisticRegression(labelColumnName: nameof(DocumentData.IsCV), featureColumnName: "Features", maximumNumberOfIterations: 20));

        return pipeline.Fit(trainData);
    }

    private async Task<string> ExtractFilesFromArchive(TrainClassifierDto trainClassifierDto)
    {
        var tempZipPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.zip");
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        Directory.CreateDirectory(tempDir);

        using (var stream = new FileStream(tempZipPath, FileMode.Create))
        {
            await trainClassifierDto.TrainDataSetZip.CopyToAsync(stream);
        }

        ZipFile.ExtractToDirectory(tempZipPath, tempDir);
        return tempDir;
    }

    public async Task<PredictionResultDto> PredictFileCategory(string modelPath, IFormFile pdfFile)
    {
        var tempPdfPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.pdf");

        try
        {
            using (var stream = new FileStream(tempPdfPath, FileMode.Create))
            {
                await pdfFile.CopyToAsync(stream);
            }

            string text = ExtractTextFromPdf(tempPdfPath);

            var mlContext = new MLContext();

            ITransformer model;
            DataViewSchema modelSchema;
            using (var stream = new FileStream(_modelPath, FileMode.Open, FileAccess.Read))
            {
                model = mlContext.Model.Load(stream, out modelSchema);
            }

            var predictor = mlContext.Model.CreatePredictionEngine<DocumentData, DocumentPrediction>(model);
            var prediction = predictor.Predict(new DocumentData { Text = text });

            return new PredictionResultDto
            {
                Success = true,
                PredictedCategory = prediction.IsCV ? "CV" : "Not CV",
                Scores = new Dictionary<string, float>
                {
                    { "Probability", prediction.Probability },
                    { "Score", prediction.Score }
                }
            };
        }
        catch (Exception ex)
        {
            return new PredictionResultDto
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
        finally
        {
            try { if (File.Exists(tempPdfPath)) File.Delete(tempPdfPath); } catch { }
        }
    }
    
    public async Task<PredictionResultDto> PredictWithClassifier(string apiKey, IFormFile pdfFile)
{
    return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
    {
        var tempPdfPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.pdf");

        try
        {
            using (var stream = new FileStream(tempPdfPath, FileMode.Create))
            {
                await pdfFile.CopyToAsync(stream);
            }

            string text = ExtractTextFromPdf(tempPdfPath);

            var prompt = $"Given the following document text, estimate the probability that it is a curriculum vitae (CV). " +
                         $"Respond only with a JSON object in the form {{ \"probability\": XX }}, where XX is a percentage between 0 and 100.\n\n{text}";

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

            var requestBody = new
            {
                model = "gpt-4o-mini",
                messages = new[]
                {
                    new { role = "user", content = prompt }
                },
                response_format = new { type = "json_object" }
            };

            var response = await httpClient.PostAsJsonAsync("https://api.openai.com/v1/chat/completions", requestBody);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadFromJsonAsync<JsonElement>();
            var result = responseContent.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
            
            var jsonResponse = JsonSerializer.Deserialize<JsonElement>(result);
            var probability = jsonResponse.GetProperty("probability").GetSingle();

            return new PredictionResultDto
            {
                Success = true,
                PredictedCategory = probability > 50 ? "CV" : "Not CV",
                Scores = new Dictionary<string, float>
                {
                    { "Probability", probability }
                }
            };
        }
        finally
        {
            try { if (File.Exists(tempPdfPath)) File.Delete(tempPdfPath); } catch { }
        }
    }, "Error while classifying document with GPT.");
}

    private string ExtractTextFromPdf(string pdfPath)
    {
        using var document = PdfDocument.Open(pdfPath);
        var text = string.Join("\n", document.GetPages().Take(1).Select(p => p.Text));
        return text.Length <= 1000 ? text : text.Substring(0, 1000); // Extract up to 1000 chars for better features
    }
}
