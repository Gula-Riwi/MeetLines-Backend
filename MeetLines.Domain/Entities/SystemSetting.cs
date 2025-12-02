using System;

namespace MeetLines.Domain.Entities
{
    public class SystemSetting
    {
        public Guid Id { get; private set; }
        public string Key { get; private set; } = null!;
        public string? Value { get; private set; } // jsonb
        public DateTimeOffset UpdatedAt { get; private set; }

        private SystemSetting() { } // EF Core

        public SystemSetting(string key, string? value)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key cannot be empty", nameof(key));

            Id = Guid.NewGuid();
            Key = key;
            Value = value;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void UpdateValue(string? value)
        {
            Value = value;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }
}
