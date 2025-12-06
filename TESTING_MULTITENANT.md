# 🧪 Guía de Prueba - Sistema Multitenant Local

## Resumen Rápido

El sistema crea subdominios automáticamente con el patrón: `{nombreProyecto}-{4letrasAleatorias}`

Ejemplo: `proyecto1-ggpm.meet-lines.local`

---

## 📋 Requisitos Previos

✅ Backend corriendo en `http://localhost:3001`
✅ Frontend corriendo en `http://localhost:5173`
✅ Archivo `hosts` ya configurado (ver más abajo)
✅ Token JWT de usuario autenticado

---

## 🚀 Pasos para Probar

### 1. **Login y Obtener Token**

```bash
# Login (obtén el token)
curl -X POST http://localhost:3001/Auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "tu-email@example.com",
    "password": "tu-password"
  }'
```

**Respuesta esperada**:
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGc...",
    "refreshToken": "...",
    "user": {...}
  }
}
```

Copia el `accessToken`.

---

### 2. **Crear un Proyecto**

```bash
# Reemplaza {TOKEN} con el token obtenido
curl -X POST http://localhost:3001/Projects \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {TOKEN}" \
  -d '{
    "name": "Mi Proyecto",
    "industry": "Technology"
  }'
```

**Respuesta esperada**:
```json
{
  "success": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "name": "Mi Proyecto",
    "subdomain": "mi-proyecto-ggpm",
    "fullUrl": "http://mi-proyecto-ggpm.meet-lines.local:3001"
  }
}
```

**Guarda el subdominio**: `mi-proyecto-ggpm`

---

### 3. **Listar Tus Proyectos**

```bash
curl -X GET http://localhost:3001/Projects \
  -H "Authorization: Bearer {TOKEN}"
```

Esto te muestra todos tus proyectos con sus subdominios.

---

### 4. **Agregar Subdominio al archivo `hosts`**

#### **Opción A: Manual (más simple)**

1. Abre `C:\Windows\System32\drivers\etc\hosts` como administrador
2. Agrega al final:
```
127.0.0.1 mi-proyecto-ggpm.meet-lines.local
127.0.0.1 otro-proyecto-abcd.meet-lines.local
```
3. Guarda

#### **Opción B: Script PowerShell (automático)**

```powershell
# Ejecuta como administrador
.\scripts\add-project-hosts.ps1 -subdomains "mi-proyecto-ggpm,otro-proyecto-abcd"
```

---

### 5. **Acceder al Dashboard**

En el navegador, accede a:

```
http://mi-proyecto-ggpm.meet-lines.local:3001
```

**Debe ocurrir**:
1. ✅ El middleware `TenantResolutionMiddleware` detecta el subdominio
2. ✅ Busca el proyecto en la BD
3. ✅ El frontend detecta el subdominio
4. ✅ Redirija automáticamente a `/dashboard`
5. ✅ Ves el dashboard del proyecto

---

## 🧩 Flujo Técnico

```
1. Usuario entra a: mi-proyecto-ggpm.meet-lines.local:3001

2. Backend (Middleware):
   - Extrae "mi-proyecto-ggpm" del host
   - Busca Project con subdomain="mi-proyecto-ggpm"
   - Si existe y status="active" → SetTenant()

3. Frontend (Router):
   - Detecta subdominio con isInProjectSubdomain()
   - Si hay subdominio → Redirija a /dashboard
   - El dashboard usa la URL con subdominio automáticamente

4. Resultado:
   - API calls van a: http://mi-proyecto-ggpm.meet-lines.local:3001/api/...
   - Middleware valida el tenant en cada request
   - Solo ves datos de ese proyecto
```

---

## 🔍 Debugging

### Ver todos los subdominios en tu máquina

```bash
# En PowerShell
Get-Content C:\Windows\System32\drivers\etc\hosts | Select-String "meet-lines.local"
```

### Ver si el middleware funciona

```bash
# Accede a un subdominio inválido (debe retornar 404)
curl http://invalid-xyz.meet-lines.local:3001/api/health
# Espera: 404 Tenant not found
```

### Ver logs del backend

```bash
# Si corres en Docker
docker logs meetlines-backend
```

---

## 📝 Ejemplo Completo

```bash
# 1. Login
TOKEN=$(curl -s -X POST http://localhost:3001/Auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"john@example.com","password":"Pass123!"}' \
  | grep -o '"accessToken":"[^"]*"' | cut -d'"' -f4)

echo "Token: $TOKEN"

# 2. Crear proyecto
SUBDOMAIN=$(curl -s -X POST http://localhost:3001/Projects \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"name":"TestProject","industry":"Tech"}' \
  | grep -o '"subdomain":"[^"]*"' | cut -d'"' -f4)

echo "Subdominio: $SUBDOMAIN"

# 3. Agregar al hosts (PowerShell como admin)
# powershell -Command ".\scripts\add-project-hosts.ps1 -subdomains '$SUBDOMAIN'"

# 4. Acceder en navegador
# http://$SUBDOMAIN.meet-lines.local:3001
```

---

## ✅ Checklist de Prueba

- [ ] Backend corre en puerto 3001
- [ ] Frontend corre en puerto 5173
- [ ] Archivo hosts tiene los subdominios
- [ ] Puedes hacer login
- [ ] Puedes crear un proyecto (obtienes el subdomain)
- [ ] Agregar subdominio al hosts file
- [ ] Acceder a `http://proyecto.meet-lines.local:3001` 
- [ ] Ves automáticamente el dashboard (no la landing)
- [ ] El dashboard funciona correctamente

---

## 🆘 Problemas Comunes

### "Tenant not found" (404)

**Causa**: El middleware no encontró el proyecto en la BD
- Verifica que el subdominio esté correcto en el hosts
- Verifica que el proyecto existe en la BD
- Verifica que el proyecto está en status "active"

### "No tienes acceso a este subdominio"

**Causa**: El JWT es de otro usuario
- Usa el token del usuario que creó el proyecto

### "Conexión rechazada"

**Causa**: El backend no está corriendo o no en puerto 3001
- Inicia: `docker-compose up` o `dotnet run`

### El frontend no redirija al dashboard

**Causa**: El tenantService no detecta el subdominio
- Verifica que el hosts está correctamente configurado
- Verifica que el navegador está usando el host correcto (no IP)

---

## 📚 Archivos Clave

- Backend: `MeetLines.API/Middleware/TenantResolutionMiddleware.cs`
- Frontend: `src/services/tenantService.js` y `src/router/index.js`
- Config: `.env.development` (Backend y Frontend)
- Hosts: `C:\Windows\System32\drivers\etc\hosts`

