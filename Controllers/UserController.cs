﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OCB_API.Models;

namespace OCB_API.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private  readonly OCBContext _context;
        public UserController(OCBContext context)
        {
            _context = context;
        }

        // GET: api/User
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<User>>>> GetUserTable()
        {
            
            var userTable = await _context.UserTable
                .Where(x => _context.UserLoginTable.Where(y => x.Id == y.UserId).FirstOrDefault().RoleUser != "S_Admin")
                .Where(x => _context.UserLoginTable.Where(y => x.Id == y.UserId).FirstOrDefault().RoleUser != "Admin")
                .ToListAsync();
            return Ok(new ApiResponse<List<User>> { Status = true, StatusCode = 200, Message = "Lấy danh sách khách hàng thành công!", Data = userTable }); ;
        }

        // GET: api/User/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<User>>> GetUser(int id)
        {
            var user = await _context.UserTable.FindAsync(id);

            if (user == null)
            {
                return NotFound(new ApiResponse<User> { Status = false, StatusCode = 404, Message = "User not found" });
            }

            return Ok(new ApiResponse<User> { Status = true, StatusCode = 200, Message = "Lấy thông tin chi tiết user thành công!", Data = user });
        }

        // POST: api/User
        [HttpPost]
        public async Task<ActionResult<ApiResponse<User>>> PostUser(User user)
        {
            _context.UserTable.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUser", new { id = user.Id }, new ApiResponse<User> { Status = true, StatusCode = 200, Message = "Tạo thông tin khách hàng thành công!", Data = user });
        }

        // PUT: api/User/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, User user)
        {
            if (id != user.Id)
            {
                return BadRequest(new ApiResponse<User> { Status = false, StatusCode = 400, Message = "Cập nhật thất bại! Vui lòng kiểm tra lại thông tin." });
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

            return Ok(new ApiResponse<User> { Status = true, StatusCode = 200, Message = "Cập nhật thông tin khách hàng thành công!" });
        }

        // DELETE: api/User/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.UserTable.FindAsync(id);
            if (user == null)
            {
                return NotFound(new ApiResponse<User> { Status = false, StatusCode = 404, Message = "User not found" });
            }

            _context.UserTable.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<User> { Status = true, StatusCode = 204, Message = "Xóa khách hàng thành công!" });
        }

        private bool UserExists(int id)
        {
            return _context.UserTable.Any(e => e.Id == id);
        }
    }
}
