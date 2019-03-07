using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Drawing;
using ZXing.QrCode;

namespace QRCodeFunction
{
    public static class GenerateQRCode
    {
        [FunctionName("GenerateQRCode")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]HttpRequest req,
            ILogger log,
            ExecutionContext context)
        {
            log.LogInformation("GenerateQRCode received a request.");

            // process input parameters
            string data = req.Query["data"];
            string size = req.Query["size"];
            string dpi = req.Query["dpi"];

            int sizeValue = 50;
            if (!string.IsNullOrEmpty(size))
            {
                int.TryParse(size, out sizeValue);
            }

            int dpiValue = 600;
            if (!string.IsNullOrEmpty(dpi))
            {
                int.TryParse(dpi, out dpiValue);
            }

            // render QR code into bitmap
            if (!string.IsNullOrEmpty(data))
            {
                var writer = new QRCodeWriter();
                var pixelData = writer.encode(data, ZXing.BarcodeFormat.QR_CODE, 0, 0);
                var renderer = new ZXing.CoreCompat.Rendering.BitmapRenderer()
                {
                    DpiX = dpiValue,
                    DpiY = dpiValue
                };
                var bitmap = renderer.Render(pixelData, ZXing.BarcodeFormat.QR_CODE, data, new ZXing.Common.EncodingOptions()
                {
                    Height = sizeValue,
                    Width = sizeValue
                });

                return new FileContentResult(ImageToByteArray(bitmap), "image/jpeg");
            }
            else
            {
                log.LogInformation("No data parameter provided.");
                return new BadRequestResult();
            }
        }

        private static byte[] ImageToByteArray(Image image)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(image, typeof(byte[]));
        }
    }
}
