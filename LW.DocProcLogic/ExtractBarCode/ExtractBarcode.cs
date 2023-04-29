using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using ZXing.SkiaSharp;

namespace LW.DocProcLogic.ExtractBarCode
{
	public static class ExtractBarcode
	{
		public static string? GetFromImage(Stream fileStream)
		{
			// create a barcode reader instance
			BarcodeReader barReader = new BarcodeReader()
			{
				AutoRotate = true,
				Options =
				{
					TryHarder = true,
					TryInverted = true,
				}
			};
			QRCodeReader qrReader = new QRCodeReader();
			// load a bitmap
			var originalBitmap = SKBitmap.FromImage(SKImage.FromEncodedData(fileStream));

			// resize the bitmap
			int targetWidth = originalBitmap.Height > originalBitmap.Width ? 1080 : 1920;
			int targetHeight = (int)Math.Round(originalBitmap.Height * (float)targetWidth / originalBitmap.Width);
			var resizedBitmap = originalBitmap.Resize(new SKImageInfo(targetWidth, targetHeight), SKFilterQuality.High);

			//try to decode the barcode multiple times
			Result resultBar = null;
			Result resultQR = null;
			for (int tryCount = 0; tryCount < 2; tryCount++)
			{
				// apply brightness and contrast adjustments
				var adjustedBitmap = AdjustBrightnessAndContrast(resizedBitmap,
					tryCount != 0 ? 1.2f : 0.8f, tryCount != 0 ? 30 : -30);

				// save adjustedBitmap as a local PNG file
				SaveBitmapAsPng(adjustedBitmap, $"E:\\adjusted_image_{tryCount}.png");

				// detect and decode the barcode inside the bitmap
				resultBar = barReader.Decode(adjustedBitmap);
				resultQR = qrReader.decode(SKBitmapToBinaryBitmap(adjustedBitmap));
				if (resultBar == null && resultQR == null) continue;
			}
			// return the result no matter what
			return resultBar?.Text ?? resultQR?.Text;
		}
		private static BinaryBitmap SKBitmapToBinaryBitmap(SKBitmap inputBitmap)
		{
			// Convert SKBitmap to LuminanceSource
			var luminanceSource = new SKBitmapLuminanceSource(inputBitmap);

			// Create a BinaryBitmap from the LuminanceSource
			var hybridBinarizer = new HybridBinarizer(luminanceSource);

			return new BinaryBitmap(hybridBinarizer);
		}
		private static void SaveBitmapAsPng(SKBitmap bitmap, string outputPath)
		{
			using (var image = SKImage.FromBitmap(bitmap))
			using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
			using (var stream = File.OpenWrite(outputPath))
			{
				data.SaveTo(stream);
			}
		}
		private static SKBitmap AdjustBrightnessAndContrast(SKBitmap inputBitmap, float contrast, int brightness)
		{
			int width = inputBitmap.Width;
			int height = inputBitmap.Height;
			var outputBitmap = new SKBitmap(width, height);

			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					var pixel = inputBitmap.GetPixel(x, y);

					int r = (int)((pixel.Red * contrast) + brightness);
					int g = (int)((pixel.Green * contrast) + brightness);
					int b = (int)((pixel.Blue * contrast) + brightness);

					byte red = (byte)Math.Clamp(r, 0, 255);
					byte green = (byte)Math.Clamp(g, 0, 255);
					byte blue = (byte)Math.Clamp(b, 0, 255);

					outputBitmap.SetPixel(x, y, new SKColor(red, green, blue));
				}
			}

			return outputBitmap;
		}
	}
}
