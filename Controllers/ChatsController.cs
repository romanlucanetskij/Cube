using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DripCube.Data;
using DripCube.Entities;
using DripCube.Dtos;

namespace DripCube.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ChatsController(AppDbContext context)
        {
            _context = context;
        }


        [HttpPost("send-user")]
        public async Task<ActionResult> SendUserMessage(SendMessageDto dto)
        {
            ChatSession? session;


            session = await _context.ChatSessions
                .Include(s => s.Messages)
                .FirstOrDefaultAsync(s => s.UserId == dto.SenderId && s.Status != ChatStatus.Closed);

            if (session == null)
            {

                var freeManager = await _context.Employees
                    .Where(e => e.Role == EmployeeRole.Manager && e.IsActive)
                    .Select(m => new
                    {
                        Manager = m,

                        ActiveChats = _context.ChatSessions.Count(c => c.ManagerId == m.Id && c.Status == ChatStatus.InProgress)
                    })
                    .OrderBy(x => x.ActiveChats)
                    .FirstOrDefaultAsync();

                int? managerId = freeManager?.Manager.Id;


                session = new ChatSession
                {
                    Id = Guid.NewGuid(),
                    UserId = dto.SenderId,
                    ManagerId = managerId,
                    Status = ChatStatus.InProgress,
                    CreatedAt = DateTime.UtcNow
                };
                _context.ChatSessions.Add(session);
            }


            var msg = new ChatMessage
            {
                ChatSessionId = session.Id,
                Sender = SenderRole.User,
                Text = dto.Text,
                SentAt = DateTime.UtcNow
            };
            _context.ChatMessages.Add(msg);
            await _context.SaveChangesAsync();

            return Ok(new { sessionId = session.Id, assignedManager = session.ManagerId });
        }


        [HttpPost("send-manager")]
        public async Task<ActionResult> SendManagerMessage(SendMessageDto dto)
        {
            if (dto.SessionId == null) return BadRequest("Нет ID сессии");

            var msg = new ChatMessage
            {
                ChatSessionId = dto.SessionId.Value,
                Sender = SenderRole.Manager,
                Text = dto.Text,
                SentAt = DateTime.UtcNow
            };

            _context.ChatMessages.Add(msg);
            await _context.SaveChangesAsync();

            return Ok();
        }


        [HttpGet("history/{sessionId}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetHistory(Guid sessionId)
        {
            var messages = await _context.ChatMessages
                .Where(m => m.ChatSessionId == sessionId)
                .OrderBy(m => m.SentAt)
                .Select(m => new MessageDto
                {
                    Sender = m.Sender.ToString(),
                    Text = m.Text,
                    SentAt = m.SentAt
                })
                .ToListAsync();

            return Ok(messages);
        }


        [HttpGet("manager-chats/{managerId}")]
        public async Task<ActionResult<IEnumerable<ChatPreviewDto>>> GetManagerChats(int managerId)
        {
            var chats = await _context.ChatSessions
                .Where(c => c.ManagerId == managerId && c.Status != ChatStatus.Closed)
                .Select(c => new ChatPreviewDto
                {
                    SessionId = c.Id,

                    UserName = _context.Users.Where(u => u.Id == c.UserId).Select(u => u.FirstName).FirstOrDefault() ?? "Anon",
                    Status = c.Status.ToString(),
                    LastMessage = c.Messages.OrderByDescending(m => m.SentAt).Select(m => m.Text).FirstOrDefault() ?? "..."
                })
                .ToListAsync();

            return Ok(chats);
        }


        [HttpGet("user-active-session/{userId}")]
        public async Task<ActionResult<object>> GetUserActiveSession(Guid userId)
        {
            var session = await _context.ChatSessions
                .FirstOrDefaultAsync(s => s.UserId == userId && s.Status != ChatStatus.Closed);

            if (session == null) return NoContent();

            return Ok(new { sessionId = session.Id });
        }


        [HttpGet("admin-all")]
        public async Task<ActionResult> GetAllChatsForAdmin()
        {
            var chats = await _context.ChatSessions
                .Include(c => c.Messages)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new
                {
                    SessionId = c.Id,
                    UserName = _context.Users.Where(u => u.Id == c.UserId).Select(u => u.FirstName).FirstOrDefault() ?? "Unknown",
                    ManagerName = c.Manager != null ? c.Manager.Login : "---",
                    Status = c.Status.ToString(),
                    LastActivity = c.Messages.OrderByDescending(m => m.SentAt).Select(m => m.SentAt).FirstOrDefault(),
                    LastMessage = c.Messages.OrderByDescending(m => m.SentAt).Select(m => m.Text).FirstOrDefault() ?? "..."
                })
                .ToListAsync();

            return Ok(chats);
        }


        [HttpDelete("{sessionId}")]
        public async Task<ActionResult> DeleteChat(Guid sessionId)
        {
            var session = await _context.ChatSessions.FindAsync(sessionId);
            if (session == null) return NotFound();

            _context.ChatSessions.Remove(session);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Chat Terminated" });
        }
    }
}