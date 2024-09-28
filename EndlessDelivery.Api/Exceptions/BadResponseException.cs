using System;

namespace EndlessDelivery.Api.Exceptions;

public class BadResponseException : Exception
{
    public BadResponseException() : base("Failed to parse response. Library may be out of date or server offline.")
    {

    }
}

