using CoreApp.Enums;

namespace CoreApp.Users;

public interface ISystemUser
{
    string Id { get; }
    string Email { get; }
    string FirstName { get; }
    string LastName { get; }
    string FullName { get; }
    string Department { get; }
    SystemUserStatus Status { get; }
    DateTime CreatedAt { get; }
}