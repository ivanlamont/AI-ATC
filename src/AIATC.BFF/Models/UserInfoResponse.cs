namespace AIATC.BFF.Models;

public class UserInfoResponse
{
    public string Provider { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Email { get; set; }
}
