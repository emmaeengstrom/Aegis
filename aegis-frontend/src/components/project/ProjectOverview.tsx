import type { Project } from '../../types'

type ProjectOverviewProps = {
  project: Project
}

function ProjectOverview({
  project,
}: ProjectOverviewProps) {
  return (
    <section className="project-overview">
      <div className="overview-card">
        <p className="overview-label">
          Project ID
        </p>

        <strong>{project.id}</strong>
      </div>

      <div className="overview-card">
        <p className="overview-label">
          Created
        </p>

        <strong>
          {new Date(
            project.createdAt,
          ).toLocaleDateString()}
        </strong>
      </div>

      <div className="overview-card">
        <p className="overview-label">
          Status
        </p>

        <strong>Active</strong>
      </div>
    </section>
  )
}

export default ProjectOverview 