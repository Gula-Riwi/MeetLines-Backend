# GuÃ­a de Arquitectura y Flujo de Trabajo (Hexagonal + DDD)

Este documento es la **referencia oficial** para el equipo de desarrollo de **MeetLines**. AquÃ­ explicamos cÃ³mo trabajar siguiendo los principios de **Arquitectura Hexagonal (Ports & Adapters)** y **Domain-Driven Design (DDD)** utilizando nuestra estructura de carpetas actual.

---

## 1. Mapa Mental de la Arquitectura ðŸ—ºï¸

Nuestro objetivo es proteger la lÃ³gica de negocio (el nÃºcleo) de los detalles tÃ©cnicos (bases de datos, frameworks, APIs).

### El NÃºcleo (El HexÃ¡gono)
Es sagrado. AquÃ­ vive el negocio. No sabe si corre en web, consola o mÃ³vil.
*   **Domain:** Entidades, Reglas, LÃ³gica pura.
*   **Application:** Casos de Uso (OrquestaciÃ³n).

### Los Puertos (Ports) ðŸ”Œ
Son las **Interfaces** que definen cÃ³mo entrar o salir del nÃºcleo.
*   **Puertos de Entrada (Input Ports):** Interfaces de los Casos de Uso (ej. `ICreateOrderUseCase`). Definen quÃ© puede hacer el sistema.
*   **Puertos de Salida (Output Ports):** Interfaces de Repositorios y Servicios (ej. `IOrderRepository`, `IEmailService`). Definen quÃ© necesita el sistema del mundo exterior.

### Los Adaptadores (Adapters) ðŸ”Œ
Son las implementaciones reales que se conectan a los puertos.
*   **Adaptador Conductor (Driving Adapter):** La **API**. Recibe HTTP y llama a los *Input Ports*.
*   **Adaptador Conducido (Driven Adapter):** La **Infrastructure**. Implementa los *Output Ports* (SQL, SMTP, etc.).

---

## 2. Flujo de Trabajo Paso a Paso ðŸ‘£

Cuando tengas que implementar una nueva funcionalidad (ej. "Crear Cliente"), sigue estrictamente este orden:

### Paso 1: Domain (El CorazÃ³n) â¤ï¸
*UbicaciÃ³n: `MeetLines.Domain`*
1.  Define la **Entidad** en `Entities/`.
    *   Propiedades privadas (`private set`).
    *   Constructor con validaciones de negocio.
    *   MÃ©todos de dominio (ej. `ActivarCliente()`).
2.  Define el **Contrato del Repositorio** (Output Port) en `Repositories/`.
    *   Interfaz `ICustomerRepository` (solo mÃ©todos necesarios: `Add`, `Find`).

### Paso 2: Application (El Cerebro) ðŸ§ 
*UbicaciÃ³n: `MeetLines.Application`*
1.  Crea los **DTOs** en `DTOs/`.
    *   `CreateCustomerRequest` (entrada) y `CustomerDto` (salida).
2.  Define la **Interfaz del Caso de Uso** (Input Port) en `Interfaces/`.
    *   `ICreateCustomerUseCase`.
3.  Implementa el **Caso de Uso** en `UseCases/`.
    *   Recibe `ICustomerRepository` por constructor.
    *   Convierte DTO â†’ Entidad.
    *   Ejecuta lÃ³gica.
    *   Guarda usando el repositorio.
    *   Devuelve DTO.

### Paso 3: Infrastructure (Los Cables) ðŸ› ï¸
*UbicaciÃ³n: `MeetLines.Infrastructure`*
1.  Implementa el **Repositorio Real** en `Data/Repositories/`.
    *   Clase `CustomerRepository` que implementa `ICustomerRepository`.
    *   Usa Entity Framework (`DbContext`) para guardar en la DB.
2.  Registra la dependencia en `IoC/DependencyInjection.cs`.
    *   `services.AddScoped<ICustomerRepository, CustomerRepository>();`

### Paso 4: API (La Puerta) ðŸšª
*UbicaciÃ³n: `MeetLines.API`*
1.  Crea el **Controller** en `Controllers/`.
    *   Inyecta `ICreateCustomerUseCase`.
    *   Recibe HTTP POST.
    *   Llama al caso de uso.
    *   Devuelve `Ok()` o `BadRequest()`.

---

## 3. Reglas de Oro (Mandamientos) ðŸ“œ

1.  ðŸš« **Domain NO toca nada:** El proyecto `Domain` no debe tener referencias a `Infrastructure`, `API`, ni librerÃ­as como Entity Framework o ASP.NET Core. Solo C# puro.
2.  ðŸš« **Application NO toca Infrastructure:** La capa `Application` solo conoce **Interfaces**. Nunca instancies una clase concreta de Infraestructura (ej. `new EmailService()`) dentro de un Caso de Uso.
3.  ðŸš« **Entidades NO salen a la API:** Nunca devuelvas una Entidad de Dominio (ej. `Customer`) en un Controller. Siempre conviÃ©rtela a un `DTO`.
4.  âœ… **LÃ³gica en su lugar:**
    *   Â¿Es una regla de negocio ("Edad > 18")? -> **Domain (Entidad)**.
    *   Â¿Es flujo de datos ("Buscar, Validar, Guardar")? -> **Application (Caso de Uso)**.

---

## 4. Estructura de Carpetas vs Conceptos

| Concepto Hexagonal | Carpeta en nuestro Proyecto |
| :--- | :--- |
| **HexÃ¡gono (NÃºcleo)** | `MeetLines.Domain` + `MeetLines.Application` |
| **Input Port** | `MeetLines.Application/Interfaces` (Casos de Uso) |
| **Output Port** | `MeetLines.Domain/Repositories` (Interfaces) |
| **Driving Adapter** | `MeetLines.API` (Controllers) |
| **Driven Adapter** | `MeetLines.Infrastructure` (Implementaciones) |

---

*Sigue esta guÃ­a y mantendremos el cÃ³digo limpio, escalable y feliz.*

By: Jhon Rojas