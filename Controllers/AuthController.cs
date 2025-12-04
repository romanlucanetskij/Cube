using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DripCube.Data;
using DripCube.Entities;
using DripCube.Dtos;
using BCrypt.Net;

namespace DripCube.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }


        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(UserRegisterDto request)
        {

            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return BadRequest("Пользователь с таким Email уже существует.");
            }


            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);


            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                PasswordHash = passwordHash,
                FirstName = request.FirstName,
                LastName = request.LastName,
                RegistrationDate = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Регистрация успешна!", userId = user.Id });
        }


        [HttpPost("login")]
        public async Task<ActionResult<object>> Login(LoginDto request)
        {

            var emp = await _context.Employees
                .FirstOrDefaultAsync(e => e.Login == request.Email || e.Email == request.Email);

            if (emp != null)
            {

                if (!BCrypt.Net.BCrypt.Verify(request.Password, emp.PasswordHash))
                {
                    return BadRequest("Неверный пароль сотрудника.");
                }



                if (emp.Role == EmployeeRole.Manager && !emp.IsActive)
                {
                    return Ok(new { role = "Manager", status = "NotActive", id = emp.Id, message = "Заполните профиль!" });
                }


                return Ok(new
                {
                    role = emp.Role.ToString(),
                    id = emp.Id,
                    name = emp.Login,
                    personalId = emp.PersonalId,
                    chatId = emp.ChatId
                });
            }


            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user != null)
            {
                if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                {
                    return BadRequest("Неверный пароль.");
                }

                return Ok(new
                {
                    role = "User",
                    id = user.Id,
                    name = user.FirstName
                });
            }

            return BadRequest("Пользователь не найден.");
        }


        [HttpPost("create-manager")]
        public async Task<ActionResult> CreateManager(CreateManagerDto dto)
        {

            if (await _context.Employees.AnyAsync(e => e.Login == dto.Login))
            {
                return BadRequest("Логин уже занят.");
            }


            var random = new Random();
            string personalId = "AI23" + random.Next(100000, 999999).ToString();
            string chatId = "AC" + random.Next(100000, 999999).ToString();


            var manager = new Employee
            {
                Role = EmployeeRole.Manager,
                Login = dto.Login,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                PersonalId = personalId,
                ChatId = chatId,
                IsActive = false,


                FirstName = "Unknown",
                LastName = "Manager",
                Email = "",
                PhoneNumber = ""
            };

            _context.Employees.Add(manager);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Manager Created",
                login = manager.Login,
                personalId = manager.PersonalId
            });
        }

        [HttpPost("activate-manager")]
        public async Task<ActionResult> ActivateManager(ActivateManagerDto dto)
        {
            var emp = await _context.Employees.FindAsync(dto.ManagerId);
            if (emp == null) return NotFound("Сотрудник не найден");

            emp.FirstName = dto.FirstName;
            emp.LastName = dto.LastName;
            emp.PhoneNumber = dto.Phone;
            emp.Email = dto.Email;
            emp.IsActive = true;

            await _context.SaveChangesAsync();


            return Ok(new
            {
                message = "Account Activated",
                name = emp.Login,
                role = emp.Role.ToString(),
                id = emp.Id,
                personalId = emp.PersonalId,
                chatId = emp.ChatId
            });
        }


        [HttpGet("managers")]
        public async Task<ActionResult> GetManagers()
        {
            var managers = await _context.Employees
                .Where(e => e.Role == EmployeeRole.Manager)
                .Select(e => new
                {
                    e.Id,
                    e.Login,
                    e.PersonalId,
                    e.FirstName,
                    e.LastName,
                    e.Email,
                    e.IsActive
                })
                .ToListAsync();
            return Ok(managers);
        }


        [HttpDelete("user/{id}")]
        public async Task<ActionResult> DeleteUser(int id)
        {
            var emp = await _context.Employees.FindAsync(id);
            if (emp == null) return NotFound();


            if (emp.Role == EmployeeRole.Admin) return BadRequest("Cannot delete Admin");

            _context.Employees.Remove(emp);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Terminated" });
        }
    }
}
