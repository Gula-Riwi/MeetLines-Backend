#!/usr/bin/env pwsh

# Script para agregar subdominios de proyectos al archivo hosts
# Uso: .\add-project-hosts.ps1 -subdomains "proyecto1-ggpm,proyecto2-abcd"

param(
    [Parameter(Mandatory=$false)]
    [string]$subdomains,
    [Parameter(Mandatory=$false)]
    [string]$baseDomain = "meet-lines.local"
)

# Requiere privilegios de administrador
if (-NOT ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Host "❌ Este script requiere privilegios de administrador" -ForegroundColor Red
    Write-Host "Por favor, ejecuta PowerShell como administrador y vuelve a intentar"
    exit 1
}

$hostsFile = "C:\Windows\System32\drivers\etc\hosts"

if (-not (Test-Path $hostsFile)) {
    Write-Host "❌ Archivo hosts no encontrado"
    exit 1
}

# Si no se especifican subdominios, mostrar uso
if (-not $subdomains) {
    Write-Host "Uso: .\add-project-hosts.ps1 -subdomains 'proyecto1-ggpm,proyecto2-abcd' [-baseDomain 'meet-lines.local']" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Ejemplo:"
    Write-Host "  .\add-project-hosts.ps1 -subdomains 'proyecto1-ggpm,proyecto2-abcd'"
    exit 0
}

$subdomainList = $subdomains -split ','

$content = Get-Content $hostsFile
$newContent = @()
$existingSubdomains = @()

# Leer el archivo y guardar las líneas que no sean comentarios de proyectos
foreach ($line in $content) {
    if ($line -match "^127\.0\.0\.1\s+[a-z0-9-]+\.${baseDomain}") {
        $newContent += $line
        $existing = $line -split '\s+' | Select-Object -Last 1
        $existingSubdomains += $existing
    } else {
        $newContent += $line
    }
}

$addedCount = 0

# Agregar los nuevos subdominios
foreach ($subdomain in $subdomainList) {
    $subdomain = $subdomain.Trim()
    $fullDomain = "$subdomain.$baseDomain"
    
    if ($existingSubdomains -notcontains $fullDomain) {
        $newContent += "127.0.0.1 $fullDomain"
        Write-Host "✅ Agregado: 127.0.0.1 $fullDomain" -ForegroundColor Green
        $addedCount++
    } else {
        Write-Host "⏭️  Ya existe: $fullDomain" -ForegroundColor Yellow
    }
}

if ($addedCount -gt 0) {
    Set-Content -Path $hostsFile -Value ($newContent -join "`n") -Encoding ASCII
    Write-Host ""
    Write-Host "✅ Se agregaron $addedCount subdominio(s) al archivo hosts" -ForegroundColor Green
    Write-Host "🌐 Ahora puedes acceder a:"
    foreach ($subdomain in $subdomainList) {
        Write-Host "   http://$($subdomain.Trim()).$baseDomain:3001" -ForegroundColor Cyan
    }
} else {
    Write-Host "ℹ️  No se agregaron nuevos subdominios (ya existían)" -ForegroundColor Blue
}
