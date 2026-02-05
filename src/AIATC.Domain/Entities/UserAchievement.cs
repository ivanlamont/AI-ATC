using System;

namespace AIATC.Domain.Entities;

public class UserAchievement
{
    public Guid UserId { get; set; }
    public Guid AchievementId { get; set; }
    public DateTime EarnedAt { get; set; }
    public string? Progress { get; set; }  // JSON string

    // Navigation properties
    public User User { get; set; } = null!;
    public Achievement Achievement { get; set; } = null!;
}
