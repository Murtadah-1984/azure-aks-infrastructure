using IdentityService.Application.Common.Interfaces;
using IdentityService.Domain.DomainEvents;
using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityService.Infrastructure.Persistence;

/// <summary>
/// Entity Framework database context
/// </summary>
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<MfaFactor> MfaFactors { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<AuthorizationCode> AuthorizationCodes { get; set; }
    public DbSet<Consent> Consents { get; set; }
    public DbSet<WebAuthnCredential> WebAuthnCredentials { get; set; }

    private readonly ICurrentUserService _currentUserService;
    private readonly IEventBus _eventBus;
    private readonly IServiceProvider? _serviceProvider;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService currentUserService,
        IEventBus eventBus,
        IServiceProvider? serviceProvider = null) : base(options)
    {
        _currentUserService = currentUserService;
        _eventBus = eventBus;
        _serviceProvider = serviceProvider;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Apply all entity configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Audit trail
        foreach (var entry in ChangeTracker.Entries<Entity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.CreatedBy = _currentUserService.UserId;
                    break;
                case EntityState.Modified:
                    entry.Entity.LastModifiedAt = DateTime.UtcNow;
                    entry.Entity.LastModifiedBy = _currentUserService.UserId;
                    break;
            }
        }

        // Collect domain events before saving
        var domainEvents = ChangeTracker.Entries<Domain.Entities.AggregateRoot>()
            .Select(x => x.Entity)
            .SelectMany(x =>
            {
                var events = x.DomainEvents;
                x.ClearDomainEvents();
                return events;
            })
            .ToList();

        // Save domain events to outbox instead of direct publishing (Outbox Pattern)
        if (_serviceProvider != null)
        {
            var outboxRepository = _serviceProvider.GetService<Domain.Interfaces.IOutboxRepository>();
            if (outboxRepository != null)
            {
                foreach (var domainEvent in domainEvents)
                {
                    var outboxMessage = new Domain.Entities.OutboxMessage(
                        domainEvent.GetType().FullName ?? domainEvent.GetType().Name,
                        System.Text.Json.JsonSerializer.Serialize(domainEvent));
                    await outboxRepository.AddAsync(outboxMessage, cancellationToken);
                }
            }
            else
            {
                // Fallback to direct publishing if outbox is not configured
                foreach (var domainEvent in domainEvents)
                {
                    await _eventBus.PublishAsync(domainEvent, cancellationToken);
                }
            }
        }
        else
        {
            // Fallback to direct publishing if service provider is not available
            foreach (var domainEvent in domainEvents)
            {
                await _eventBus.PublishAsync(domainEvent, cancellationToken);
            }
        }

        var result = await base.SaveChangesAsync(cancellationToken);

        return result;
    }
}
