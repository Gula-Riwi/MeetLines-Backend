@echo off
rem Script para iniciar Wiki.js con Docker Compose
cd /d "%~dp0"
docker compose up -d
if %errorlevel% neq 0 (
    echo Error al iniciar los contenedores. Verifique que Docker esté instalado y en ejecución.
) else (
    echo Wiki.js está corriendo en http://localhost:3000
)
pause
