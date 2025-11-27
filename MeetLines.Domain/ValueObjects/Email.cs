using System;
using System.Text.RegularExpressions;

namespace MeetLines.Domain.ValueObjects
{
    public sealed record Email
    {
        public string Address { get; init; }

        public Email(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentException("Email required", nameof(address));

            var rx = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            if (!rx.IsMatch(address)) throw new ArgumentException("Invalid email", nameof(address));

            Address = address;
        }

        public override string ToString() => Address;
    }
}
