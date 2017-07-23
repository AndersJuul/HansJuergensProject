namespace HansJuergenWeb.Contracts
{
    public class FileProcessedEvent
    {
        public string[] FileNames { get; set; }

        public string Email { get; set; }

        public string Description { get; set; }

        public string DataFolder { get; set; }
    }
}