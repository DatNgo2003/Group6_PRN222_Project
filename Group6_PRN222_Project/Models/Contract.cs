using System;
using System.Collections.Generic;

namespace Group6_PRN222_Project.Models;

public partial class Contract
{
    public int ContractId { get; set; }

    public int? EventId { get; set; }

    public string ContractNumber { get; set; } = null!;

    public string VendorName { get; set; } = null!;

    public decimal? ContractValue { get; set; }

    public DateOnly? SignDate { get; set; }

    public string? FileUrl { get; set; }

    public string? Status { get; set; }

    public virtual Event? Event { get; set; }
}
