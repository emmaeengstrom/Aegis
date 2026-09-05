import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import type {
  AuditLog,
  Project,
  ProjectMember,
  ProjectTab,
  ProjectTask,
} from '../types'
import {
  addProjectMember,
  createProjectTask,
  deleteProjectTask,
  getProject,
  getProjectAuditLogs,
  getProjectMembers,
  getProjectTasks,
  removeProjectMember,
  updateProjectMemberRole,
  updateProjectTask,
} from '../api/projects'

import ProjectOverview from '../components/project/ProjectOverview'
import ProjectActivity from '../components/project/ProjectActivity'
import ProjectMembers from '../components/project/ProjectMembers'
import ProjectTasks from '../components/project/ProjectTasks'
import '../App.css'

function ProjectPage() {
  const { id } = useParams()
  const navigate = useNavigate()

  const [project, setProject] =
    useState<Project | null>(null)

  const [members, setMembers] =
    useState<ProjectMember[]>([])

  const [tasks, setTasks] =
    useState<ProjectTask[]>([])

  const [activeTab, setActiveTab] =
    useState<ProjectTab>('overview')

  const [isLoading, setIsLoading] =
    useState(true)

  const [isLoadingMembers, setIsLoadingMembers] =
    useState(false)

  const [isLoadingTasks, setIsLoadingTasks] =
    useState(false)

  const [error, setError] =
    useState('')

  const [membersError, setMembersError] =
    useState('')

  const [tasksError, setTasksError] =
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

  const [showAddTaskForm, setShowAddTaskForm] =
    useState(false)

  const [taskTitle, setTaskTitle] =
    useState('')

  const [taskDescription, setTaskDescription] =
    useState('')

  const [isAddingTask, setIsAddingTask] =
    useState(false)

  const [addTaskError, setAddTaskError] =
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

  async function loadTasks() {
    setActiveTab('tasks')
    setTasksError('')
    setIsLoadingTasks(true)

    try {
      const response =
        await getProjectTasks(id!)

      if (response.status === 401) {
        navigate('/login')
        return
      }

      if (response.status === 404) {
        setTasksError(
          'Project not found or you do not have access.',
        )
        return
      }

      if (!response.ok) {
        setTasksError(
          'Could not load tasks.',
        )
        return
      }

      const data: ProjectTask[] =
        await response.json()

      setTasks(data)
    } catch {
      setTasksError(
        'Could not connect to the server.',
      )
    } finally {
      setIsLoadingTasks(false)
    }
  }

  async function handleAddTask(
    event: FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault()

    setAddTaskError('')
    setIsAddingTask(true)

    try {
      const response =
        await createProjectTask(
          id!,
          taskTitle,
          taskDescription,
        )

      if (response.status === 401) {
        navigate('/login')
        return
      }

      if (response.status === 404) {
        setAddTaskError(
          'You do not have permission to create tasks.',
        )
        return
      }

      if (!response.ok) {
        const message =
          await response.text()

        setAddTaskError(
          message || 'Could not create task.',
        )
        return
      }

      const newTask: ProjectTask =
        await response.json()

      setTasks((currentTasks) => [
        newTask,
        ...currentTasks,
      ])

      setTaskTitle('')
      setTaskDescription('')
      setShowAddTaskForm(false)
    } catch {
      setAddTaskError(
        'Could not connect to the server.',
      )
    } finally {
      setIsAddingTask(false)
    }
  }

  async function handleTaskStatusChange(
    task: ProjectTask,
    newStatus: ProjectTask['status'],
  ) {
    try {
      const response =
        await updateProjectTask(
          id!,
          task.id,
          task.title,
          task.description,
          newStatus,
        )

      if (response.status === 401) {
        navigate('/login')
        return
      }

      if (response.status === 404) {
        alert(
          'You do not have permission to update this task.',
        )
        return
      }

      if (!response.ok) {
        alert(
          'Could not update task status.',
        )
        return
      }

      setTasks((currentTasks) =>
        currentTasks.map((currentTask) =>
          currentTask.id === task.id
            ? {
                ...currentTask,
                status: newStatus,
              }
            : currentTask,
        ),
      )
    } catch {
      alert(
        'Could not connect to the server.',
      )
    }
  }

  async function handleDeleteTask(
    task: ProjectTask,
  ) {
    const confirmed = window.confirm(
      `Delete "${task.title}"?`,
    )

    if (!confirmed) {
      return
    }

    try {
      const response =
        await deleteProjectTask(
          id!,
          task.id,
        )

      if (response.status === 401) {
        navigate('/login')
        return
      }

      if (response.status === 404) {
        alert(
          'You do not have permission to delete this task.',
        )
        return
      }

      if (!response.ok) {
        alert(
          'Could not delete task.',
        )
        return
      }

      setTasks((currentTasks) =>
        currentTasks.filter(
          (currentTask) =>
            currentTask.id !== task.id,
        ),
      )
    } catch {
      alert(
        'Could not connect to the server.',
      )
    }
  }

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
                activeTab === 'tasks'
                  ? 'active'
                  : ''
              }`}
              onClick={loadTasks}
            >
              Tasks
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

              {activeTab === 'tasks' && (
                <ProjectTasks
                  tasks={tasks}
                  isLoadingTasks={
                    isLoadingTasks
                  }
                  tasksError={tasksError}
                  showAddTaskForm={
                    showAddTaskForm
                  }
                  taskTitle={taskTitle}
                  taskDescription={
                    taskDescription
                  }
                  isAddingTask={
                    isAddingTask
                  }
                  addTaskError={
                    addTaskError
                  }
                  onToggleAddTaskForm={() =>
                    setShowAddTaskForm(
                      (current) =>
                        !current,
                    )
                  }
                  onCancelAddTaskForm={() => {
                    setShowAddTaskForm(false)
                    setAddTaskError('')
                    setTaskTitle('')
                    setTaskDescription('')
                  }}
                  onTaskTitleChange={
                    setTaskTitle
                  }
                  onTaskDescriptionChange={
                    setTaskDescription
                  }
                  onAddTask={
                    handleAddTask
                  }
                  onTaskStatusChange={
                    handleTaskStatusChange
                  }
                  onDeleteTask={
                    handleDeleteTask
                  } 
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