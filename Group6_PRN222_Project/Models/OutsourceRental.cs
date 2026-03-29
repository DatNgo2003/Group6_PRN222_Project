using System;
using System.Collections.Generic;

namespace Group6_PRN222_Project.Models;

public partial class OutsourceRental
{
    public int RentalId { get; set; }

    public int? EventId { get; set; }

    public int VendorId { get; set; }

    public int EquipmentId { get; set; }

    public int RentQuantity { get; set; }

    public DateTime ExpectedReturnDate { get; set; }

    public string? Status { get; set; }

    public virtual Equipment Equipment { get; set; } = null!;

    public virtual Event? Event { get; set; }

    public virtual Vendor Vendor { get; set; } = null!;
}
