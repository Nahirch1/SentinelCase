import {
  IncidentSeverity,
  IncidentStatus,
} from '../types/incidents'

export function getSeverityLabel(
  severity: number,
) {
  switch (severity) {
    case IncidentSeverity.Low:
      return 'Baja'
    case IncidentSeverity.Medium:
      return 'Media'
    case IncidentSeverity.High:
      return 'Alta'
    case IncidentSeverity.Critical:
      return 'Crítica'
    default:
      return 'Desconocida'
  }
}

export function getStatusLabel(
  status: number,
) {
  switch (status) {
    case IncidentStatus.Open:
      return 'Abierto'
    case IncidentStatus.UnderInvestigation:
      return 'En investigación'
    case IncidentStatus.Contained:
      return 'Contenido'
    case IncidentStatus.Resolved:
      return 'Resuelto'
    case IncidentStatus.Closed:
      return 'Cerrado'
    default:
      return 'Desconocido'
  }
}

export function getSeverityClass(
  severity: number,
) {
  switch (severity) {
    case IncidentSeverity.Critical:
      return 'severity-critical'
    case IncidentSeverity.High:
      return 'severity-high'
    case IncidentSeverity.Medium:
      return 'severity-medium'
    case IncidentSeverity.Low:
      return 'severity-low'
    default:
      return ''
  }
}

export function getStatusClass(
  status: number,
) {
  switch (status) {
    case IncidentStatus.Open:
      return 'status-open'
    case IncidentStatus.UnderInvestigation:
      return 'status-investigation'
    case IncidentStatus.Contained:
      return 'status-contained'
    case IncidentStatus.Resolved:
      return 'status-resolved'
    case IncidentStatus.Closed:
      return 'status-closed'
    default:
      return ''
  }
}

export function getNextIncidentStatus(
  status: number,
): number | null {
  switch (status) {
    case IncidentStatus.Open:
      return IncidentStatus.UnderInvestigation
    case IncidentStatus.UnderInvestigation:
      return IncidentStatus.Contained
    case IncidentStatus.Contained:
      return IncidentStatus.Resolved
    case IncidentStatus.Resolved:
      return IncidentStatus.Closed
    default:
      return null
  }
}
