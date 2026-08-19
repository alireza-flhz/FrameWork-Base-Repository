using System;
using System.Collections.Generic;
using System.Linq;

namespace BaseRepository.Domain.Common;

public sealed class Result
{
    public bool Succeeded { get; }
    public IReadOnlyList<string> Errors { get; }

    private Result(bool succeeded, IReadOnlyList<string> errors)
    {
        Succeeded = succeeded;
        Errors = errors;
    }

    public static Result Success() => new(true, Array.Empty<string>());

    public static Result Failure(params string[] errors) => new(false, errors);

    public static Result Failure(IEnumerable<string> errors) => new(false, errors.ToArray());
}

public sealed class Result<T>
{
    public bool Succeeded { get; }
    public IReadOnlyList<string> Errors { get; }
    public T? Value { get; }

    private Result(T? value, bool succeeded, IReadOnlyList<string> errors)
    {
        Value = value;
        Succeeded = succeeded;
        Errors = errors;
    }

    public static Result<T> Success(T value) => new(value, true, Array.Empty<string>());

    public static Result<T> Failure(params string[] errors) => new(default, false, errors);

    public static Result<T> Failure(IEnumerable<string> errors) => new(default, false, errors.ToArray());
}
