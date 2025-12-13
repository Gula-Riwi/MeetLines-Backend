using System;
using System.Collections.Generic;

namespace MeetLines.Application.DTOs.TwoFactor
{
    public class Enable2FARequest
    {
        // No request body needed, uses current user
    }

    public class Enable2FAResponse
    {
        public string Secret { get; set; } = string.Empty;
        public string QrCodeUri { get; set; } = string.Empty;
        public List<string> BackupCodes { get; set; } = new List<string>();
    }

    public class Verify2FASetupRequest
    {
        public string Code { get; set; } = string.Empty;
    }

    public class Disable2FARequest
    {
        public string Code { get; set; } = string.Empty;
    }

    public class Login2FARequest
    {
        public string TwoFactorCode { get; set; } = string.Empty;
        public string? DeviceInfo { get; set; }
        public string? IpAddress { get; set; }
    }

    public class TwoFactorResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class BackupCodesResponse
    {
        public List<string> BackupCodes { get; set; } = new List<string>();
    }
}
