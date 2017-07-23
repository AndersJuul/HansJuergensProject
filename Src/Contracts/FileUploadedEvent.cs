﻿using System;

namespace HansJuergenWeb.Contracts
{
    public class FileUploadedEvent
    {
        public string[] FileNames { get; set; }

        public string Email { get; set; }

        public string Description { get; set; }

        public Guid Id { get; set; }
    }
}