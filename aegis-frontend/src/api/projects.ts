import { apiFetch } from '../api'

export async function getProject(
  id: string,
): Promise<Response> {
  return apiFetch(`/api/projects/${id}`)
}

export async function getProjectMembers(
  id: string,
): Promise<Response> {
  return apiFetch(
    `/api/projects/${id}/members`,
  )
}

export async function addProjectMember(
  id: string,
  email: string,
  role: string,
): Promise<Response> {
  return apiFetch(
    `/api/projects/${id}/members`,
    {
      method: 'POST',
      body: JSON.stringify({
        email,
        role,
      }),
    },
  )
}

export async function updateProjectMemberRole(
  id: string,
  userId: number,
  role: string,
): Promise<Response> {
  return apiFetch(
    `/api/projects/${id}/members/${userId}/role`,
    {
      method: 'PUT',
      body: JSON.stringify({
        role,
      }),
    },
  )
}

export async function removeProjectMember(
  id: string,
  userId: number,
): Promise<Response> {
  return apiFetch(
    `/api/projects/${id}/members/${userId}`,
    {
      method: 'DELETE',
    },
  )
}

export async function getProjectAuditLogs(
  id: string,
): Promise<Response> {
  return apiFetch(
    `/api/projects/${id}/audit-logs`,
  )
} 