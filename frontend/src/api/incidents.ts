import type {
  IncidentListItem,
  PagedResult,
} from '../types/incidents'

const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5106'

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
    throw new Error(
      `No se pudieron obtener los incidentes (${response.status}).`,
    )
  }

  return response.json()
}

import type {
  IncidentDetail,
  IncidentHistoryItem,
  IncidentNoteItem,
} from '../types/incidents'

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
    throw new Error(
      `No se pudo obtener el incidente (${response.status}).`,
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
    throw new Error(
      `No se pudo obtener el historial (${response.status}).`,
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
    throw new Error(
      `No se pudieron obtener las notas (${response.status}).`,
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
    throw new Error(
      `No se pudo asignar el incidente (${response.status}).`,
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
    throw new Error(
      `No se pudo cambiar el estado (${response.status}).`,
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
    throw new Error(
      `No se pudo agregar la nota (${response.status}).`,
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
    throw new Error(
      `No se pudo crear el incidente (${response.status}).`,
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
    throw new Error(
      `No se pudo actualizar el incidente (${response.status}).`,
    )
  }
}
