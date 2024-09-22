using System;

namespace EndlessDelivery.Api.Exceptions;

public class InternalServerException : Exception
{
    public InternalServerException() : base("Internal server error.")
    {

    }
}
