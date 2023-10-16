using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OCB_API.Models;

namespace OCB_API.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class GiftController : ControllerBase
    {
        private readonly OCBContext _context;
        private readonly IWebHostEnvironment _environment;
        public GiftController(OCBContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }
        // GET: api/Gift
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<Gift>>>> GetGifts()
        {
            var gifts = await _context.GiftTable.ToListAsync();
            return Ok(new ApiResponse<List<Gift>> { Status = true, StatusCode = 200, Message = "Gifts retrieved successfully", Data = gifts });
        }

        // GET: api/Gift/5
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<Gift>>> GetGift(int id)
        {
            var gift = await _context.GiftTable.FindAsync(id);

            if (gift == null)
            {
                return NotFound(new ApiResponse<Gift> { Status = false, StatusCode = 404, Message = "Gift not found" });
            }

            return Ok(new ApiResponse<Gift> { Status = true, StatusCode = 200, Message = "Gift retrieved successfully", Data = gift });
        }

        // POST: api/Gift
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        public async Task<ActionResult<ApiResponse<Gift>>> PostGift([FromForm] Gift gift, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new ApiResponse<object> { Status = false, StatusCode = 400, Message = "No file uploaded.", Data = null });

            try
            {
                // Ensure the 'uploads' folder exists
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "images");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // Generate a unique file name
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Save the file to the server
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var imagePath = Path.Combine(_environment.WebRootPath, "images", uniqueFileName);
                byte[] imageBytes = System.IO.File.ReadAllBytes(imagePath);
                string base64Image = Convert.ToBase64String(imageBytes);
                // Update the Gift model with the file name
                gift.Image = "data:image/jpeg;base64," + base64Image;

                // Save the Gift model to the database
                _context.GiftTable.Add(gift);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object> { Status = true, StatusCode = 200, Message = "Gift successfully", Data = gift });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object> { Status = false, StatusCode = 500, Message = $"Internal server error: {ex.Message}", Data = null });
            }
        }
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("{id}/image")]
        public IActionResult GetGiftImage(int id)
        {
            var gift = _context.GiftTable.Find(id);

            if (gift == null || string.IsNullOrEmpty(gift.Image))
                return NotFound(new ApiResponse<object> { Status = false, StatusCode = 404, Message = "Gift not found", Data = null });

            var successResponse = new ApiResponse<string>
            {
                Status = true,
                StatusCode = 200,
                Message = "Image retrieved successfully",
                Data = gift.Image,
            };

            return Ok(successResponse);
        }

        // PUT: api/Gift/5
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutGift(int id, Gift gift)
        {
            if (id != gift.Id)
            {
                return BadRequest(new ApiResponse<Gift> { Status = false, StatusCode = 400, Message = "Cập nhật thất bại! Vui lòng kiểm tra lại thông tin." });
            }

            _context.Entry(gift).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GiftExists(id))
                {
                    return NotFound(new ApiResponse<Gift> { Status = false, StatusCode = 404, Message = "Gift not found" });
                }
                else
                {
                    throw;
                }
            }

            return Ok(new ApiResponse<Gift> { Status = true, StatusCode = 200, Message = "Cập nhật thông tin quà tặng thành công!" });
        }

        // DELETE: api/Gift/5
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGift(int id)
        {
            var gift = await _context.GiftTable.FindAsync(id);
            if (gift == null)
            {
                return NotFound(new ApiResponse<Gift> { Status = false, StatusCode = 404, Message = "Gift not found" });
            }

            _context.GiftTable.Remove(gift);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<Gift> { Status = true, StatusCode = 204, Message = "Xóa quà tặng thành công!" });
        }

        private bool GiftExists(int id)
        {
            return _context.GiftTable.Any(e => e.Id == id);
        }
    }
}
