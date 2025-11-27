# MySaaSAgent.Application

**Propósito**

Esta capa constituye la **capa de aplicación** o **capa de casos de uso**. Su responsabilidad es orquestar la lógica del dominio sin contener lógica de negocio propia. Actúa como un intermediario entre la API (entrada) y el dominio (reglas de negocio), coordinando flujos, validaciones de alto nivel y transformaciones de datos.

**Estructura de carpetas**

- `DTOs/` – Objetos de transferencia de datos (Data Transfer Objects). Representan la información que entra o sale de los casos de uso. Son simples POCOs sin lógica y sirven para desacoplar la API del dominio.
- `Interfaces/` – Contratos que la capa de aplicación necesita de otras capas. Por ejemplo, `ICustomerRepository`, `IEmailSender`. Las implementaciones reales viven en la capa de Infraestructura.
- `UseCases/` (o `Services/`) – Clases que implementan cada caso de uso del negocio, por ejemplo `CreateCustomerUseCase`, `GetOrdersUseCase`. Cada caso de uso recibe sus dependencias a través de inyección de dependencias (constructor) y ejecuta la lógica de dominio mediante entidades y repositorios.
- `Mappers/` o `Extensions/` – Código que convierte entre DTOs y entidades del dominio (y viceversa). Mantiene la capa de aplicación libre de lógica de dominio.

**Ejemplo de caso de uso (CreateCustomerUseCase)**
        // Aplicar reglas de negocio del dominio (por ejemplo, validar email)
        customer.Validate();
        // Persistir usando el repositorio
        await _repository.AddAsync(customer);
        // Enviar correo de bienvenida
        await _emailSender.SendWelcomeEmailAsync(customer.Email);
        // Convertir entidad a DTO de salida
        return new CustomerDto { Id = customer.Id, Name = customer.Name, Email = customer.Email };
    }
}
```

**Cómo se integra con el resto del proyecto**

1. **Dependencias**: La capa de aplicación depende únicamente del proyecto `MySaaSAgent.Domain` (entidades, value objects) y de sus propias interfaces. No referencia a la capa de Infraestructura ni a la API.
2. **Inyección de dependencias (DI)**: En `MySaaSAgent.Infrastructure/IoC` se registran las implementaciones de las interfaces definidas aquí. La API, al iniciar, recibe los casos de uso a través del contenedor.
3. **Flujo típico**:
   - Un controlador de la API llama a un caso de uso mediante su interfaz (`ICreateCustomerUseCase`).
   - El caso de uso valida reglas de negocio usando entidades del dominio y persiste datos mediante `ICustomerRepository`.
   - El caso de uso devuelve un DTO que el controlador envía como respuesta.

**Buenas prácticas**

- Mantener cada caso de uso enfocado en una única acción del negocio (principio de responsabilidad única).
- No colocar lógica de infraestructura (acceso a base de datos, llamadas HTTP) aquí; delegar a las interfaces.
- Utilizar DTOs para evitar exponer entidades del dominio directamente a la API.
- Aplicar validaciones de entrada en la capa de aplicación cuando son reglas de negocio (no solo de formato).
- Documentar cada caso de uso con comentarios XML para que Swagger pueda generar descripciones.

**Cómo ejecutar pruebas de la capa de aplicación**

```bash
# Desde la raíz del proyecto
cd MySaaSAgent.Application
dotnet test   # Si existen tests unitarios en el proyecto
```

Esto ejecutará los tests que verifiquen cada caso de uso de forma aislada, usando mocks para las interfaces de infraestructura.


**Propósito**

Esta capa constituye la **capa de aplicación** o **capa de casos de uso**. Su responsabilidad es orquestar la lógica del dominio sin contener lógica de negocio propia. Actúa como un intermediario entre la API (entrada) y el dominio (reglas de negocio), coordinando flujos, validaciones de alto nivel y transformaciones de datos.

**Estructura de carpetas**

- `DTOs/` – Objetos de transferencia de datos (Data Transfer Objects). Representan la información que entra o sale de los casos de uso. Son simples POCOs sin lógica y sirven para desacoplar la API del dominio.
- `Interfaces/` – Contratos que la capa de aplicación necesita de otras capas. Por ejemplo, `ICustomerRepository`, `IEmailSender`. Las implementaciones reales viven en la capa de Infraestructura.
- `UseCases/` (o `Services/`) – Clases que implementan cada caso de uso del negocio, por ejemplo `CreateCustomerUseCase`, `GetOrdersUseCase`. Cada caso de uso recibe sus dependencias a través de inyección de dependencias (constructor) y ejecuta la lógica de dominio mediante entidades y repositorios.
- `Mappers/` o `Extensions/` – Código que convierte entre DTOs y entidades del dominio (y viceversa). Mantiene la capa de aplicación libre de lógica de dominio.

**Cómo se integra con el resto del proyecto**

1. **Dependencias**: La capa de aplicación depende únicamente del proyecto `MySaaSAgent.Domain` (entidades, value objects) y de sus propias interfaces. No referencia a la capa de Infraestructura ni a la API.
2. **Inyección de dependencias (DI)**: En `MySaaSAgent.Infrastructure/IoC` se registran las implementaciones de las interfaces definidas aquí. La API, al iniciar, recibe los casos de uso a través del contenedor.
3. **Flujo típico**:
   - Un controlador de la API llama a un caso de uso mediante su interfaz (`ICreateCustomerUseCase`).
   - El caso de uso valida reglas de negocio usando entidades del dominio y persiste datos mediante `ICustomerRepository`.
   - El caso de uso devuelve un DTO que el controlador envía como respuesta.

**Buenas prácticas**

- Mantener cada caso de uso enfocado en una única acción del negocio (principio de responsabilidad única).
- No colocar lógica de infraestructura (acceso a base de datos, llamadas HTTP) aquí; delegar a las interfaces.
- Utilizar DTOs para evitar exponer entidades del dominio directamente a la API.
- Aplicar validaciones de entrada en la capa de aplicación cuando son reglas de negocio (no solo de formato).
- Documentar cada caso de uso con comentarios XML para que Swagger pueda generar descripciones.

**Cómo ejecutar pruebas de la capa de aplicación**

```bash
# Desde la raíz del proyecto
cd MySaaSAgent.Application
dotnet test   # Si existen tests unitarios en el proyecto
```

Esto ejecutará los tests que verifiquen cada caso de uso de forma aislada, usando mocks para las interfaces de infraestructura.
