using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MeetLines.Domain.ValueObjects
{
    public static class SubdomainValidator
    {
        private static readonly Regex FormatRegex = new Regex("^[a-z0-9]([a-z0-9-]{1,61}[a-z0-9])?$", RegexOptions.Compiled);
        
        private static readonly HashSet<string> ReservedSubdomains = new HashSet<string>
        {
            "www", "api", "admin", "app", "dashboard", "cdn", "mail", "ftp", "smtp", "pop", "imap", 
            "meetlines", "meet-lines", "support", "help", "blog", "status", "dev", "staging", "test",
            "auth", "login", "register", "signup", "signin", "account", "profile", "billing"
        };

        public static bool IsValid(string subdomain, out string? errorMessage)
        {
            if (string.IsNullOrWhiteSpace(subdomain))
            {
                errorMessage = "Subdomain cannot be empty.";
                return false;
            }

            if (subdomain.Length < 3 || subdomain.Length > 63)
            {
                errorMessage = "Subdomain must be between 3 and 63 characters.";
                return false;
            }

            if (!FormatRegex.IsMatch(subdomain))
            {
                errorMessage = "Subdomain can only contain lowercase letters, numbers, and hyphens, and cannot start or end with a hyphen.";
                return false;
            }

            if (ReservedSubdomains.Contains(subdomain))
            {
                errorMessage = $"Subdomain '{subdomain}' is reserved and cannot be used.";
                return false;
            }

            errorMessage = null;
            return true;
        }
    }
}
