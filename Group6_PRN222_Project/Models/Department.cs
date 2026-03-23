using System;
using System.Collections.Generic;

namespace Group6_PRN222_Project.Models;

public partial class Department
{
    public int DepartmentId { get; set; }

    public string DepartmentName { get; set; } = null!;

    public string? Status { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
