# MeetLines.Domain

**PropÃ³sito**

Esta capa contiene el **modelo de dominio** puro, es decir, las reglas de negocio y los conceptos centrales de la aplicaciÃ³n sin ninguna dependencia externa (bases de datos, frameworks web, etc.). Es el corazÃ³n del sistema: aquÃ­ se define *quÃ©* es el negocio y *cÃ³mo* se mantiene su consistencia.

**Estructura de carpetas**

- `Entities/` â€“ Clases que representan los conceptos del negocio con identidad propia (por ejemplo, `Customer`, `Order`). Cada entidad contiene sus atributos y mÃ©todos que garantizan invariantes del dominio.
- `Aggregates/` â€“ RaÃ­ces de agregados que agrupan entidades relacionadas y definen lÃ­mites de consistencia. Por ejemplo, un `OrderAggregate` que contiene la entidad `Order` y sus `OrderLine`.
- `ValueObjects/` â€“ Objetos que se definen Ãºnicamente por sus valores (por ejemplo, `Money`, `Address`). Son inmutables y se comparan por igualdad de valores.
- `Enums/` â€“ Enumeraciones que describen estados o tipos dentro del dominio (por ejemplo, `OrderStatus`).
- `Events/` â€“ Eventos de dominio que representan algo que ha ocurrido en el modelo (por ejemplo, `CustomerCreatedEvent`). Son Ãºtiles para arquitectura basada en eventos o para notificaciones internas.
- `Repositories/` â€“ **Interfaces** que describen los contratos de persistencia (por ejemplo, `ICustomerRepository`). Las implementaciones reales vivirÃ¡n en la capa de Infraestructura.

**CÃ³mo se integra con el resto del proyecto**

1. **Dependencias**: La capa de dominio **no depende** de ninguna otra capa. SÃ³lo referencia a tipos del propio dominio y a .NET estÃ¡ndar.
# MeetLines.Domain

**PropÃ³sito**

Esta capa contiene el **modelo de dominio** puro, es decir, las reglas de negocio y los conceptos centrales de la aplicaciÃ³n sin ninguna dependencia externa (bases de datos, frameworks web, etc.). Es el corazÃ³n del sistema: aquÃ­ se define *quÃ©* es el negocio y *cÃ³mo* se mantiene su consistencia.

**Estructura de carpetas**

- `Entities/` â€“ Clases que representan los conceptos del negocio con identidad propia (por ejemplo, `Customer`, `Order`). Cada entidad contiene sus atributos y mÃ©todos que garantizan invariantes del dominio.
- `Aggregates/` â€“ RaÃ­ces de agregados que agrupan entidades relacionadas y definen lÃ­mites de consistencia. Por ejemplo, un `OrderAggregate` que contiene la entidad `Order` y sus `OrderLine`.
- `ValueObjects/` â€“ Objetos que se definen Ãºnicamente por sus valores (por ejemplo, `Money`, `Address`). Son inmutables y se comparan por igualdad de valores.
- `Enums/` â€“ Enumeraciones que describen estados o tipos dentro del dominio (por ejemplo, `OrderStatus`).
- `Events/` â€“ Eventos de dominio que representan algo que ha ocurrido en el modelo (por ejemplo, `CustomerCreatedEvent`). Son Ãºtiles para arquitectura basada en eventos o para notificaciones internas.
- `Repositories/` â€“ **Interfaces** que describen los contratos de persistencia (por ejemplo, `ICustomerRepository`). Las implementaciones reales vivirÃ¡n en la capa de Infraestructura.

**CÃ³mo se integra con el resto del proyecto**

1. **Dependencias**: La capa de dominio **no depende** de ninguna otra capa. SÃ³lo referencia a tipos del propio dominio y a .NET estÃ¡ndar.
2. **Uso por la capa de AplicaciÃ³n**: Los casos de uso (en `MeetLines.Application`) consumen entidades, value objects y repositorios definidos aquÃ­. Cuando la aplicaciÃ³n necesita crear o consultar datos, llama a los mÃ©todos de los repositorios (interfaces) y trabaja con las entidades del dominio.
3. **Persistencia**: La capa de Infraestructura implementa las interfaces de repositorio y traduce entidades a modelos de base de datos. La lÃ³gica de negocio sigue estando en el dominio.

**Buenas prÃ¡cticas**

- Mantener la lÃ³gica de negocio (validaciones, invariantes) dentro de las entidades o value objects, nunca en la capa de aplicaciÃ³n o infraestructura.
- Evitar referencias a frameworks externos (por ejemplo, Entity Framework) dentro de esta capa.
- Utilizar eventos de dominio para comunicar cambios importantes a otras partes del sistema.
- Documentar cada entidad y sus invariantes con comentarios XML para que los desarrolladores comprendan las reglas de negocio.

## ðŸ“ Ejemplos de ImplementaciÃ³n

### 1. Entidad Rica (Rich Domain Model)
AsÃ­ se ve una entidad que se protege a sÃ­ misma (DDD puro):

```csharp
public class Subscription
{
    // Propiedades de solo lectura desde fuera (private set)
    public Guid Id { get; private set; }
    public string Status { get; private set; }
    public DateTime? RenewalDate { get; private set; }

    // Constructor que obliga a tener datos vÃ¡lidos al nacer
    public Subscription(Guid userId, string plan)
    {
        if (userId == Guid.Empty) throw new ArgumentException("Usuario requerido");
        Id = Guid.NewGuid();
        UserId = userId;
        Plan = plan;
        Status = "active"; // Estado inicial por defecto
    }

    // MÃ©todos semÃ¡nticos para modificar el estado
    public void Cancel()
    {
        Status = "cancelled";
        RenewalDate = null;
    }
}
```

### 2. Aggregate Root (El Jefe)
CÃ³mo el Aggregate coordina a sus entidades hijas:

```csharp
public class LeadAggregate
{
    public Lead Lead { get; private set; } // La raÃ­z
    private readonly List<LeadInteraction> _interactions = new(); // Los hijos

    public void AddInteraction(string message, string channel)
    {
        // 1. Crea la interacciÃ³n (el hijo)
        var interaction = new LeadInteraction(Lead.Id, "user", channel, message);
        _interactions.Add(interaction);

        // 2. Actualiza al padre (sincronizaciÃ³n automÃ¡tica)
        Lead.RecordInteraction(); // Actualiza LastInteractionAt
    }
}
```

## ðŸ§ª CÃ³mo probar esta capa
Las pruebas unitarias aquÃ­ son rÃ¡pidas y no requieren base de datos.
```bash
dotnet test MeetLines.Tests --filter "Category=Domain"
```
# Ejecutar pruebas unitarias (si existen)
dotnet test
```

Las pruebas deben enfocarse en validar que las entidades y sus mÃ©todos mantienen los invariantes y que los eventos se disparan correctamente.
