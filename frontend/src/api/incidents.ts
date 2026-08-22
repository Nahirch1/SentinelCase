import type {
  IncidentDetail,
  IncidentHistoryItem,
  IncidentListItem,
  IncidentNoteItem,
  PagedResult,
} from '../types/incidents'

const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5106'

async function throwApiError(
  response: Response,
  fallbackMessage: string,
): Promise<never> {
  let detail: string | null = null

  try {
    const body = await response.json()

    if (typeof body?.detail === 'string') {
      detail = body.detail
    } else if (typeof body?.title === 'string') {
      detail = body.title
    }
  } catch {
    detail = null
  }

  if (response.status === 401) {
    throw new Error('La sesión de desarrollo no es válida o expiró.')
  }

  if (response.status === 403) {
    throw new Error('No tenés permisos para realizar esta acción.')
  }

  if (response.status === 404) {
    throw new Error('El recurso solicitado no existe.')
  }

  if (response.status === 409) {
    throw new Error(
      detail ?? 'La operación entra en conflicto con el estado actual.',
    )
  }

  if (response.status === 429) {
    throw new Error(
      'Se alcanzó el límite de solicitudes. Intentá nuevamente en unos segundos.',
    )
  }

  throw new Error(
    detail ?? `${fallbackMessage} (${response.status}).`,
  )
}

export interface GetIncidentsParams {
  pageNumber?: number
  pageSize?: number
  status?: number
  severity?: number
  searchTerm?: string
  assignedTo?: string
}

export async function getIncidents(
  params: GetIncidentsParams = {},
  token?: string,
): Promise<PagedResult<IncidentListItem>> {
  const searchParams = new URLSearchParams()

  searchParams.set(
    'pageNumber',
    String(params.pageNumber ?? 1),
  )

  searchParams.set(
    'pageSize',
    String(params.pageSize ?? 20),
  )

  if (params.status !== undefined) {
    searchParams.set(
      'status',
      String(params.status),
    )
  }

  if (params.severity !== undefined) {
    searchParams.set(
      'severity',
      String(params.severity),
    )
  }

  if (params.searchTerm?.trim()) {
    searchParams.set(
      'searchTerm',
      params.searchTerm.trim(),
    )
  }

  if (params.assignedTo?.trim()) {
    searchParams.set(
      'assignedTo',
      params.assignedTo.trim(),
    )
  }

  const response = await fetch(
    `${API_BASE_URL}/api/incidents?${searchParams.toString()}`,
    {
      headers: token
        ? {
            Authorization: `Bearer ${token}`,
          }
        : undefined,
    },
  )

  if (!response.ok) {
    await throwApiError(
      response,
      'No se pudieron obtener los incidentes',
    )
  }

  return response.json()
}


export async function getIncidentById(
  id: string,
  token?: string,
): Promise<IncidentDetail> {
  const response = await fetch(
    `${API_BASE_URL}/api/incidents/${id}`,
    {
      headers: token
        ? {
            Authorization: `Bearer ${token}`,
          }
        : undefined,
    },
  )

  if (!response.ok) {
    await throwApiError(
      response,
      'No se pudo obtener el incidente',
    )
  }

  return response.json()
}

export async function getIncidentHistory(
  id: string,
  token?: string,
): Promise<IncidentHistoryItem[]> {
  const response = await fetch(
    `${API_BASE_URL}/api/incidents/${id}/history`,
    {
      headers: token
        ? {
            Authorization: `Bearer ${token}`,
          }
        : undefined,
    },
  )

  if (!response.ok) {
    await throwApiError(
      response,
      'No se pudo obtener el historial',
    )
  }

  return response.json()
}

export async function getIncidentNotes(
  id: string,
  token?: string,
): Promise<IncidentNoteItem[]> {
  const response = await fetch(
    `${API_BASE_URL}/api/incidents/${id}/notes`,
    {
      headers: token
        ? {
            Authorization: `Bearer ${token}`,
          }
        : undefined,
    },
  )

  if (!response.ok) {
    await throwApiError(
      response,
      'No se pudieron obtener las notas',
    )
  }

  return response.json()
}

export async function assignIncident(
  id: string,
  analystIdentifier: string,
  token?: string,
): Promise<void> {
  const response = await fetch(
    `${API_BASE_URL}/api/incidents/${id}/assignment`,
    {
      method: 'PATCH',
      headers: {
        'Content-Type': 'application/json',
        ...(token
          ? {
              Authorization: `Bearer ${token}`,
            }
          : {}),
      },
      body: JSON.stringify({
        analystIdentifier,
      }),
    },
  )

  if (!response.ok) {
    await throwApiError(
      response,
      'No se pudo asignar el incidente',
    )
  }
}

export async function changeIncidentStatus(
  id: string,
  status: number,
  token?: string,
): Promise<void> {
  const response = await fetch(
    `${API_BASE_URL}/api/incidents/${id}/status`,
    {
      method: 'PATCH',
      headers: {
        'Content-Type': 'application/json',
        ...(token
          ? {
              Authorization: `Bearer ${token}`,
            }
          : {}),
      },
      body: JSON.stringify({
        status,
      }),
    },
  )

  if (!response.ok) {
    await throwApiError(
      response,
      'No se pudo cambiar el estado',
    )
  }
}

export async function addIncidentNote(
  id: string,
  content: string,
  token?: string,
): Promise<void> {
  const response = await fetch(
    `${API_BASE_URL}/api/incidents/${id}/notes`,
    {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        ...(token
          ? {
              Authorization: `Bearer ${token}`,
            }
          : {}),
      },
      body: JSON.stringify({
        content,
      }),
    },
  )

  if (!response.ok) {
    await throwApiError(
      response,
      'No se pudo agregar la nota',
    )
  }
}

export interface CreateIncidentInput {
  title: string
  description: string
  severity: number
  detectedAt: string
}

export interface CreateIncidentResult {
  id: string
}

export async function createIncident(
  input: CreateIncidentInput,
  token?: string,
): Promise<CreateIncidentResult> {
  const response = await fetch(
    `${API_BASE_URL}/api/incidents`,
    {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        ...(token
          ? {
              Authorization: `Bearer ${token}`,
            }
          : {}),
      },
      body: JSON.stringify(input),
    },
  )

  if (!response.ok) {
    await throwApiError(
      response,
      'No se pudo crear el incidente',
    )
  }

  return response.json()
}

export interface UpdateIncidentInput {
  title: string
  description: string
  severity: number
}

export async function updateIncident(
  id: string,
  input: UpdateIncidentInput,
  token?: string,
): Promise<void> {
  const response = await fetch(
    `${API_BASE_URL}/api/incidents/${id}`,
    {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
        ...(token
          ? {
              Authorization: `Bearer ${token}`,
            }
          : {}),
      },
      body: JSON.stringify(input),
    },
  )

  if (!response.ok) {
    await throwApiError(
      response,
      'No se pudo actualizar el incidente',
    )
  }
}

import type {
  IncidentSummary,
} from '../types/incidents'

export async function getIncidentSummary(
  token?: string,
): Promise<IncidentSummary> {
  const response = await fetch(
    `${API_BASE_URL}/api/incidents/summary`,
    {
      headers: token
        ? {
            Authorization: `Bearer ${token}`,
          }
        : undefined,
    },
  )

  if (!response.ok) {
    throw new Error(
      `No se pudo obtener el resumen (${response.status}).`,
    )
  }

  return response.json()
}
