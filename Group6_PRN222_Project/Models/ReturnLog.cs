using System;
using System.Collections.Generic;

namespace Group6_PRN222_Project.Models;

public partial class ReturnLog
{
    public int LogId { get; set; }

    public int EventId { get; set; }

    public int EquipmentId { get; set; }

    public int ReturnedQuantity { get; set; }

    public int DefectiveQuantity { get; set; }

    public int LostQuantity { get; set; }

    public DateTime CheckDate { get; set; }

    public string? Note { get; set; }

    public virtual Equipment Equipment { get; set; } = null!;

    public virtual Event Event { get; set; } = null!;
}
