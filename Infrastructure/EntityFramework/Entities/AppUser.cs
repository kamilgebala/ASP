using CoreApp.Enums;
using CoreApp.Users;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.EntityFramework.Entities;

public class AppUser : IdentityUser, ISystemUser
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string FullName { get; set; }
    public override required string Email { get; set; }
    public required string Department { get; set; }
    public required SystemUserStatus Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; private set; }
    public DateTime? DeactivatedAt { get; private set; }

    public void Activate()
    {
        if (Status == SystemUserStatus.Inactive)
            Status = SystemUserStatus.Active;
    }

    public void Deactivate(DateTime now)
    {
        if (Status == SystemUserStatus.Active)
        {
            Status = SystemUserStatus.Inactive;
            DeactivatedAt = now;
        }
    }

    public void RecordLogin(DateTime now)
    {
        LastLoginAt = now;
    }
}