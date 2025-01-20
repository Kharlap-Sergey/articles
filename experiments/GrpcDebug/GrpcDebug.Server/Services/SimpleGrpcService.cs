using System.Runtime.CompilerServices;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcDebug.Server;

namespace GrpcDebug.Server.Services;

public class SimpleGrpcService : SimpleService.SimpleServiceBase
{
    private readonly ILogger<SimpleGrpcService> _logger;

    public SimpleGrpcService(ILogger<SimpleGrpcService> logger)
    {
        _logger = logger;
    }

    public override async Task SimpleStreaming(
        SimpleRequest request,
        IServerStreamWriter<SimpleResponse> responseStream,
        ServerCallContext context)
    {
        await foreach (var response in GetResponses(request))
        {
            await responseStream.WriteAsync(response);
        }
    }

    public override async Task SimpleStreamingWithBuffer(
        SimpleRequest request,
        IServerStreamWriter<SimpleResponse> responseStream,
        ServerCallContext context)
    {
        responseStream.WriteOptions = new WriteOptions(WriteFlags.BufferHint);

        await foreach (var response in GetResponses(request).WithCancellation(context.CancellationToken))
        {
            await responseStream.WriteAsync(response);
        }
    }

    private static async IAsyncEnumerable<SimpleResponse> GetResponses(
        SimpleRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await Task.Delay(10, cancellationToken);
        for (var i = 0; i < request.Count; i++)
        {
            if (cancellationToken.IsCancellationRequested)
                yield break;

            yield return new SimpleResponse
            {
                Message = $"Response {i + 1} {DateTime.UtcNow.ToTimestamp()}"
            };
        }

        await Task.Yield();
    }
}