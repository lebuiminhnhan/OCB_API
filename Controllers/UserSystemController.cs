using DocumentFormat.OpenXml.Spreadsheet;
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
    public class UserSystemController : ControllerBase
    {
        private readonly OCBContext _context;
        private readonly IEmailService _emailService;
        public UserSystemController(OCBContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // GET: api/User
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<UserLogin>>>> GetUserTable()
        {

            var userTable = await _context.UserLoginTable
                .Include(x => x.User)
                .ToListAsync();

            return Ok(new ApiResponse<List<UserLogin>> { Status = true, StatusCode = 200, Message = "UserTable retrieved successfully", Data = userTable }); ;
        }

        // GET: api/User/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<UserLogin>>> GetUser(int id)
        {
            var user = await _context.UserLoginTable.FindAsync(id);

            user.User = _context.UserTable.FirstOrDefault(x => x.Id == user.UserId);

            if (user == null)
            {
                return NotFound(new ApiResponse<User> { Status = false, StatusCode = 404, Message = "User not found" });
            }

            return Ok(new ApiResponse<UserLogin> { Status = true, StatusCode = 200, Message = "User retrieved successfully", Data = user});
        }

        // POST: api/User
        [HttpPost]
        public async Task<ActionResult<ApiResponse<UserLogin>>> PostUser(UserLogin user)
        {
            user.Password = RandomString(6);
            _context.UserLoginTable.Add(user);
            await _context.SaveChangesAsync();
            var userData = _context.UserTable.FirstOrDefault(x => x.Id == user.UserId);
            var emailBody = $"Tài khoản đăng nhập: {user.UserName}\nMật khẩu của bạn là: {user.Password}";
            _emailService.SendContactEmail("Admin OCB", userData.Email, "OCB thông tin tài khoản mới", emailBody);
            return CreatedAtAction("GetUser", new { id = user.Id }, new ApiResponse<User> { Status = true, StatusCode = 200, Message = "Đã tạo thành công tài khoản cho người dùng nội bộ, vui lòng người dùng kiểm tra email để biết thông tin tài khoản và mật khẩu!", Data = null });
        }

        private string RandomString(int length)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[length];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            return new String(stringChars);
        }

        // PUT: api/User/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, UserLogin user)
        {
            if (id != user.Id)
            {
                return BadRequest(new ApiResponse<User> { Status = false, StatusCode = 400, Message = "Invalid request" });
            }

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound(new ApiResponse<User> { Status = false, StatusCode = 404, Message = "User not found" });
                }
                else
                {
                    throw;
                }
            }

            return Ok(new ApiResponse<UserLogin> { Status = true, StatusCode = 200, Message = "Cập nhật thông tin thành công!" });
        }

        // DELETE: api/User/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.UserLoginTable.FindAsync(id);
            if (user == null)
            {
                return NotFound(new ApiResponse<User> { Status = false, StatusCode = 404, Message = "User not found" });
            }

            _context.UserLoginTable.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<User> { Status = true, StatusCode = 204, Message = "Xóa tài khoản thành công!" });
        }

        private bool UserExists(int id)
        {
            return _context.UserTable.Any(e => e.Id == id);
        }
    }
}
