using System;
using System.Collections.Generic;

namespace Group6_PRN222_Project.Models;

public partial class DiscountPolicy
{
    public int PolicyId { get; set; }

    public int? EventId { get; set; }

    public string? PolicyName { get; set; }

    public decimal? DiscountPercent { get; set; }

    public string? Status { get; set; }

    public virtual Event? Event { get; set; }
}
