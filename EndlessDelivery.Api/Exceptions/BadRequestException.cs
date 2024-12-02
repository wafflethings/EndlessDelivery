public class BadRequestException : Exception
{
    public BadRequestException(string reason) : base($"Server rejected request as {reason}")
    {

    }
}
