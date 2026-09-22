using Microsoft.AspNetCore.Mvc;
using UniPharApi.Models;

namespace UniPharApi.Controllers
{
    [ApiController]
    [Route("api/{brandSlug}/contact")]
    public class ContactController : ControllerBase
    {
        private readonly ILogger<ContactController> _logger;

        public ContactController(ILogger<ContactController> logger)
        {
            _logger = logger;
        }

        [HttpPost("submit")]
        public IActionResult Submit(string brandSlug, [FromBody] ContactSubmission submission)
        {
            if (submission == null ||
                string.IsNullOrWhiteSpace(submission.Name) ||
                string.IsNullOrWhiteSpace(submission.Email) ||
                string.IsNullOrWhiteSpace(submission.Message))
            {
                return BadRequest(new { message = "Name, email, and message are all required." });
            }

            submission.Brand = brandSlug;
            submission.SubmittedAt = DateTime.UtcNow;

            _logger.LogInformation(
                "Contact form submission — Brand: {Brand}, Name: {Name}, Email: {Email}, SubmittedAt: {SubmittedAt}",
                submission.Brand, submission.Name, submission.Email, submission.SubmittedAt);

            return Ok(new { message = "Thank you — your message has been received." });
        }
    }
}