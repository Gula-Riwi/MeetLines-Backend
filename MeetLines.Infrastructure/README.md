# MeetLines.Infrastructure
# MeetLines.Infrastructure

Esta capa contiene **implementaciones concretas** de los contratos definidos en la capa de AplicaciÃ³n/Dominio.

## QuÃ© debe ir aquÃ­
- **ExternalServices** â€“ Clientes o adaptadores para APIs externas, servicios de mensajerÃ­a, etc.
- **IoC** â€“ ConfiguraciÃ³n del contenedor de inyecciÃ³n de dependencias (por ejemplo, `services.AddScoped<IRepository, RepositoryImplementation>()`). AquÃ­ se registran todas las implementaciones de interfaces.
- **Data Access** â€“ Implementaciones de repositorios que acceden a bases de datos (EF Core, Dapper, etc.)
## ðŸ“ Ejemplos de ImplementaciÃ³n

### 1. Repositorio (Driven Adapter)
Implementa la interfaz definida en Domain/Application.

```csharp
public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<SaasUserAggregate> GetAggregateAsync(Guid id)
    {
        // Carga el usuario y sus relaciones (Eager Loading)
        var user = await _context.Users
            .Include(u => u.Subscriptions)
            .FirstOrDefaultAsync(u => u.Id == id);
        
        if (user == null) return null;

        // Reconstruye el Aggregate (si es necesario)
        return new SaasUserAggregate(user);
    }

    public async Task SaveAsync(SaasUserAggregate aggregate)
    {
        // EF Core detecta cambios automÃ¡ticamente
        if (_context.Entry(aggregate.User).State == EntityState.Detached)
        {
            _context.Users.Add(aggregate.User);
        }
        await _context.SaveChangesAsync();
    }
}
```

### 2. ConfiguraciÃ³n de EF Core (DbContext)
Mapeo de entidades a tablas.

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Subscription>(entity =>
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Status).IsRequired();
        // RelaciÃ³n con User
        entity.HasOne<SaasUser>()
              .WithMany()
              .HasForeignKey(e => e.UserId);
    });
}
```

## ðŸ§ª CÃ³mo probar esta capa
Usamos **Testcontainers** o una base de datos en memoria.
```bash
dotnet test MeetLines.Tests --filter "Category=Integration"
```
- **Migrations / DB Context** â€“ Si usas Entity Framework, los `DbContext` y migraciones se ubican aquÃ­.

La infraestructura **solo depende** de la capa de Dominio (entidades, value objects) y de paquetes externos; nunca depende de la API.
