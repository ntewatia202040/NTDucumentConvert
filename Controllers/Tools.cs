
using Microsoft.AspNetCore.Mvc;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIORenderer;
using Syncfusion.Pdf;






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
                return RedirectToAction("AllConveteredDocument",
                    new { type = ConversionType });
            }

            string uploadFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "Uploads");

            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            string filePath = Path.Combine(uploadFolder, file.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            try
            {
                //======================== PDF TO WORD ========================
                if (ConversionType == "Pdf To Word")
                {
                    string wordPath = Path.ChangeExtension(filePath, ".docx");

                    Spire.Pdf.PdfDocument pdf = new Spire.Pdf.PdfDocument();
                    pdf.LoadFromFile(filePath);

                    pdf.SaveToFile(wordPath, Spire.Pdf.FileFormat.DOCX);
                    pdf.Close();

                    TempData["DownloadFile"] = Path.GetFileName(wordPath);
                    TempData["Success"] = "PDF converted to Word successfully.";

                    DeleteFileAfter2Minutes(filePath);   // Uploaded PDF
                    DeleteFileAfter2Minutes(wordPath);   // Converted DOCX
                }

                //======================== WORD TO PDF ========================
                else if (ConversionType == "Word To Pdf")
                {
                    string pdfFileName = Path.GetFileNameWithoutExtension(file.FileName) + ".pdf";
                    string pdfFilePath = Path.Combine(uploadFolder, pdfFileName);

                    try
                    {
                        // -------- Syncfusion --------
                        using (Syncfusion.DocIO.DLS.WordDocument document =
                               new Syncfusion.DocIO.DLS.WordDocument(filePath, Syncfusion.DocIO.FormatType.Automatic))
                        {
                            using (Syncfusion.DocIORenderer.DocIORenderer renderer =
                                   new Syncfusion.DocIORenderer.DocIORenderer())
                            {
                                using (Syncfusion.Pdf.PdfDocument pdfDocument =
                                       renderer.ConvertToPDF(document))
                                {
                                    using (FileStream outputStream = new FileStream(pdfFilePath, FileMode.Create))
                                    {
                                        pdfDocument.Save(outputStream);
                                    }
                                }
                            }
                        }
                        TempData["Success"] = "Success";
                        DeleteFileAfter2Minutes(filePath);      // Uploaded DOCX
                        DeleteFileAfter2Minutes(pdfFilePath);   // Converted PDF
                        
                    }
                    catch
                    {
                        // -------- Fallback Spire --------
                        Spire.Doc.Document document = new Spire.Doc.Document();
                        document.LoadFromFile(filePath);
                        document.SaveToFile(pdfFilePath, Spire.Doc.FileFormat.PDF);
                        document.Close();

                        TempData["Success"] = "Success";
                        DeleteFileAfter2Minutes(filePath);      // Uploaded DOCX
                        DeleteFileAfter2Minutes(pdfFilePath);   // Converted PDF
                    }

                    TempData["DownloadFile"] = pdfFileName;

                   
                }
                else
                {
                    TempData["Success"] = "Invalid conversion type.";
                }
            }
            catch (Exception ex)
            {
                TempData["Success"] = ex.Message;
            }

            return RedirectToAction(
                "AllConveteredDocument",
                new { type = ConversionType });
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
            {
                return NotFound();
            }

            byte[] bytes = System.IO.File.ReadAllBytes(filePath);

            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                fileName);
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
