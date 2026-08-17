import { useEffect, useMemo, useState } from 'react'

import { getIncidents } from '../api/incidents'
import { IncidentRow } from '../components/IncidentRow'

import {
  IncidentSeverity,
  IncidentStatus,
  type IncidentListItem,
} from '../types/incidents'

interface DashboardPageProps {
  onOpenIncident: (id: string) => void
}

export function DashboardPage({
  onOpenIncident,
}: DashboardPageProps) {
  const [incidents, setIncidents] =
    useState<IncidentListItem[]>([])

  const [loading, setLoading] =
    useState(true)

  const [error, setError] =
    useState<string | null>(null)

  useEffect(() => {
    const token =
      import.meta.env.VITE_API_TOKEN

    getIncidents(
      {
        pageNumber: 1,
        pageSize: 100,
      },
      token,
    )
      .then((result) => {
        setIncidents(result.items)
        setError(null)
      })
      .catch((requestError: unknown) => {
        setError(
          requestError instanceof Error
            ? requestError.message
            : 'Ocurrió un error al consultar la API.',
        )
      })
      .finally(() => {
        setLoading(false)
      })
  }, [])

  const summary = useMemo(() => {
    return {
      open: incidents.filter(
        (incident) =>
          incident.status === IncidentStatus.Open,
      ).length,

      critical: incidents.filter(
        (incident) =>
          incident.severity === IncidentSeverity.Critical,
      ).length,

      investigation: incidents.filter(
        (incident) =>
          incident.status ===
          IncidentStatus.UnderInvestigation,
      ).length,

      contained: incidents.filter(
        (incident) =>
          incident.status === IncidentStatus.Contained,
      ).length,

      resolved: incidents.filter(
        (incident) =>
          incident.status === IncidentStatus.Resolved,
      ).length,
    }
  }, [incidents])

  return (
    <>
      <section className="dashboard-grid">
        <div className="dashboard-card">
          <span>Abiertos</span>
          <strong>{summary.open}</strong>
        </div>

        <div className="dashboard-card dashboard-card-critical">
          <span>Críticos</span>
          <strong>{summary.critical}</strong>
        </div>

        <div className="dashboard-card dashboard-card-info">
          <span>En investigación</span>
          <strong>{summary.investigation}</strong>
        </div>

        <div className="dashboard-card dashboard-card-warning">
          <span>Contenidos</span>
          <strong>{summary.contained}</strong>
        </div>

        <div className="dashboard-card dashboard-card-success">
          <span>Resueltos</span>
          <strong>{summary.resolved}</strong>
        </div>
      </section>

      <section className="content-panel">
        <div className="panel-header">
          <div>
            <h2>Incidentes recientes</h2>
            <p>
              Información obtenida directamente desde la API.
            </p>
          </div>
        </div>

        {loading && (
          <div className="status-message">
            Consultando Sistema Centinela...
          </div>
        )}

        {error && (
          <div className="status-message status-error">
            {error}
          </div>
        )}

        {!loading &&
          !error &&
          incidents.length === 0 && (
            <div className="status-message">
              No hay incidentes registrados.
            </div>
          )}

        {!loading &&
          !error &&
          incidents.length > 0 && (
            <div className="incident-list">
              {incidents
                .slice(0, 8)
                .map((incident) => (
                  <IncidentRow
                    key={incident.id}
                    incident={incident}
                    onOpen={onOpenIncident}
                  />
                ))}
            </div>
          )}
      </section>
    </>
  )
}
