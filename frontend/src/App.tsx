import { useState } from 'react'

import {
  Sidebar,
  type AppPage,
} from './components/Sidebar'

import { Topbar } from './components/Topbar'
import { DashboardPage } from './pages/DashboardPage'
import { IncidentDetailPage } from './pages/IncidentDetailPage'
import { NewIncidentPage } from './pages/NewIncidentPage'
import { IncidentsPage } from './pages/IncidentsPage'

function App() {
  const [activePage, setActivePage] =
    useState<AppPage>('dashboard')

  const [selectedIncidentId, setSelectedIncidentId] =
    useState<string | null>(null)

  const pageTitle = selectedIncidentId
    ? 'Detalle del incidente'
    : activePage === 'dashboard'
      ? 'Resumen'
      : activePage === 'incidents'
        ? 'Incidentes'
        : 'Nuevo incidente'

  const pageDescription = selectedIncidentId
    ? 'Información, trazabilidad y acciones sobre el incidente.'
    : activePage === 'dashboard'
      ? 'Gestión y seguimiento de incidentes de seguridad.'
      : activePage === 'incidents'
        ? 'Consulta, búsqueda y filtrado de incidentes.'
        : 'Registro de un nuevo incidente de seguridad.'

  return (
    <div className="app-shell">
      <Sidebar
        activePage={activePage}
        onNavigate={(page) => {
          setSelectedIncidentId(null)
          setActivePage(page)
        }}
      />

      <main className="main-content">
        <Topbar
          title={pageTitle}
          description={pageDescription}
        />

        {selectedIncidentId ? (
          <IncidentDetailPage
            incidentId={selectedIncidentId}
            onBack={() => setSelectedIncidentId(null)}
          />
        ) : (
          <>
            {activePage === 'dashboard' && (
              <DashboardPage
                onOpenIncident={setSelectedIncidentId}
              />
            )}

            {activePage === 'incidents' && (
              <IncidentsPage
                onOpenIncident={setSelectedIncidentId}
              />
            )}

            {activePage === 'new-incident' && (
              <NewIncidentPage
                onCreated={setSelectedIncidentId}
              />
            )}
          </>
        )}
      </main>
    </div>
  )
}

export default App
