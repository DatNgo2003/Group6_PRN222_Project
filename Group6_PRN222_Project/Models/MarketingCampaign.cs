using System;
using System.Collections.Generic;

namespace Group6_PRN222_Project.Models;

public partial class MarketingCampaign
{
    public int CampaignId { get; set; }

    public int? EventId { get; set; }

    public string? AdContent { get; set; }

    public decimal? AdSpend { get; set; }

    public decimal? EngagementRate { get; set; }

    public string? Status { get; set; }

    public virtual Event? Event { get; set; }
}
