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
}
