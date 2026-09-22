namespace UniPharApi.Models
{
    public class ContactSubmission
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Message { get; set; }
        public string Brand { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    }
}