using Microsoft.EntityFrameworkCore;
using MeetLines.Domain.Entities;
using MeetLines.Domain.Enums;

namespace MeetLines.Infrastructure.Data
{
    public class MeetLinesPgDbContext : DbContext
    {
        public MeetLinesPgDbContext(DbContextOptions<MeetLinesPgDbContext> options) : base(options) { }

        public DbSet<SaasUser> SaasUsers { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Channel> Channels { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Webhook> Webhooks { get; set; }
        public DbSet<EventLog> EventLogs { get; set; }
        public DbSet<Template> Templates { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<LoginSession> LoginSessions { get; set; }
        public DbSet<SystemSetting> SystemSettings { get; set; }
        public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        public DbSet<TransferToken> TransferTokens { get; set; }
        public DbSet<Employee> Employees { get; set; }
        
        // WhatsApp Bot System
        public DbSet<ProjectBotConfig> ProjectBotConfigs { get; set; }
        public DbSet<KnowledgeBase> KnowledgeBases { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<CustomerFeedback> CustomerFeedbacks { get; set; }
        public DbSet<CustomerReactivation> CustomerReactivations { get; set; }
        public DbSet<BotMetrics> BotMetrics { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresExtension("uuid-ossp");

            // SaasUsers
            modelBuilder.Entity<SaasUser>(b =>
            {
                b.ToTable("saas_users");
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).HasDefaultValueSql("uuid_generate_v4()");
                b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
                b.Property(x => x.UpdatedAt).HasDefaultValueSql("now()");
                b.HasIndex(x => x.Email).IsUnique();
                b.Property(x => x.IsEmailVerified).HasDefaultValue(false);
                b.HasIndex(x => x.ExternalProviderId).HasDatabaseName("idx_saasusers_externalprovider");
            });

            // Subscriptions
            modelBuilder.Entity<Subscription>(b =>
            {
                b.ToTable("subscriptions");
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).HasDefaultValueSql("uuid_generate_v4()");
                b.Property(x => x.Price).HasColumnType("numeric(10,2)");
                b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
                b.HasOne<SaasUser>().WithMany().HasForeignKey(s => s.UserId).OnDelete(DeleteBehavior.Cascade);
                b.HasIndex(x => x.UserId).HasDatabaseName("idx_subscriptions_user");
            });

            // Projects
            modelBuilder.Entity<Project>(b =>
            {
                b.ToTable("projects");
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).HasDefaultValueSql("uuid_generate_v4()");
                b.Property(x => x.WorkingHours).HasColumnType("jsonb");
                b.Property(x => x.Config).HasColumnType("jsonb");
                b.Property(x => x.Subdomain).IsRequired().HasMaxLength(63);
                b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
                b.Property(x => x.UpdatedAt).HasDefaultValueSql("now()");
                // WhatsApp Integration
                b.Property(x => x.WhatsappVerifyToken).HasColumnName("whatsapp_verify_token").HasMaxLength(256);
                b.Property(x => x.WhatsappPhoneNumberId).HasColumnName("whatsapp_phone_number_id").HasMaxLength(100);
                b.Property(x => x.WhatsappAccessToken).HasColumnName("whatsapp_access_token");
                b.Property(x => x.WhatsappForwardWebhook).HasColumnName("whatsapp_forward_webhook");
                b.HasOne<SaasUser>().WithMany().HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
                b.HasIndex(x => x.UserId).HasDatabaseName("idx_projects_user");
                b.HasIndex(x => x.Subdomain).IsUnique().HasDatabaseName("idx_projects_subdomain");
                b.HasIndex(x => x.WhatsappPhoneNumberId).HasDatabaseName("idx_projects_whatsapp_phone_number_id");
            });

            // Channels
            modelBuilder.Entity<Channel>(b =>
            {
                b.ToTable("channels");
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).HasDefaultValueSql("uuid_generate_v4()");
                b.Property(x => x.Credentials).HasColumnType("jsonb");
                b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
                b.Property(x => x.UpdatedAt).HasDefaultValueSql("now()");
                b.HasOne<Project>().WithMany().HasForeignKey(c => c.ProjectId).OnDelete(DeleteBehavior.Cascade);
                b.HasIndex(x => x.ProjectId).HasDatabaseName("idx_channels_project");
                b.HasIndex(x => x.Type).HasDatabaseName("idx_channels_type");
            });
            

            // Appointments
            modelBuilder.Entity<Appointment>(b =>
            {
                b.ToTable("appointments");
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).ValueGeneratedOnAdd(); // Serial
                
                b.Property(x => x.ProjectId).HasColumnName("project_id");
                b.Property(x => x.LeadId).HasColumnName("lead_id"); // Restored
                b.Property(x => x.AppUserId).HasColumnName("app_users_id"); 
                b.Property(x => x.ServiceId).HasColumnName("service_id");
                b.Property(x => x.EmployeeId).HasColumnName("employee_id");
                
                b.Property(x => x.StartTime).HasColumnName("start_time");
                b.Property(x => x.EndTime).HasColumnName("end_time");
                b.Property(x => x.Status).HasColumnName("status").HasMaxLength(20).HasDefaultValue("pending");
                
                b.Property(x => x.PriceSnapshot).HasColumnName("price_snapshot").HasColumnType("numeric(15,2)");
                b.Property(x => x.CurrencySnapshot).HasColumnName("currency_snapshot").HasMaxLength(3).HasDefaultValue("COP");
                b.Property(x => x.MeetingLink).HasColumnName("meeting_link");
                b.Property(x => x.UserNotes).HasColumnName("user_notes");
                b.Property(x => x.AdminNotes).HasColumnName("admin_notes");

                b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
                b.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");

                b.HasOne<Project>().WithMany().HasForeignKey(a => a.ProjectId).OnDelete(DeleteBehavior.Cascade);
                b.HasOne<Employee>().WithMany().HasForeignKey(a => a.EmployeeId).OnDelete(DeleteBehavior.SetNull); // Set Null if employee deleted

                b.HasIndex(x => x.ProjectId).HasDatabaseName("idx_appointments_project");
                b.HasIndex(x => x.AppUserId).HasDatabaseName("idx_appointments_appuser");
                b.HasIndex(x => x.EmployeeId).HasDatabaseName("idx_appointments_employee");
            });

            // Webhooks
            modelBuilder.Entity<Webhook>(b =>
            {
                b.ToTable("webhooks");
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).HasDefaultValueSql("uuid_generate_v4()");
                b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
                b.HasOne<Project>().WithMany().HasForeignKey(w => w.ProjectId).OnDelete(DeleteBehavior.Cascade);
                b.HasIndex(x => x.ProjectId).HasDatabaseName("idx_webhooks_project");
                b.HasIndex(x => x.Event).HasDatabaseName("idx_webhooks_event");
            });

            // EventLog
            modelBuilder.Entity<EventLog>(b =>
            {
                b.ToTable("event_log");
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).HasDefaultValueSql("uuid_generate_v4()");
                b.Property(x => x.Payload).HasColumnType("jsonb");
                b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
                b.HasIndex(x => x.Processed).HasDatabaseName("idx_eventlog_processed");
            });

            // Templates
            modelBuilder.Entity<Template>(b =>
            {
                b.ToTable("templates");
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).HasDefaultValueSql("uuid_generate_v4()");
                b.Property(x => x.Variables).HasColumnType("jsonb");
                b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
                b.Property(x => x.UpdatedAt).HasDefaultValueSql("now()");
                b.HasIndex(x => x.ProjectId).HasDatabaseName("idx_templates_project");
            });

            // Tags
            modelBuilder.Entity<Tag>(b =>
            {
                b.ToTable("tags");
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).HasDefaultValueSql("uuid_generate_v4()");
                b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
                b.HasOne<Project>().WithMany().HasForeignKey(t => t.ProjectId).OnDelete(DeleteBehavior.Cascade);
                b.HasIndex(x => new { x.ProjectId, x.TagName }).IsUnique().HasDatabaseName("ux_tags_project_tag");
            });
            

            // AuditLogs
            modelBuilder.Entity<AuditLog>(b =>
            {
                b.ToTable("audit_logs");
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).HasDefaultValueSql("uuid_generate_v4()");
                b.Property(x => x.Details).HasColumnType("jsonb");
                b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
                b.HasIndex(x => x.UserId).HasDatabaseName("idx_auditlogs_user");
            });

            // LoginSessions
            modelBuilder.Entity<LoginSession>(b =>
            {
                b.ToTable("login_sessions");
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).HasDefaultValueSql("uuid_generate_v4()");
                b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
                // b.HasOne<SaasUser>().WithMany().HasForeignKey(s => s.UserId).OnDelete(DeleteBehavior.Cascade);
                // Eliminamos la FK estricta para permitir que UserId sea de Empleado o SaasUser
                // b.HasOne<SaasUser>().WithMany().HasForeignKey(s => s.UserId).OnDelete(DeleteBehavior.Cascade);
                b.HasIndex(x => x.UserId).HasDatabaseName("idx_sessions_user");
                b.HasIndex(x => x.TokenHash).HasDatabaseName("idx_sessions_tokenhash");
            });

            // SystemSettings
            modelBuilder.Entity<SystemSetting>(b =>
            {
                b.ToTable("system_settings");
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).HasDefaultValueSql("uuid_generate_v4()");
                b.Property(x => x.Value).HasColumnType("jsonb");
                b.Property(x => x.UpdatedAt).HasDefaultValueSql("now()");
                b.HasIndex(x => x.Key).IsUnique();
            });
            
            
            // EmailVerificationTokens
            modelBuilder.Entity<EmailVerificationToken>(b =>
            {
                b.ToTable("email_verification_tokens");
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).HasDefaultValueSql("uuid_generate_v4()");
                b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
                b.HasOne<SaasUser>().WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
                b.HasIndex(x => x.UserId).HasDatabaseName("idx_emailverif_user");
                b.HasIndex(x => x.Token).IsUnique().HasDatabaseName("idx_emailverif_token");
            });

            // PasswordResetTokens
            modelBuilder.Entity<PasswordResetToken>(b =>
            {
                b.ToTable("password_reset_tokens");
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).HasDefaultValueSql("uuid_generate_v4()");
                b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
                b.HasOne<SaasUser>().WithMany().HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
                b.HasIndex(x => x.UserId).HasDatabaseName("idx_passreset_user");
                b.HasIndex(x => x.Token).IsUnique().HasDatabaseName("idx_passreset_token");
            });

            modelBuilder.Entity<TransferToken>(b =>
            {
                b.ToTable("transfer_tokens");
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).HasDefaultValueSql("uuid_generate_v4()");
                b.Property(x => x.Token).IsRequired().HasMaxLength(512);
                b.Property(x => x.Tenant).IsRequired().HasMaxLength(128);
                b.Property(x => x.ExpiresAt).IsRequired();
                b.Property(x => x.Used).HasDefaultValue(false);
                b.HasIndex(x => x.Token).IsUnique().HasDatabaseName("idx_transfertoken_token");
                b.HasIndex(x => x.UserId).HasDatabaseName("idx_transfertoken_user");
            });

            // Employees
            modelBuilder.Entity<Employee>(b =>
            {
                b.ToTable("employees");
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).HasDefaultValueSql("uuid_generate_v4()");
                b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
                b.Property(x => x.UpdatedAt).HasDefaultValueSql("now()");
                b.HasOne<Project>().WithMany().HasForeignKey(e => e.ProjectId).OnDelete(DeleteBehavior.Cascade);
                b.HasIndex(x => x.ProjectId).HasDatabaseName("idx_employees_project");
                b.HasIndex(x => x.Username).IsUnique().HasDatabaseName("idx_employees_username");
                b.HasIndex(x => x.Email).IsUnique().HasDatabaseName("idx_employees_email"); // Ensure unique email
                b.HasIndex(x => x.Area).HasDatabaseName("idx_employees_area"); // Index for Area lookups
            });

            // ProjectBotConfig
            modelBuilder.Entity<ProjectBotConfig>(b =>
            {
                b.ToTable("project_bot_configs");
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).HasDefaultValueSql("uuid_generate_v4()");
                b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
                b.Property(x => x.UpdatedAt).HasDefaultValueSql("now()");
                b.HasOne<Project>().WithMany().HasForeignKey(c => c.ProjectId).OnDelete(DeleteBehavior.Cascade);
                b.HasIndex(x => x.ProjectId).IsUnique().HasDatabaseName("idx_botconfig_project");
            });

            // KnowledgeBase
            modelBuilder.Entity<KnowledgeBase>(b =>
            {
                b.ToTable("knowledge_bases");
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
                b.Property(x => x.ProjectId).HasColumnName("project_id");
                b.Property(x => x.Category).HasColumnName("category");
                b.Property(x => x.Question).HasColumnName("question");
                b.Property(x => x.Answer).HasColumnName("answer");
                b.Property(x => x.Keywords).HasColumnName("keywords");
                b.Property(x => x.Priority).HasColumnName("priority");
                b.Property(x => x.IsActive).HasColumnName("is_active");
                b.Property(x => x.UsageCount).HasColumnName("usage_count");
                b.Property(x => x.LastUsedAt).HasColumnName("last_used_at");
                b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
                b.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");
                b.HasOne<Project>().WithMany().HasForeignKey(k => k.ProjectId).OnDelete(DeleteBehavior.Cascade);
                b.HasIndex(x => x.ProjectId).HasDatabaseName("idx_kb_project");
                b.HasIndex(x => x.Category).HasDatabaseName("idx_kb_category");
            });

            // Conversation
            modelBuilder.Entity<Conversation>(b =>
            {
                b.ToTable("conversations");
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).HasDefaultValueSql("uuid_generate_v4()");
                b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
                b.HasOne<Project>().WithMany().HasForeignKey(c => c.ProjectId).OnDelete(DeleteBehavior.Cascade);
                b.HasOne<Employee>().WithMany().HasForeignKey(c => c.HandledByEmployeeId).OnDelete(DeleteBehavior.SetNull);
                b.HasIndex(x => x.ProjectId).HasDatabaseName("idx_conv_project");
                b.HasIndex(x => x.CustomerPhone).HasDatabaseName("idx_conv_phone");
                b.HasIndex(x => x.CreatedAt).HasDatabaseName("idx_conv_created");
            });

            // CustomerFeedback
            modelBuilder.Entity<CustomerFeedback>(b =>
            {
                b.ToTable("customer_feedbacks");
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).HasDefaultValueSql("uuid_generate_v4()");
                b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
                b.HasOne<Project>().WithMany().HasForeignKey(f => f.ProjectId).OnDelete(DeleteBehavior.Cascade);
                b.HasOne<Appointment>().WithMany().HasForeignKey(f => f.AppointmentId).OnDelete(DeleteBehavior.SetNull);
                b.HasIndex(x => x.ProjectId).HasDatabaseName("idx_feedback_project");
                b.HasIndex(x => x.AppointmentId).HasDatabaseName("idx_feedback_appointment");
                b.HasIndex(x => x.Rating).HasDatabaseName("idx_feedback_rating");
            });

            // CustomerReactivation
            modelBuilder.Entity<CustomerReactivation>(b =>
            {
                b.ToTable("customer_reactivations");
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).HasDefaultValueSql("uuid_generate_v4()");
                b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
                b.HasOne<Project>().WithMany().HasForeignKey(r => r.ProjectId).OnDelete(DeleteBehavior.Cascade);
                b.HasOne<Appointment>().WithMany().HasForeignKey(r => r.NewAppointmentId).OnDelete(DeleteBehavior.SetNull);
                b.HasIndex(x => x.ProjectId).HasDatabaseName("idx_reactivation_project");
                b.HasIndex(x => x.CustomerPhone).HasDatabaseName("idx_reactivation_phone");
                b.HasIndex(x => x.Reactivated).HasDatabaseName("idx_reactivation_status");
            });

            // BotMetrics
            modelBuilder.Entity<BotMetrics>(b =>
            {
                b.ToTable("bot_metrics");
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).HasDefaultValueSql("uuid_generate_v4()");
                b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
                b.HasOne<Project>().WithMany().HasForeignKey(m => m.ProjectId).OnDelete(DeleteBehavior.Cascade);
                b.HasIndex(x => new { x.ProjectId, x.Date }).IsUnique().HasDatabaseName("idx_metrics_project_date");
            });
        }
    }
}