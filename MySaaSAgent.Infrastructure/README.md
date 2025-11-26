# MySaaSAgent.Infrastructure

Esta capa contiene **implementaciones concretas** de los contratos definidos en la capa de Aplicación/Dominio.

## Qué debe ir aquí
- **ExternalServices** – Clientes o adaptadores para APIs externas, servicios de mensajería, etc.
- **IoC** – Configuración del contenedor de inyección de dependencias (por ejemplo, `services.AddScoped<IRepository, RepositoryImplementation>()`). Aquí se registran todas las implementaciones de interfaces.
- **Data Access** – Implementaciones de repositorios que acceden a bases de datos (EF Core, Dapper, etc.)
- **Migrations / DB Context** – Si usas Entity Framework, los `DbContext` y migraciones se ubican aquí.

La infraestructura **solo depende** de la capa de Dominio (entidades, value objects) y de paquetes externos; nunca depende de la API.
