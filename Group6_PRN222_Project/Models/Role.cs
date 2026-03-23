using System;
using System.Collections.Generic;

namespace Group6_PRN222_Project.Models;

public partial class Role
{
    public int RoleId { get; set; }

    public string RoleName { get; set; } = null!;

    public string? Status { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
