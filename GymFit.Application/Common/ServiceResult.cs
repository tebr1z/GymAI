namespace GymFit.Application.Common;

public readonly struct ServiceResult<T>
{
    public bool IsSuccess { get; init; }
    public T? Value { get; init; }
    public string? Error { get; init; }
    public ServiceFailureKind Kind { get; init; }

    public static ServiceResult<T> Ok(T value) =>
        new()
        {
            IsSuccess = true,
            Value = value,
            Error = null,
            Kind = default
        };

    public static ServiceResult<T> Fail(string error, ServiceFailureKind kind = ServiceFailureKind.BadRequest) =>
        new()
        {
            IsSuccess = false,
            Value = default,
            Error = error,
            Kind = kind
        };
}

public readonly struct ServiceResult
{
    public bool IsSuccess { get; init; }
    public string? Error { get; init; }
    public ServiceFailureKind Kind { get; init; }

    public static ServiceResult Ok() =>
        new() { IsSuccess = true, Error = null, Kind = default };

    public static ServiceResult Fail(string error, ServiceFailureKind kind = ServiceFailureKind.BadRequest) =>
        new() { IsSuccess = false, Error = error, Kind = kind };
}
