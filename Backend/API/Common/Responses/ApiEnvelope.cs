namespace Renova.Api.Common.Responses;

public sealed record ApiEnvelope<T>(
    bool Success,
    T? Data,
    ApiEnvelopeMetadata Metadata)
{
    public static ApiEnvelope<T> CreateSuccess(T data, string traceId)
    {
        return new ApiEnvelope<T>(
            true,
            data,
            new ApiEnvelopeMetadata(traceId, DateTimeOffset.UtcNow));
    }
}

public sealed record ApiEnvelopeMetadata(
    string TraceId,
    DateTimeOffset Timestamp);
