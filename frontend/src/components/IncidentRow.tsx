import type { IncidentListItem } from '../types/incidents'

import {
  getSeverityClass,
  getSeverityLabel,
  getStatusClass,
  getStatusLabel,
} from '../utils/incidents'

interface IncidentRowProps {
  incident: IncidentListItem
  onOpen?: (id: string) => void
}

export function IncidentRow({
  incident,
  onOpen,
}: IncidentRowProps) {
  return (
    <article
      className="incident-row"
      onClick={() => onOpen?.(incident.id)}
      role={onOpen ? 'button' : undefined}
      tabIndex={onOpen ? 0 : undefined}
      onKeyDown={(event) => {
        if (
          onOpen &&
          (event.key === 'Enter' || event.key === ' ')
        ) {
          onOpen(incident.id)
        }
      }}
    >
      <div className="incident-main">
        <strong>{incident.title}</strong>
        <span>{incident.id}</span>
      </div>

      <div className="incident-meta">
        <span
          className={`incident-badge ${getSeverityClass(
            incident.severity,
          )}`}
        >
          {getSeverityLabel(incident.severity)}
        </span>

        <span
          className={`incident-badge ${getStatusClass(
            incident.status,
          )}`}
        >
          {getStatusLabel(incident.status)}
        </span>

        <span className="incident-assignee">
          {incident.assignedTo ?? 'Sin asignar'}
        </span>
      </div>
    </article>
  )
}
