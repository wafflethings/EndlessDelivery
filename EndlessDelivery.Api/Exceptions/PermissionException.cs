namespace EndlessDelivery.Api.Exceptions;

public class PermissionException : Exception
{
    public PermissionException(string requirement = "") : base($"The current user has insufficient permissions to perform this action. {requirement}")
    {

    }
}
