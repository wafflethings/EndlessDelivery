using System;

namespace EndlessDelivery.Api.Exceptions;

public class PermissionException : Exception
{
    public PermissionException() : base("The current user has insufficient persmissions to perform this action.")
    {

    }
}
