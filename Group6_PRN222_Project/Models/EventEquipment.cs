using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Group6_PRN222_Project.Models;

public partial class EventEquipment
{
    public int EventId { get; set; }

    public int EquipmentId { get; set; }

    public int RequestedQuantity { get; set; }

    public int ApprovedQuantity { get; set; }

    public int ExportedQuantity { get; set; }

    [NotMapped]
    public int Quantity => RequestedQuantity;

    public string? Status { get; set; }

    public string? Note { get; set; }

    public virtual Equipment Equipment { get; set; } = null!;

    public virtual Event Event { get; set; } = null!;
}
