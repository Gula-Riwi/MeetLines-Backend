# SharedKernel

**Propósito**

Esta capa contiene **código compartido** que puede ser reutilizado por cualquier otra capa del sistema (API, Aplicación, Dominio, Infraestructura). Su objetivo es evitar duplicación y proporcionar una única fuente de verdad para conceptos genéricos que no pertenecen a un dominio específico.

**Qué debe ir aquí**

- **Tipos y estructuras comunes** – Por ejemplo, `Result<T>`, `Maybe<T>`, `Error`, que facilitan la comunicación de resultados y manejo de errores entre capas.
- **Extensiones y helpers** – Métodos de extensión genéricos (validaciones, conversiones, logging wrappers) que pueden usarse en cualquier proyecto.
- **Constantes y enumeraciones globales** – Valores que se comparten en todo el sistema, como códigos de error, nombres de configuración, etc.
- **Interfaces base** – Contratos genéricos que pueden ser implementados por distintas capas, por ejemplo `IAuditable`, `IEntity`, `IEvent`.
- **Utilidades de dominio neutro** – Funciones de hashing, generación de IDs, utilidades de fecha/hora, etc.

**Cómo se integra con el resto del proyecto**

1. **Dependencias**: Todas las demás capas (`API`, `Application`, `Domain`, `Infrastructure`) pueden referenciar este proyecto. No hay referencias inversas.
2. **Uso en código**: Cuando una capa necesita un tipo o helper genérico, simplemente importa el namespace `SharedKernel`. Por ejemplo, un controlador puede devolver `Result<CustomerDto>` para indicar éxito o error sin depender de la capa de dominio.
3. **Mantenimiento**: Al agregar una nueva utilidad o tipo común, se coloca aquí para que esté disponible globalmente, evitando crear copias en cada proyecto.

**Buenas prácticas**

- Mantener la capa **independiente** de frameworks específicos (no usar `Microsoft.AspNetCore.*` ni `EntityFramework` aquí).
- Documentar cada tipo y método con comentarios XML para que aparezcan en la documentación de los demás proyectos.
- Evitar lógica de negocio; solo incluir conceptos genéricos y utilitarios.

**Cómo probar la capa SharedKernel**

```bash
# Desde la raíz del proyecto
cd SharedKernel
# Ejecutar pruebas unitarias (si existen)
dotnet test
```

Las pruebas deben cubrir los helpers, tipos genéricos y cualquier lógica de utilidad para garantizar su correcto funcionamiento en todas las capas que lo consumen.
