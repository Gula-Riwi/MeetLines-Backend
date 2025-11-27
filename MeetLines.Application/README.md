# MeetLines.Application

**PropÃ³sito**

Esta capa constituye la **capa de aplicaciÃ³n** o **capa de casos de uso**. Su responsabilidad es orquestar la lÃ³gica del dominio sin contener lÃ³gica de negocio propia. ActÃºa como un intermediario entre la API (entrada) y el dominio (reglas de negocio), coordinando flujos, validaciones de alto nivel y transformaciones de datos.

**Estructura de carpetas**

- `DTOs/` â€“ Objetos de transferencia de datos (Data Transfer Objects). Representan la informaciÃ³n que entra o sale de los casos de uso. Son simples POCOs sin lÃ³gica y sirven para desacoplar la API del dominio.
- `Interfaces/` â€“ Contratos que la capa de aplicaciÃ³n necesita de otras capas. Por ejemplo, `ICustomerRepository`, `IEmailSender`. Las implementaciones reales viven en la capa de Infraestructura.
- `UseCases/` (o `Services/`) â€“ Clases que implementan cada caso de uso del negocio, por ejemplo `CreateCustomerUseCase`, `GetOrdersUseCase`. Cada caso de uso recibe sus dependencias a travÃ©s de inyecciÃ³n de dependencias (constructor) y ejecuta la lÃ³gica de dominio mediante entidades y repositorios.
- `Mappers/` o `Extensions/` â€“ CÃ³digo que convierte entre DTOs y entidades del dominio (y viceversa). Mantiene la capa de aplicaciÃ³n libre de lÃ³gica de dominio.

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

**CÃ³mo se integra con el resto del proyecto**

1. **Dependencias**: La capa de aplicaciÃ³n depende Ãºnicamente del proyecto `MeetLines.Domain` (entidades, value objects) y de sus propias interfaces. No referencia a la capa de Infraestructura ni a la API.
2. **InyecciÃ³n de dependencias (DI)**: En `MeetLines.Infrastructure/IoC` se registran las implementaciones de las interfaces definidas aquÃ­. La API, al iniciar, recibe los casos de uso a travÃ©s del contenedor.
3. **Flujo tÃ­pico**:
   - Un controlador de la API llama a un caso de uso mediante su interfaz (`ICreateCustomerUseCase`).
   - El caso de uso valida reglas de negocio usando entidades del dominio y persiste datos mediante `ICustomerRepository`.
   - El caso de uso devuelve un DTO que el controlador envÃ­a como respuesta.

**Buenas prÃ¡cticas**

- Mantener cada caso de uso enfocado en una Ãºnica acciÃ³n del negocio (principio de responsabilidad Ãºnica).
- No colocar lÃ³gica de infraestructura (acceso a base de datos, llamadas HTTP) aquÃ­; delegar a las interfaces.
- Utilizar DTOs para evitar exponer entidades del dominio directamente a la API.
- Aplicar validaciones de entrada en la capa de aplicaciÃ³n cuando son reglas de negocio (no solo de formato).
- Documentar cada caso de uso con comentarios XML para que Swagger pueda generar descripciones.

**CÃ³mo ejecutar pruebas de la capa de aplicaciÃ³n**

```bash
# Desde la raÃ­z del proyecto
cd MeetLines.Application
dotnet test   # Si existen tests unitarios en el proyecto
```

Esto ejecutarÃ¡ los tests que verifiquen cada caso de uso de forma aislada, usando mocks para las interfaces de infraestructura.


**PropÃ³sito**

Esta capa constituye la **capa de aplicaciÃ³n** o **capa de casos de uso**. Su responsabilidad es orquestar la lÃ³gica del dominio sin contener lÃ³gica de negocio propia. ActÃºa como un intermediario entre la API (entrada) y el dominio (reglas de negocio), coordinando flujos, validaciones de alto nivel y transformaciones de datos.

**Estructura de carpetas**

- `DTOs/` â€“ Objetos de transferencia de datos (Data Transfer Objects). Representan la informaciÃ³n que entra o sale de los casos de uso. Son simples POCOs sin lÃ³gica y sirven para desacoplar la API del dominio.
- `Interfaces/` â€“ Contratos que la capa de aplicaciÃ³n necesita de otras capas. Por ejemplo, `ICustomerRepository`, `IEmailSender`. Las implementaciones reales viven en la capa de Infraestructura.
- `UseCases/` (o `Services/`) â€“ Clases que implementan cada caso de uso del negocio, por ejemplo `CreateCustomerUseCase`, `GetOrdersUseCase`. Cada caso de uso recibe sus dependencias a travÃ©s de inyecciÃ³n de dependencias (constructor) y ejecuta la lÃ³gica de dominio mediante entidades y repositorios.
- `Mappers/` o `Extensions/` â€“ CÃ³digo que convierte entre DTOs y entidades del dominio (y viceversa). Mantiene la capa de aplicaciÃ³n libre de lÃ³gica de dominio.

**CÃ³mo se integra con el resto del proyecto**

1. **Dependencias**: La capa de aplicaciÃ³n depende Ãºnicamente del proyecto `MeetLines.Domain` (entidades, value objects) y de sus propias interfaces. No referencia a la capa de Infraestructura ni a la API.
2. **InyecciÃ³n de dependencias (DI)**: En `MeetLines.Infrastructure/IoC` se registran las implementaciones de las interfaces definidas aquÃ­. La API, al iniciar, recibe los casos de uso a travÃ©s del contenedor.
3. **Flujo tÃ­pico**:
   - Un controlador de la API llama a un caso de uso mediante su interfaz (`ICreateCustomerUseCase`).
   - El caso de uso valida reglas de negocio usando entidades del dominio y persiste datos mediante `ICustomerRepository`.
   - El caso de uso devuelve un DTO que el controlador envÃ­a como respuesta.

**Buenas prÃ¡cticas**

- Mantener cada caso de uso enfocado en una Ãºnica acciÃ³n del negocio (principio de responsabilidad Ãºnica).
- No colocar lÃ³gica de infraestructura (acceso a base de datos, llamadas HTTP) aquÃ­; delegar a las interfaces.
- Utilizar DTOs para evitar exponer entidades del dominio directamente a la API.
- Aplicar validaciones de entrada en la capa de aplicaciÃ³n cuando son reglas de negocio (no solo de formato).
- Documentar cada caso de uso con comentarios XML para que Swagger pueda generar descripciones.

**CÃ³mo ejecutar pruebas de la capa de aplicaciÃ³n**

```bash
# Desde la raÃ­z del proyecto
cd MeetLines.Application
dotnet test   # Si existen tests unitarios en el proyecto
```

Esto ejecutarÃ¡ los tests que verifiquen cada caso de uso de forma aislada, usando mocks para las interfaces de infraestructura.
