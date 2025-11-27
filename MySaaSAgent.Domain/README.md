# MySaaSAgent.Domain

**Propósito**

Esta capa contiene el **modelo de dominio** puro, es decir, las reglas de negocio y los conceptos centrales de la aplicación sin ninguna dependencia externa (bases de datos, frameworks web, etc.). Es el corazón del sistema: aquí se define *qué* es el negocio y *cómo* se mantiene su consistencia.

**Estructura de carpetas**

- `Entities/` – Clases que representan los conceptos del negocio con identidad propia (por ejemplo, `Customer`, `Order`). Cada entidad contiene sus atributos y métodos que garantizan invariantes del dominio.
- `Aggregates/` – Raíces de agregados que agrupan entidades relacionadas y definen límites de consistencia. Por ejemplo, un `OrderAggregate` que contiene la entidad `Order` y sus `OrderLine`.
- `ValueObjects/` – Objetos que se definen únicamente por sus valores (por ejemplo, `Money`, `Address`). Son inmutables y se comparan por igualdad de valores.
- `Enums/` – Enumeraciones que describen estados o tipos dentro del dominio (por ejemplo, `OrderStatus`).
- `Events/` – Eventos de dominio que representan algo que ha ocurrido en el modelo (por ejemplo, `CustomerCreatedEvent`). Son útiles para arquitectura basada en eventos o para notificaciones internas.
- `Repositories/` – **Interfaces** que describen los contratos de persistencia (por ejemplo, `ICustomerRepository`). Las implementaciones reales vivirán en la capa de Infraestructura.

**Cómo se integra con el resto del proyecto**

1. **Dependencias**: La capa de dominio **no depende** de ninguna otra capa. Sólo referencia a tipos del propio dominio y a .NET estándar.
# MySaaSAgent.Domain

**Propósito**

Esta capa contiene el **modelo de dominio** puro, es decir, las reglas de negocio y los conceptos centrales de la aplicación sin ninguna dependencia externa (bases de datos, frameworks web, etc.). Es el corazón del sistema: aquí se define *qué* es el negocio y *cómo* se mantiene su consistencia.

**Estructura de carpetas**

- `Entities/` – Clases que representan los conceptos del negocio con identidad propia (por ejemplo, `Customer`, `Order`). Cada entidad contiene sus atributos y métodos que garantizan invariantes del dominio.
- `Aggregates/` – Raíces de agregados que agrupan entidades relacionadas y definen límites de consistencia. Por ejemplo, un `OrderAggregate` que contiene la entidad `Order` y sus `OrderLine`.
- `ValueObjects/` – Objetos que se definen únicamente por sus valores (por ejemplo, `Money`, `Address`). Son inmutables y se comparan por igualdad de valores.
- `Enums/` – Enumeraciones que describen estados o tipos dentro del dominio (por ejemplo, `OrderStatus`).
- `Events/` – Eventos de dominio que representan algo que ha ocurrido en el modelo (por ejemplo, `CustomerCreatedEvent`). Son útiles para arquitectura basada en eventos o para notificaciones internas.
- `Repositories/` – **Interfaces** que describen los contratos de persistencia (por ejemplo, `ICustomerRepository`). Las implementaciones reales vivirán en la capa de Infraestructura.

**Cómo se integra con el resto del proyecto**

1. **Dependencias**: La capa de dominio **no depende** de ninguna otra capa. Sólo referencia a tipos del propio dominio y a .NET estándar.
2. **Uso por la capa de Aplicación**: Los casos de uso (en `MySaaSAgent.Application`) consumen entidades, value objects y repositorios definidos aquí. Cuando la aplicación necesita crear o consultar datos, llama a los métodos de los repositorios (interfaces) y trabaja con las entidades del dominio.
3. **Persistencia**: La capa de Infraestructura implementa las interfaces de repositorio y traduce entidades a modelos de base de datos. La lógica de negocio sigue estando en el dominio.

**Buenas prácticas**

- Mantener la lógica de negocio (validaciones, invariantes) dentro de las entidades o value objects, nunca en la capa de aplicación o infraestructura.
- Evitar referencias a frameworks externos (por ejemplo, Entity Framework) dentro de esta capa.
- Utilizar eventos de dominio para comunicar cambios importantes a otras partes del sistema.
- Documentar cada entidad y sus invariantes con comentarios XML para que los desarrolladores comprendan las reglas de negocio.

## 📝 Ejemplos de Implementación

### 1. Entidad Rica (Rich Domain Model)
Así se ve una entidad que se protege a sí misma (DDD puro):

```csharp
public class Subscription
{
    // Propiedades de solo lectura desde fuera (private set)
    public Guid Id { get; private set; }
    public string Status { get; private set; }
    public DateTime? RenewalDate { get; private set; }

    // Constructor que obliga a tener datos válidos al nacer
    public Subscription(Guid userId, string plan)
    {
        if (userId == Guid.Empty) throw new ArgumentException("Usuario requerido");
        Id = Guid.NewGuid();
        UserId = userId;
        Plan = plan;
        Status = "active"; // Estado inicial por defecto
    }

    // Métodos semánticos para modificar el estado
    public void Cancel()
    {
        Status = "cancelled";
        RenewalDate = null;
    }
}
```

### 2. Aggregate Root (El Jefe)
Cómo el Aggregate coordina a sus entidades hijas:

```csharp
public class LeadAggregate
{
    public Lead Lead { get; private set; } // La raíz
    private readonly List<LeadInteraction> _interactions = new(); // Los hijos

    public void AddInteraction(string message, string channel)
    {
        // 1. Crea la interacción (el hijo)
        var interaction = new LeadInteraction(Lead.Id, "user", channel, message);
        _interactions.Add(interaction);

        // 2. Actualiza al padre (sincronización automática)
        Lead.RecordInteraction(); // Actualiza LastInteractionAt
    }
}
```

## 🧪 Cómo probar esta capa
Las pruebas unitarias aquí son rápidas y no requieren base de datos.
```bash
dotnet test MySaaSAgent.Tests --filter "Category=Domain"
```
# Ejecutar pruebas unitarias (si existen)
dotnet test
```

Las pruebas deben enfocarse en validar que las entidades y sus métodos mantienen los invariantes y que los eventos se disparan correctamente.
