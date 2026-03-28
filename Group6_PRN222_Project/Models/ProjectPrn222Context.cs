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

    public virtual DbSet<OutsourceRental> OutsourceRentals { get; set; }

    public virtual DbSet<Participant> Participants { get; set; }

    public virtual DbSet<ReturnLog> ReturnLogs { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<SurveyResponse> SurveyResponses { get; set; }

    public virtual DbSet<SystemAuditLog> SystemAuditLogs { get; set; }

    public virtual DbSet<Task> Tasks { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Vendor> Vendors { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("server=DATDEPTRAIKHOAI\\Dat; database=Project_PRN222;uid=sa;pwd=1234;TrustServerCertificate=true");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Budget>(entity =>
        {
            entity.HasKey(e => e.BudgetId).HasName("PK__Budgets__E38E79C47054A9E3");

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
                .HasConstraintName("FK__Budgets__Approve__0E6E26BF");

            entity.HasOne(d => d.Event).WithMany(p => p.Budgets)
                .HasForeignKey(d => d.EventId)
                .HasConstraintName("FK__Budgets__EventID__0F624AF8");
        });

        modelBuilder.Entity<Contract>(entity =>
        {
            entity.HasKey(e => e.ContractId).HasName("PK__Contract__C90D34094A6CE5FD");

            entity.HasIndex(e => e.ContractNumber, "UQ__Contract__C51D43DAAE10F256").IsUnique();

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
                .HasConstraintName("FK__Contracts__Event__10566F31");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.DepartmentId).HasName("PK__Departme__B2079BCDEBB49F56");

            entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");
            entity.Property(e => e.DepartmentName).HasMaxLength(100);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Active");
        });

        modelBuilder.Entity<DiscountPolicy>(entity =>
        {
            entity.HasKey(e => e.PolicyId).HasName("PK__Discount__2E133944C3D3EF1D");

            entity.Property(e => e.PolicyId).HasColumnName("PolicyID");
            entity.Property(e => e.DiscountPercent).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.EventId).HasColumnName("EventID");
            entity.Property(e => e.PolicyName).HasMaxLength(100);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Active");

            entity.HasOne(d => d.Event).WithMany(p => p.DiscountPolicies)
                .HasForeignKey(d => d.EventId)
                .HasConstraintName("FK__DiscountP__Event__114A936A");
        });

        modelBuilder.Entity<Equipment>(entity =>
        {
            entity.HasKey(e => e.EquipmentId).HasName("PK__Equipmen__34474599621CC62E");

            entity.Property(e => e.EquipmentId).HasColumnName("EquipmentID");
            entity.Property(e => e.EquipmentName).HasMaxLength(255);
            entity.Property(e => e.UnitPrice)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.EventId).HasName("PK__Events__7944C8708419F94E");

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
                .HasConstraintName("FK__Events__Organize__14270015");
        });

        modelBuilder.Entity<EventEquipment>(entity =>
        {
            entity.HasKey(e => new { e.EventId, e.EquipmentId }).HasName("PK__EventEqu__FA00BC292DCB644E");

            entity.Property(e => e.EventId).HasColumnName("EventID");
            entity.Property(e => e.EquipmentId).HasColumnName("EquipmentID");
            entity.Property(e => e.Note).HasMaxLength(255);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Pending");

            entity.HasOne(d => d.Equipment).WithMany(p => p.EventEquipments)
                .HasForeignKey(d => d.EquipmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__EventEqui__Equip__123EB7A3");

            entity.HasOne(d => d.Event).WithMany(p => p.EventEquipments)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__EventEqui__Event__1332DBDC");
        });

        modelBuilder.Entity<FieldReport>(entity =>
        {
            entity.HasKey(e => e.ReportId).HasName("PK__FieldRep__D5BD48E589B5CF59");

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
                .HasConstraintName("FK__FieldRepo__Event__151B244E");

            entity.HasOne(d => d.Staff).WithMany(p => p.FieldReports)
                .HasForeignKey(d => d.StaffId)
                .HasConstraintName("FK__FieldRepo__Staff__160F4887");
        });

        modelBuilder.Entity<MarketingCampaign>(entity =>
        {
            entity.HasKey(e => e.CampaignId).HasName("PK__Marketin__3F5E8D79D3E55B9E");

            entity.Property(e => e.CampaignId).HasColumnName("CampaignID");
            entity.Property(e => e.AdSpend).HasColumnType("money");
            entity.Property(e => e.EngagementRate).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.EventId).HasColumnName("EventID");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Running");

            entity.HasOne(d => d.Event).WithMany(p => p.MarketingCampaigns)
                .HasForeignKey(d => d.EventId)
                .HasConstraintName("FK__Marketing__Event__17036CC0");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__20CF2E326161F464");

            entity.Property(e => e.NotificationId).HasColumnName("NotificationID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsRead).HasDefaultValue(false);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Notificat__UserI__208CD6FA");
        });

        modelBuilder.Entity<OutsourceRental>(entity =>
        {
            entity.HasKey(e => e.RentalId).HasName("PK__Outsourc__97005963C2C184D2");

            entity.Property(e => e.RentalId).HasColumnName("RentalID");
            entity.Property(e => e.EquipmentId).HasColumnName("EquipmentID");
            entity.Property(e => e.EventId).HasColumnName("EventID");
            entity.Property(e => e.ExpectedReturnDate).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Pending");
            entity.Property(e => e.VendorId).HasColumnName("VendorID");

            entity.HasOne(d => d.Equipment).WithMany(p => p.OutsourceRentals)
                .HasForeignKey(d => d.EquipmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Outsource_Equipments");

            entity.HasOne(d => d.Event).WithMany(p => p.OutsourceRentals)
                .HasForeignKey(d => d.EventId)
                .HasConstraintName("FK_Outsource_Events");

            entity.HasOne(d => d.Vendor).WithMany(p => p.OutsourceRentals)
                .HasForeignKey(d => d.VendorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Outsource_Vendors");
        });

        modelBuilder.Entity<Participant>(entity =>
        {
            entity.HasKey(e => e.ParticipantId).HasName("PK__Particip__7227997E6385ECF8");

            entity.HasIndex(e => e.Email, "UQ__Particip__A9D1053471A61015").IsUnique();

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

        modelBuilder.Entity<ReturnLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__ReturnLo__5E5499A88F6A9758");

            entity.Property(e => e.LogId).HasColumnName("LogID");
            entity.Property(e => e.CheckDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EquipmentId).HasColumnName("EquipmentID");
            entity.Property(e => e.EventId).HasColumnName("EventID");
            entity.Property(e => e.Note).HasMaxLength(255);

            entity.HasOne(d => d.Equipment).WithMany(p => p.ReturnLogs)
                .HasForeignKey(d => d.EquipmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Return_Equipments");

            entity.HasOne(d => d.Event).WithMany(p => p.ReturnLogs)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Return_Events");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE3AEDA5CF38");

            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.RoleName).HasMaxLength(50);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Active");
        });

        modelBuilder.Entity<SurveyResponse>(entity =>
        {
            entity.HasKey(e => e.ResponseId).HasName("PK__SurveyRe__1AAA640C9194FF2F");

            entity.Property(e => e.ResponseId).HasColumnName("ResponseID");
            entity.Property(e => e.EventId).HasColumnName("EventID");
            entity.Property(e => e.ParticipantId).HasColumnName("ParticipantID");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Completed");

            entity.HasOne(d => d.Event).WithMany(p => p.SurveyResponses)
                .HasForeignKey(d => d.EventId)
                .HasConstraintName("FK__SurveyRes__Event__17F790F9");

            entity.HasOne(d => d.Participant).WithMany(p => p.SurveyResponses)
                .HasForeignKey(d => d.ParticipantId)
                .HasConstraintName("FK__SurveyRes__Parti__18EBB532");
        });

        modelBuilder.Entity<SystemAuditLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__SystemAu__5E5499A87FC8A69B");

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
                .HasConstraintName("FK__SystemAud__UserI__19DFD96B");
        });

        modelBuilder.Entity<Task>(entity =>
        {
            entity.HasKey(e => e.TaskId).HasName("PK__Tasks__7C6949D1CD0186B5");

            entity.Property(e => e.TaskId).HasColumnName("TaskID");
            entity.Property(e => e.Deadline).HasColumnType("datetime");
            entity.Property(e => e.EventId).HasColumnName("EventID");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("To Do");
            entity.Property(e => e.TaskName).HasMaxLength(255);

            entity.HasOne(d => d.AssignedToNavigation).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.AssignedTo)
                .HasConstraintName("FK__Tasks__AssignedT__1AD3FDA4");

            entity.HasOne(d => d.Event).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.EventId)
                .HasConstraintName("FK__Tasks__EventID__1BC821DD");
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.TicketId).HasName("PK__Tickets__712CC6274EED07D4");

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
                .HasConstraintName("FK__Tickets__EventID__1CBC4616");

            entity.HasOne(d => d.Participant).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.ParticipantId)
                .HasConstraintName("FK__Tickets__Partici__1DB06A4F");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CCACBE146AF9");

            entity.HasIndex(e => e.Username, "UQ__Users__536C85E4C4237957").IsUnique();

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
                .HasConstraintName("FK__Users__Departmen__1EA48E88");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK__Users__RoleID__1F98B2C1");
        });

        modelBuilder.Entity<Vendor>(entity =>
        {
            entity.HasKey(e => e.VendorId).HasName("PK__Vendors__FC8618D3B81B3D84");

            entity.Property(e => e.VendorId).HasColumnName("VendorID");
            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.VendorName).HasMaxLength(255);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

