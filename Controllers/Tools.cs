using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using Microsoft.PowerBI.Api.Models;
using Spire.Pdf;



namespace NTConvert.Controllers
{
    public class ToolsController : Controller
    {
        public ActionResult PdfToWord()
        {
            ViewBag.Type = "PdfToWord";
            return View("~/Views/Tools/ConversionPages.cshtml");
        }

        public ActionResult WordToPdf()
        {
            ViewBag.Type = "WordToPdf";
            return View("~/Views/Tools/ConversionPages.cshtml");
        }

        public ActionResult ExcelToPdf()
        {
            ViewBag.Type = "ExcelToPdf";
            return View("~/Views/Tools/ConversionPages.cshtml");
        }

        public ActionResult PdfToExcel()
        {
            ViewBag.Type = "PdfToExcel";
            return View("~/Views/Tools/ConversionPages.cshtml");
        }

        public ActionResult PptToPdf()
        {
            ViewBag.Type = "PdfToExcel";
            return View("~/Views/Tools/ConversionPages.cshtml");
        }

        public ActionResult PdfToPpt()
        {
            ViewBag.Type = "PdfToExcel";
            return View("~/Views/Tools/ConversionPages.cshtml");
        }




        [HttpGet]
        public IActionResult AllConveterDocument(string type)
        {
            ViewBag.Type = type;
       
            return View("~/Views/Tools/DownloadFile.cshtml");
        }

        [HttpPost]
        public IActionResult AllConveter(IFormFile file, string ConversionType)
        {
            if (file == null || file.Length == 0)
            {
                ViewBag.Message = "Please select a file.";
                return View();
            }

            string uploadFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "Uploads");

            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            string pdfPath = Path.Combine(uploadFolder, file.FileName);

            using (var stream = new FileStream(pdfPath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            if (ConversionType == "PDF To Word")
            {
                string wordPath = Path.ChangeExtension(pdfPath, ".docx");

                PdfDocument pdf = new PdfDocument();
                pdf.LoadFromFile(pdfPath);

                pdf.SaveToFile(wordPath, Spire.Pdf.FileFormat.DOCX);

                TempData["DownloadFile"] = Path.GetFileName(wordPath);
                TempData["Success"] = "File converted successfully.";


            }
            return RedirectToAction("AllConveterDocument", ConversionType);
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
    }
}
