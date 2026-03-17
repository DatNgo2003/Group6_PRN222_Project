using System;
using System.Collections.Generic;

namespace Group6_PRN222_Project.Models;

public partial class SystemAuditLog
{
    public int LogId { get; set; }

    public int? UserId { get; set; }

    public string? Action { get; set; }

    public string? TableName { get; set; }

    public DateTime? ActionTime { get; set; }

    public virtual User? User { get; set; }
}
