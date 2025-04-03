using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;

namespace MvcNetCoreAzureAIVision.Services
{
    public class AzureOcrService
    {
        //Clase para interactuar con los servicios de Azure Cognitive para Computer Vision
        private readonly ComputerVisionClient _client;

        public AzureOcrService(IConfiguration configuration)
        {
            var endpoint = configuration["AzureComputerVision:Endpoint"];
            var key = configuration["AzureComputerVision:SubscriptionKey"];

            //Se encarga de manejar la autenticación basada en claves API para los servicios de Azure Cognitive
            _client = new ComputerVisionClient(new ApiKeyServiceClientCredentials(key))
            {
                Endpoint = endpoint
            };
        }

        //Recibe la imagen a procesar y devuelve un string del texto reconido
        public async Task<string> ExtractTextFromImage(Stream imageStream)
        {
            // Método que inicia el reconocimiento de texto en la imagen
            var textHeaders = await _client.ReadInStreamAsync(imageStream);
            string operationLocation = textHeaders.OperationLocation;
            //Extraemos el id que identifica la operación
            string operationId = operationLocation.Substring(operationLocation.Length - 36);

            ReadOperationResult results;
            do
            {
                //Método para consultar el estado del resultado
                results = await _client.GetReadResultAsync(Guid.Parse(operationId));
                await Task.Delay(1000);
            }
            //Continua mientras el estado sea en progreso o no iniciado
            while (results.Status == OperationStatusCodes.Running || results.Status == OperationStatusCodes.NotStarted);

            //StringBuilder para almacenar el texto
            var extractedText = new System.Text.StringBuilder();
            if (results.Status == OperationStatusCodes.Succeeded)
            {
                //Aqui se gestiona la extracción del texto por líneas
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
