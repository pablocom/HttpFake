namespace HttpFake;

public enum InterceptionBehaviour
{
    Strict = 0, 
    Lax = 1
}

public class NoReceivedRequestReceivedRequestMatchingException : Exception
{
    public NoReceivedRequestReceivedRequestMatchingException(string message) : base(message)
    {
    }
}