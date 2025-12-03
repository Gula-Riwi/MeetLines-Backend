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
                b.HasOne<SaasUser>().WithMany().HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
                b.HasIndex(x => x.UserId).HasDatabaseName("idx_projects_user");
                b.HasIndex(x => x.Subdomain).IsUnique().HasDatabaseName("idx_projects_subdomain");
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
                b.Property(x => x.Id).HasDefaultValueSql("uuid_generate_v4()");
                b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
                b.Property(x => x.UpdatedAt).HasDefaultValueSql("now()");
                b.HasOne<Project>().WithMany().HasForeignKey(a => a.ProjectId).OnDelete(DeleteBehavior.Cascade);
                b.HasIndex(x => x.ProjectId).HasDatabaseName("idx_appointments_project");
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
                b.HasOne<SaasUser>().WithMany().HasForeignKey(s => s.UserId).OnDelete(DeleteBehavior.Cascade);
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
        }
    }
}