using BalochiAcademy.Domain.Common;

namespace BalochiAcademy.Domain.Entities;

public class SubscriptionPlan : BaseEntity
{
    public string   Name         { get; set; } = string.Empty;
    public string?  Description  { get; set; }
    public decimal  Price        { get; set; }
    public int      DurationDays { get; set; }
    public int      AiTokenQuota { get; set; }   // AI tokens granted per subscription period
    public bool     IsActive     { get; set; } = true;
    public int      SortOrder    { get; set; }
    public ICollection<UserSubscription> UserSubscriptions { get; set; } = [];
}

public class UserSubscription : BaseEntity
{
    public int      UserId                 { get; set; }
    public int      PlanId                 { get; set; }
    public DateTime StartDate              { get; set; }
    public DateTime EndDate                { get; set; }
    public string   Status                 { get; set; } = "active";
    public int      AiTokenQuota           { get; set; }   // snapshot of Plan.AiTokenQuota at grant time
    public int      AiTokensUsedThisPeriod { get; set; }   // resets on each new purchase/renewal
    public string?  PaymentMethod          { get; set; }   // set only for cash purchases pending verification
    public string?  ReferenceNumber        { get; set; }   // student-provided payment reference, cash purchases only
    public User             User { get; set; } = null!;
    public SubscriptionPlan Plan { get; set; } = null!;
}
