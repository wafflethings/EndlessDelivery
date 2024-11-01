namespace EndlessDelivery.Api.Exceptions;

public class PermissionException : Exception
{
    public PermissionException() : base("The current user has insufficient permissions to perform this action.")
    {

    }
}
