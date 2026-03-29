using System;
using System.Collections.Generic;

namespace Group6_PRN222_Project.Models;

public partial class Vendor
{
    public int VendorId { get; set; }

    public string VendorName { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<OutsourceRental> OutsourceRentals { get; set; } = new List<OutsourceRental>();
}
