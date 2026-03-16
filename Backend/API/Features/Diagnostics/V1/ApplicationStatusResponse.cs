namespace Renova.Api.Features.Diagnostics.V1;

public sealed record ApplicationStatusResponse(
    string Application,
    string Environment,
    string Version,
    DateTimeOffset ServerTimeUtc);
