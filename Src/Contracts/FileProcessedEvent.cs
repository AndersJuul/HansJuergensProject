﻿namespace HansJuergenWeb.Contracts
{
    public class FileProcessedEvent
    {
        public string FileName { get; set; }

        public string Email { get; set; }

        public string Description { get; set; }
    }
}