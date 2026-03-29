using System;
using System.Collections.Generic;

namespace Group6_PRN222_Project.Models;

public partial class FieldReport
{
    public int ReportId { get; set; }

    public int? EventId { get; set; }

    public int? StaffId { get; set; }

    public string? ReportType { get; set; }

    public string? Content { get; set; }

    public decimal? EstimatePrice { get; set; }

    public DateTime? ReportTime { get; set; }

    public string? Status { get; set; }

    public virtual Event? Event { get; set; }

    public virtual User? Staff { get; set; }
}
