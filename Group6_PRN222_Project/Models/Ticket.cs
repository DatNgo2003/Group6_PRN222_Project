using System;
using System.Collections.Generic;

namespace Group6_PRN222_Project.Models;

public partial class Ticket
{
    public int TicketId { get; set; }

    public int? EventId { get; set; }

    public int? ParticipantId { get; set; }

    public Guid? Qrcode { get; set; }

    public string? PaymentStatus { get; set; }

    public DateTime? CheckInTime { get; set; }

    public virtual Event? Event { get; set; }

    public virtual Participant? Participant { get; set; }
}
