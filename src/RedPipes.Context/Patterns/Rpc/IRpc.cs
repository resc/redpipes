namespace RedPipes.Patterns.Rpc
{
    /// <summary> Marker interface for requests, indicates the expected response type for the request/response pair </summary>
    public interface IRpc<TRequest,TResponse> where TRequest : IRpc<TRequest,TResponse> { }
}
