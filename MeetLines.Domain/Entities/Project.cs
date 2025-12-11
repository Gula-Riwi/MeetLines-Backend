using System;

namespace MeetLines.Domain.Entities
{
    public class Project
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public string Name { get; private set; }
        // WhatsApp integration fields
        public string? WhatsappVerifyToken { get; private set; }
        public string? WhatsappPhoneNumberId { get; private set; }
        public string? WhatsappAccessToken { get; private set; }
        public string? WhatsappForwardWebhook { get; private set; }
        // Telegram integration fields
        public string? TelegramBotToken { get; private set; }
        public string? TelegramBotUsername { get; private set; }
        public string? TelegramForwardWebhook { get; private set; }
        public string? Industry { get; private set; }
        public string? Description { get; private set; }
        public string? WorkingHours { get; private set; } // jsonb
        public string? Config { get; private set; } // jsonb
        public string Subdomain { get; private set; }
        public string Status { get; private set; }
        public string? Address { get; private set; }
        public string? City { get; private set; }
        public string? Country { get; private set; }
        public double? Latitude { get; private set; }
        public double? Longitude { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset UpdatedAt { get; private set; }

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public double? Distance { get; set; }

        private Project() { Name = null!; Status = null!; Subdomain = null!; } // EF Core

        public Project(Guid userId, string name, string subdomain, string? industry = null, string? description = null, string? address = null, string? city = null, string? country = null, double? latitude = null, double? longitude = null)
        {
            if (userId == Guid.Empty) throw new ArgumentException("UserId cannot be empty", nameof(userId));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty", nameof(name));
            
            if (!ValueObjects.SubdomainValidator.IsValid(subdomain, out var error))
                throw new ArgumentException(error, nameof(subdomain));

            Id = Guid.NewGuid();
            UserId = userId;
            Name = name;
            Subdomain = subdomain;
            Industry = industry;
            Description = description;
            Address = address;
            City = city;
            Country = country;
            Latitude = latitude;
            Longitude = longitude;
            Status = "active";
            CreatedAt = DateTimeOffset.UtcNow;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void UpdateWhatsappIntegration(string? verifyToken, string? phoneNumberId, string? accessToken, string? forwardWebhook)
        {
            WhatsappVerifyToken = string.IsNullOrWhiteSpace(verifyToken) ? null : verifyToken;
            WhatsappPhoneNumberId = string.IsNullOrWhiteSpace(phoneNumberId) ? null : phoneNumberId;
            WhatsappAccessToken = string.IsNullOrWhiteSpace(accessToken) ? null : accessToken;
            WhatsappForwardWebhook = string.IsNullOrWhiteSpace(forwardWebhook) ? null : forwardWebhook;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void UpdateTelegramIntegration(string? botToken, string? botUsername, string? forwardWebhook)
        {
            TelegramBotToken = string.IsNullOrWhiteSpace(botToken) ? null : botToken;
            TelegramBotUsername = string.IsNullOrWhiteSpace(botUsername) ? null : botUsername;
            TelegramForwardWebhook = string.IsNullOrWhiteSpace(forwardWebhook) ? null : forwardWebhook;
            UpdatedAt = DateTimeOffset.UtcNow;
        }


        public void UpdateDetails(string name, string? industry, string? description, string? address, string? city, string? country, double? latitude = null, double? longitude = null)
          
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty", nameof(name));
            Name = name;
            Industry = industry;
            Description = description;
            Address = address;
            City = city;
            Country = country;
            Latitude = latitude;
            Longitude = longitude;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void UpdateSubdomain(string newSubdomain)
        {
            if (!ValueObjects.SubdomainValidator.IsValid(newSubdomain, out var error))
                throw new ArgumentException(error, nameof(newSubdomain));
            
            Subdomain = newSubdomain;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void UpdateConfig(string? workingHours, string? config)
        {
            WorkingHours = workingHours;
            Config = config;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        /// <summary>
        /// Cambia el estado del proyecto a deshabilitado (borrado lógico)
        /// </summary>
        public void Disable()
        {
            Status = "disabled";
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }
}
