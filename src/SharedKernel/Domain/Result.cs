namespace SharedKernel.Domain;

public record Result<T, TE>
{
    public bool IsSuccess { get; init; }
    public T? Ok { get; private init; }
    public TE? Error { get; private init; }

    private Result(T? ok, TE? err)
    {
        Ok = ok;
        Error = err;
    }

    public static Result<T, TE> Success(T ok) => new(ok, default) { IsSuccess = true };

    public static Result<T, TE> Fail(TE err) => new(default, err) { IsSuccess = false };
};