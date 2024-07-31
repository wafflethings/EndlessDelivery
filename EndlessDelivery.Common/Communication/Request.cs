namespace EndlessDelivery.Common.Communication;

public class Request<T>
{
    public T Value;

    public Request(T value) => Value = value;
}
