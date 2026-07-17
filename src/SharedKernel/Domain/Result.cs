namespace SharedKernel.Domain;

public record Result<T, TE> : IResult<TE>
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

public record Result<TE> : IResult<TE>
{
    public bool IsSuccess { get; init; }
    public TE? Error { get; init; }

    private Result(TE? err)
    {
        Error = err;
    }

    public static Result<TE> Success()
    {
        return new Result<TE>(default(TE)) { IsSuccess = true };
    }

    public static Result<TE> Fail(TE err)
    {
        return new Result<TE>(err) { IsSuccess = false };
    }
}

public interface IResult<out TE> { bool IsSuccess { get; } TE? Error { get; } }

public static class Result
{
    public static Result<TE> Check<TE>(params IResult<TE>[] results)
    {
        foreach (var r in results)
        {
            if (!r.IsSuccess) return Result<TE>.Fail(r.Error!);
        }
        return Result<TE>.Success();
    }
}