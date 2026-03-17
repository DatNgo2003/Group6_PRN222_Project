using System;
using System.Collections.Generic;

namespace Group6_PRN222_Project.Models;

public partial class Budget
{
    public int BudgetId { get; set; }

    public int? EventId { get; set; }

    public decimal? TotalAllocated { get; set; }

    public decimal? SpentAmount { get; set; }

    public string? ApprovalStatus { get; set; }

    public int? ApprovedBy { get; set; }

    public virtual User? ApprovedByNavigation { get; set; }

    public virtual Event? Event { get; set; }
}
