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

**Cómo probar la capa de dominio**

```bash
# Desde la raíz del proyecto
cd MySaaSAgent.Domain
# Ejecutar pruebas unitarias (si existen)
dotnet test
```

Las pruebas deben enfocarse en validar que las entidades y sus métodos mantienen los invariantes y que los eventos se disparan correctamente.
