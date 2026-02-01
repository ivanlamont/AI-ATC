using System;
using System.Collections.Generic;

namespace AIATC.Domain.Entities;

public class Achievement
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public string? Tier { get; set; }
    public string Criteria { get; set; } = string.Empty;  // JSON string
    public int Points { get; set; }

    // Navigation properties
    public ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();
}
