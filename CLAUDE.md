# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Popsies Quiz API is a real-time multiplayer quiz platform built with .NET 10, implementing a **modular monolith** architecture with **Clean Architecture** (Hexagonal/Ports & Adapters) principles. The application uses event-driven communication between modules and maintains database-per-module isolation.

**Invariants:**
- Each domain and business rule must follow 'X:\Code\popsies-api\Invariants.md' invariants as guideline.

**Tech Stack:**
- .NET 10, ASP.NET Core, Entity Framework Core
- PostgreSQL (database per module)
- Keycloak (authentication)
- RabbitMQ + MassTransit (event bus)
- Redis (caching)
- Serilog + Elasticsearch (logging)
- xUnit, FluentAssertions, NSubstitute (testing)

## Commands

```bash
# Build the solution
dotnet build

# Run all tests
dotnet test

# Run specific test file
dotnet test --filter "FullyQualifiedName~UserTests"

# Run the API locally
dotnet run --project src/API/Popsies.API

# Add EF Core migration (Identity module example)
dotnet ef migrations add MigrationName \
  --project src/Modules/Identity/Popsies.Modules.Identity.Infrastructure \
  --startup-project src/API/Popsies.API

# Update database
dotnet ef database update \
  --project src/Modules/Identity/Popsies.Modules.Identity.Infrastructure \
  --startup-project src/API/Popsies.API

# Start infrastructure dependencies (PostgreSQL, Keycloak, Elasticsearch, RabbitMQ)
docker-compose up -d

# Format code
dotnet format
```

## Architecture Overview

### Modular Monolith Structure

Each module follows Clean Architecture with four layers:

```
Modules/[ModuleName]/
├── Domain/              # Pure business logic, aggregates, value objects, domain events
├── Application/         # Use cases (CQRS commands/queries), repository interfaces
├── Infrastructure/      # Persistence (EF Core), external service implementations
└── Api/                 # HTTP endpoints, module registration (Extensions.cs)
```

**Current Modules:**
- **Identity** - Users, Guests, Authentication (Keycloak integration) - *fully implemented*
- **Quiz** - Quiz management, questions, categories - *scaffolding*
- **Session** - Real-time quiz sessions with SignalR - *scaffolding*
- **Social** - Friendships, friend requests, blocks - *scaffolding*
- **Notification** - Push/email notifications - *scaffolding*

### Critical Patterns

#### 1. Result Pattern (No Exceptions)

**Never throw exceptions for business logic errors.** Use the `Result<T>` monad:

```csharp
// Domain/value object creation
public static Result<Email> Create(string email)
{
    if (string.IsNullOrWhiteSpace(email))
        return Result.Failure<Email>(Error.Validation("Email", "Email cannot be empty"));

    if (!Regex.IsMatch(email, EmailPattern))
        return Result.Failure<Email>(Error.Validation("Email", "Invalid email format"));

    return Result.Success(new Email(email));
}

// Command handler orchestration
public async Task<Result<Guid>> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
{
    var emailResult = Email.Create(command.Email);
    if (emailResult.IsFailure)
        return Result.Failure<Guid>(emailResult.Error);

    var email = emailResult.Value;
    // ... continue with business logic
}

// API controller response
var result = await _commandDispatcher.SendAsync(command);
if (!result.IsSuccess)
    return BadRequest(result.Error);
return Ok(result.Value);
```

**Error types:**
- `Error.NotFound(entityName, key)`
- `Error.Validation(field, message)`
- `Error.Conflict(message)`
- `Error.Unauthorized(message)`
- `Error.Forbidden(message)`
- `Error.Create(code, message)` - custom errors

#### 2. CQRS Pattern

Commands (state-changing) and Queries (read-only) are handled separately:

```csharp
// Command (Application/UseCases/[Feature]/[Feature]Command.cs)
public sealed record RegisterUserCommand(
    string Username,
    string Email,
    string Password,
    string ConfirmPassword) : ICommand<Guid>;

// Handler (Application/UseCases/[Feature]/[Feature]CommandHandler.cs)
public sealed class RegisterUserCommandHandler(
    IUserRepository userRepository,
    IKeycloakService keycloakService,
    IUnitOfWork unitOfWork) : ICommandHandler<RegisterUserCommand, Guid>
{
    public async Task<Result<Guid>> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        // 1. Validate using value objects
        var emailResult = Email.Create(command.Email);
        if (emailResult.IsFailure)
            return Result.Failure<Guid>(emailResult.Error);

        // 2. Check business rules
        if (await userRepository.ExistsByEmailAsync(email, cancellationToken))
            return Result.Failure<Guid>(Error.Conflict("Email already in use"));

        // 3. Create aggregate via factory method
        var user = User.Create(userId, username, email);

        // 4. Persist via repository
        await userRepository.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(user.Id);
    }
}
```

**Registration in module's Extensions.cs:**
```csharp
services.AddScoped<ICommandHandler<RegisterUserCommand, Guid>, RegisterUserCommandHandler>();
```

#### 3. Domain Events & Event Bus

Domain events enable loosely-coupled inter-module communication:

```csharp
// 1. Define event (Domain/[Entity]/Events/[Event]Event.cs)
public sealed record UserRegisteredEvent : DomainEvent
{
    public Guid UserId { get; init; }
    public string Username { get; init; }
    public string Email { get; init; }
}

// 2. Raise event in aggregate
public sealed class User : AggregateRoot
{
    public static User Create(Guid id, Username username, Email email)
    {
        var user = new User { Id = id, Username = username, Email = email };
        user.RaiseDomainEvent(new UserRegisteredEvent(id, username.FullUsername, email.Value));
        return user;
    }
}

// 3. Events auto-published after UnitOfWork.SaveChangesAsync()
// No manual publishing needed!

// 4. Handle event in another module
public sealed class WelcomeEmailHandler : IEventHandler<UserRegisteredEvent>
{
    public async Task HandleAsync(UserRegisteredEvent @event, CancellationToken cancellationToken)
    {
        // Send welcome email
    }
}
```

**Event bus uses RabbitMQ with MassTransit.** Register consumers in `Program.cs`:
```csharp
builder.Services.AddMassTransitWithRabbitMq(builder.Configuration, cfg =>
{
    cfg.AddEventConsumer<UserRegisteredEvent>();
    cfg.AddEventConsumer<UserProfileUpdatedEvent>();
    // ... register all domain event consumers
});
```

#### 4. Aggregate Roots & Value Objects

**Aggregate Root:**
```csharp
public sealed class User : AggregateRoot
{
    // Private setters, no public constructor
    public Username Username { get; private set; } = null!;
    public Email Email { get; private set; } = null!;

    // Factory method for creation
    public static User Create(Guid id, Username username, Email email)
    {
        var user = new User { Id = id, Username = username, Email = email };
        user.RaiseDomainEvent(new UserRegisteredEvent(...));
        return user;
    }

    // Business logic methods return Result
    public Result UpdateProfile(string displayName, string? avatarUrl)
    {
        if (IsDeleted)
            return Result.Failure(Error.Create("User.CannotUpdate", "Cannot update deleted user"));

        // Validation and business logic
        DisplayName = displayName;
        AvatarUrl = avatarUrl;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new UserProfileUpdatedEvent(...));
        return Result.Success();
    }
}
```

**Value Object:**
```csharp
public sealed class Email : ValueObject
{
    public string Value { get; }

    private Email(string value) => Value = value;

    public static Result<Email> Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure<Email>(Error.Validation("Email", "Email cannot be empty"));

        if (!Regex.IsMatch(email, EmailPattern))
            return Result.Failure<Email>(Error.Validation("Email", "Invalid email format"));

        return Result.Success(new Email(email.ToLowerInvariant()));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
```

#### 5. Repository Pattern

**Interface in Application layer:**
```csharp
// Application/Common/Repositories/IUserRepository.cs
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken);
    Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken);
    Task AddAsync(User user, CancellationToken cancellationToken);
    void Update(User user);
    void Remove(User user);
}
```

**Implementation in Infrastructure layer:**
```csharp
// Infrastructure/Persistence/Repositories/UserRepository.cs
internal sealed class UserRepository(IdentityDbContext context) : IUserRepository
{
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => await context.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken)
        => await context.Users
            .AnyAsync(u => u.Email == email, cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken)
        => await context.Users.AddAsync(user, cancellationToken);
}
```

**EF Core Configuration:**
```csharp
// Infrastructure/Persistence/Configurations/UserConfiguration.cs
internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users", "identity");
        builder.HasKey(u => u.Id);

        // Value object conversion
        builder.Property(u => u.Email)
            .HasConversion(
                email => email.Value,
                value => Email.Create(value).Value)
            .IsRequired();

        builder.Property(u => u.Username)
            .HasConversion(
                username => username.FullUsername,
                value => Username.Parse(value).Value)
            .IsRequired();
    }
}
```

## Adding New Features

### Step 1: Domain Layer

1. **Create aggregate root** in `Domain/[EntityName]/[EntityName].cs`
2. **Create value objects** in `Domain/ValueObjects/[Name].cs`
3. **Create domain events** in `Domain/[EntityName]/Events/[Event]Event.cs`
4. **Define business rules** using Result pattern

### Step 2: Application Layer

1. **Create command/query** in `Application/UseCases/[Feature]/[Feature]Command.cs`
2. **Create handler** in `Application/UseCases/[Feature]/[Feature]CommandHandler.cs`
3. **Define repository interface** in `Application/Common/Repositories/I[Entity]Repository.cs`
4. **Define service interfaces** in `Application/Common/Services/I[Service].cs` (if needed)

### Step 3: Infrastructure Layer

1. **Implement repository** in `Infrastructure/Persistence/Repositories/[Entity]Repository.cs`
2. **Configure EF Core mapping** in `Infrastructure/Persistence/Configurations/[Entity]Configuration.cs`
3. **Add DbSet** to module's DbContext
4. **Create migration**: `dotnet ef migrations add [Name] --project Infrastructure --startup-project API`
5. **Implement services** in `Infrastructure/Services/[Service].cs` (if needed)

### Step 4: API Layer

1. **Create controller** in `Api/Controllers/[Feature]Controller.cs`
2. **Register dependencies** in `Api/Extensions.cs` (AddModule method)
3. **Create DTOs** in `Contracts/[Feature]Request.cs` and `[Feature]Response.cs`

### Step 5: Testing

1. **Domain tests** in `tests/Unit/[Module]/Domain/Aggregates/[Entity]Tests.cs`
2. **Handler tests** in `tests/Unit/[Module]/Handlers/[Handler]Tests.cs`
3. Use **NSubstitute** for mocking dependencies
4. Use **FluentAssertions** for readable assertions

## Key Architectural Decisions

### Result Pattern Over Exceptions
Business logic errors return `Result<T>` instead of throwing exceptions. Only infrastructure exceptions (database failures, network issues) should throw.

### Event-Driven Module Communication
Modules communicate via domain events published through the event bus (RabbitMQ + MassTransit). This keeps modules loosely coupled.

### Database Per Module
Each module has its own database schema and DbContext. This enables independent scaling and potential future extraction to microservices.

### Value Objects for Validation
All validation happens at the domain boundary using value objects. Once created, value objects are immutable and guaranteed valid.

### CQRS Separation
Commands modify state, queries read state. This separation enables different optimization strategies and keeps handlers focused.

### Keycloak for Authentication
User passwords are managed by Keycloak (external IdP). The Identity module creates local user records linked to Keycloak IDs.

### Automatic Migrations
Database migrations run automatically on application startup via `app.UseIdentityModule()` in each module's Extensions.cs.

## Configuration

**appsettings.json structure:**
```json
{
  "ConnectionStrings": {
    "IdentityDb": "Host=localhost;Port=5432;Database=popsies_identity;..."
  },
  "Keycloak": {
    "Realm": "popsies",
    "AuthServerUrl": "http://localhost:8080",
    "ClientId": "popsies-api",
    "ClientSecret": "...",
    "AdminApiUrl": "http://localhost:8080/admin/realms/popsies"
  },
  "RabbitMQ": {
    "Host": "localhost",
    "VirtualHost": "/",
    "Username": "guest",
    "Password": "guest"
  },
  "Elasticsearch": {
    "Uri": "http://localhost:9200"
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  }
}
```

## Testing Patterns

**Domain tests:**
```csharp
[Fact]
public void Create_WithValidData_ShouldCreateUserAndRaiseEvent()
{
    var user = User.Create(userId, username, email);

    user.Id.Should().Be(userId);
    user.Username.Should().Be(username);
    user.DomainEvents.Should().ContainSingle();
    user.DomainEvents.Should().ContainItemsAssignableTo<UserRegisteredEvent>();
}
```

**Handler tests with mocking:**
```csharp
[Fact]
public async Task Handle_ShouldRegisterUser_WhenValidCommand()
{
    // Arrange
    var command = new RegisterUserCommand("user", "test@example.com", "Pass123!", "Pass123!");
    _userRepository.ExistsByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
        .Returns(false);
    _keycloakService.CreateUserAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
        .Returns(new KeycloakUserCreationResult { Success = true, KeycloakUserId = "keycloak-id" });

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.IsSuccess.Should().BeTrue();
    await _userRepository.Received(1).AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
}
```

## Common Pitfalls to Avoid

1. **Don't throw exceptions for business logic** - Use Result pattern
2. **Don't bypass value objects** - Always create through `Create()` factory method
3. **Don't publish events manually** - UnitOfWork handles this automatically after SaveChangesAsync
4. **Don't add business logic to Infrastructure layer** - Keep it in Domain
5. **Don't reference Domain directly from API layer** - Go through Application layer
6. **Don't forget CancellationToken** - Every async method should accept it
7. **Don't create aggregates with `new`** - Use static factory methods
8. **Don't make value objects mutable** - They should be immutable after creation
9. **Don't skip repository abstraction** - Always go through IRepository interfaces
10. **Don't hardcode infrastructure concerns in Application layer** - Use dependency injection

## Module Independence

Each module should:
- Have its own database schema (e.g., `identity`, `quiz`, `session`)
- Define its own DbContext
- Expose public contracts via Contracts project
- Communicate through domain events only
- Never directly reference another module's Domain/Application layers
- Register services in its own `Extensions.cs`

## Global Exception Handling

The API uses Problem Details (RFC 9457) for error responses via `GlobalExceptionHandler`:

```csharp
// Maps exceptions to HTTP status codes
ValidationException → 400 Bad Request
ConflictException → 409 Conflict
NotFoundException → 404 Not Found
UnauthorizedException → 401 Unauthorized
DomainException → 400 Bad Request
Other exceptions → 500 Internal Server Error
```

Responses include:
- `type`: RFC 9110 link
- `title`: Human-readable summary
- `status`: HTTP status code
- `detail`: Specific error message
- `instance`: Request path
- `errorCode`: Application error code (for PopsiesException)
- `errors`: Validation errors dictionary (for ValidationException)
- `traceId`: Correlation ID for debugging


## Notes for Claude Code

- Always build all application after development task, and fix any remaining errors. Keep doing this until no errors remains.
- Always run all unit tests after development task, and if there are any failing tests, fix the code. Keep doing this until no errors remains.