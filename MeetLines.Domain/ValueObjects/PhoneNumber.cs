using System;
using System.Text.RegularExpressions;

namespace MeetLines.Domain.ValueObjects
{
    public sealed record PhoneNumber
    {
        public string Number { get; init; }

        public PhoneNumber(string number)
        {
            if (string.IsNullOrWhiteSpace(number)) throw new ArgumentException("Phone required", nameof(number));
            var rx = new Regex(@"^[0-9\+\-\s\(\)]+$");
            if (!rx.IsMatch(number)) throw new ArgumentException("Invalid phone number", nameof(number));

            Number = number;
        }

        public override string ToString() => Number;
    }
}
