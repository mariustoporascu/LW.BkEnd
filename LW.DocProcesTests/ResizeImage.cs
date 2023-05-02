using LW.DocProcLogic.Anaf;
using Microsoft.Extensions.Configuration;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LW.DocProcesTests
{
	public class ResizeImage
	{
		[Fact]
		public void CheckImageResizeResult()
		{
			var filePath = @"E:\test.jpeg";
			var savePath = @"E:\test_result.jpeg";
			var result = ResizeImageAndWriteToStream(File.OpenRead(filePath), 1080, 1080);
			using (var fileStream = File.Create(savePath))
			{
				result.Seek(0, SeekOrigin.Begin);
				result.CopyTo(fileStream);
			}
			result.Should().NotBeNull();
		}
		private SKBitmap ResizeImageMaintainAspectRatio(SKBitmap originalImage, int newWidth, int newHeight)
		{
			// Calculate the aspect ratio
			float originalAspectRatio = (float)originalImage.Width / originalImage.Height;
			float newAspectRatio = (float)newWidth / newHeight;

			if (originalAspectRatio > newAspectRatio)
			{
				// Keep the width and adjust the height
				newHeight = (int)(newWidth / originalAspectRatio);
			}
			else
			{
				// Keep the height and adjust the width
				newWidth = (int)(newHeight * originalAspectRatio);
			}

			// Create the new empty bitmap with the calculated dimensions
			SKBitmap resizedImage = new SKBitmap(newWidth, newHeight);

			// Draw the original image onto the new bitmap
			using (SKCanvas canvas = new SKCanvas(resizedImage))
			{
				canvas.DrawBitmap(originalImage, new SKRect(0, 0, newWidth, newHeight));
			}

			return resizedImage;
		}
		private MemoryStream ResizeImageAndWriteToStream(Stream inputStream, int newWidth, int newHeight)
		{
			MemoryStream outputStream = new MemoryStream();

			using (SKBitmap originalImage = SKBitmap.Decode(inputStream))
			{
				using (SKBitmap resizedImage = ResizeImageMaintainAspectRatio(originalImage, newWidth, newHeight))
				{
					SKPixmap pixmap = new SKPixmap(resizedImage.Info, resizedImage.GetPixels());
					if (pixmap.Encode(outputStream, SKEncodedImageFormat.Jpeg, 90))
					{
						outputStream.Flush();
						outputStream.Position = 0; // Reset the position of the MemoryStream to the beginning
					}
				}
			}

			return outputStream;
		}
	}
}
