import { useEffect, useState } from 'react'
import type { FormEvent } from 'react' 
import { useNavigate } from 'react-router-dom'
import type { Project } from '../types'
import { apiFetch } from '../api' 
import '../App.css' 



function DashboardPage() {
  const navigate = useNavigate()

  const [projects, setProjects] = useState<Project[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState('')

  const [showCreateForm, setShowCreateForm] = useState(false)
  const [projectName, setProjectName] = useState('')
  const [projectDescription, setProjectDescription] = useState('')
  const [isCreating, setIsCreating] = useState(false)
  const [createError, setCreateError] = useState('')

  const email =
    sessionStorage.getItem('aegis_user_email') ?? 'User'

  useEffect(() => {
    async function loadProjects() {
      const token = sessionStorage.getItem('aegis_token')

      if (!token) {
        navigate('/login')
        return
      }

      try {
        const response = await apiFetch('/api/projects') 

        if (response.status === 401) {
          navigate('/login')
          return
        }

        if (!response.ok) {
          setError('Could not load projects.')
          return
        }

        const data: Project[] = await response.json()
        setProjects(data)
      } catch {
        setError('Could not connect to the server.')
      } finally {
        setIsLoading(false)
      }
    }

    loadProjects()
  }, [navigate])

  async function handleCreateProject(
    event: FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault()

    const token = sessionStorage.getItem('aegis_token')

    if (!token) {
      navigate('/login')
      return
    }

    setCreateError('')
    setIsCreating(true)

    try {
      const response = await apiFetch('/api/projects', {
        method: 'POST',
        body: JSON.stringify({
            name: projectName,
            description: projectDescription,
        }),
        })

      if (response.status === 401) { 
        navigate('/login')
        return
      }

      if (!response.ok) {
        setCreateError('Could not create project.')
        return
      }

      const newProject: Project = await response.json()

      setProjects((currentProjects) => [
        newProject,
        ...currentProjects,
      ])

      setProjectName('')
      setProjectDescription('')
      setShowCreateForm(false)
    } catch {
      setCreateError('Could not connect to the server.')
    } finally {
      setIsCreating(false)
    }
  }

  return (
    <div className="app">
      <header className="topbar">
        <div className="brand">
          <div className="brand-mark">A</div>
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
            <button className="nav-item active">
              Dashboard
            </button>

            <button className="nav-item">
              Projects
            </button>

            <button className="nav-item">
              Activity
            </button>
          </nav>

          <button className="nav-item">
            Settings
          </button>
        </aside>

        <main className="content">
          <section className="page-header">
            <div>
              <p className="eyebrow">Workspace</p>

              <h1>Dashboard</h1>

              <p className="subtitle">
                Manage your projects, members and activity.
              </p>
            </div>

            <button
              className="primary-button"
              onClick={() =>
                setShowCreateForm((current) => !current)
              }
            >
              {showCreateForm ? 'Cancel' : 'New project'}
            </button>
          </section>

          {showCreateForm && (
            <section className="create-project-card">
              <h2>Create project</h2>

              <form
                className="create-project-form"
                onSubmit={handleCreateProject}
              >
                <label>
                  Project name

                  <input
                    type="text"
                    value={projectName}
                    onChange={(event) =>
                      setProjectName(event.target.value)
                    }
                    maxLength={100}
                    required
                    placeholder="Security review"
                  />
                </label>

                <label>
                  Description

                  <textarea
                    value={projectDescription}
                    onChange={(event) =>
                      setProjectDescription(event.target.value)
                    }
                    maxLength={500}
                    placeholder="Describe the project..."
                  />
                </label>

                {createError && (
                  <div className="auth-error">
                    {createError}
                  </div>
                )}

                <div className="create-project-actions">
                  <button
                    type="button"
                    className="secondary-button"
                    onClick={() => setShowCreateForm(false)}
                  >
                    Cancel
                  </button>

                  <button
                    type="submit"
                    className="primary-button"
                    disabled={isCreating}
                  >
                    {isCreating
                      ? 'Creating...'
                      : 'Create project'}
                  </button>
                </div>
              </form>
            </section>
          )}

          <section className="projects-section">
            <div className="section-heading">
              <h2>Your projects</h2>

              <span>
                {projects.length}{' '}
                {projects.length === 1
                  ? 'project'
                  : 'projects'}
              </span>
            </div>

            {isLoading && <p>Loading projects...</p>}

            {error && (
              <div className="auth-error">
                {error}
              </div>
            )}

            {!isLoading &&
              !error &&
              projects.length === 0 && (
                <p>You do not have any projects yet.</p>
              )}

            <div className="project-grid">
              {projects.map((project) => (
                <article
                  key={project.id}
                  className="project-card"
                  onClick={() =>
                    navigate(`/projects/${project.id}`)
                  }
                  style={{ cursor: 'pointer' }}
                >
                  <div className="project-card-top">
                    <div>
                      <h3>{project.name}</h3>

                      <p>
                        {project.description ||
                          'No description.'}
                      </p>
                    </div>
                  </div>

                  <div className="project-meta">
                    <span>
                      Created{' '}
                      {new Date(
                        project.createdAt,
                      ).toLocaleDateString()}
                    </span>

                    <span>Open project →</span>
                  </div>
                </article>
              ))}
            </div>
          </section>
        </main>
      </div>
    </div>
  )
}

export default DashboardPage