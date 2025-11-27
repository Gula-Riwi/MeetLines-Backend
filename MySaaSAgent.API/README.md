# MySaaSAgent.API

**Propósito**

Este proyecto es el punto de entrada de la aplicación a través de una **Web API** basada en ASP.NET Core. Su única responsabilidad es recibir peticiones HTTP, convertirlas en llamadas a la capa de Aplicación (casos de uso) y devolver respuestas HTTP apropiadas. No contiene lógica de negocio ni acceso a datos; esas responsabilidades están delegadas a capas inferiores.

**Estructura de carpetas**

- `Controllers/` – Controladores MVC/Web API. Cada controlador expone uno o varios endpoints (GET, POST, PUT, DELETE, etc.). Los controladores deben ser *muy ligeros*: validar la entrada, llamar a un caso de uso a través de una interfaz y devolver el resultado (DTO o código de estado). No deben contener lógica de dominio.
- `Filters/` – Filtros de acción y de excepción que se ejecutan antes o después de los controladores. Aquí se pueden colocar lógica transversal como logging, manejo de errores global, autorización, validación de modelos, etc.
- `Middlewares/` – Middleware de la tubería HTTP de ASP.NET Core. Útil para cosas como trazado de solicitudes, compresión, CORS, autenticación basada en JWT, etc.
- `Program.cs` (y opcional `Startup.cs`) – Punto de arranque de la aplicación. Configura el host, registra los servicios en el contenedor de DI, habilita Swagger/OpenAPI, configura CORS, etc.
- `appsettings.json` y `appsettings.Development.json` – Archivo(s) de configuración. Aquí se definen cadenas de conexión, URLs de servicios externos, banderas de características y cualquier otro valor que la aplicación necesite en tiempo de ejecución.

**Ejemplo de controlador**
        // Aquí se llamaría a un caso de uso GetCustomerUseCase (no implementado aún)
        return Ok();
    }
}
```

**Cómo se integra con el resto del proyecto**

1. **Dependencias**: La capa API solo referencia al proyecto `MySaaSAgent.Application`. A través de interfaces definidas en la capa de Aplicación, la API solicita servicios (por ejemplo, `ICreateCustomerUseCase`).
2. **Inyección de dependencias (DI)**: En `Program.cs` se registra el contenedor IoC que resuelve esas interfaces con implementaciones concretas que viven en `MySaaSAgent.Infrastructure`.
3. **Flujo típico**:
   - El cliente envía una petición HTTP.
   - ASP.NET Core dirige la petición al controlador correspondiente.
   - El controlador llama a un caso de uso (`ICreateCustomerUseCase`).
   - El caso de uso usa repositorios (interfaces) para acceder a datos; la infraestructura provee la implementación.
   - El caso de uso devuelve un DTO que el controlador envía como respuesta JSON.

**Buenas prácticas**

- Mantener los controladores *thin* (mínima lógica).
- Usar DTOs para la entrada/salida y mapearlos a entidades del dominio en la capa de Aplicación.
- Manejar errores mediante filtros de excepción y devolver códigos HTTP correctos.
- Documentar los endpoints con Swagger y anotaciones XML.
- No colocar lógica de negocio ni acceso a bases de datos aquí.

**Cómo ejecutar la API**

```bash
# Desde la raíz del proyecto
cd MySaaSAgent.API
dotnet run
```

Esto iniciará el servidor en `https://localhost:5001` (o el puerto configurado) y podrás explorar la documentación Swagger en `https://localhost:5001/swagger`.


**Propósito**

Este proyecto es el punto de entrada de la aplicación a través de una **Web API** basada en ASP.NET Core. Su única responsabilidad es recibir peticiones HTTP, convertirlas en llamadas a la capa de Aplicación (casos de uso) y devolver respuestas HTTP apropiadas. No contiene lógica de negocio ni acceso a datos; esas responsabilidades están delegadas a capas inferiores.

**Estructura de carpetas**

- `Controllers/` – Controladores MVC/Web API. Cada controlador expone uno o varios endpoints (GET, POST, PUT, DELETE, etc.). Los controladores deben ser *muy ligeros*: validar la entrada, llamar a un caso de uso a través de una interfaz y devolver el resultado (DTO o código de estado). No deben contener lógica de dominio.
- `Filters/` – Filtros de acción y de excepción que se ejecutan antes o después de los controladores. Aquí se pueden colocar lógica transversal como logging, manejo de errores global, autorización, validación de modelos, etc.
- `Middlewares/` – Middleware de la tubería HTTP de ASP.NET Core. Útil para cosas como trazado de solicitudes, compresión, CORS, autenticación basada en JWT, etc.
- `Program.cs` (y opcional `Startup.cs`) – Punto de arranque de la aplicación. Configura el host, registra los servicios en el contenedor de DI, habilita Swagger/OpenAPI, configura CORS, etc.
- `appsettings.json` y `appsettings.Development.json` – Archivo(s) de configuración. Aquí se definen cadenas de conexión, URLs de servicios externos, banderas de características y cualquier otro valor que la aplicación necesite en tiempo de ejecución.

**Cómo se integra con el resto del proyecto**

1. **Dependencias**: La capa API solo referencia al proyecto `MySaaSAgent.Application`. A través de interfaces definidas en la capa de Aplicación, la API solicita servicios (por ejemplo, `ICustomerService`).
2. **Inyección de dependencias (DI)**: En `Program.cs` se registra el contenedor IoC que resuelve esas interfaces con implementaciones concretas que viven en `MySaaSAgent.Infrastructure`.
3. **Flujo típico**:
   - El cliente envía una petición HTTP.
   - ASP.NET Core dirige la petición al controlador correspondiente.
   - El controlador llama a un caso de uso (`ICreateCustomerUseCase`).
   - El caso de uso usa repositorios (interfaces) para acceder a datos; la infraestructura provee la implementación.
   - El caso de uso devuelve un DTO que el controlador envía como respuesta JSON.

**Buenas prácticas**

- Mantener los controladores *thin* (mínima lógica). 
- Usar DTOs para la entrada/salida y mapearlos a entidades del dominio en la capa de Aplicación.
- Manejar errores mediante filtros de excepción y devolver códigos HTTP correctos.
- Documentar los endpoints con Swagger y anotaciones XML.
- No colocar lógica de negocio ni acceso a bases de datos aquí.

**Cómo ejecutar la API**

```bash
# Desde la raíz del proyecto
cd MySaaSAgent.API
dotnet run
```

Esto iniciará el servidor en `https://localhost:5001` (o el puerto configurado) y podrás explorar la documentación Swagger en `https://localhost:5001/swagger`.
