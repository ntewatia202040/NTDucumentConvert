using Microsoft.AspNetCore.Mvc;

using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIORenderer;
using Syncfusion.Pdf;
using Syncfusion.XlsIO;
using Syncfusion.XlsIORenderer;







namespace NTConvert.Controllers
{
    public class ToolsController : Controller
    {
        public ActionResult PdfToWord()
        {
            ViewBag.Type = "Pdf To Word";
            return View("~/Views/Tools/ConversionPages.cshtml");
        }

        public ActionResult WordToPdf()
        {
            ViewBag.Type = "Word To Pdf";
            return View("~/Views/Tools/ConversionPages.cshtml");
        }

        public ActionResult ExcelToPdf()
        {
            ViewBag.Type = "Excel To Pdf";
            return View("~/Views/Tools/ConversionPages.cshtml");
        }

        public ActionResult PdfToExcel()
        {
            ViewBag.Type = "Pdf To Excel";
            return View("~/Views/Tools/ConversionPages.cshtml");
        }

        public ActionResult PptToPdf()
        {
            ViewBag.Type = "Pdf To Excel";
            return View("~/Views/Tools/ConversionPages.cshtml");
        }

        public ActionResult PdfToPpt()
        {
            ViewBag.Type = "Pdf To Excel";
            return View("~/Views/Tools/ConversionPages.cshtml");
        }




        [HttpGet]
        public IActionResult AllConveteredDocument(string type)
        {
            ViewBag.Type = type;

            TempData["DownloadFile"] = TempData["DownloadFile"]?.ToString();
            TempData["Success"] = TempData["Success"]?.ToString();

            return View("~/Views/Tools/DownloadFile.cshtml");
        }

        [HttpPost]
        public IActionResult AllConveter(IFormFile file, string ConversionType)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Success"] = "Please select a file.";
                return RedirectToAction("AllConveteredDocument", new { type = ConversionType });
            }

            string uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads");

            if (!Directory.Exists(uploadFolder))
                Directory.CreateDirectory(uploadFolder);

            // Unique file name to avoid overwrite
            string uniqueFileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            string filePath = Path.Combine(uploadFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            try
            {
                // ===================== PDF TO WORD =====================
                if (ConversionType == "Pdf To Word")
                {
                    string wordFileName = Path.GetFileNameWithoutExtension(uniqueFileName) + ".docx";
                    string wordPath = Path.Combine(uploadFolder, wordFileName);

                    using (var client = new HttpClient())
                    {
                        using (var form = new MultipartFormDataContent())
                        {
                            form.Add(
                                new ByteArrayContent(System.IO.File.ReadAllBytes(filePath)),
                                "file",
                                Path.GetFileName(filePath)
                            );

                            var response = client.PostAsync(
                                "http://localhost:8080/api/v1/convert/pdf/docx",
                                form
                            ).Result;

                            var bytes = response.Content.ReadAsByteArrayAsync().Result;
                            System.IO.File.WriteAllBytes(wordPath, bytes);
                        }
                    }

                    TempData["DownloadFile"] = wordFileName;
                    TempData["Success"] = "PDF converted to Word successfully (API).";

                    DeleteFileAfter2Minutes(filePath);
                    DeleteFileAfter2Minutes(wordPath);
                }

                // ===================== WORD TO PDF =====================
                else if (ConversionType == "Word To Pdf")
                {
                    string pdfFileName = Path.GetFileNameWithoutExtension(uniqueFileName) + ".pdf";
                    string pdfFilePath = Path.Combine(uploadFolder, pdfFileName);

                    using (var document = new Syncfusion.DocIO.DLS.WordDocument(filePath, Syncfusion.DocIO.FormatType.Automatic))
                    using (var renderer = new Syncfusion.DocIORenderer.DocIORenderer())
                    using (var pdfDoc = renderer.ConvertToPDF(document))
                    using (var output = new FileStream(pdfFilePath, FileMode.Create))
                    {
                        pdfDoc.Save(output);
                    }

                    TempData["DownloadFile"] = pdfFileName;
                    TempData["Success"] = "Word converted to PDF successfully.";

                    DeleteFileAfter2Minutes(filePath);
                    DeleteFileAfter2Minutes(pdfFilePath);
                }

                // ===================== EXCEL TO PDF =====================
                else if (ConversionType == "Excel To Pdf")
                {
                    string pdfFileName = Path.GetFileNameWithoutExtension(uniqueFileName) + ".pdf";
                    string pdfFilePath = Path.Combine(uploadFolder, pdfFileName);

                    using (ExcelEngine excelEngine = new ExcelEngine())
                    {
                        IApplication application = excelEngine.Excel;
                        application.DefaultVersion = ExcelVersion.Xlsx;

                        using (FileStream inputStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                        {
                            IWorkbook workbook = application.Workbooks.Open(inputStream);

                            XlsIORenderer renderer = new XlsIORenderer();
                            using (Syncfusion.Pdf.PdfDocument pdfDocument = renderer.ConvertToPDF(workbook))
                            using (FileStream outputStream = new FileStream(pdfFilePath, FileMode.Create))
                            {
                                pdfDocument.Save(outputStream);
                            }

                            workbook.Close();
                        }
                    }

                    TempData["DownloadFile"] = pdfFileName;
                    TempData["Success"] = "Excel converted to PDF successfully.";

                    DeleteFileAfter2Minutes(filePath);
                    DeleteFileAfter2Minutes(pdfFilePath);
                }
                else
                {
                    TempData["Success"] = "Invalid conversion type.";
                }
            }
            catch (Exception ex)
            {
                TempData["Success"] = "Error: " + ex.Message;
            }

            return RedirectToAction("AllConveteredDocument", new { type = ConversionType });
        }



        [HttpGet]
        public IActionResult DownloadFile(string fileName)
        {
            string filePath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "Uploads",
                fileName);

            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var bytes = System.IO.File.ReadAllBytes(filePath);
            var extension = Path.GetExtension(fileName).ToLower();

            string contentType = extension switch
            {
                ".pdf" => "application/pdf",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                _ => "application/octet-stream"
            };

            return File(bytes, contentType, fileName);
        }

        private void DeleteFileAfter2Minutes(string filePath)
        {
            Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromMinutes(2));

                try
                {
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
                catch
                {
                    // Ignore
                }
            });
        }
    }
}
