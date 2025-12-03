# Implementación Multitenant - MeetLines Backend

## Descripción General

Este documento describe la implementación completa del sistema multitenant en MeetLines Backend. El sistema permite que cada usuario propietario tenga su propio proyecto con un subdominio único, accesible a través de URLs como `proyecto.meet-lines.com`.

## Componentes Implementados

### 1. **Middleware de Resolución de Tenant** (`TenantResolutionMiddleware`)

**Ubicación**: `MeetLines.API/Middleware/TenantResolutionMiddleware.cs`

**Responsabilidades**:
- Extrae el subdominio del host HTTP
- Valida que el subdominio sea válido usando `SubdomainValidator`
- Consulta la base de datos para obtener el proyecto asociado
- Resuelve el tenant (proyecto) y lo almacena en el contexto HTTP

**Flujo**:
```
Host: proyecto.meet-lines.com
    ↓
Extrae: "proyecto"
    ↓
Valida formato y reservados
    ↓
Busca Project con subdomain="proyecto"
    ↓
Si existe y status="active" → SetTenant()
    ↓
Si no existe → 404 Not Found
```

**Configuración requerida en `appsettings.json`**:
```json
"Multitenancy": {
  "BaseDomain": "meet-lines.com",
  "Protocol": "https",
  "Enabled": true
}
```

### 2. **Servicio de Tenant** (`ITenantService`)

**Ubicación**: 
- Interfaz: `MeetLines.Application/Services/ITenantService.cs`
- Implementación: `MeetLines.Infrastructure/Services/TenantService.cs`

**Responsabilidades**:
- Almacena el Tenant ID y Subdominio en el contexto HTTP
- Proporciona métodos para obtener el tenant actual

**Métodos**:
```csharp
Guid? GetCurrentTenantId()      // Obtiene el ID del proyecto (tenant)
string? GetCurrentSubdomain()    // Obtiene el subdominio
void SetTenant(Guid tenantId, string subdomain)  // Establece el tenant
```

### 3. **Servicio de Filtro de Query** (`ITenantQueryFilter`)

**Ubicación**:
- Interfaz: `MeetLines.Application/Services/ITenantQueryFilter.cs`
- Implementación: `MeetLines.Infrastructure/Services/TenantQueryFilter.cs`

**Responsabilidades**:
- Proporciona métodos para construir filtros de queries basados en el tenant actual
- Facilita a los repositorios acceder a la información del tenant

**Métodos**:
```csharp
Guid? GetCurrentTenantId()   // ID del tenant actual
string? GetCurrentSubdomain()  // Subdominio actual
bool HasActiveTenant()         // Verifica si hay un tenant activo
```

### 4. **Validador de Subdominio** (`SubdomainValidator`)

**Ubicación**: `MeetLines.Domain/ValueObjects/SubdomainValidator.cs`

**Reglas de Validación**:
- Formato: `^[a-z0-9]([a-z0-9-]{1,61}[a-z0-9])?$`
- Longitud: 3-63 caracteres
- Debe empezar y terminar con letra o número
- Puede contener guiones en el medio
- No puede ser un subdominio reservado (www, api, admin, auth, etc.)

### 5. **Repositorio de Proyectos** (`IProjectRepository`)

**Ubicación**: `MeetLines.Infrastructure/Repositories/ProjectRepository.cs`

**Métodos Clave para Multitenant**:
```csharp
async Task<Project?> GetBySubdomainAsync(string subdomain)
    // Busca un proyecto por su subdominio (usado por middleware)

async Task<bool> ExistsSubdomainAsync(string subdomain)
    // Verifica unicidad de subdominio
```

### 6. **Entidad Project** (`Project`)

**Ubicación**: `MeetLines.Domain/Entities/Project.cs`

**Propiedades**:
```csharp
Guid Id              // Identificador único
Guid UserId          // Dueño del proyecto
string Name          // Nombre del proyecto
string Subdomain     // Subdominio único (clave para multitenant)
string Status        // "active" o "disabled"
DateTimeOffset CreatedAt
DateTimeOffset UpdatedAt
```

**Estados**:
- `active`: Proyecto accesible
- `disabled`: Borrado lógico

### 7. **Casos de Uso de Proyectos**

Todos los casos de uso validan que el usuario sea propietario del proyecto:

- **CreateProjectUseCase**: Crea nuevo proyecto con subdominio único
- **GetProjectByIdUseCase**: Obtiene proyecto (solo si es propietario)
- **UpdateProjectUseCase**: Actualiza proyecto (solo si es propietario)
- **DeleteProjectUseCase**: Elimina (soft delete) proyecto
- **GetUserProjectsUseCase**: Lista todos los proyectos del usuario

**Ejemplo - CreateProjectUseCase**:
```
1. Valida suscripción activa
2. Verifica límite de proyectos según plan
3. Valida/genera subdominio
4. Verifica unicidad del subdominio
5. Crea el proyecto
6. Devuelve ProjectResponse con FullUrl
```

## Flujo de Solicitud Multitenant

```
┌─────────────────────────────────────────────────────────────┐
│  Solicitud HTTP a proyecto.meet-lines.com/api/...           │
└─────────────────────────────────────────────────────────────┘
                             ↓
┌─────────────────────────────────────────────────────────────┐
│ TenantResolutionMiddleware                                  │
│ - Extrae "proyecto" del host                                │
│ - Valida formato                                            │
│ - Busca Project WHERE subdomain="proyecto"                 │
└─────────────────────────────────────────────────────────────┘
                             ↓
                    ¿Proyecto existe?
                    /              \
                  SÍ               NO
                  ↓                 ↓
          SetTenant()           404 Not Found
             ↓
      Pasar al Controller
             ↓
      Verificar Autorización
             ↓
      Usar Caso de Uso
             ↓
      Respuesta
```

## Configuración en Program.cs

```csharp
// Middleware debe ejecutarse ANTES de Authentication
app.UseMiddleware<TenantResolutionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
```

## Seguridad

### Niveles de Validación

1. **Middleware**: Solo proyectos activos son accesibles
2. **Servicio de Tenant**: Validación de host
3. **Casos de Uso**: Verificación de propiedad (UserId)
4. **Repositorios**: Queries filtradas por userId

### Ejemplo - Protección en GetProjectById

```csharp
// 1. Verificar que proyecto existe
var project = await _projectRepository.GetAsync(projectId);
if (project == null) return Fail("Project not found");

// 2. Verificar que usuario es propietario
var isOwner = await _projectRepository.IsUserProjectOwnerAsync(userId, projectId);
if (!isOwner) return Fail("You do not have permission");
```

## DTOs Relacionados

### `CreateProjectRequest`
```csharp
string Name              // Requerido, 2-100 caracteres
string? Subdomain        // Opcional, 3-63 caracteres
string? Industry         // Opcional
string? Description      // Opcional, max 500 caracteres
```

### `ProjectResponse`
```csharp
Guid Id
string Name
string Subdomain
string FullUrl           // Ej: https://proyecto.meet-lines.com
string? Industry
string? Description
string Status
DateTimeOffset CreatedAt
DateTimeOffset UpdatedAt
```

### `UserProjectsResponse`
```csharp
IEnumerable<ProjectResponse> Projects
int TotalProjects
int MaxProjects          // Según plan de suscripción
```

## Registro de Dependencias

**Ubicación**: `MeetLines.Infrastructure/IoC/InfrastructureServiceCollectionExtensions.cs`

```csharp
// Servicios Multitenancy
services.AddHttpContextAccessor();
services.AddScoped<ITenantService, TenantService>();
services.AddScoped<ITenantQueryFilter, TenantQueryFilter>();
```

## Base de Datos

### Índice en tabla `projects`
```sql
CREATE UNIQUE INDEX idx_projects_subdomain ON projects(subdomain);
CREATE INDEX idx_projects_user ON projects(user_id);
```

## Límites por Plan

| Plan | Máximo de Proyectos |
|------|-------------------|
| beginner | 1 |
| intermediate | 2 |
| complete | Ilimitado |

## Validaciones de Subdominio Reservados

```csharp
"www", "api", "admin", "app", "dashboard", "cdn", "mail",
"ftp", "smtp", "pop", "imap", "meetlines", "support", "help",
"blog", "status", "dev", "staging", "test", "auth", "login",
"register", "signup", "signin", "account", "profile", "billing"
```

## Ejemplos de Uso

### 1. Crear un Proyecto
```
POST /api/projects
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "Mi Empresa",
  "subdomain": "miempresa",
  "industry": "Tecnología",
  "description": "Empresa de tecnología"
}

// Respuesta 201 Created
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "name": "Mi Empresa",
  "subdomain": "miempresa",
  "fullUrl": "https://miempresa.meet-lines.com",
  "industry": "Tecnología",
  "status": "active",
  "createdAt": "2025-12-03T10:00:00Z"
}
```

### 2. Acceder a un Proyecto
```
URL: https://miempresa.meet-lines.com/api/projects

El middleware:
1. Extrae "miempresa" del host
2. Busca Project WHERE subdomain="miempresa"
3. Encuentra el proyecto y seta el tenant
4. Las queries subsecuentes usan ese tenant
```

### 3. Obtener Proyectos del Usuario
```
GET /api/projects
Authorization: Bearer {token}

// Respuesta 200 OK
{
  "projects": [
    {
      "id": "123e4567-e89b-12d3-a456-426614174000",
      "name": "Mi Empresa",
      "subdomain": "miempresa",
      "fullUrl": "https://miempresa.meet-lines.com",
      "status": "active"
    }
  ],
  "totalProjects": 1,
  "maxProjects": 2
}
```

## Extensibilidad Futura

### Agregar Filtrado por Tenant a Otros Repositorios

Para repositorios como `ChannelRepository`, `AppointmentRepository`, etc.:

```csharp
// En el repositorio
private readonly ITenantQueryFilter _tenantFilter;

public ChannelRepository(MeetLinesPgDbContext context, ITenantQueryFilter tenantFilter)
{
    _context = context;
    _tenantFilter = tenantFilter;
}

public async Task<IEnumerable<Channel>> GetByProjectAsync(Guid projectId, CancellationToken ct)
{
    var tenantId = _tenantFilter.GetCurrentTenantId();
    
    // Asegurar que el projectId pertenece al tenant actual
    return await _context.Channels
        .Where(c => c.ProjectId == projectId && c.Project.Id == tenantId)
        .ToListAsync(ct);
}
```

## Troubleshooting

### El middleware no se ejecuta
**Solución**: Verificar que esté registrado en `Program.cs` ANTES de Authentication

### Error "Tenant not found" (404)
**Causas**:
- Subdominio no existe en BD
- Proyecto está deshabilitado (status != "active")
- Subdominio inválido según `SubdomainValidator`

### Usuario accede a proyecto de otro usuario
**Prevención**: Los casos de uso validan `IsUserProjectOwnerAsync()`

### Subdominio duplicado
**Prevención**: Índice único en BD + validación en `ExistsSubdomainAsync()`

## Testing

### Prueba Manual del Middleware
```bash
curl -H "Host: miempresa.meet-lines.com" http://localhost:5000/api/projects

# Debe extraer "miempresa" y resolver el tenant
```

### Prueba de Seguridad
```bash
# Usuario A intenta acceder a proyecto de Usuario B
GET /api/projects/{project-id-de-usuario-b}
Authorization: Bearer {token-usuario-a}

# Debe devolver 403 Forbidden (implementar en controller)
```

---

**Última actualización**: 3 de Diciembre de 2025
**Versión**: 1.0 - Implementación Completa
