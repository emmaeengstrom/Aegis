export type Project = {
  id: number
  name: string
  description: string
  createdAt: string
}

export type ProjectMember = {
  userId: number
  email: string
  role: string
}

export type AuditLog = {
  id: number
  userId: number
  userEmail: string
  action: string
  details: string
  createdAt: string
}

export type ProjectTask = {
  id: number
  title: string
  description: string
  status: 'Todo' | 'InProgress' | 'Done'
  createdAt: string
  projectId: number
}

export type ProjectTab =
  | 'overview'
  | 'tasks'
  | 'members'
  | 'activity' 