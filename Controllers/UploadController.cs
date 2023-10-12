using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OCB_API.Models;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace OCB_API.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        private readonly OCBContext _context;
        public UploadController(OCBContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [HttpPost]
        public async Task<IActionResult> UploadImage([FromForm] Gift gift, IFormFile file)
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

                // Update the Gift model with the file name
                gift.Image = uniqueFileName;

                // Save the Gift model to the database
                _context.GiftTable.Add(gift);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object> { Status = true, StatusCode = 200, Message = "File uploaded successfully", Data = new { FilePath = filePath } });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object> { Status = false, StatusCode = 500, Message = $"Internal server error: {ex.Message}", Data = null });
            }
        }

        [HttpGet("{id}/image")]
        public IActionResult GetGiftImage(int id)
        {
            var gift = _context.GiftTable.Find(id);

            if (gift == null || string.IsNullOrEmpty(gift.Image))
                return NotFound(new ApiResponse<object> { Status = false, StatusCode = 404, Message = "Gift not found", Data = null });

            var imagePath = Path.Combine(_environment.WebRootPath, "images", gift.Image);

            if (!System.IO.File.Exists(imagePath))
                return NotFound(new ApiResponse<object> { Status = false, StatusCode = 404, Message = "Image not found", Data = null });

            // Return the image file
            return PhysicalFile(imagePath, "image/jpeg"); // You can adjust the content type based on your file type
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<Gift>>>> GetGifts()
        {
            var gifts = await _context.GiftTable.ToListAsync();
            return Ok(new ApiResponse<IEnumerable<Gift>> { Status = true, StatusCode = 200, Message = "Gifts retrieved successfully", Data = gifts });
        }
    }
}
