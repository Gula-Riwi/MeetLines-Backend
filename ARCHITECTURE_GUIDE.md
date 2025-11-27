# Guía de Arquitectura y Flujo de Trabajo (Hexagonal + DDD)

Este documento es la **referencia oficial** para el equipo de desarrollo de **MySaaSAgent**. Aquí explicamos cómo trabajar siguiendo los principios de **Arquitectura Hexagonal (Ports & Adapters)** y **Domain-Driven Design (DDD)** utilizando nuestra estructura de carpetas actual.

---

## 1. Mapa Mental de la Arquitectura 🗺️

Nuestro objetivo es proteger la lógica de negocio (el núcleo) de los detalles técnicos (bases de datos, frameworks, APIs).

### El Núcleo (El Hexágono)
Es sagrado. Aquí vive el negocio. No sabe si corre en web, consola o móvil.
*   **Domain:** Entidades, Reglas, Lógica pura.
*   **Application:** Casos de Uso (Orquestación).

### Los Puertos (Ports) 🔌
Son las **Interfaces** que definen cómo entrar o salir del núcleo.
*   **Puertos de Entrada (Input Ports):** Interfaces de los Casos de Uso (ej. `ICreateOrderUseCase`). Definen qué puede hacer el sistema.
*   **Puertos de Salida (Output Ports):** Interfaces de Repositorios y Servicios (ej. `IOrderRepository`, `IEmailService`). Definen qué necesita el sistema del mundo exterior.

### Los Adaptadores (Adapters) 🔌
Son las implementaciones reales que se conectan a los puertos.
*   **Adaptador Conductor (Driving Adapter):** La **API**. Recibe HTTP y llama a los *Input Ports*.
*   **Adaptador Conducido (Driven Adapter):** La **Infrastructure**. Implementa los *Output Ports* (SQL, SMTP, etc.).

---

## 2. Flujo de Trabajo Paso a Paso 👣

Cuando tengas que implementar una nueva funcionalidad (ej. "Crear Cliente"), sigue estrictamente este orden:

### Paso 1: Domain (El Corazón) ❤️
*Ubicación: `MySaaSAgent.Domain`*
1.  Define la **Entidad** en `Entities/`.
    *   Propiedades privadas (`private set`).
    *   Constructor con validaciones de negocio.
    *   Métodos de dominio (ej. `ActivarCliente()`).
2.  Define el **Contrato del Repositorio** (Output Port) en `Repositories/`.
    *   Interfaz `ICustomerRepository` (solo métodos necesarios: `Add`, `Find`).

### Paso 2: Application (El Cerebro) 🧠
*Ubicación: `MySaaSAgent.Application`*
1.  Crea los **DTOs** en `DTOs/`.
    *   `CreateCustomerRequest` (entrada) y `CustomerDto` (salida).
2.  Define la **Interfaz del Caso de Uso** (Input Port) en `Interfaces/`.
    *   `ICreateCustomerUseCase`.
3.  Implementa el **Caso de Uso** en `UseCases/`.
    *   Recibe `ICustomerRepository` por constructor.
    *   Convierte DTO → Entidad.
    *   Ejecuta lógica.
    *   Guarda usando el repositorio.
    *   Devuelve DTO.

### Paso 3: Infrastructure (Los Cables) 🛠️
*Ubicación: `MySaaSAgent.Infrastructure`*
1.  Implementa el **Repositorio Real** en `Data/Repositories/`.
    *   Clase `CustomerRepository` que implementa `ICustomerRepository`.
    *   Usa Entity Framework (`DbContext`) para guardar en la DB.
2.  Registra la dependencia en `IoC/DependencyInjection.cs`.
    *   `services.AddScoped<ICustomerRepository, CustomerRepository>();`

### Paso 4: API (La Puerta) 🚪
*Ubicación: `MySaaSAgent.API`*
1.  Crea el **Controller** en `Controllers/`.
    *   Inyecta `ICreateCustomerUseCase`.
    *   Recibe HTTP POST.
    *   Llama al caso de uso.
    *   Devuelve `Ok()` o `BadRequest()`.

---

## 3. Reglas de Oro (Mandamientos) 📜

1.  🚫 **Domain NO toca nada:** El proyecto `Domain` no debe tener referencias a `Infrastructure`, `API`, ni librerías como Entity Framework o ASP.NET Core. Solo C# puro.
2.  🚫 **Application NO toca Infrastructure:** La capa `Application` solo conoce **Interfaces**. Nunca instancies una clase concreta de Infraestructura (ej. `new EmailService()`) dentro de un Caso de Uso.
3.  🚫 **Entidades NO salen a la API:** Nunca devuelvas una Entidad de Dominio (ej. `Customer`) en un Controller. Siempre conviértela a un `DTO`.
4.  ✅ **Lógica en su lugar:**
    *   ¿Es una regla de negocio ("Edad > 18")? -> **Domain (Entidad)**.
    *   ¿Es flujo de datos ("Buscar, Validar, Guardar")? -> **Application (Caso de Uso)**.

---

## 4. Estructura de Carpetas vs Conceptos

| Concepto Hexagonal | Carpeta en nuestro Proyecto |
| :--- | :--- |
| **Hexágono (Núcleo)** | `MySaaSAgent.Domain` + `MySaaSAgent.Application` |
| **Input Port** | `MySaaSAgent.Application/Interfaces` (Casos de Uso) |
| **Output Port** | `MySaaSAgent.Domain/Repositories` (Interfaces) |
| **Driving Adapter** | `MySaaSAgent.API` (Controllers) |
| **Driven Adapter** | `MySaaSAgent.Infrastructure` (Implementaciones) |

---

*Sigue esta guía y mantendremos el código limpio, escalable y feliz.*

By: Jhon Rojas