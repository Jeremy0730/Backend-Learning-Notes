public sealed class TransientTrace
{
    public Guid Id { get; } = Guid.NewGuid();
}

public sealed class TransientTraceReader
{
    private readonly TransientTrace trace;

    public TransientTraceReader(TransientTrace trace)
    {
        this.trace = trace;
    }

    public Guid TraceId => trace.Id;

    public TransientTrace Trace => trace;
}
