# MeetLines.API

**PropÃ³sito**

Este proyecto es el punto de entrada de la aplicaciÃ³n a travÃ©s de una **Web API** basada en ASP.NET Core. Su Ãºnica responsabilidad es recibir peticiones HTTP, convertirlas en llamadas a la capa de AplicaciÃ³n (casos de uso) y devolver respuestas HTTP apropiadas. No contiene lÃ³gica de negocio ni acceso a datos; esas responsabilidades estÃ¡n delegadas a capas inferiores.

**Estructura de carpetas**

- `Controllers/` â€“ Controladores MVC/Web API. Cada controlador expone uno o varios endpoints (GET, POST, PUT, DELETE, etc.). Los controladores deben ser *muy ligeros*: validar la entrada, llamar a un caso de uso a travÃ©s de una interfaz y devolver el resultado (DTO o cÃ³digo de estado). No deben contener lÃ³gica de dominio.
- `Filters/` â€“ Filtros de acciÃ³n y de excepciÃ³n que se ejecutan antes o despuÃ©s de los controladores. AquÃ­ se pueden colocar lÃ³gica transversal como logging, manejo de errores global, autorizaciÃ³n, validaciÃ³n de modelos, etc.
- `Middlewares/` â€“ Middleware de la tuberÃ­a HTTP de ASP.NET Core. Ãštil para cosas como trazado de solicitudes, compresiÃ³n, CORS, autenticaciÃ³n basada en JWT, etc.
- `Program.cs` (y opcional `Startup.cs`) â€“ Punto de arranque de la aplicaciÃ³n. Configura el host, registra los servicios en el contenedor de DI, habilita Swagger/OpenAPI, configura CORS, etc.
- `appsettings.json` y `appsettings.Development.json` â€“ Archivo(s) de configuraciÃ³n. AquÃ­ se definen cadenas de conexiÃ³n, URLs de servicios externos, banderas de caracterÃ­sticas y cualquier otro valor que la aplicaciÃ³n necesite en tiempo de ejecuciÃ³n.

**Ejemplo de controlador**
        // AquÃ­ se llamarÃ­a a un caso de uso GetCustomerUseCase (no implementado aÃºn)
        return Ok();
    }
}
```

**CÃ³mo se integra con el resto del proyecto**

1. **Dependencias**: La capa API solo referencia al proyecto `MeetLines.Application`. A travÃ©s de interfaces definidas en la capa de AplicaciÃ³n, la API solicita servicios (por ejemplo, `ICreateCustomerUseCase`).
2. **InyecciÃ³n de dependencias (DI)**: En `Program.cs` se registra el contenedor IoC que resuelve esas interfaces con implementaciones concretas que viven en `MeetLines.Infrastructure`.
3. **Flujo tÃ­pico**:
   - El cliente envÃ­a una peticiÃ³n HTTP.
   - ASP.NET Core dirige la peticiÃ³n al controlador correspondiente.
   - El controlador llama a un caso de uso (`ICreateCustomerUseCase`).
   - El caso de uso usa repositorios (interfaces) para acceder a datos; la infraestructura provee la implementaciÃ³n.
   - El caso de uso devuelve un DTO que el controlador envÃ­a como respuesta JSON.

**Buenas prÃ¡cticas**

- Mantener los controladores *thin* (mÃ­nima lÃ³gica).
- Usar DTOs para la entrada/salida y mapearlos a entidades del dominio en la capa de AplicaciÃ³n.
- Manejar errores mediante filtros de excepciÃ³n y devolver cÃ³digos HTTP correctos.
- Documentar los endpoints con Swagger y anotaciones XML.
- No colocar lÃ³gica de negocio ni acceso a bases de datos aquÃ­.

**CÃ³mo ejecutar la API**

```bash
# Desde la raÃ­z del proyecto
cd MeetLines.API
dotnet run
```

Esto iniciarÃ¡ el servidor en `https://localhost:5001` (o el puerto configurado) y podrÃ¡s explorar la documentaciÃ³n Swagger en `https://localhost:5001/swagger`.


**PropÃ³sito**

Este proyecto es el punto de entrada de la aplicaciÃ³n a travÃ©s de una **Web API** basada en ASP.NET Core. Su Ãºnica responsabilidad es recibir peticiones HTTP, convertirlas en llamadas a la capa de AplicaciÃ³n (casos de uso) y devolver respuestas HTTP apropiadas. No contiene lÃ³gica de negocio ni acceso a datos; esas responsabilidades estÃ¡n delegadas a capas inferiores.

**Estructura de carpetas**

- `Controllers/` â€“ Controladores MVC/Web API. Cada controlador expone uno o varios endpoints (GET, POST, PUT, DELETE, etc.). Los controladores deben ser *muy ligeros*: validar la entrada, llamar a un caso de uso a travÃ©s de una interfaz y devolver el resultado (DTO o cÃ³digo de estado). No deben contener lÃ³gica de dominio.
- `Filters/` â€“ Filtros de acciÃ³n y de excepciÃ³n que se ejecutan antes o despuÃ©s de los controladores. AquÃ­ se pueden colocar lÃ³gica transversal como logging, manejo de errores global, autorizaciÃ³n, validaciÃ³n de modelos, etc.
- `Middlewares/` â€“ Middleware de la tuberÃ­a HTTP de ASP.NET Core. Ãštil para cosas como trazado de solicitudes, compresiÃ³n, CORS, autenticaciÃ³n basada en JWT, etc.
- `Program.cs` (y opcional `Startup.cs`) â€“ Punto de arranque de la aplicaciÃ³n. Configura el host, registra los servicios en el contenedor de DI, habilita Swagger/OpenAPI, configura CORS, etc.
- `appsettings.json` y `appsettings.Development.json` â€“ Archivo(s) de configuraciÃ³n. AquÃ­ se definen cadenas de conexiÃ³n, URLs de servicios externos, banderas de caracterÃ­sticas y cualquier otro valor que la aplicaciÃ³n necesite en tiempo de ejecuciÃ³n.

**CÃ³mo se integra con el resto del proyecto**

1. **Dependencias**: La capa API solo referencia al proyecto `MeetLines.Application`. A travÃ©s de interfaces definidas en la capa de AplicaciÃ³n, la API solicita servicios (por ejemplo, `ICustomerService`).
2. **InyecciÃ³n de dependencias (DI)**: En `Program.cs` se registra el contenedor IoC que resuelve esas interfaces con implementaciones concretas que viven en `MeetLines.Infrastructure`.
3. **Flujo tÃ­pico**:
   - El cliente envÃ­a una peticiÃ³n HTTP.
   - ASP.NET Core dirige la peticiÃ³n al controlador correspondiente.
   - El controlador llama a un caso de uso (`ICreateCustomerUseCase`).
   - El caso de uso usa repositorios (interfaces) para acceder a datos; la infraestructura provee la implementaciÃ³n.
   - El caso de uso devuelve un DTO que el controlador envÃ­a como respuesta JSON.

**Buenas prÃ¡cticas**

- Mantener los controladores *thin* (mÃ­nima lÃ³gica). 
- Usar DTOs para la entrada/salida y mapearlos a entidades del dominio en la capa de AplicaciÃ³n.
- Manejar errores mediante filtros de excepciÃ³n y devolver cÃ³digos HTTP correctos.
- Documentar los endpoints con Swagger y anotaciones XML.
- No colocar lÃ³gica de negocio ni acceso a bases de datos aquÃ­.

**CÃ³mo ejecutar la API**

```bash
# Desde la raÃ­z del proyecto
cd MeetLines.API
dotnet run
```

Esto iniciarÃ¡ el servidor en `https://localhost:5001` (o el puerto configurado) y podrÃ¡s explorar la documentaciÃ³n Swagger en `https://localhost:5001/swagger`.
