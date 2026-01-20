using FluentValidation;
using MediatR;
using Popsies.Shared.Abstractions.Results;

namespace Popsies.Shared.Infrastructure.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count == 0)
            return await next();

        return CreateValidationFailureResult<TResponse>(failures);
    }

    private static TResponse CreateValidationFailureResult<TResponse>(
        List<FluentValidation.Results.ValidationFailure> failures)
    {
        var responseType = typeof(TResponse);

        // Handle Result (no generic parameter)
        if (responseType == typeof(Result))
        {
            var error = Error.Validation(failures.First().PropertyName, failures.First().ErrorMessage);
            return (TResponse)(object)Result.Failure(error);
        }

        // Handle Result<T>
        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var valueType = responseType.GetGenericArguments()[0];
            var error = Error.Validation(failures.First().PropertyName, failures.First().ErrorMessage);

            var failureMethod = typeof(Result)
                .GetMethod(nameof(Result.Failure), new[] { typeof(Error) })
                ?.MakeGenericMethod(valueType);

            return (TResponse)failureMethod!.Invoke(null, new object[] { error })!;
        }

        throw new InvalidOperationException(
            $"ValidationBehavior only supports Result or Result<T>, but got {responseType.Name}");
    }
}
