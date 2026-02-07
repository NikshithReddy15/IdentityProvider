namespace Models.Enums;

public enum AccountStatus : sbyte
{
    NotVerified = 0,
    Active = 1,
    Inactive = 2,
    LockedOut = 3,
}