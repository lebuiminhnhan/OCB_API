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
    public class InfoRegisterController : ControllerBase
    {
        private readonly OCBContext _context;

        public InfoRegisterController(OCBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<InfoRegister>>>> GetInfoRegisters()
        {
            var infoRegisters = await _context.InfoRegisterTable.ToListAsync();
            var response = new ApiResponse<IEnumerable<InfoRegister>>
            {
                Status = true,
                StatusCode = 200,
                Message = "InfoRegisters retrieved successfully",
                Data = infoRegisters
            };

            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<InfoRegister>>> GetInfoRegister(int id)
        {
            var infoRegister = await _context.InfoRegisterTable.FindAsync(id);

            if (infoRegister == null)
            {
                var notFoundResponse = new ApiResponse<InfoRegister>
                {
                    Status = false,
                    StatusCode = 404,
                    Message = "InfoRegister not found",
                    Data = null
                };

                return NotFound(notFoundResponse);
            }

            var successResponse = new ApiResponse<InfoRegister>
            {
                Status = true,
                StatusCode = 200,
                Message = "InfoRegister retrieved successfully",
                Data = infoRegister
            };

            return Ok(successResponse);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteInfoRegister(int id)
        {
            var infoRegister = await _context.InfoRegisterTable.FindAsync(id);

            if (infoRegister == null)
            {
                var notFoundResponse = new ApiResponse<object>
                {
                    Status = false,
                    StatusCode = 404,
                    Message = "InfoRegister not found",
                    Data = null
                };

                return NotFound(notFoundResponse);
            }

            _context.InfoRegisterTable.Remove(infoRegister);
            await _context.SaveChangesAsync();

            var successResponse = new ApiResponse<object>
            {
                Status = true,
                StatusCode = 200,
                Message = "Xóa đăng kí nhận thông tin thành công!",
                Data = null
            };

            return Ok(successResponse);
        }

        [HttpGet("export-excel")]
        public IActionResult ExportExcel()
        {
            var infoRegisters = _context.InfoRegisterTable.ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Đăng kí nhận thông tin");

                // Add headers
                worksheet.Cell(1, 1).Value = "ID";
                worksheet.Cell(1, 2).Value = "Email";

                // Add data
                for (int i = 0; i < infoRegisters.Count; i++)
                {
                    var infoRegister = infoRegisters[i];
                    worksheet.Cell(i + 2, 1).Value = infoRegister.Id;
                    worksheet.Cell(i + 2, 2).Value = infoRegister.Email;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;

                    var response = new FileContentResult(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                    {
                        FileDownloadName = "InfoRegisters.xlsx"
                    };

                    return response;
                }
            }
        }

        private bool InfoRegisterExists(int id)
        {
            return _context.InfoRegisterTable.Any(e => e.Id == id);
        }
    }
}
