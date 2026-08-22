export const IncidentSeverity = {
  Low: 1,
  Medium: 2,
  High: 3,
  Critical: 4,
} as const

export type IncidentSeverity =
  (typeof IncidentSeverity)[keyof typeof IncidentSeverity]

export const IncidentStatus = {
  Open: 1,
  UnderInvestigation: 2,
  Contained: 3,
  Resolved: 4,
  Closed: 5,
} as const

export type IncidentStatus =
  (typeof IncidentStatus)[keyof typeof IncidentStatus]

export const IncidentHistoryEventType = {
  Created: 1,
  DetailsUpdated: 2,
  SeverityChanged: 3,
  StatusChanged: 4,
  Closed: 5,
  Assigned: 6,
  NoteAdded: 7,
} as const

export type IncidentHistoryEventType =
  (typeof IncidentHistoryEventType)[keyof typeof IncidentHistoryEventType]

export interface IncidentListItem {
  id: string
  title: string
  severity: IncidentSeverity
  status: IncidentStatus
  detectedAt: string
  createdAt: string
  assignedTo: string | null
  assignedAt: string | null
}

export interface IncidentDetail extends IncidentListItem {
  description: string
}

export interface IncidentHistoryItem {
  id: string
  eventType: IncidentHistoryEventType
  description: string
  previousValue: string | null
  newValue: string | null
  performedBy: string
  occurredAt: string
}

export interface IncidentNoteItem {
  id: string
  content: string
  createdBy: string
  createdAt: string
}

export interface PagedResult<T> {
  items: T[]
  pageNumber: number
  pageSize: number
  totalCount: number
  totalPages: number
  hasPreviousPage: boolean
  hasNextPage: boolean
}

export interface IncidentSummary {
  total: number
  open: number
  critical: number
  underInvestigation: number
  contained: number
  resolved: number
  closed: number
  lowSeverity: number
  mediumSeverity: number
  highSeverity: number
  criticalSeverity: number
}
