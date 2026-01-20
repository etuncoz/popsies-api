using System.Reflection;
using MediatR;
using Popsies.Shared.Abstractions.Results;
using Popsies.Shared.Abstractions.Users;
using Popsies.Shared.Infrastructure.Attributes;

namespace Popsies.Shared.Infrastructure.Behaviors;

public sealed class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ICurrentUserProvider _currentUserProvider;

    public AuthorizationBehavior(ICurrentUserProvider currentUserProvider)
    {
        _currentUserProvider = currentUserProvider;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var authorizeAttribute = request.GetType().GetCustomAttribute<AuthorizeAttribute>();

        if (authorizeAttribute is null)
            return await next();

        var currentUser = _currentUserProvider.GetCurrentUser();

        // Check permissions
        if (authorizeAttribute.Permissions?.Length > 0)
        {
            var hasPermission = authorizeAttribute.Permissions
                .Any(p => currentUser.Permissions.Contains(p));

            if (!hasPermission)
            {
                return CreateForbiddenResult<TResponse>(
                    $"User lacks required permissions: {string.Join(", ", authorizeAttribute.Permissions)}");
            }
        }

        // Check roles
        if (authorizeAttribute.Roles?.Length > 0)
        {
            var hasRole = authorizeAttribute.Roles
                .Any(r => currentUser.Roles.Contains(r));

            if (!hasRole)
            {
                return CreateForbiddenResult<TResponse>(
                    $"User lacks required roles: {string.Join(", ", authorizeAttribute.Roles)}");
            }
        }

        return await next();
    }

    private static TResponse CreateForbiddenResult<TResponse>(string message)
    {
        var responseType = typeof(TResponse);

        if (responseType == typeof(Result))
            return (TResponse)(object)Result.Failure(Error.Forbidden(message));

        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var valueType = responseType.GetGenericArguments()[0];
            var error = Error.Forbidden(message);

            var failureMethod = typeof(Result)
                .GetMethod(nameof(Result.Failure), new[] { typeof(Error) })
                ?.MakeGenericMethod(valueType);

            return (TResponse)failureMethod!.Invoke(null, new object[] { error })!;
        }

        throw new InvalidOperationException(
            $"AuthorizationBehavior only supports Result or Result<T>, but got {responseType.Name}");
    }
}
