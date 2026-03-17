using System;
using System.Collections.Generic;

namespace Group6_PRN222_Project.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? FullName { get; set; }

    public string? Email { get; set; }

    public int? DepartmentId { get; set; }

    public int? RoleId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Budget> Budgets { get; set; } = new List<Budget>();

    public virtual Department? Department { get; set; }

    public virtual ICollection<Event> Events { get; set; } = new List<Event>();

    public virtual ICollection<FieldReport> FieldReports { get; set; } = new List<FieldReport>();

    public virtual Role? Role { get; set; }

    public virtual ICollection<SystemAuditLog> SystemAuditLogs { get; set; } = new List<SystemAuditLog>();

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
}
