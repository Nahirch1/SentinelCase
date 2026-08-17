import { useState } from 'react'

import {
  clearApiToken,
  getApiToken,
  setApiToken,
} from '../auth/token'

export function ApiAccess() {
  const [token, setToken] = useState(
    getApiToken() ?? '',
  )

  const [saved, setSaved] = useState(
    Boolean(getApiToken()),
  )

  function handleSave() {
    setApiToken(token)
    setSaved(Boolean(token.trim()))
  }

  function handleClear() {
    clearApiToken()
    setToken('')
    setSaved(false)
  }

  return (
    <div className="api-access">
      <div className="api-access-header">
        <span>Acceso API</span>
        <strong>
          {saved ? 'Configurado' : 'Sin token'}
        </strong>
      </div>

      <input
        type="password"
        value={token}
        onChange={(event) =>
          setToken(event.target.value)
        }
        placeholder="JWT de desarrollo"
      />

      <div className="api-access-actions">
        <button
          type="button"
          onClick={handleSave}
        >
          Guardar
        </button>

        <button
          type="button"
          onClick={handleClear}
        >
          Limpiar
        </button>
      </div>
    </div>
  )
}
