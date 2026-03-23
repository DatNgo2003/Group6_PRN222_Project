using System;
using System.Collections.Generic;

namespace Group6_PRN222_Project.Models;

public partial class Participant
{
    public int ParticipantId { get; set; }

    public string? FullName { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public bool? IsVip { get; set; }

    public bool? IsBlacklisted { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<SurveyResponse> SurveyResponses { get; set; } = new List<SurveyResponse>();

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
