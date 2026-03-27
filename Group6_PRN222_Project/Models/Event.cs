using System;
using System.Collections.Generic;

namespace Group6_PRN222_Project.Models;

public partial class Event
{
    public int EventId { get; set; }

    public string EventName { get; set; } = null!;

    public string? Concept { get; set; }

    public string? Location { get; set; }

    public string? DeploymentMapUrl { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public TimeOnly? EventTime { get; set; }

    public decimal? Amount { get; set; }

    public string? Images { get; set; }

    public string? Status { get; set; }

    public string? Scale { get; set; }

    public int? Capacity { get; set; }

    public int? OrganizerId { get; set; }

    public virtual ICollection<Budget> Budgets { get; set; } = new List<Budget>();

    public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();

    public virtual ICollection<DiscountPolicy> DiscountPolicies { get; set; } = new List<DiscountPolicy>();

    public virtual ICollection<EventEquipment> EventEquipments { get; set; } = new List<EventEquipment>();

    public virtual ICollection<FieldReport> FieldReports { get; set; } = new List<FieldReport>();

    public virtual ICollection<MarketingCampaign> MarketingCampaigns { get; set; } = new List<MarketingCampaign>();

    public virtual User? Organizer { get; set; }

    public virtual ICollection<SurveyResponse> SurveyResponses { get; set; } = new List<SurveyResponse>();

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
