namespace CoreApp.Authorization;

public enum AppPolicies
{
    AdminOnly,
    ParkingEmployeeOnly,
    ActiveUser
}

public static class AppPoliciesExtensions
{
    public static string Name(this AppPolicies policy) => policy.ToString();
}