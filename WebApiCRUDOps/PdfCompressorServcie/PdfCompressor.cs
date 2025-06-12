using iText.IO.Image;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Xobject;
using PdfSharpCore.Drawing;
using PdfSharpCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System.IO;
using System.Reflection.Metadata;

namespace WebApiCRUDOps.PdfCompressorServcie
{
    public class PdfCompressor
    {
        // Path to Ghostscript executable (adjust to your env)
        private static readonly string _ghostscriptPath = @"C:\Program Files\gs\gs10.05.1\bin\gswin64c.exe";

        public enum CompressionLevel { High, Moderate, Low }

        public static byte[] CompressPdf(byte[] pdfBytes, CompressionLevel level)
        {
            // Define DPI and JPEG quality per compression level
            int targetDpi = level switch
            {
                CompressionLevel.High => 72,
                CompressionLevel.Moderate => 100,
                CompressionLevel.Low => 150,
                _ => 100
            };

            int jpegQuality = level switch
            {
                CompressionLevel.High => 25,      // most compression, lower quality
                CompressionLevel.Moderate => 50,  // balanced
                CompressionLevel.Low => 75,       // less compression, better quality
                _ => 65
            };

            using var inputPdf = new MemoryStream(pdfBytes);
            using var outputPdf = new MemoryStream();

            var reader = new PdfReader(inputPdf);
            var writer = new PdfWriter(outputPdf, new WriterProperties()
                        .SetCompressionLevel(9)); // 9 = max compression for zlib
            var pdfDoc = new PdfDocument(reader, writer);

            for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
            {
                var page = pdfDoc.GetPage(i);
                var resources = page.GetResources();
                var xObjects = resources.GetResource(PdfName.XObject);

                if (xObjects == null) continue;

                foreach (var xObj in xObjects.KeySet())
                {
                    var obj = xObjects.GetAsStream(xObj);
                    if (obj is PdfStream stream && stream.Get(PdfName.Subtype)?.Equals(PdfName.Image) == true)
                    {
                        var pdfImage = new PdfImageXObject(stream);
                        byte[] imageBytes = pdfImage.GetImageBytes(true);

                        using var image = Image.Load(imageBytes);

                        // Calculate new image size based on DPI downscaling (assuming source 300 DPI)
                        int newWidth = image.Width * targetDpi / 300;
                        int newHeight = image.Height * targetDpi / 300;

                        image.Mutate(x => x.Resize(newWidth, newHeight));

                        // Save compressed image to JPEG stream with quality setting
                        using var ms = new MemoryStream();
                        image.Save(ms, new JpegEncoder { Quality = jpegQuality });
                        ms.Position = 0;

                        var newImageData = ImageDataFactory.Create(ms.ToArray());
                        var newPdfImage = new PdfImageXObject(newImageData);

                        // Replace old image with new compressed image
                        xObjects.Put(xObj, newPdfImage.GetPdfObject());
                    }
                }
            }

            // Optional: Remove metadata to save space
            var docInfo = pdfDoc.GetDocumentInfo();

            // Clear known metadata properties by setting them to empty string or null
            docInfo.SetTitle("");
            docInfo.SetAuthor("");
            docInfo.SetSubject("");
            docInfo.SetKeywords("");
            docInfo.SetCreator("");
            // No SetProducer() method available, so skip that

            // Clear custom metadata entries (MoreInfo dictionary)
            docInfo.SetMoreInfo(new System.Collections.Generic.Dictionary<string, string>());

            // Remove metadata stream from catalog (embedded XMP metadata)
            pdfDoc.GetCatalog().Remove(PdfName.Metadata);


            pdfDoc.Close();
            return outputPdf.ToArray();
        }
        public static byte[] CompressWithGhostscript(byte[] inputPdfBytes, string mode)
        {
            string tempInput = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".pdf");
            string tempOutput = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".pdf");

            try
            {
                string pdfSetting = mode.ToLower() switch
                {
                    "low" => "/screen",
                    "medium" => "/ebook",
                    "high" => "/printer",
                    _ => throw new ArgumentException($"Invalid compression mode: {mode}")
                };

                File.WriteAllBytes(tempInput, inputPdfBytes);

                var args = $"-sDEVICE=pdfwrite -dCompatibilityLevel=1.4 -dPDFSETTINGS={pdfSetting} " +
                           $"-dNOPAUSE -dBATCH -sOutputFile=\"{tempOutput}\" \"{tempInput}\"";

                var process = new System.Diagnostics.Process();
                process.StartInfo.FileName = _ghostscriptPath;
                process.StartInfo.Arguments = args;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;

                Console.WriteLine($"Running: \"{_ghostscriptPath}\" {args}");

                process.Start();
                string stdOut = process.StandardOutput.ReadToEnd();
                string stdErr = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    Console.WriteLine("Ghostscript STDERR: " + stdErr);
                    throw new Exception($"Ghostscript failed: {stdErr}");
                }

                return File.ReadAllBytes(tempOutput);
            }
            finally
            {
                if (File.Exists(tempInput)) File.Delete(tempInput);
                if (File.Exists(tempOutput)) File.Delete(tempOutput);
            }
        }

        public static async Task<byte[]> ConvertImagetoPdf(List<IFormFile> files)
        {
            using var outputStream = new MemoryStream();
            using var document = new PdfSharpCore.Pdf.PdfDocument();



            foreach (var file in files)
            {
                using var imageStream = file.OpenReadStream();
                using var image = await Image.LoadAsync(imageStream);

                // Resize the image if it's too large (e.g., max width = 800px)
                const int targetWidth = 800;
                if (image.Width > targetWidth)
                {
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Size = new Size(targetWidth, 0), // Maintain aspect ratio
                        Mode = ResizeMode.Max
                    }));
                }

                // Save to JPEG format with quality = 70
                using var ms = new MemoryStream();
                await image.SaveAsJpegAsync(ms, new JpegEncoder { Quality = 70 });
                ms.Seek(0, SeekOrigin.Begin);

                // Add image to PDF
                var pdfPage = document.AddPage();
                using var gfx = XGraphics.FromPdfPage(pdfPage);
                using var pdfImage = XImage.FromStream(() => ms);

                pdfPage.Width = pdfImage.PixelWidth;
                pdfPage.Height = pdfImage.PixelHeight;

                gfx.DrawImage(pdfImage, 0, 0);
            }

            document.Save(outputStream, false);
            return outputStream.ToArray();
        }
    }

}

