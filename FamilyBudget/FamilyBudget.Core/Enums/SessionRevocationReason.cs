namespace FamilyBudget.Core.Enums
{
    public enum SessionRevocationReason
    {
        UserLogout,
        RemoteLogout,
        LogoutFromAllDevices,
        RefreshTokenReuse,
        SecurityAction
    }
}
