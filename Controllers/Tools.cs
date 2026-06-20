using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using Microsoft.PowerBI.Api.Models;
using Spire.Doc;
using Spire.Pdf;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Reflection.Metadata;




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
            if (ConversionType == "Pdf To Word")
            {
                string wordPath = Path.ChangeExtension(filePath, ".docx");

                PdfDocument pdf = new PdfDocument();
                pdf.LoadFromFile(filePath);

                pdf.SaveToFile(wordPath, Spire.Pdf.FileFormat.DOCX);

                TempData["DownloadFile"] = Path.GetFileName(wordPath);
                TempData["Success"] = "PDF converted to Word successfully.";
                    DeleteOldFiles(filePath);
                }
            else if (ConversionType == "Word To Pdf")
            {
                string pdfFileName =
                    Path.GetFileNameWithoutExtension(file.FileName) + ".pdf";

                string pdfFilePath =
                    Path.Combine(uploadFolder, pdfFileName);

                    Spire.Doc.Document document = new Spire.Doc.Document();
                    document.LoadFromFile(filePath);

                document.SaveToFile(
                    pdfFilePath,
                    Spire.Doc.FileFormat.PDF);

                document.Close();

                TempData["DownloadFile"] = pdfFileName;
                TempData["Success"] = "Word converted to PDF successfully.";
                    DeleteOldFiles(filePath);
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

        private void DeleteOldFiles(string uploadFolder)
        {
            var files = Directory.GetFiles(uploadFolder);

            foreach (var file in files)
            {
                if (System.IO.File.GetCreationTime(file) < DateTime.Now.AddMinutes(-10))
               
                {
                    try
                    {
                        System.IO.File.Delete(file);
                    }
                    catch
                    {
                    }
                }
            }
        }
    }
}
