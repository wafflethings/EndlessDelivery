namespace EndlessDelivery.Api.Exceptions;

public class BadResponseException : Exception
{
    public BadResponseException(string response) : base($"Failed to parse response. Library may be out of date or server offline (\"{response}\").")
    {

    }
}

