using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Group6_PRN222_Project.Models;

public partial class ProjectPrn222Context : DbContext
{
    public ProjectPrn222Context()
    {
    }

    public ProjectPrn222Context(DbContextOptions<ProjectPrn222Context> options)
        : base(options)
    {
    }

    public virtual DbSet<Budget> Budgets { get; set; }

    public virtual DbSet<Contract> Contracts { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<DiscountPolicy> DiscountPolicies { get; set; }

    public virtual DbSet<Equipment> Equipments { get; set; }

    public virtual DbSet<Event> Events { get; set; }

    public virtual DbSet<EventEquipment> EventEquipments { get; set; }

    public virtual DbSet<FieldReport> FieldReports { get; set; }

    public virtual DbSet<MarketingCampaign> MarketingCampaigns { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Participant> Participants { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<SurveyResponse> SurveyResponses { get; set; }

    public virtual DbSet<SystemAuditLog> SystemAuditLogs { get; set; }

    public virtual DbSet<Task> Tasks { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Budget>(entity =>
        {
            entity.HasKey(e => e.BudgetId).HasName("PK__Budgets__E38E79C41E0C0C6A");

            entity.Property(e => e.BudgetId).HasColumnName("BudgetID");
            entity.Property(e => e.ApprovalStatus)
                .HasMaxLength(50)
                .HasDefaultValue("Pending");
            entity.Property(e => e.EventId).HasColumnName("EventID");
            entity.Property(e => e.Profit).HasColumnType("money");
            entity.Property(e => e.SpentAmount)
                .HasDefaultValue(0m)
                .HasColumnType("money");
            entity.Property(e => e.TotalAllocated).HasColumnType("money");

            entity.HasOne(d => d.ApprovedByNavigation).WithMany(p => p.Budgets)
                .HasForeignKey(d => d.ApprovedBy)
                .HasConstraintName("FK__Budgets__Approve__778AC167");

            entity.HasOne(d => d.Event).WithMany(p => p.Budgets)
                .HasForeignKey(d => d.EventId)
                .HasConstraintName("FK__Budgets__EventID__787EE5A0");
        });

        modelBuilder.Entity<Contract>(entity =>
        {
            entity.HasKey(e => e.ContractId).HasName("PK__Contract__C90D3409B9FE3723");

            entity.HasIndex(e => e.ContractNumber, "UQ__Contract__C51D43DA4ABC3628").IsUnique();

            entity.Property(e => e.ContractId).HasColumnName("ContractID");
            entity.Property(e => e.ContractNumber)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ContractValue).HasColumnType("money");
            entity.Property(e => e.EventId).HasColumnName("EventID");
            entity.Property(e => e.FileUrl).HasMaxLength(500);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Active");
            entity.Property(e => e.VendorName).HasMaxLength(255);

            entity.HasOne(d => d.Event).WithMany(p => p.Contracts)
                .HasForeignKey(d => d.EventId)
                .HasConstraintName("FK__Contracts__Event__797309D9");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.DepartmentId).HasName("PK__Departme__B2079BCD7889FEF9");

            entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");
            entity.Property(e => e.DepartmentName).HasMaxLength(100);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Active");
        });

        modelBuilder.Entity<DiscountPolicy>(entity =>
        {
            entity.HasKey(e => e.PolicyId).HasName("PK__Discount__2E133944D372D064");

            entity.Property(e => e.PolicyId).HasColumnName("PolicyID");
            entity.Property(e => e.DiscountPercent).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.EventId).HasColumnName("EventID");
            entity.Property(e => e.PolicyName).HasMaxLength(100);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Active");

            entity.HasOne(d => d.Event).WithMany(p => p.DiscountPolicies)
                .HasForeignKey(d => d.EventId)
                .HasConstraintName("FK__DiscountP__Event__7A672E12");
        });

        modelBuilder.Entity<Equipment>(entity =>
        {
            entity.HasKey(e => e.EquipmentId).HasName("PK__Equipmen__34474599FABAEB0E");

            entity.Property(e => e.EquipmentId).HasColumnName("EquipmentID");
            entity.Property(e => e.EquipmentName).HasMaxLength(200);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Available");
            entity.Property(e => e.UnitPrice)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.EventId).HasName("PK__Events__7944C870E95F672B");

            entity.Property(e => e.EventId).HasColumnName("EventID");
            entity.Property(e => e.Amount).HasColumnType("money");
            entity.Property(e => e.DeploymentMapUrl)
                .HasMaxLength(500)
                .HasColumnName("DeploymentMapURL");
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.EventName).HasMaxLength(255);
            entity.Property(e => e.Location).HasMaxLength(255);
            entity.Property(e => e.OrganizerId).HasColumnName("OrganizerID");
            entity.Property(e => e.Scale).HasMaxLength(20);
            entity.Property(e => e.StartDate).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Planning");

            entity.HasOne(d => d.Organizer).WithMany(p => p.Events)
                .HasForeignKey(d => d.OrganizerId)
                .HasConstraintName("FK__Events__Organize__7D439ABD");
        });

        modelBuilder.Entity<EventEquipment>(entity =>
        {
            entity.HasKey(e => new { e.EventId, e.EquipmentId }).HasName("PK__EventEqu__FA00BC290181549F");

            entity.Property(e => e.EventId).HasColumnName("EventID");
            entity.Property(e => e.EquipmentId).HasColumnName("EquipmentID");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Requested");

            entity.HasOne(d => d.Equipment).WithMany(p => p.EventEquipments)
                .HasForeignKey(d => d.EquipmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__EventEqui__Equip__7B5B524B");

            entity.HasOne(d => d.Event).WithMany(p => p.EventEquipments)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__EventEqui__Event__7C4F7684");
        });

        modelBuilder.Entity<FieldReport>(entity =>
        {
            entity.HasKey(e => e.ReportId).HasName("PK__FieldRep__D5BD48E5DA8EF374");

            entity.Property(e => e.ReportId).HasColumnName("ReportID");
            entity.Property(e => e.EventId).HasColumnName("EventID");
            entity.Property(e => e.ReportTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ReportType).HasMaxLength(50);
            entity.Property(e => e.StaffId).HasColumnName("StaffID");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Submitted");

            entity.HasOne(d => d.Event).WithMany(p => p.FieldReports)
                .HasForeignKey(d => d.EventId)
                .HasConstraintName("FK__FieldRepo__Event__7E37BEF6");

            entity.HasOne(d => d.Staff).WithMany(p => p.FieldReports)
                .HasForeignKey(d => d.StaffId)
                .HasConstraintName("FK__FieldRepo__Staff__7F2BE32F");
        });

        modelBuilder.Entity<MarketingCampaign>(entity =>
        {
            entity.HasKey(e => e.CampaignId).HasName("PK__Marketin__3F5E8D798295502B");

            entity.Property(e => e.CampaignId).HasColumnName("CampaignID");
            entity.Property(e => e.AdSpend).HasColumnType("money");
            entity.Property(e => e.EngagementRate).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.EventId).HasColumnName("EventID");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Running");

            entity.HasOne(d => d.Event).WithMany(p => p.MarketingCampaigns)
                .HasForeignKey(d => d.EventId)
                .HasConstraintName("FK__Marketing__Event__00200768");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__20CF2E32AF3F9006");

            entity.Property(e => e.NotificationId).HasColumnName("NotificationID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsRead).HasDefaultValue(false);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Notificat__UserI__09A971A2");
        });

        modelBuilder.Entity<Participant>(entity =>
        {
            entity.HasKey(e => e.ParticipantId).HasName("PK__Particip__7227997E09346239");

            entity.HasIndex(e => e.Email, "UQ__Particip__A9D10534C49CB1C6").IsUnique();

            entity.Property(e => e.ParticipantId).HasColumnName("ParticipantID");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.IsBlacklisted).HasDefaultValue(false);
            entity.Property(e => e.IsVip)
                .HasDefaultValue(false)
                .HasColumnName("IsVIP");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Active");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.Participants)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Participants_Users");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE3A39F13C83");

            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.RoleName).HasMaxLength(50);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Active");
        });

        modelBuilder.Entity<SurveyResponse>(entity =>
        {
            entity.HasKey(e => e.ResponseId).HasName("PK__SurveyRe__1AAA640CF2EB03D9");

            entity.Property(e => e.ResponseId).HasColumnName("ResponseID");
            entity.Property(e => e.EventId).HasColumnName("EventID");
            entity.Property(e => e.ParticipantId).HasColumnName("ParticipantID");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Completed");

            entity.HasOne(d => d.Event).WithMany(p => p.SurveyResponses)
                .HasForeignKey(d => d.EventId)
                .HasConstraintName("FK__SurveyRes__Event__01142BA1");

            entity.HasOne(d => d.Participant).WithMany(p => p.SurveyResponses)
                .HasForeignKey(d => d.ParticipantId)
                .HasConstraintName("FK__SurveyRes__Parti__02084FDA");
        });

        modelBuilder.Entity<SystemAuditLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__SystemAu__5E5499A8EBA70192");

            entity.Property(e => e.LogId).HasColumnName("LogID");
            entity.Property(e => e.ActionTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Recorded");
            entity.Property(e => e.TableName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.SystemAuditLogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__SystemAud__UserI__02FC7413");
        });

        modelBuilder.Entity<Task>(entity =>
        {
            entity.HasKey(e => e.TaskId).HasName("PK__Tasks__7C6949D1BF116074");

            entity.Property(e => e.TaskId).HasColumnName("TaskID");
            entity.Property(e => e.Deadline).HasColumnType("datetime");
            entity.Property(e => e.EventId).HasColumnName("EventID");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("To Do");
            entity.Property(e => e.TaskName).HasMaxLength(255);

            entity.HasOne(d => d.AssignedToNavigation).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.AssignedTo)
                .HasConstraintName("FK__Tasks__AssignedT__03F0984C");

            entity.HasOne(d => d.Event).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.EventId)
                .HasConstraintName("FK__Tasks__EventID__04E4BC85");
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.TicketId).HasName("PK__Tickets__712CC627C8187CA0");

            entity.Property(e => e.TicketId).HasColumnName("TicketID");
            entity.Property(e => e.CheckInTime).HasColumnType("datetime");
            entity.Property(e => e.EventId).HasColumnName("EventID");
            entity.Property(e => e.ParticipantId).HasColumnName("ParticipantID");
            entity.Property(e => e.PaymentStatus).HasMaxLength(50);
            entity.Property(e => e.Price)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Qrcode)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("QRCode");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Valid");
            entity.Property(e => e.TicketType)
                .HasMaxLength(50)
                .HasDefaultValue("Standard");

            entity.HasOne(d => d.Event).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.EventId)
                .HasConstraintName("FK__Tickets__EventID__05D8E0BE");

            entity.HasOne(d => d.Participant).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.ParticipantId)
                .HasConstraintName("FK__Tickets__Partici__06CD04F7");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CCACE6731C33");

            entity.HasIndex(e => e.Username, "UQ__Users__536C85E44CE2E35E").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Active");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Department).WithMany(p => p.Users)
                .HasForeignKey(d => d.DepartmentId)
                .HasConstraintName("FK__Users__Departmen__07C12930");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK__Users__RoleID__08B54D69");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
