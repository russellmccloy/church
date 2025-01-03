namespace church_api.Controllers;

public static class AuthPolicies
{
    public const string PilotAdmins = nameof(PilotAdmins);
    public const string ValidUsers = nameof(ValidUsers);
    public const string RequireChatParticipant = nameof(RequireChatParticipant);
    public const string RequireCollectionParticipant = nameof(RequireCollectionParticipant);
}
