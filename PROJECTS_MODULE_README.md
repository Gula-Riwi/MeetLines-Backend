# Gestión de Proyectos - Documentación

## Overview

Este módulo implementa el sistema de gestión de proyectos/empresas siguiendo arquitectura **DDD Hexagonal** con validaciones automáticas según el plan de suscripción del usuario.

### Estructura de Planes

| Plan       | Proyectos Máximos | Descripción      |
|-----------|------------------|------------------|
| beginner  | 1                | Plan básico      |
| intermediate | 2             | Plan intermedio  |
| complete  | ∞ (ilimitado)    | Plan completo    |

---

## Arquitectura

```
┌─────────────────────────────────────────────────────────────┐
│                      API Controller                          │
│                  ProjectsController                          │
└──────────────────────────┬──────────────────────────────────┘
                           │
           ┌───────────────┼───────────────┐
           ▼               ▼               ▼
    ┌────────────┐ ┌────────────┐ ┌────────────────┐
    │ UseCase    │ │ UseCase    │ │ UseCase        │
    │ Create     │ │ Update     │ │ GetUserProjects│
    │ Delete     │ │ GetById    │ │                │
    └─────┬──────┘ └─────┬──────┘ └────────┬───────┘
          │              │                 │
          └──────────────┬─────────────────┘
                         │
          ┌──────────────┴──────────────┐
          ▼                             ▼
    ┌──────────────┐           ┌──────────────┐
    │ Repository   │           │ Validators   │
    │ Interfaces   │           │ FluentValid  │
    └──────┬───────┘           └──────────────┘
           │
    ┌──────▼───────┐
    │ Infrastructure│ (PostgreSQL)
    └──────────────┘
```

---

## DTOs

### CreateProjectRequest
```json
{
  "name": "string (2-100 chars, required)",
  "industry": "string (optional, max 50 chars)",
  "description": "string (optional, max 500 chars)"
}
```

### ProjectResponse
```json
{
  "id": "uuid",
  "name": "string",
  "industry": "string",
  "description": "string",
  "status": "active|disabled",
  "createdAt": "ISO8601",
  "updatedAt": "ISO8601"
}
```

### UserProjectsResponse
```json
{
  "plan": "beginner|intermediate|complete",
  "maxProjects": "number",
  "currentProjects": "number",
  "canCreateMore": "boolean",
  "projects": [ProjectResponse]
}
```

### UpdateProjectRequest
```json
{
  "name": "string (2-100 chars, required)",
  "industry": "string (optional, max 50 chars)",
  "description": "string (optional, max 500 chars)"
}
```

---

## API Endpoints

### 1. Obtener todos los proyectos del usuario

**Endpoint:** `GET /api/projects`

**Headers:**
```
Authorization: Bearer {jwt_token}
```

**Response Success (200):**
```json
{
  "plan": "intermediate",
  "maxProjects": 2,
  "currentProjects": 1,
  "canCreateMore": true,
  "projects": [
    {
      "id": "uuid",
      "name": "Mi Empresa",
      "industry": "Tecnología",
      "description": "Descripción...",
      "status": "active",
      "createdAt": "2025-01-15T10:30:00Z",
      "updatedAt": "2025-01-15T10:30:00Z"
    }
  ]
}
```

**Response Error (400):**
```json
{
  "error": "No active subscription found for this user"
}
```

---

### 2. Obtener proyecto por ID

**Endpoint:** `GET /api/projects/{projectId}`

**Headers:**
```
Authorization: Bearer {jwt_token}
```

**Response Success (200):**
```json
{
  "id": "uuid",
  "name": "Mi Empresa",
  "industry": "Tecnología",
  "description": "Descripción...",
  "status": "active",
  "createdAt": "2025-01-15T10:30:00Z",
  "updatedAt": "2025-01-15T10:30:00Z"
}
```

**Response Errors:**
- `400`: Project not found
- `400`: You do not have permission to access this project

---

### 3. Crear nuevo proyecto

**Endpoint:** `POST /api/projects`

**Headers:**
```
Authorization: Bearer {jwt_token}
Content-Type: application/json
```

**Body:**
```json
{
  "name": "Nueva Empresa",
  "industry": "Tecnología",
  "description": "Descripción de la empresa"
}
```

**Response Success (201):**
```json
{
  "id": "uuid",
  "name": "Nueva Empresa",
  "industry": "Tecnología",
  "description": "Descripción de la empresa",
  "status": "active",
  "createdAt": "2025-01-15T10:30:00Z",
  "updatedAt": "2025-01-15T10:30:00Z"
}
```

**Response Errors (400):**
- `"No active subscription found for this user"`
- `"Cannot create more projects. Your plan (beginner) allows a maximum of 1 project(s). Current projects: 1"`
- Validation errors del DTO

**Validaciones de CreateProjectRequest:**
- `name`: requerido, mínimo 2 caracteres, máximo 100
- `industry`: opcional, máximo 50 caracteres
- `description`: opcional, máximo 500 caracteres

---

### 4. Actualizar proyecto

**Endpoint:** `PUT /api/projects/{projectId}`

**Headers:**
```
Authorization: Bearer {jwt_token}
Content-Type: application/json
```

**Body:**
```json
{
  "name": "Empresa Actualizada",
  "industry": "Fintech",
  "description": "Nueva descripción"
}
```

**Response Success (200):**
```json
{
  "id": "uuid",
  "name": "Empresa Actualizada",
  "industry": "Fintech",
  "description": "Nueva descripción",
  "status": "active",
  "createdAt": "2025-01-15T10:30:00Z",
  "updatedAt": "2025-01-15T11:45:00Z"
}
```

**Response Errors:**
- `400`: Project not found
- `400`: You do not have permission to update this project
- Validation errors del DTO

---

### 5. Eliminar proyecto

**Endpoint:** `DELETE /api/projects/{projectId}`

**Headers:**
```
Authorization: Bearer {jwt_token}
```

**Response Success (204):** No content

**Response Errors:**
- `400`: Project not found
- `400`: You do not have permission to delete this project

---

## Validaciones de Negocio

### Limitación de Proyectos por Plan

La validación ocurre en tiempo de creación:

```csharp
// Beginner: máximo 1 proyecto
if (plan == "beginner" && currentCount >= 1)
    // Error: Cannot create more projects

// Intermediate: máximo 2 proyectos
if (plan == "intermediate" && currentCount >= 2)
    // Error: Cannot create more projects

// Complete: ilimitado
if (plan == "complete")
    // Sin restricción
```

### Autorización de Acceso

Cada operación valida que el usuario sea propietario del proyecto:

```csharp
if (!await _projectRepository.IsUserProjectOwnerAsync(userId, projectId))
    // Error: You do not have permission...
```

### Estado del Proyecto

- Los proyectos se crean con estado `active`
- Al eliminar, el estado cambia a `disabled` (borrado lógico)
- Solo se retornan proyectos con estado `active`

---

## Flujos de Uso

### Crear proyecto cuando el usuario alcanza el límite

**Entrada:**
```bash
curl -X POST http://localhost:5000/api/projects \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Tercera Empresa",
    "industry": "Tech"
  }'
```

**Respuesta (si plan = beginner y ya tiene 1 proyecto):**
```json
{
  "error": "Cannot create more projects. Your plan (beginner) allows a maximum of 1 project(s). Current projects: 1"
}
```

### Actualizar suscripción y crear nuevo proyecto

1. Usuario actualiza su suscripción a `intermediate`
2. Ahora puede crear hasta 2 proyectos
3. El siguiente POST a crear proyecto será exitoso

---

## Manejo de Errores

| Código | Error                                                | Causa                          |
|--------|------------------------------------------------------|--------------------------------|
| 400    | User ID is invalid                                   | UUID vacío en el token         |
| 400    | No active subscription found for this user           | Usuario sin suscripción activa |
| 400    | Cannot create more projects...                       | Límite alcanzado               |
| 400    | Project not found                                    | ID de proyecto no existe       |
| 400    | You do not have permission...                        | Usuario no es propietario      |
| 401    | Unauthorized                                         | Token JWT inválido/expirado    |
| 422    | Validation errors (del validator FluentValidation)   | Datos inválidos en request     |

---

## Implementación de Repositorios

Los repositorios necesarios ya están implementados en `MeetLines.Infrastructure.Repositories`:

- `ProjectRepository`: Gestiona operaciones CRUD de proyectos
- `SubscriptionRepository`: Obtiene la suscripción activa del usuario

Ambos usan `MeetLinesPgDbContext` para acceder a PostgreSQL.

---

## Ejemplo Completo de Uso

```bash
# 1. Login (obtener token)
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "password123"
  }'

# Respuesta contiene: {token: "jwt_token"}

# 2. Obtener proyectos
curl -X GET http://localhost:5000/api/projects \
  -H "Authorization: Bearer jwt_token"

# 3. Crear proyecto
curl -X POST http://localhost:5000/api/projects \
  -H "Authorization: Bearer jwt_token" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Nueva Empresa",
    "industry": "Tecnología",
    "description": "Mi primera empresa"
  }'

# Respuesta:
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "Nueva Empresa",
  "industry": "Tecnología",
  "description": "Mi primera empresa",
  "status": "active",
  "createdAt": "2025-01-15T10:30:00Z",
  "updatedAt": "2025-01-15T10:30:00Z"
}

# 4. Actualizar proyecto
curl -X PUT http://localhost:5000/api/projects/550e8400-e29b-41d4-a716-446655440000 \
  -H "Authorization: Bearer jwt_token" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Empresa Actualizada",
    "industry": "Fintech",
    "description": "Descripción actualizada"
  }'

# 5. Eliminar proyecto
curl -X DELETE http://localhost:5000/api/projects/550e8400-e29b-41d4-a716-446655440000 \
  -H "Authorization: Bearer jwt_token"

# Response: 204 No Content
```

---

## Testing Local

Para probar localmente sin docker-compose:

```bash
cd MeetLines.API
dotnet watch
```

Luego utiliza los curl examples anteriores o Postman.

---

## Notas Técnicas

### DDD Hexagonal
- **Ports (Interfaces)**: `IProjectRepository`, `ISubscriptionRepository`, use cases
- **Adapters (Implementaciones)**: `ProjectRepository`, `SubscriptionRepository`, controller
- **Domain**: Entities (`Project`, `Subscription`), Validations

### Validaciones
- **Nivel de Entidad**: Validaciones en los constructores (invariantes de negocio)
- **Nivel de DTO**: FluentValidation en `CreateProjectRequestValidator`, `UpdateProjectRequestValidator`
- **Nivel de Caso de Uso**: Lógica de negocio (límites por plan, autorización)

### Errores de Compilación
Si falta algún registro de dependencia, el compilador te indicará qué interfaz no está registrada en `ApplicationServiceCollectionExtensions` o `InfrastructureServiceCollectionExtensions`.
