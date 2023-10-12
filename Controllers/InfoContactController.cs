using ClosedXML.Excel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OCB_API.Models;

namespace OCB_API.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class InfoContactController : ControllerBase
    {

        private readonly OCBContext _context;
        public InfoContactController(OCBContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<InfoContact>>>> GetInfoContacts()
        {
            var infoContacts = await _context.InfoContactTable.ToListAsync();
            var response = new ApiResponse<IEnumerable<InfoContact>>
            {
                Status = true,
                StatusCode = 200,
                Message = "InfoContacts retrieved successfully",
                Data = infoContacts
            };

            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<InfoContact>>> GetInfoContact(int id)
        {
            var infoContact = await _context.InfoContactTable.FindAsync(id);

            if (infoContact == null)
            {
                var notFoundResponse = new ApiResponse<InfoContact>
                {
                    Status = false,
                    StatusCode = 404,
                    Message = "InfoContact not found",
                    Data = null
                };

                return NotFound(notFoundResponse);
            }

            var successResponse = new ApiResponse<InfoContact>
            {
                Status = true,
                StatusCode = 200,
                Message = "InfoContact retrieved successfully",
                Data = infoContact
            };

            return Ok(successResponse);
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<InfoContact>>> PostInfoContact(InfoContact infoContact)
        {
            _context.InfoContactTable.Add(infoContact);
            await _context.SaveChangesAsync();

            var successResponse = new ApiResponse<InfoContact>
            {
                Status = true,
                StatusCode = 201,
                Message = "InfoContact created successfully",
                Data = infoContact
            };

            return CreatedAtAction(nameof(GetInfoContact), new { id = infoContact.Id }, successResponse);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutInfoContact(int id, InfoContact infoContact)
        {
            if (id != infoContact.Id)
            {
                return BadRequest(new ApiResponse<object> { Status = false, StatusCode = 400, Message = "Invalid request", Data = null });
            }

            _context.Entry(infoContact).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InfoContactExists(id))
                {
                    return NotFound(new ApiResponse<object> { Status = false, StatusCode = 404, Message = "InfoContact not found", Data = null });
                }
                else
                {
                    throw;
                }
            }

            var successResponse = new ApiResponse<object>
            {
                Status = true,
                StatusCode = 200,
                Message = "InfoContact updated successfully",
                Data = null
            };

            return Ok(successResponse);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInfoContact(int id)
        {
            var infoContact = await _context.InfoContactTable.FindAsync(id);

            if (infoContact == null)
            {
                return NotFound(new ApiResponse<object> { Status = false, StatusCode = 404, Message = "InfoContact not found", Data = null });
            }

            _context.InfoContactTable.Remove(infoContact);
            await _context.SaveChangesAsync();

            var successResponse = new ApiResponse<object>
            {
                Status = true,
                StatusCode = 200,
                Message = "InfoContact deleted successfully",
                Data = null
            };

            return Ok(successResponse);
        }

        [HttpGet("export-excel")]
        public IActionResult ExportExcel()
        {
            var infoContact = _context.InfoContactTable.ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Thông tin liên hệ");

                // Add headers
                worksheet.Cell(1, 1).Value = "ID";
                worksheet.Cell(1, 2).Value = "Tên liên hệ";
                worksheet.Cell(1, 3).Value = "Email";
                worksheet.Cell(1, 4).Value = "Chức vụ";
                worksheet.Cell(1, 5).Value = "Tin nhắn";

                // Add data
                for (int i = 0; i < infoContact.Count; i++)
                {
                    var infoRegister = infoContact[i];
                    worksheet.Cell(i + 2, 1).Value = infoRegister.Id;
                    worksheet.Cell(i + 2, 2).Value = infoRegister.Name;
                    worksheet.Cell(i + 2, 3).Value = infoRegister.Email;
                    worksheet.Cell(i + 2, 4).Value = infoRegister.Subject;
                    worksheet.Cell(i + 2, 5).Value = infoRegister.Message;
                }

                var fileName = DateTime.Now.ToString() + "infoContact.xlsx";

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;

                    var response = new FileContentResult(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                    {
                        FileDownloadName =  fileName
                    };

                    return response;
                }
            }
        }

        private bool InfoContactExists(int id)
        {
            return _context.InfoContactTable.Any(e => e.Id == id);
        }
    }
}
