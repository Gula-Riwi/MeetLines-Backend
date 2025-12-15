using Microsoft.EntityFrameworkCore;
using MeetLines.Domain.Entities;
using MeetLines.Domain.Enums;

namespace MeetLines.Infrastructure.Data
{
    public class MeetLinesPgDbContext : DbContext
    {
        public MeetLinesPgDbContext(DbContextOptions<MeetLinesPgDbContext> options) : base(options) { }

        public DbSet<SaasUser> SaasUsers { get; set; }
        // public DbSet<Subscription> Subscriptions { get; set; } // DISABLED - Schema mismatch
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
        public DbSet<EmployeePasswordResetToken> EmployeePasswordResetTokens { get; set; }
        public DbSet<TransferToken> TransferTokens { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<AppUserPasswordResetToken> AppUserPasswordResetTokens { get; set; }
        public DbSet<Payment> Payments { get; set; }
        
        // WhatsApp Bot System
        public DbSet<ProjectBotConfig> ProjectBotConfigs { get; set; }
        public DbSet<KnowledgeBase> KnowledgeBases { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<CustomerFeedback> CustomerFeedbacks { get; set; }
        public DbSet<CustomerReactivation> CustomerReactivations { get; set; }
        public DbSet<BotMetrics> BotMetrics { get; set; }
        public DbSet<ProjectPhoto> ProjectPhotos { get; set; }
        public DbSet<TwoFactorBackupCode> TwoFactorBackupCodes { get; set; }

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
                
                // Two-Factor Authentication
                b.Property(x => x.TwoFactorEnabled).HasColumnName("two_factor_enabled").HasDefaultValue(false);
                b.Property(x => x.TwoFactorSecret).HasColumnName("two_factor_secret").HasMaxLength(255);
            });

            // Subscriptions (DISABLED - Schema mismatch)
            // modelBuilder.Entity<Subscription>(b =>
            // {
            //     b.ToTable("subscriptions");
            //     b.HasKey(x => x.Id);
            //     b.Property(x => x.Id).HasDefaultValueSql("uuid_generate_v4()");
            //     b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
            //     b.HasOne<SaasUser>().WithMany().HasForeignKey(s => s.UserId).OnDelete(DeleteBehavior.Cascade);
            //     b.HasIndex(x => x.UserId).HasDatabaseName("idx_subscriptions_user");
            // });

            // Projects
            modelBuilder.Entity<Project>(b =>
            {
                b.ToTable("projects");
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).HasDefaultValueSql("uuid_generate_v4()");
                b.Property(x => x.WorkingHours).HasColumnType("jsonb");
                b.Property(x => x.Config).HasColumnType("jsonb");
                b.Property(x => x.Subdomain).IsRequired().HasMaxLength(63);
                b.Property(x => x.Address).HasMaxLength(255).HasColumnName("address");
                b.Property(x => x.City).HasMaxLength(100).HasColumnName("city");
                b.Property(x => x.Country).HasMaxLength(100).HasColumnName("country");
                b.Property(x => x.Latitude).HasColumnName("latitude");
                b.Property(x => x.Longitude).HasColumnName("longitude");
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
            
            // Payments
            modelBuilder.Entity<Payment>(b =>
            {
                b.ToTable("Payments");
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).HasColumnName("Id").HasDefaultValueSql("uuid_generate_v4()");
                b.Property(x => x.UserId).HasColumnName("User_Id");
                b.Property(x => x.MercadoPagoPreferenceId).HasColumnName("Mercado_Pago_preference_Id");
                b.Property(x => x.Plan).HasColumnName("Plan");
                b.Property(x => x.Amount).HasColumnName("Amount").HasColumnType("numeric(10,2)");
                b.Property(x => x.Currency).HasColumnName("Currency");
                b.Property(x => x.Status).HasColumnName("Status");
                b.Property(x => x.CreatedAt).HasColumnName("Created_At").HasDefaultValueSql("now()");

                b.HasOne<SaasUser>().WithMany().HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
                b.HasIndex(x => x.UserId).HasDatabaseName("idx_payments_user");
            });
            

            // Appointments
            modelBuilder.Entity<Appointment>(b =>
            {
                b.ToTable("appointments");
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd(); // Serial
                
                b.Property(x => x.ProjectId).HasColumnName("project_id");
                b.Ignore(x => x.LeadId); // Column doesn't exist in database
                b.Property(x => x.AppUserId).HasColumnName("app_users_id").IsRequired(false); // Nullable
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
                b.Property(x => x.ReminderSent).HasColumnName("reminder_sent").HasDefaultValue(false);

                b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
                b.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");

                b.HasOne(a => a.Project).WithMany().HasForeignKey(a => a.ProjectId).OnDelete(DeleteBehavior.Cascade);
                b.HasOne(a => a.Employee).WithMany().HasForeignKey(a => a.EmployeeId).OnDelete(DeleteBehavior.SetNull); 
                b.HasOne(a => a.AppUser).WithMany().HasForeignKey(a => a.AppUserId).OnDelete(DeleteBehavior.SetNull);
                b.HasOne(a => a.Service).WithMany().HasForeignKey(a => a.ServiceId).OnDelete(DeleteBehavior.Restrict);

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

            modelBuilder.Entity<EmployeePasswordResetToken>(b =>
            {
                b.ToTable("employee_password_reset_tokens");
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).HasDefaultValueSql("uuid_generate_v4()");
                b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
                b.HasOne<Employee>().WithMany().HasForeignKey(p => p.EmployeeId).OnDelete(DeleteBehavior.Cascade);
                b.HasIndex(x => x.EmployeeId).HasDatabaseName("idx_emp_passreset_employee");
                b.HasIndex(x => x.Token).IsUnique().HasDatabaseName("idx_emp_passreset_token");
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
                b.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
                b.Property(x => x.ProjectId).HasColumnName("project_id");
                b.Property(x => x.BotName).HasColumnName("bot_name");
                b.Property(x => x.Industry).HasColumnName("industry");
                b.Property(x => x.Tone).HasColumnName("tone");
                b.Property(x => x.Timezone).HasColumnName("timezone");
                b.Property(x => x.ReceptionConfigJson).HasColumnName("reception_config_json").HasColumnType("jsonb");
                b.Property(x => x.TransactionalConfigJson).HasColumnName("transactional_config_json").HasColumnType("jsonb");
                b.Property(x => x.FeedbackConfigJson).HasColumnName("feedback_config_json").HasColumnType("jsonb");
                b.Property(x => x.ReactivationConfigJson).HasColumnName("reactivation_config_json").HasColumnType("jsonb");
                b.Property(x => x.IntegrationsConfigJson).HasColumnName("integrations_config_json").HasColumnType("jsonb");
                b.Property(x => x.AdvancedConfigJson).HasColumnName("advanced_config_json").HasColumnType("jsonb");
                b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
                b.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");
                b.Property(x => x.CreatedBy).HasColumnName("created_by");
                b.Property(x => x.UpdatedBy).HasColumnName("updated_by");
                b.Property(x => x.IsActive).HasColumnName("is_active");
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
                b.Property(x => x.Keywords).HasColumnName("keywords").HasColumnType("jsonb");
                b.Property(x => x.Priority).HasColumnName("priority");
                b.Property(x => x.IsActive).HasColumnName("is_active");
                b.Property(x => x.UsageCount).HasColumnName("usage_count");
                b.Property(x => x.LastUsedAt).HasColumnName("last_used_at");
                b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
                b.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");
                b.HasOne<Project>().WithMany().HasForeignKey(kb => kb.ProjectId).OnDelete(DeleteBehavior.Cascade);
                b.HasIndex(x => x.ProjectId).HasDatabaseName("idx_kb_project");
                b.HasIndex(x => x.Category).HasDatabaseName("idx_kb_category");
            });

            // Conversation
            modelBuilder.Entity<Conversation>(b =>
            {
                b.ToTable("conversations");
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
                b.Property(x => x.ProjectId).HasColumnName("project_id");
                b.Property(x => x.CustomerPhone).HasColumnName("customer_phone");
                b.Property(x => x.CustomerName).HasColumnName("customer_name");
                b.Property(x => x.CustomerMessage).HasColumnName("customer_message");
                b.Property(x => x.BotResponse).HasColumnName("bot_response");
                b.Property(x => x.BotType).HasColumnName("bot_type");
                b.Property(x => x.Intent).HasColumnName("intent");
                b.Property(x => x.IntentConfidence).HasColumnName("intent_confidence");
                b.Property(x => x.Sentiment).HasColumnName("sentiment");
                b.Property(x => x.RequiresHumanAttention).HasColumnName("requires_human_attention");
                b.Property(x => x.HandledByHuman).HasColumnName("handled_by_human");
                b.Property(x => x.HandledByEmployeeId).HasColumnName("handled_by_employee_id");
                b.Property(x => x.MetadataJson).HasColumnName("metadata_json").HasColumnType("jsonb");
                b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
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
                b.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
                b.Property(x => x.ProjectId).HasColumnName("project_id");
                b.Property(x => x.AppointmentId).HasColumnName("appointment_id");
                b.Property(x => x.CustomerPhone).HasColumnName("customer_phone");
                b.Property(x => x.CustomerName).HasColumnName("customer_name");
                b.Property(x => x.Rating).HasColumnName("rating");
                b.Property(x => x.Comment).HasColumnName("comment");
                b.Property(x => x.Sentiment).HasColumnName("sentiment");
                b.Property(x => x.OwnerNotified).HasColumnName("owner_notified");
                b.Property(x => x.OwnerResponse).HasColumnName("owner_response");
                b.Property(x => x.OwnerRespondedAt).HasColumnName("owner_responded_at");
                b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
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
                b.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
                b.Property(x => x.ProjectId).HasColumnName("project_id");
                b.Property(x => x.CustomerPhone).HasColumnName("customer_phone");
                b.Property(x => x.CustomerName).HasColumnName("customer_name");
                b.Property(x => x.LastVisitDate).HasColumnName("last_visit_date");
                b.Property(x => x.DaysInactive).HasColumnName("days_inactive");
                b.Property(x => x.AttemptNumber).HasColumnName("attempt_number");
                b.Property(x => x.MessageSent).HasColumnName("message_sent");
                b.Property(x => x.CustomerResponded).HasColumnName("customer_responded");
                b.Property(x => x.CustomerResponse).HasColumnName("customer_response");
                b.Property(x => x.Reactivated).HasColumnName("reactivated");
                b.Property(x => x.NewAppointmentId).HasColumnName("new_appointment_id");
                b.Property(x => x.DiscountOffered).HasColumnName("discount_offered");
                b.Property(x => x.DiscountPercentage).HasColumnName("discount_percentage");
                b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
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
                b.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
                b.Property(x => x.ProjectId).HasColumnName("project_id");
                b.Property(x => x.Date).HasColumnName("date");
                b.Property(x => x.TotalConversations).HasColumnName("total_conversations");
                b.Property(x => x.BotConversations).HasColumnName("bot_conversations");
                b.Property(x => x.HumanConversations).HasColumnName("human_conversations");
                b.Property(x => x.AppointmentsBooked).HasColumnName("appointments_booked");
                b.Property(x => x.ConversionRate).HasColumnName("conversion_rate");
                b.Property(x => x.AverageFeedbackRating).HasColumnName("average_feedback_rating");
                b.Property(x => x.CustomersReactivated).HasColumnName("customers_reactivated");
                b.Property(x => x.ReactivationRate).HasColumnName("reactivation_rate");
                b.Property(x => x.AverageResponseTime).HasColumnName("average_response_time");
                b.Property(x => x.CustomerSatisfactionScore).HasColumnName("customer_satisfaction_score");
                b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
                b.HasOne<Project>().WithMany().HasForeignKey(m => m.ProjectId).OnDelete(DeleteBehavior.Cascade);
                b.HasIndex(x => new { x.ProjectId, x.Date }).IsUnique().HasDatabaseName("idx_metrics_project_date");
            });

            // ProjectPhotos
            modelBuilder.Entity<ProjectPhoto>(b =>
            {
                b.ToTable("project_photos");
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
                b.Property(x => x.ProjectId).HasColumnName("project_id");
                b.Property(x => x.Url).HasColumnName("url").IsRequired();
                b.Property(x => x.PublicId).HasColumnName("public_id").IsRequired();
                b.Property(x => x.IsMain).HasColumnName("is_main").HasDefaultValue(false);
                b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
                
                b.HasOne<Project>().WithMany().HasForeignKey(p => p.ProjectId).OnDelete(DeleteBehavior.Cascade);
                b.HasIndex(x => x.ProjectId).HasDatabaseName("idx_project_photos_project");
            });

            // Services
            modelBuilder.Entity<Service>(b =>
            {
                b.ToTable("services");
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).HasColumnName("id");
                b.Property(x => x.ProjectId).HasColumnName("project_id");
                b.Property(x => x.Name).HasColumnName("name").HasMaxLength(100);
                b.Property(x => x.Description).HasColumnName("description");
                b.Property(x => x.Price).HasColumnName("price").HasPrecision(15, 2);
                b.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3);
                b.Property(x => x.DurationMinutes).HasColumnName("duration_minutes");
                b.Property(x => x.IsActive).HasColumnName("is_active");
                b.Property(x => x.CreatedAt).HasColumnName("created_at");
                b.Property(x => x.UpdatedAt).HasColumnName("updated_at");
                b.HasOne<Project>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
            });

            // AppUsers
            modelBuilder.Entity<AppUser>(b =>
            {
                b.ToTable("app_users");
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
                b.Property(x => x.Email).HasColumnName("email").HasMaxLength(150).IsRequired();
                b.Property(x => x.PasswordHash).HasColumnName("password_hash").HasMaxLength(255);
                b.Property(x => x.FullName).HasColumnName("full_name").HasMaxLength(150).IsRequired();
                b.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(50);
                b.Property(x => x.IsEmailVerified).HasColumnName("is_email_verified").HasDefaultValue(false);
                b.Property(x => x.IsPhoneVerified).HasColumnName("is_phone_verified").HasDefaultValue(false);
                b.Property(x => x.AuthProvider).HasColumnName("auth_provider").HasMaxLength(50).HasDefaultValue("email");
                b.Property(x => x.ExternalProviderId).HasColumnName("external_provider_id").HasMaxLength(255);
                b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
                b.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
                b.HasIndex(x => x.Email).IsUnique().HasDatabaseName("app_users_email_key");
                
                // Two-Factor Authentication
                b.Property(x => x.TwoFactorEnabled).HasColumnName("two_factor_enabled").HasDefaultValue(false);
                b.Property(x => x.TwoFactorSecret).HasColumnName("two_factor_secret").HasMaxLength(255);
            });

            // TwoFactorBackupCodes
            modelBuilder.Entity<TwoFactorBackupCode>(b =>
            {
                b.ToTable("two_factor_backup_codes");
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
                b.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
                b.Property(x => x.UserType).HasColumnName("user_type").HasMaxLength(50).IsRequired();
                b.Property(x => x.Code).HasColumnName("code").HasMaxLength(20).IsRequired();
                b.Property(x => x.IsUsed).HasColumnName("is_used").HasDefaultValue(false);
                b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
                b.Property(x => x.UsedAt).HasColumnName("used_at");
                
                b.HasIndex(x => new { x.UserId, x.UserType }).HasDatabaseName("idx_backup_codes_user");
                b.HasIndex(x => x.Code).HasDatabaseName("idx_backup_codes_code");
            });

            // AppUserPasswordResetToken
            modelBuilder.Entity<AppUserPasswordResetToken>(b =>
            {
                b.ToTable("app_user_password_reset_tokens");
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
                b.Property(x => x.AppUserId).HasColumnName("app_user_id");
                b.Property(x => x.Token).HasColumnName("token").HasMaxLength(512);
                b.Property(x => x.ExpiresAt).HasColumnName("expires_at");
                b.Property(x => x.IsUsed).HasColumnName("is_used").HasDefaultValue(false);
                b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");

                b.HasOne<AppUser>().WithMany().HasForeignKey(x => x.AppUserId).OnDelete(DeleteBehavior.Cascade);
                b.HasIndex(x => x.AppUserId).HasDatabaseName("idx_appuser_passreset_user");
                b.HasIndex(x => x.Token).IsUnique().HasDatabaseName("app_user_password_reset_tokens_token_key");
            });
        }
    }
}