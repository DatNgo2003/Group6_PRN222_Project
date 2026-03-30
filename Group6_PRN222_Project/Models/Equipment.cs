using System;
using System.Collections.Generic;

namespace Group6_PRN222_Project.Models;

public partial class Equipment
{
    public int EquipmentId { get; set; }

    public string EquipmentName { get; set; } = null!;

    public decimal? UnitPrice { get; set; }

    public int TotalOwned { get; set; }

    public int AvailableQuantity { get; set; }

    public int DefectiveQuantity { get; set; }

    public int OutsourcedQuantity { get; set; }

    public virtual ICollection<EventEquipment> EventEquipments { get; set; } = new List<EventEquipment>();

    public virtual ICollection<OutsourceRental> OutsourceRentals { get; set; } = new List<OutsourceRental>();

    public virtual ICollection<ReturnLog> ReturnLogs { get; set; } = new List<ReturnLog>();
}
