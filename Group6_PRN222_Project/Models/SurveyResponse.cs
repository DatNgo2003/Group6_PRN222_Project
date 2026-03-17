using System;
using System.Collections.Generic;

namespace Group6_PRN222_Project.Models;

public partial class SurveyResponse
{
    public int ResponseId { get; set; }

    public int? EventId { get; set; }

    public int? ParticipantId { get; set; }

    public int? Rating { get; set; }

    public string? Comments { get; set; }

    public virtual Event? Event { get; set; }

    public virtual Participant? Participant { get; set; }
}
