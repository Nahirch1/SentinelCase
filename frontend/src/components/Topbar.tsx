interface TopbarProps {
  title: string
  description: string
}

export function Topbar({
  title,
  description,
}: TopbarProps) {
  return (
    <header className="topbar">
      <div>
        <h1>{title}</h1>
        <p>{description}</p>
      </div>
    </header>
  )
}
