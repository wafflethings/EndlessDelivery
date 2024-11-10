namespace EndlessDelivery.Api.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string serverReason) : base($"Not found - {serverReason}")
    {

    }
}
