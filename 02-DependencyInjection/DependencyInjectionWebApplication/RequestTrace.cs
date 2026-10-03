public sealed class RequestTrace
{
    public Guid Id { get; } = Guid.NewGuid();
}

public sealed class RequestTraceReader
{
    private readonly RequestTrace trace;

    public RequestTraceReader(RequestTrace trace)
    {
        this.trace = trace;
    }

    public Guid TraceId => trace.Id;

    public RequestTrace Trace => trace;
}
