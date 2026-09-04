import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import type {
  AuditLog,
  Project,
  ProjectMember,
  ProjectTab,
} from '../types'
import {
  addProjectMember,
  getProject,
  getProjectAuditLogs,
  getProjectMembers,
  removeProjectMember,
  updateProjectMemberRole,
} from '../api/projects'

import ProjectOverview from '../components/project/ProjectOverview'
import ProjectActivity from '../components/project/ProjectActivity'
import ProjectMembers from '../components/project/ProjectMembers'
import '../App.css'

function ProjectPage() {
  const { id } = useParams()
  const navigate = useNavigate()

  const [project, setProject] =
    useState<Project | null>(null)

  const [members, setMembers] =
    useState<ProjectMember[]>([])

  const [activeTab, setActiveTab] =
    useState<ProjectTab>('overview')

  const [isLoading, setIsLoading] =
    useState(true)

  const [isLoadingMembers, setIsLoadingMembers] =
    useState(false)

  const [error, setError] =
    useState('')

  const [membersError, setMembersError] =
    useState('')

  const [showAddMemberForm, setShowAddMemberForm] =
    useState(false)

  const [memberEmail, setMemberEmail] =
    useState('')

  const [memberRole, setMemberRole] =
    useState('Viewer')

  const [isAddingMember, setIsAddingMember] =
    useState(false)

  const [addMemberError, setAddMemberError] =
    useState('')

  const [auditLogs, setAuditLogs] =
    useState<AuditLog[]>([])

  const [isLoadingActivity, setIsLoadingActivity] =
    useState(false)

  const [activityError, setActivityError] =
    useState('')

  const email =
    sessionStorage.getItem('aegis_user_email') ?? 'User'

  useEffect(() => {
    async function loadProject() {
      try {
        const response = await getProject(id!)

        if (response.status === 401) {
          navigate('/login')
          return
        }

        if (response.status === 404) {
          setError(
            'Project not found or you do not have access.',
          )
          return
        }

        if (!response.ok) {
          setError('Could not load project.')
          return
        }

        const data: Project =
          await response.json()

        setProject(data)
      } catch {
        setError(
          'Could not connect to the server.',
        )
      } finally {
        setIsLoading(false)
      }
    }

    loadProject()
  }, [id, navigate])

  async function loadMembers() {
    setActiveTab('members')
    setMembersError('')
    setIsLoadingMembers(true)

    try {
      const response =
        await getProjectMembers(id!)

      if (response.status === 401) {
        navigate('/login')
        return
      }

      if (response.status === 404) {
        setMembersError(
          'Only the project owner can manage members.',
        )
        return
      }

      if (!response.ok) {
        setMembersError(
          'Could not load members.',
        )
        return
      }

      const data: ProjectMember[] =
        await response.json()

      setMembers(data)
    } catch {
      setMembersError(
        'Could not connect to the server.',
      )
    } finally {
      setIsLoadingMembers(false)
    }
  }

  async function handleAddMember(
    event: FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault()

    setAddMemberError('')
    setIsAddingMember(true)

    try {
      const response =
        await addProjectMember(
          id!,
          memberEmail,
          memberRole,
        )

      if (response.status === 401) {
        navigate('/login')
        return
      }

      if (!response.ok) {
        const message =
          await response.text()

        setAddMemberError(
          message || 'Could not add member.',
        )
        return
      }

      const newMember: ProjectMember =
        await response.json()

      setMembers((currentMembers) => [
        ...currentMembers,
        newMember,
      ])

      setMemberEmail('')
      setMemberRole('Viewer')
      setShowAddMemberForm(false)
    } catch {
      setAddMemberError(
        'Could not connect to the server.',
      )
    } finally {
      setIsAddingMember(false)
    }
  }

  async function handleRoleChange(
    userId: number,
    newRole: string,
  ) {
    try {
      const response =
        await updateProjectMemberRole(
          id!,
          userId,
          newRole,
        )

      if (response.status === 401) {
        navigate('/login')
        return
      }

      if (!response.ok) {
        alert(
          'Could not update member role.',
        )
        return
      }

      setMembers((currentMembers) =>
        currentMembers.map((member) =>
          member.userId === userId
            ? {
                ...member,
                role: newRole,
              }
            : member,
        ),
      )
    } catch {
      alert(
        'Could not connect to the server.',
      )
    }
  }

  async function handleRemoveMember(
    userId: number,
    memberEmail: string,
  ) {
    const confirmed = window.confirm(
      `Remove ${memberEmail} from this project?`,
    )

    if (!confirmed) {
      return
    }

    try {
      const response =
        await removeProjectMember(
          id!,
          userId,
        )

      if (response.status === 401) {
        navigate('/login')
        return
      }

      if (!response.ok) {
        alert(
          'Could not remove member.',
        )
        return
      }

      setMembers((currentMembers) =>
        currentMembers.filter(
          (member) =>
            member.userId !== userId,
        ),
      )
    } catch {
      alert(
        'Could not connect to the server.',
      )
    }
  }

  async function loadActivity() {
    setActiveTab('activity')
    setActivityError('')
    setIsLoadingActivity(true)

    try {
      const response =
        await getProjectAuditLogs(id!)

      if (response.status === 401) {
        navigate('/login')
        return
      }

      if (response.status === 404) {
        setActivityError(
          'Project not found or you do not have access.',
        )
        return
      }

      if (!response.ok) {
        setActivityError(
          'Could not load activity.',
        )
        return
      }

      const data: AuditLog[] =
        await response.json()

      setAuditLogs(data)
    } catch {
      setActivityError(
        'Could not connect to the server.',
      )
    } finally {
      setIsLoadingActivity(false)
    }
  }

  return (
    <div className="app">
      <header className="topbar">
        <div className="brand">
          <div className="brand-mark">
            A
          </div>

          <span>Aegis</span>
        </div>

        <div className="user-menu">
          <span>{email}</span>

          <div className="avatar">
            {email.charAt(0).toUpperCase()}
          </div>
        </div>
      </header>

      <div className="layout">
        <aside className="sidebar">
          <nav>
            <button
              className="nav-item"
              onClick={() =>
                navigate('/dashboard')
              }
            >
              Dashboard
            </button>

            <button
              className={`nav-item ${
                activeTab === 'overview'
                  ? 'active'
                  : ''
              }`}
              onClick={() =>
                setActiveTab('overview')
              }
            >
              Overview
            </button>

            <button
              className={`nav-item ${
                activeTab === 'members'
                  ? 'active'
                  : ''
              }`}
              onClick={loadMembers}
            >
              Members
            </button>

            <button
              className={`nav-item ${
                activeTab === 'activity'
                  ? 'active'
                  : ''
              }`}
              onClick={loadActivity}
            >
              Activity
            </button>
          </nav>

          <button className="nav-item">
            Settings
          </button>
        </aside>

        <main className="content">
          {isLoading && (
            <p>Loading project...</p>
          )}

          {error && (
            <div className="auth-error">
              {error}
            </div>
          )}

          {!isLoading && project && (
            <>
              <section className="page-header">
                <div>
                  <p className="eyebrow">
                    Project
                  </p>

                  <h1>{project.name}</h1>

                  <p className="subtitle">
                    {project.description ||
                      'No description.'}
                  </p>
                </div>
              </section>

              {activeTab === 'overview' && (
                <ProjectOverview
                  project={project}
                />
              )}

              {activeTab === 'members' && (
                <ProjectMembers
                  members={members}
                  isLoadingMembers={
                    isLoadingMembers
                  }
                  membersError={
                    membersError
                  }
                  showAddMemberForm={
                    showAddMemberForm
                  }
                  memberEmail={
                    memberEmail
                  }
                  memberRole={
                    memberRole
                  }
                  isAddingMember={
                    isAddingMember
                  }
                  addMemberError={
                    addMemberError
                  }
                  onToggleAddMemberForm={() =>
                    setShowAddMemberForm(
                      (current) =>
                        !current,
                    )
                  }
                  onCancelAddMemberForm={() =>
                    setShowAddMemberForm(
                      false,
                    )
                  }
                  onMemberEmailChange={
                    setMemberEmail
                  }
                  onMemberRoleChange={
                    setMemberRole
                  }
                  onAddMember={
                    handleAddMember
                  }
                  onRoleChange={
                    handleRoleChange
                  }
                  onRemoveMember={
                    handleRemoveMember
                  }
                />
              )}

              {activeTab === 'activity' && (
                <ProjectActivity
                  auditLogs={auditLogs}
                  isLoadingActivity={
                    isLoadingActivity
                  }
                  activityError={
                    activityError
                  }
                />
              )}
            </>
          )}
        </main>
      </div>
    </div>
  )
}

export default ProjectPage