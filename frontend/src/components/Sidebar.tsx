export type AppPage =
  | 'dashboard'
  | 'incidents'
  | 'new-incident'

interface SidebarProps {
  activePage: AppPage
  onNavigate: (page: AppPage) => void
}

export function Sidebar({
  activePage,
  onNavigate,
}: SidebarProps) {
  return (
    <aside className="sidebar">
      <div className="brand">
        Sistema Centinela
      </div>

      <nav className="nav">
        <button
          type="button"
          className={
            activePage === 'dashboard'
              ? 'nav-item nav-item-active'
              : 'nav-item'
          }
          onClick={() => onNavigate('dashboard')}
        >
          Resumen
        </button>

        <button
          type="button"
          className={
            activePage === 'incidents'
              ? 'nav-item nav-item-active'
              : 'nav-item'
          }
          onClick={() => onNavigate('incidents')}
        >
          Incidentes
        </button>

        <button
          type="button"
          className={
            activePage === 'new-incident'
              ? 'nav-item nav-item-active'
              : 'nav-item'
          }
          onClick={() => onNavigate('new-incident')}
        >
          Nuevo incidente
        </button>
      </nav>
    </aside>
  )
}
