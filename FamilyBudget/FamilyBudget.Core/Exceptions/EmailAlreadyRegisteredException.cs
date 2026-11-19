namespace FamilyBudget.Core.Exceptions;

public sealed class EmailAlreadyRegisteredException : Exception
{
    public EmailAlreadyRegisteredException()
        : base("Email is already registered.")
    {
    }
}
