# Sistema Centinela

**Sistema Centinela** es una API backend para la gestión y seguimiento de incidentes de ciberseguridad, desarrollada con .NET y orientada a representar el flujo de trabajo de un entorno SOC (Security Operations Center).

El sistema permite registrar incidentes, clasificarlos, asignarlos a analistas, modificar su estado, incorporar notas de investigación y mantener un historial de auditoría de las acciones realizadas.

> El nombre interno de la solución y sus namespaces continúa siendo `SentinelCase`.

## Objetivo

El objetivo de Sistema Centinela es implementar un backend con una arquitectura mantenible y cercana a un escenario profesional de gestión de incidentes.

El proyecto no se limita a operaciones CRUD. Incorpora reglas de dominio, autorización basada en roles, trazabilidad, observabilidad, persistencia, pruebas automatizadas, contenerización e integración continua.

## Funcionalidades principales

- Registro de incidentes de seguridad.
- Consulta individual y listado paginado.
- Filtrado por estado y severidad.
- Búsqueda por texto.
- Filtrado por analista asignado.
- Actualización de incidentes.
- Asignación de incidentes a analistas.
- Gestión del ciclo de estados.
- Notas de investigación.
- Historial de auditoría.
- Autenticación mediante JWT.
- Autorización basada en roles y políticas.
- Validación de solicitudes.
- Manejo centralizado de excepciones.
- Rate limiting.
- Health checks.
- Logging estructurado.
- Trazas y métricas mediante OpenTelemetry.

## Arquitectura

La solución está organizada en cuatro proyectos principales:

```text
SentinelCase
├── SentinelCase.Domain
├── SentinelCase.Application
├── SentinelCase.Infrastructure
└── SentinelCase.Api
```
### Domain

Contiene el núcleo del dominio:

- SecurityIncident
- IncidentHistoryEntry
- IncidentNote
- Estados y severidades.
- Eventos de auditoría.
- Reglas y excepciones de dominio.

Esta capa no depende de infraestructura ni de la API.

### Application

Contiene los casos de uso de la aplicación mediante un enfoque CQRS, separando comandos y consultas.

También contiene interfaces de repositorios, paginación, validaciones, behaviors y la abstracción del usuario actual.

### Infrastructure

Implementa la persistencia y acceso a datos mediante Entity Framework Core y SQL Server.

Incluye repositorios, configuraciones de entidades, ApplicationDbContext y migraciones.

### API

Expone las funcionalidades mediante ASP.NET Core.

Incluye endpoints HTTP, autenticación JWT, autorización por roles y políticas, manejo de excepciones, health checks, rate limiting, logging estructurado y OpenTelemetry.

## Stack tecnológico

- .NET 10
- ASP.NET Core
- C#
- Entity Framework Core
- SQL Server 2025
- MediatR
- FluentValidation
- JWT Bearer Authentication
- Serilog
- OpenTelemetry
- xUnit
- Docker
- Docker Compose
- GitHub Actions

## API

Ruta base: /api/incidents

### Endpoints principales

POST   /api/incidents
GET    /api/incidents
GET    /api/incidents/{id}
PUT    /api/incidents/{id}
PATCH  /api/incidents/{id}/assignment
PATCH  /api/incidents/{id}/status
GET    /api/incidents/{id}/history
POST   /api/incidents/{id}/notes
GET    /api/incidents/{id}/notes

El listado permite paginación y filtros por estado, severidad, texto y analista asignado.

## Seguridad

La API utiliza autenticación mediante JWT Bearer.

Roles definidos:

- Analyst
- SocManager
- Administrator

Políticas de autorización:

- CanCreateIncident
- CanManageIncidentStatus
- CanAssignIncident

Las operaciones sensibles se restringen según las responsabilidades del usuario.

## Auditoría y seguimiento

Las modificaciones relevantes de un incidente quedan registradas en su historial.

Cada incidente también puede contener notas de investigación para conservar información generada durante su análisis y seguimiento.
## Observabilidad

Sistema Centinela incorpora OpenTelemetry para instrumentación de:

- solicitudes ASP.NET Core
- clientes HTTP
- métricas del runtime de .NET
- trazas
- métricas

La telemetría puede exportarse mediante OTLP hacia un OpenTelemetry Collector.

El repositorio incluye el archivo:

otel-collector-config.yaml

También se utiliza Serilog para logging estructurado con identificación de trazas y usuario asociado a las solicitudes.

## Health checks

La API expone:

GET /health
GET /health/ready

El readiness check comprueba también la disponibilidad de la base de datos.

## Rate limiting

La API incorpora limitación global de solicitudes.

Al superar el límite configurado se devuelve:

429 Too Many Requests
## Testing

El proyecto incluye pruebas unitarias y de integración.

Las pruebas cubren:

- autenticación
- autorización
- creación y consulta de incidentes
- actualización
- asignación
- cambios de estado
- historial
- notas
- conflictos
- rate limiting

Estado actual:

81 tests
81 passed
0 failed

Para ejecutar las pruebas:

dotnet test SentinelCase.slnx

## Docker

Construir la imagen de la API:

docker build -t sentinelcase-api:local .

Levantar la API y SQL Server:

docker compose up -d

Comprobar los servicios:

docker compose ps

Detener los servicios:

docker compose down
## Configuración

La conexión con SQL Server se configura mediante:

ConnectionStrings__DefaultConnection

Docker Compose utiliza:

MSSQL_SA_PASSWORD

OpenTelemetry puede configurarse mediante:

OTEL_EXPORTER_OTLP_ENDPOINT
OTEL_EXPORTER_OTLP_PROTOCOL

Las credenciales reales no deben almacenarse en el repositorio.

## Ejecución local

Restaurar dependencias:

dotnet restore SentinelCase.slnx

Compilar:

dotnet build SentinelCase.slnx

Ejecutar pruebas:

dotnet test SentinelCase.slnx

Ejecutar la API:

dotnet run --project src/SentinelCase.Api/SentinelCase.Api.csproj

## Integración continua

El repositorio utiliza GitHub Actions.

En cada push o pull request hacia main se ejecutan automáticamente:

- Restore
- Build en Release
- Tests
- Build de la imagen Docker

## Estado del proyecto

Sistema Centinela cuenta con arquitectura por capas, CQRS, SQL Server, autenticación y autorización, gestión del flujo de incidentes, auditoría, notas, filtros y paginación, manejo de errores, rate limiting, health checks, logging estructurado, OpenTelemetry, pruebas automatizadas, Docker y CI.

## Autor

**Nahir Chambi**

Proyecto desarrollado como parte de un portfolio orientado a desarrollo backend y ciberseguridad.
