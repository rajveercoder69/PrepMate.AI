using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;
using WebApiCRUDOps.DataUtility.DataRepository.IDataRepository;
using WebApiCRUDOps.DataUtility.Model;
using WebApiCRUDOps.DataUtility.Model.Dto;
using WebApiCRUDOps.PdfCompressorServcie;
using PdfSharpCore;

namespace WebApiCRUDOps.Controllers
{
    [Route("/PersonDetails")]
    [ApiController]
    [Authorize]
    public class BookController : ControllerBase
    {
        private readonly ISaveChange _saveChanges;
        private readonly ResponseDto _ResponseDto;
        private readonly IMapper _mapper;

        public BookController(ISaveChange saveChanges, IMapper mapper)
        {
            _saveChanges = saveChanges;
            _ResponseDto = new ResponseDto();
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize("Admin")]
        [Route("fetchingpersonsdetailbyadmin")]
        public IActionResult getPersonsDetails()
        {
            try
            {
                _ResponseDto.Result = _saveChanges.personproduct.GetAll().ToList();
                if (_ResponseDto.Result == null)
                {
                    return NotFound(); // id not found in the list
                }
                _saveChanges.save();
                _ResponseDto.IsSuccess = true;
                return Ok(_ResponseDto);


            }
            catch (Exception ex)
            {
                _ResponseDto.IsSuccess = false;
                _ResponseDto.Error = ex.Message;
                // Log the exception if necessary
                return StatusCode(500, _ResponseDto);
            }
        }
        [HttpGet]
        [Route("individual")]
        public IActionResult getPersondetails([FromBody] string firstName)
        {
            if (firstName == null)
            {
                return NotFound();
            }
            var userName = HttpContext.User.FindFirst("name")?.Value
                ?? HttpContext.User.Identity?.Name;
            if (userName == firstName)
            {
                try
                {
                    Person persondetail = _saveChanges.personproduct.GetSingle(x => x.firstName == firstName);
                    _ResponseDto.Result = _mapper.Map<PersonDto>(persondetail);
                    _saveChanges.save();
                    return Ok(persondetail);
                }
                catch (Exception ex)
                {
                    _ResponseDto.IsSuccess = false;
                    _ResponseDto.Error = ex.Message;
                    // Log the exception if necessary
                    return StatusCode(500, _ResponseDto);
                }
            }
            _ResponseDto.IsSuccess = false;
            _ResponseDto.Error = "Invalid Request";
            return BadRequest(_ResponseDto);
        }
        [HttpGet("getallpdfsbyid")]
        public IActionResult GetAllPdfsByPersonId([FromBody] int personId)
        {
            if (personId <= 0)
            {
                _ResponseDto.IsSuccess = false;
                _ResponseDto.Error = "Invalid person ID.";
                return BadRequest(_ResponseDto);
            }
            try
            {
                var pdfFiles = _saveChanges.pdfUploadRepository
                    .GetAll()
                    .Where(p => p.PersonId == personId)
                    .Select(p => new
                    {
                        p.Id,
                        p.Email,
                        p.UploadedAt,
                        FileName = $"Compressed_{p.Id}.pdf"
                        // Optional: Include a download URL if needed
                    })
                    .ToList();

                if (!pdfFiles.Any())
                {
                    _ResponseDto.IsSuccess = false;
                    _ResponseDto.Error = "No PDF files found for this person.";
                    return NotFound(_ResponseDto);
                }

                _ResponseDto.IsSuccess = true;
                _ResponseDto.Result = pdfFiles;
                return Ok(_ResponseDto);
            }
            catch (Exception ex)
            {
                _ResponseDto.IsSuccess = false;
                _ResponseDto.Error = ex.Message;
                return StatusCode(500, _ResponseDto);
            }
        }

        [HttpGet("getpdfbyid")]
        public IActionResult getPdfFile([FromQuery] int personId, [FromQuery] int fileId)
        {
            if (personId == null && fileId == null)
            {
                _ResponseDto.Error = "PersonId cannot be null and fileId cannot be null";
                _ResponseDto.IsSuccess = false;
                _ResponseDto.Result = null;
                return NotFound(_ResponseDto);
            }
            try
            {
                // var pdfFile = _saveChanges.pdfUploadRepository.GetFirstOrDefault(p => p.PersonId == personId && p.Id == fileId);
                var all = _saveChanges.pdfUploadRepository.GetAll().Where(p => p.PersonId == personId).ToList();
                var pdfFile = all.FirstOrDefault(p => p.Id == fileId);

                if (pdfFile == null)
                {
                    _ResponseDto.Error = "No PDF files found for the given PersonId";
                    _ResponseDto.IsSuccess = false;
                    _ResponseDto.Result = null;
                    return NotFound(_ResponseDto);
                }
                return File(pdfFile.CompressedPdf!, "application/pdf", $"Compressed_{pdfFile.Id}.pdf");
            }
            catch (Exception ex)
            {
                _ResponseDto.Error = ex.Message;
                _ResponseDto.IsSuccess = false;
                _ResponseDto.Result = null;
                return StatusCode(500, _ResponseDto);
            }

        }
        [HttpGet("downloadallpdfs/{personId}")]
        public IActionResult DownloadAllPdfs(int personId)
        {
            if (personId <= 0)
                return BadRequest("Invalid PersonId");

            var pdfFiles = _saveChanges.pdfUploadRepository.GetAll()
                            .Where(p => p.PersonId == personId)
                            .ToList();

            if (!pdfFiles.Any())
                return NotFound("No PDFs found for this PersonId");

            using var zipStream = new MemoryStream();
            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
            {
                foreach (var pdf in pdfFiles)
                {
                    // Add original PDF file if present
                    if (pdf.OriginalPdf != null)
                    {
                        var originalEntry = archive.CreateEntry($"Original_{pdf.Id}.pdf", CompressionLevel.Fastest);
                        using var entryStream = originalEntry.Open();
                        entryStream.Write(pdf.OriginalPdf, 0, pdf.OriginalPdf.Length);
                    }

                    // Add compressed PDF file if present
                    if (pdf.CompressedPdf != null)
                    {
                        var compressedEntry = archive.CreateEntry($"Compressed_{pdf.Id}.pdf", CompressionLevel.Fastest);
                        using var entryStream = compressedEntry.Open();
                        entryStream.Write(pdf.CompressedPdf, 0, pdf.CompressedPdf.Length);
                    }
                }
            }

            zipStream.Position = 0; // reset stream position before returning

            string zipFileName = $"Person_{personId}_Pdfs_{DateTime.UtcNow:yyyyMMddHHmmss}.zip";
            return File(zipStream.ToArray(), "application/zip", zipFileName);
        }
        [HttpPost("upload-pdf")]
        public async Task<IActionResult> UploadPdf([FromForm] PdfDto dto)
        {
            try
            {
                var person = _saveChanges.personproduct.GetSingle(x => x.personId == dto.PersonId);
                if (person == null)
                {
                    _ResponseDto.IsSuccess = false;
                    _ResponseDto.Error = "Person not found";
                    return NotFound(_ResponseDto);
                }

                using var memoryStream = new MemoryStream();
                await dto.PdfFile.CopyToAsync(memoryStream);
                byte[] originalBytes = memoryStream.ToArray();

                var parsedLevel = Enum.TryParse(dto.CompressionLevel, true, out PdfCompressor.CompressionLevel level)
                    ? level
                    : PdfCompressor.CompressionLevel.Moderate;

                byte[] compressedBytes = PdfCompressor.CompressPdf(originalBytes, level);

                // Step 2: Further compress using Ghostscript
                byte[] compressedBytesGhost = PdfCompressor.CompressWithGhostscript(compressedBytes, dto.CompressionLevel);

                // Use AutoMapper
                var pdfUpload = _mapper.Map<PdfUpload>(dto);
                pdfUpload.OriginalPdf = originalBytes;
                pdfUpload.CompressedPdf = compressedBytesGhost;

                _saveChanges.pdfUploadRepository.Add(pdfUpload);
                _saveChanges.save();

                _ResponseDto.IsSuccess = true;
                _ResponseDto.Result = $"PDF uploaded and compressed ({dto.CompressionLevel}) for person ID {dto.PersonId} and File ID {pdfUpload.Id}";
                return Ok(_ResponseDto);
            }
            catch (Exception ex)
            {
                _ResponseDto.IsSuccess = false;
                _ResponseDto.Error = ex.Message;
                return StatusCode(500, _ResponseDto);
            }
        }

        [HttpPost]
        [Route("CreatePersonProfile")]
        public IActionResult createPerson([FromBody] Person person)
        {
            if (person == null)
                return BadRequest();
            try
            {
                _saveChanges.personproduct.Add(person);
                _saveChanges.save();
                return Ok(_ResponseDto);
            }
            catch (Exception ex)
            {
                _ResponseDto.IsSuccess = false;
                _ResponseDto.Error = ex.Message + "OR user is already presnent with the username";
                // Log the exception if necessary
                return StatusCode(501, _ResponseDto);
                throw;
            }
        }

        [HttpPut]
        [Route("updatePersondetail")]
        public IActionResult updatePerson([FromBody] Person person)
        {
            if (person == null)
                return NoContent();
            try
            {
                //var ShirtUpdetails= ShirtData.UpdateShirtdetails(firstDetailsShirt,shirt);
                _saveChanges.personproduct.update(person);
                _saveChanges.save();
                return Ok(_ResponseDto);
            }
            catch (Exception ex)
            {
                _ResponseDto.IsSuccess = false;
                _ResponseDto.Error = ex.Message;
                // Log the exception if necessary
                return StatusCode(401, _ResponseDto);
                throw;
            }

        }
        [HttpDelete]
        [Route("deleteindividual")]
        public IActionResult deletePerson([FromHeader] int personid)
        {
            try
            {
                // var shirtExisting = ShirtData.CheckShirtId(id);
                Person persnd = _saveChanges.personproduct.GetSingle(x => x.personId == personid);
                if (persnd != null)
                {
                    _saveChanges.personproduct.Remove(persnd);
                    _saveChanges.save();
                }
                _ResponseDto.Result = _mapper.Map<PersonDto>(persnd);
                var pdfFiles = _saveChanges.pdfUploadRepository
                    .GetAll()
                    .Where(p => p.PersonId == personid)
                    .ToList();

                if (pdfFiles != null)
                {
                    foreach (PdfUpload pdfFile in pdfFiles)
                    {
                        _saveChanges.pdfUploadRepository.Remove(pdfFile);

                    }
                    _saveChanges.save();
                }
            }
            catch (Exception ex)
            {
                _ResponseDto.IsSuccess = false;
                _ResponseDto.Error = ex.Message;
                // Log the exception if necessary
                return StatusCode(401, _ResponseDto);
                throw;
            }
            return Ok(_ResponseDto);
        }
        [HttpDelete]
        [Route("deletePdf")]
        public IActionResult DeletePdf([FromBody] int fileId)
        {
            try
            {
                PdfUpload pdfUpload = _saveChanges.pdfUploadRepository.GetSingle(p => p.Id == fileId);
                if (pdfUpload != null)
                {
                    _saveChanges.pdfUploadRepository.Remove(pdfUpload);
                    _saveChanges.save();
                }
            }
            catch (Exception ex) {
                _ResponseDto.Error = ex.Message;
                _ResponseDto.IsSuccess = false;
                return StatusCode(501, _ResponseDto);
                throw;
            }
            return Ok(_ResponseDto);
        }
        [HttpPost]
        [Route("imagetopdf")]
        public async Task<IActionResult> Imagetopdf([FromForm] List<IFormFile> files, [FromForm] int personId)
        {
            try
            {
                if (files == null || files.Count == 0)
                {
                    _ResponseDto.Error = "No file Found to convert";
                    _ResponseDto.IsSuccess = false;
                    return BadRequest(_ResponseDto);
                }
                var pdfBytes = await PdfCompressor.ConvertImagetoPdf(files);
                var personDetails = _saveChanges.pdfUploadRepository.GetSingle(p => p.PersonId == personId);
                PdfUpload pdf = new PdfUpload()
                {
                    PersonId = personDetails.PersonId,
                    Email = personDetails.Email,
                    OriginalPdf = pdfBytes
                };
                //PdfUpload pdfu= _mapper.Map<PdfUpload>(personDetails);
                //pdfu.OriginalPdf = pdfBytes;
                _saveChanges.pdfUploadRepository.Add(pdf);
                _saveChanges.save();
                var pdfStream = new MemoryStream(pdfBytes);
                return File(pdfStream, "application/pdf", $"ConvertedPdf_{pdf.PersonId}.pdf");

            }
            catch (Exception ex)
            {
                _ResponseDto.Result = false;
                _ResponseDto.Error = ex.Message;
                return BadRequest(_ResponseDto);
            }
        }
        [HttpPost]
        [Route("UploadPdfTextNotes")]
        public async Task<IActionResult> UploadPDfTextNotes([FromForm] LearningPdfDto lpd)
        {
            try
            {
                var person = _saveChanges.personproduct.GetSingle(x => x.personId == lpd.PersonId);
                if (person == null)
                {
                    _ResponseDto.IsSuccess = false;
                    _ResponseDto.Error = "Person not found";
                    return NotFound(_ResponseDto);
                }

                using var memoryStream = new MemoryStream();
                await lpd.PdfFile.CopyToAsync(memoryStream);
                byte[] originalBytes = memoryStream.ToArray();

                // Use AutoMapper
                var pdfUpload = _mapper.Map<LearningPdf>(lpd);
                pdfUpload.Pdf = originalBytes;

                _saveChanges.learningPdfsRepository.Add(pdfUpload);
                _saveChanges.save();

                _ResponseDto.IsSuccess = true;
                _ResponseDto.Result = $"PDF uploaded successfully for person ID {lpd.PersonId} and File ID {pdfUpload.Id}";
                return Ok(_ResponseDto);
            }
            catch (Exception ex)
            {
                _ResponseDto.IsSuccess = false;
                _ResponseDto.Error = ex.Message;
                return StatusCode(500, _ResponseDto);
            }
        }
        [HttpGet]
        [Route("PdfListforMcp")]
        public IActionResult pdfListforMcp([FromQuery] int personId, [FromQuery] string category)
        {
            if (personId > 0 && !string.IsNullOrEmpty(category))
            {
                try
                {
                    var pdfFiles = _saveChanges.learningPdfsRepository
                       .GetAll()
                         .Where(p => p.PersonId == personId && p.CategoryName == category)
                         .Select(p => new
                         {
                             p.Id,
                             p.CategoryName,
                             p.Title,
                             p.CreateDateTime,
                             p.Description
                         })
                         .ToList();

                    if (!pdfFiles.Any())
                    {
                        _ResponseDto.IsSuccess = false;
                        _ResponseDto.Error = "No PDF files found for this person.";
                        return NotFound(_ResponseDto);
                    }

                    _ResponseDto.IsSuccess = true;
                    _ResponseDto.Result = pdfFiles;
                    return Ok(_ResponseDto);
                }
                catch (Exception ex)
                {
                    _ResponseDto.IsSuccess = false;
                    _ResponseDto.Error = ex.Message;
                    return StatusCode(500, _ResponseDto);
                }
            }
            else
            {
                _ResponseDto.IsSuccess = false;
                _ResponseDto.Error = "Invalid personID or Invalid category";
                return StatusCode(500, _ResponseDto);
            }
        }
        [HttpGet]
        [Route("fileForMCP")]
        public IActionResult fileForMCP([FromQuery] int personId, [FromQuery] int fileId)
        {
            if (personId == null && fileId == null)
            {
                _ResponseDto.Error = "PersonId cannot be null and fileId cannot be null";
                _ResponseDto.IsSuccess = false;
                _ResponseDto.Result = null;
                return NotFound(_ResponseDto);
            }
            try
            {
                // var pdfFile = _saveChanges.pdfUploadRepository.GetFirstOrDefault(p => p.PersonId == personId && p.Id == fileId);
                var all = _saveChanges.learningPdfsRepository.GetAll().Where(p => p.PersonId == personId).ToList();
                var pdfFile = all.FirstOrDefault(p => p.Id == fileId);

                if (pdfFile == null)
                {
                    _ResponseDto.Error = "No PDF files found for the given PersonId";
                    _ResponseDto.IsSuccess = false;
                    _ResponseDto.Result = null;
                    return NotFound(_ResponseDto);
                }
                return File(pdfFile.Pdf!, "application/pdf", $"LLMFile_{pdfFile.Title}.pdf");
            }
            catch (Exception ex)
            {
                _ResponseDto.Error = ex.Message;
                _ResponseDto.IsSuccess = false;
                _ResponseDto.Result = null;
                return StatusCode(500, _ResponseDto);
            }

        }
    } 
}
