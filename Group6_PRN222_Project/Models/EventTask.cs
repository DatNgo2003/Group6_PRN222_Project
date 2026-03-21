using System;
using System.Collections.Generic;

namespace Group6_PRN222_Project.Models;

public partial class EventTask
{
    public int TaskId { get; set; }

    public int? EventId { get; set; }

    public string? TaskName { get; set; }

    public string? Description { get; set; }

    public DateTime? Deadline { get; set; }

    public string? Status { get; set; }

    public int? AssignedTo { get; set; }

    public virtual User? AssignedToNavigation { get; set; }

    public virtual Event? Event { get; set; }
}
