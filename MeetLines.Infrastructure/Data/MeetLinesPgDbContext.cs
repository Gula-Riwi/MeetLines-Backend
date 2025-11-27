using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
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
        public DbSet<Lead> Leads { get; set; }
        public DbSet<LeadInteraction> LeadInteractions { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Webhook> Webhooks { get; set; }
        public DbSet<EventLog> EventLogs { get; set; }
        public DbSet<Template> Templates { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<LeadTag> LeadTags { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<LoginSession> LoginSessions { get; set; }
        public DbSet<SystemSetting> SystemSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresExtension("uuid-ossp");

            var userStatusConv = new EnumToStringConverter<UserStatus>();
            var subscriptionCycleConv = new EnumToStringConverter<SubscriptionCycle>();
            var subscriptionStatusConv = new EnumToStringConverter<SubscriptionStatus>();
            var projectStatusConv = new EnumToStringConverter<ProjectStatus>();
            var channelTypeConv = new EnumToStringConverter<ChannelType>();
            var leadStageConv = new EnumToStringConverter<LeadStage>();
            var urgencyConv = new EnumToStringConverter<Urgency>();
            var senderTypeConv = new EnumToStringConverter<SenderType>();
            var appointmentStatusConv = new EnumToStringConverter<AppointmentStatus>();
            var templateTypeConv = new EnumToStringConverter<TemplateType>();

            // SaasUsers
            modelBuilder.Entity<SaasUser>(b =>
            {
                b.ToTable("saas_users");
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).HasDefaultValueSql("uuid_generate_v4()");
                b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
                b.Property(x => x.UpdatedAt).HasDefaultValueSql("now()");
                b.HasIndex(x => x.Email).IsUnique();
                b.Property(x => x.Status).HasConversion(userStatusConv).HasDefaultValue(UserStatus.Active);
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
                b.Property(x => x.Cycle).HasConversion(subscriptionCycleConv);
                b.Property(x => x.Status).HasConversion(subscriptionStatusConv).HasDefaultValue(SubscriptionStatus.Active);
            });

            // Projects
            modelBuilder.Entity<Project>(b =>
            {
                b.ToTable("projects");
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).HasDefaultValueSql("uuid_generate_v4()");
                b.Property(x => x.WorkingHours).HasColumnType("jsonb");
                b.Property(x => x.Config).HasColumnType("jsonb");
                b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
                b.Property(x => x.UpdatedAt).HasDefaultValueSql("now()");
                b.HasOne<SaasUser>().WithMany().HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
                b.HasIndex(x => x.UserId).HasDatabaseName("idx_projects_user");
                b.Property(x => x.Status).HasConversion(projectStatusConv).HasDefaultValue(ProjectStatus.Active);
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
                b.Property(x => x.Type).HasConversion(channelTypeConv);
            });

            // Leads
            modelBuilder.Entity<Lead>(b =>
            {
                b.ToTable("leads");
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).HasDefaultValueSql("uuid_generate_v4()");
                b.HasOne<Project>().WithMany().HasForeignKey(l => l.ProjectId).OnDelete(DeleteBehavior.Cascade);
                b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
                b.Property(x => x.UpdatedAt).HasDefaultValueSql("now()");
                b.HasIndex(x => x.ProjectId).HasDatabaseName("idx_leads_project");
                b.HasIndex(x => x.Phone).HasDatabaseName("idx_leads_phone");
                b.Property(x => x.Stage).HasConversion(leadStageConv).HasDefaultValue(LeadStage.New);
                b.Property(x => x.Urgency).HasConversion(urgencyConv).HasDefaultValue(Urgency.Low);
            });

            // Lead Interactions
            modelBuilder.Entity<LeadInteraction>(b =>
            {
                b.ToTable("lead_interactions");
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).HasDefaultValueSql("uuid_generate_v4()");
                b.Property(x => x.Metadata).HasColumnType("jsonb");
                b.Property(x => x.Timestamp).HasDefaultValueSql("now()");
                b.HasOne<Lead>().WithMany().HasForeignKey(li => li.LeadId).OnDelete(DeleteBehavior.Cascade);
                b.HasIndex(x => x.LeadId).HasDatabaseName("idx_li_lead");
                b.Property(x => x.Sender).HasConversion(senderTypeConv);
                b.Property(x => x.Channel).HasConversion(channelTypeConv);
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
                b.Property(x => x.Status).HasConversion(appointmentStatusConv).HasDefaultValue(AppointmentStatus.Pending);
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
                b.Property(x => x.Type).HasConversion(templateTypeConv);
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

            // LeadTags
            modelBuilder.Entity<LeadTag>(b =>
            {
                b.ToTable("lead_tags");
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).HasDefaultValueSql("uuid_generate_v4()");
                b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
                b.HasOne<Lead>().WithMany().HasForeignKey(lt => lt.LeadId).OnDelete(DeleteBehavior.Cascade);
                b.HasOne<Tag>().WithMany().HasForeignKey(lt => lt.TagId).OnDelete(DeleteBehavior.Cascade);
                b.HasIndex(x => x.LeadId).HasDatabaseName("idx_leadtags_lead");
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
        }
    }
}
