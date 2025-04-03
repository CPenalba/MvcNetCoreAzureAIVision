using Microsoft.AspNetCore.Mvc;
using MvcNetCoreAzureAIVision.Services;

namespace MvcNetCoreAzureAIVision.Controllers
{
    public class OcrController : Controller
    {
        private readonly AzureOcrService _ocrService;

        public OcrController(AzureOcrService ocrService)
        {
            _ocrService = ocrService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                ViewBag.Error = "Por favor, selecciona un archivo válido.";
                return View("Index");
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };
            var extension = Path.GetExtension(imageFile.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
            {
                ViewBag.Error = "Formato de archivo no soportado. Use JPG, PNG, BMP o GIF.";
                return View("Index");
            }

            try
            {
                //Permite mostrar la imagen directamente en la vista 
                using (var memoryStream = new MemoryStream())
                {
                    await imageFile.CopyToAsync(memoryStream);
                    var imageBytes = memoryStream.ToArray();
                    ViewBag.ImageBase64 = Convert.ToBase64String(imageBytes);
                }

                //Obtiene un stream de lectura del archivo
                using (var stream = imageFile.OpenReadStream())
                {
                    var extractedText = await _ocrService.ExtractTextFromImage(stream);
                    ViewBag.ExtractedText = extractedText; //Almacena el resultado 
                    ViewBag.FileName = imageFile.FileName;
                    ViewBag.IsSuccess = true;
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error al procesar la imagen: {ex.Message}";
            }

            return View("Index");
        }
    }
}
