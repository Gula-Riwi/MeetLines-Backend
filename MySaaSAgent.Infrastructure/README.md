# MySaaSAgent.Infrastructure
# MySaaSAgent.Infrastructure

Esta capa contiene **implementaciones concretas** de los contratos definidos en la capa de Aplicación/Dominio.

## Qué debe ir aquí
- **ExternalServices** – Clientes o adaptadores para APIs externas, servicios de mensajería, etc.
- **IoC** – Configuración del contenedor de inyección de dependencias (por ejemplo, `services.AddScoped<IRepository, RepositoryImplementation>()`). Aquí se registran todas las implementaciones de interfaces.
- **Data Access** – Implementaciones de repositorios que acceden a bases de datos (EF Core, Dapper, etc.)
## 📝 Ejemplos de Implementación

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
        // EF Core detecta cambios automáticamente
        if (_context.Entry(aggregate.User).State == EntityState.Detached)
        {
            _context.Users.Add(aggregate.User);
        }
        await _context.SaveChangesAsync();
    }
}
```

### 2. Configuración de EF Core (DbContext)
Mapeo de entidades a tablas.

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Subscription>(entity =>
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Status).IsRequired();
        // Relación con User
        entity.HasOne<SaasUser>()
              .WithMany()
              .HasForeignKey(e => e.UserId);
    });
}
```

## 🧪 Cómo probar esta capa
Usamos **Testcontainers** o una base de datos en memoria.
```bash
dotnet test MySaaSAgent.Tests --filter "Category=Integration"
```
- **Migrations / DB Context** – Si usas Entity Framework, los `DbContext` y migraciones se ubican aquí.

La infraestructura **solo depende** de la capa de Dominio (entidades, value objects) y de paquetes externos; nunca depende de la API.
