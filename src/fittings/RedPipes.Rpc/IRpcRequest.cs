namespace RedPipes.Patterns.Rpc
{
    /// <summary> Marker interface for requests, indicates the expected response type for the request/response pair </summary>
    public interface IRpcRequest<TRequest, TResponse> where TRequest : IRpcRequest<TRequest, TResponse>
    {

    }
}
