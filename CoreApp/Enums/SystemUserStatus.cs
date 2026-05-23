using System.Text.Json.Serialization;

namespace CoreApp.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SystemUserStatus
{
    Active,
    Inactive,
    Locked,
    PendingActivation
}