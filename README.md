# Sistema Centinela

**Sistema Centinela** es un sistema para la gestión y seguimiento de incidentes de ciberseguridad, desarrollado principalmente con C#/.NET y orientado a representar el flujo de trabajo de un entorno SOC (Security Operations Center).

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
- React
- TypeScript
- Vite
- Nginx
- Oxlint
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

Sistema Centinela incorpora una pila de observabilidad basada en OpenTelemetry.

La API instrumenta:

- solicitudes ASP.NET Core
- clientes HTTP
- métricas del runtime de .NET
- trazas distribuidas
- métricas técnicas
- métricas de dominio

La telemetría se exporta mediante OTLP hacia OpenTelemetry Collector.

El flujo de observabilidad es:

```text
SentinelCase.Api
      |
      v
OpenTelemetry Collector
      |
      +----> Prometheus ----> Grafana
      |
      +----> Jaeger
```

Prometheus almacena las métricas y Grafana proporciona un dashboard operacional de SentinelCase.

Jaeger permite inspeccionar las trazas generadas por la API.

Entre las métricas personalizadas se encuentran:

- incidentes creados, clasificados por severidad
- cambios de estado de incidentes
- mensajes Outbox procesados
- fallos de procesamiento del Outbox
- duración del procesamiento del Outbox

También se utiliza Serilog para logging estructurado, incluyendo identificación de trazas y usuario asociado a las solicitudes.

## Health checks

La API expone:

```text
GET /health
GET /health/ready
```

El readiness check comprueba también la disponibilidad de SQL Server.

## Rate limiting

La API incorpora limitación global mediante una ventana fija por dirección IP.

Configuración actual:

```text
100 solicitudes por minuto
QueueLimit = 0
```

Al superar el límite se devuelve:

```text
429 Too Many Requests
```

La protección fue validada mediante k6.

## Pruebas de carga

El repositorio incluye pruebas reproducibles con k6 dentro de:

```text
load-tests/
```

Se incluyen pruebas para:

- health checks
- validación del rate limiting
- consultas autenticadas de incidentes

En una prueba de referencia contra:

```text
GET /api/incidents?pageNumber=1&pageSize=20
```

utilizando autenticación JWT, Entity Framework Core y SQL Server, se obtuvieron:

```text
Solicitudes:       77
Respuestas OK:     77/77
Errores HTTP:      0.00 %
Latencia media:    35.41 ms
Mediana:           13.90 ms
p95:               54.29 ms
p99:               474.42 ms
Máximo observado:  1.22 s
```

Los thresholds definidos fueron:

```text
http_req_failed < 1 %
p95 < 500 ms
p99 < 1000 ms
```

Todos los thresholds se cumplieron.

Estos resultados corresponden a una prueba local y representan una referencia de desarrollo, no un benchmark de capacidad máxima del sistema.

## Testing

El proyecto incluye pruebas unitarias y de integración.

Las pruebas cubren:

- autenticación
- autorización
- creación y consulta de incidentes
- resumen operacional de incidentes
- actualización
- asignación
- cambios de estado
- historial
- notas
- conflictos
- rate limiting

Estado actual:

```text
87 tests
87 passed
0 failed
```

Para ejecutar las pruebas:

```bash
dotnet test SentinelCase.slnx
```

## Docker

Construir la imagen de la API:

docker build -t sentinelcase-api:local .

Levantar frontend, API y SQL Server:

docker compose up -d

Comprobar los servicios:

docker compose ps

Frontend: http://localhost:5173

API: http://localhost:8080

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
- Build del frontend
- Lint del frontend
- Build de la imagen Docker del frontend

## Estado del proyecto

Sistema Centinela cuenta con arquitectura por capas, CQRS, SQL Server, autenticación y autorización, gestión del flujo de incidentes, auditoría, notas, filtros y paginación, manejo de errores, rate limiting, health checks, logging estructurado, OpenTelemetry, pruebas automatizadas, Docker y CI.

## Autor

**Nahir Chambi**

Proyecto desarrollado como parte de un portfolio orientado a desarrollo backend y ciberseguridad.
