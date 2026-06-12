namespace untitled1.Models.Settings
{
    public class EmailSettings
    {
        /// <summary>Set to false to disable email sending (logs only). Useful during development.</summary>
        public bool Enabled { get; set; } = false;

        public string Host { get; set; } = "smtp.gmail.com";
        public int Port { get; set; } = 587;
        public bool EnableSsl { get; set; } = true;

        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public string FromEmail { get; set; } = "noreply@filmix.vn";
        public string FromName { get; set; } = "FILMIX";
    }
}
