namespace EndlessDelivery.Common.Communication;

public class Response<T>
{
    public T Value;

    public Response(T value) => Value = value;
}
