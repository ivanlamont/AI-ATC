using System;
using System.Collections.Generic;

namespace AIATC.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string OAuthProvider { get; set; } = string.Empty;
    public string OAuthSubjectId { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? AvatarUrl { get; set; }
    public string Role { get; set; } = "user";
    public bool IsGuest { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string? Settings { get; set; }  // JSON string

    // Navigation properties
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
    public ICollection<Score> Scores { get; set; } = new List<Score>();
    public ICollection<SavedScenario> SavedScenarios { get; set; } = new List<SavedScenario>();
    public ICollection<Scenario> CreatedScenarios { get; set; } = new List<Scenario>();
    public ICollection<UserAchievement> Achievements { get; set; } = new List<UserAchievement>();
}
