using System;
using System.Collections.Generic;

namespace Group6_PRN222_Project.Models;

public partial class Equipment
{
    public int EquipmentId { get; set; }

    public string? EquipmentName { get; set; }

    public int? TotalStock { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<EventEquipment> EventEquipments { get; set; } = new List<EventEquipment>();
}
