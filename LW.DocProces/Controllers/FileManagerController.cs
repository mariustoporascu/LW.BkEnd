using LW.DocProces.Models;
using LW.DocProcLogic.FileManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace LW.DocProces.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FileManagerController : Controller
    {
        private readonly IFileManager _fileManager;
        private readonly ILogger<FileManagerController> _logger;

        public FileManagerController(
            IFileManager fileManager,
            ILogger<FileManagerController> logger
        )
        {
            _fileManager = fileManager;
            _logger = logger;
        }

        [Authorize]
        [HttpPost("rescanCode")]
        public async Task<IActionResult> RescanCodeAsync()
        {
            var conexId = Guid.Parse(User.Claims.FirstOrDefault(x => x.Type == "conexId").Value);
            var documentId = Request.Form["documentId"];
            var formFiles = Request.Form.Files;

            if (formFiles.Count == 0)
            {
                return NoContent();
            }
            List<bool> bools = new List<bool>();
            foreach (FormFile file in formFiles)
            {
                bools.Add(await _fileManager.OnFileRescan(file, conexId, new Guid(documentId)));
            }
            if (bools.Any(b => b == false))
            {
                return BadRequest(
                    new
                    {
                        Message = new
                        {
                            Succes = bools.Where(b => b == true).Count(),
                            Failed = bools.Where(b => b == false).Count()
                        },
                        Error = true
                    }
                );
            }
            return Ok(new { Message = "All files were succesfully scanned", Error = false });
        }

        [Authorize]
        [HttpPost("uploadFiles")]
        public async Task<IActionResult> UploadFilesAsync()
        {
            var conexId = Guid.Parse(User.Claims.FirstOrDefault(x => x.Type == "conexId").Value);
            //conexId = new Guid("1DB3D1E6-E108-4866-BEDD-3F1330ED7114");
            var firmaDiscountId = Request.Form["firmaDiscountId"];
            var formFiles = Request.Form.Files;

            if (formFiles.Count == 0)
            {
                return NoContent();
            }
            List<bool> bools = new List<bool>();
            foreach (FormFile file in formFiles)
            {
                bools.Add(
                    await _fileManager.OnFileUpload(file, conexId, new Guid(firmaDiscountId))
                );
            }
            if (bools.Any(b => b == false))
            {
                return BadRequest(
                    new
                    {
                        Message = new
                        {
                            Succes = bools.Where(b => b == true).Count(),
                            Failed = bools.Where(b => b == false).Count()
                        },
                        Error = true
                    }
                );
            }
            return Ok(new { Message = "All files were succesfully uploaded", Error = false });
        }

        [Authorize]
        [HttpGet("getFileStream")]
        public async Task<IActionResult> GetFileStream(string identifier)
        {
            var conexId = Guid.Parse(User.Claims.FirstOrDefault(x => x.Type == "conexId").Value);
            try
            {
                var fileStream = await _fileManager.GetFileStream(identifier, conexId);
                if (fileStream == null)
                {
                    return NoContent();
                }
                fileStream.Seek(0, SeekOrigin.Begin);
                return File(fileStream, "application/octet-stream");
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, e);
                return NoContent();
            }
        }

        [Authorize]
        [HttpDelete("deleteDocument")]
        public async Task<IActionResult> DeleteDocument(Guid fisierId, Guid documentId)
        {
            var conexId = Guid.Parse(User.Claims.FirstOrDefault(x => x.Type == "conexId").Value);
            if (!_fileManager.CheckDocExists(conexId, documentId))
            {
                return BadRequest(new { Message = "Document does not exist", Error = false });
            }
            var result = await _fileManager.DeleteBlobAndDocument(fisierId, documentId);
            if (!result)
            {
                return BadRequest(new { Message = "Delete operation failed", Error = false });
            }
            return Ok(new { Message = "Operation completed succesfully", Error = false });
        }

        [AllowAnonymous]
        [HttpPost("functionResult")]
        public async Task<IActionResult> FunctionResult(
            [FromBody] FunctionResultModel functionResultModel
        )
        {
            var result = await _fileManager.OnFileProcessed(
                functionResultModel.BlobName,
                functionResultModel.AnalyzeResult
            );
            if (!result)
            {
                return BadRequest();
            }
            return Ok();
        }
    }
}
