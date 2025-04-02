using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;

namespace MvcNetCoreAzureAIVision.Services
{
    public class AzureOcrService
    {
        private readonly ComputerVisionClient _client;

        public AzureOcrService(IConfiguration configuration)
        {
            var endpoint = configuration["AzureComputerVision:Endpoint"];
            var key = configuration["AzureComputerVision:SubscriptionKey"];

            _client = new ComputerVisionClient(new ApiKeyServiceClientCredentials(key))
            {
                Endpoint = endpoint
            };
        }

        public async Task<string> ExtractTextFromImage(Stream imageStream)
        {
            var textHeaders = await _client.ReadInStreamAsync(imageStream);
            string operationLocation = textHeaders.OperationLocation;
            string operationId = operationLocation.Substring(operationLocation.Length - 36);

            ReadOperationResult results;
            do
            {
                results = await _client.GetReadResultAsync(Guid.Parse(operationId));
                await Task.Delay(1000);
            }
            while (results.Status == OperationStatusCodes.Running || results.Status == OperationStatusCodes.NotStarted);

            var extractedText = new System.Text.StringBuilder();
            if (results.Status == OperationStatusCodes.Succeeded)
            {
                foreach (var page in results.AnalyzeResult.ReadResults)
                {
                    foreach (var line in page.Lines)
                    {
                        extractedText.AppendLine(line.Text);
                    }
                }
            }
            return extractedText.ToString();
        }
    }
}
