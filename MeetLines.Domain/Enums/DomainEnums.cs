namespace MeetLines.Domain.Enums
{
    public enum UserStatus { Active, Disabled }
    public enum SubscriptionCycle { Monthly, Yearly }
    public enum SubscriptionStatus { Active, Canceled, PastDue }
    public enum ProjectStatus { Active, Disabled }
    public enum ChannelType { Whatsapp, Instagram, Email, WebForm, Sms }
    public enum LeadStage { New, Qualified, Booked, Lost, Won }
    public enum Urgency { Low, Medium, High }
    public enum SenderType { Lead, Bot, Human }
    public enum AppointmentStatus { Pending, Confirmed, Cancelled, NoShow }
    public enum TemplateType { Greeting, FollowUp, Reminder, Qualification }
    
    // ===== NUEVOS ENUMS PARA AUTENTICACIÓN =====
    
    /// <summary>
    /// Tipos de proveedores de autenticación
    /// </summary>
    public enum AuthProvider
    {
        Local = 0,      // Email y contraseña
        Google = 1,     // OAuth Google
        Facebook = 2    // OAuth Facebook/Meta
    }
    
    /// <summary>
    /// Estados de verificación de email
    /// </summary>
    public enum EmailVerificationStatus
    {
        Pending = 0,
        Verified = 1,
        Expired = 2
    }
    
    /// <summary>
    /// Estados del token de recuperación de contraseña
    /// </summary>
    public enum PasswordResetTokenStatus
    {
        Active = 0,
        Used = 1,
        Expired = 2
    }
}