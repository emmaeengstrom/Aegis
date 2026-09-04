import type { AuditLog } from '../../types'

type ProjectActivityProps = {
  auditLogs: AuditLog[]
  isLoadingActivity: boolean
  activityError: string
}

function formatActivityAction(action: string) {
  switch (action) {
    case 'ProjectCreated':
      return 'Project created'

    case 'ProjectUpdated':
      return 'Project updated'

    case 'ProjectMemberAdded':
      return 'Member added'

    case 'ProjectMemberRoleChanged':
      return 'Member role changed'

    case 'ProjectMemberRemoved':
      return 'Member removed'

    default:
      return action
  }
}

function getActivityIcon(action: string) {
  switch (action) {
    case 'ProjectCreated':
      return '+'

    case 'ProjectUpdated':
      return '✎'

    case 'ProjectMemberAdded':
      return '+'

    case 'ProjectMemberRoleChanged':
      return '↔'

    case 'ProjectMemberRemoved':
      return '−'

    default:
      return '•'
  }
}

function ProjectActivity({
  auditLogs,
  isLoadingActivity,
  activityError,
}: ProjectActivityProps) {
  return (
    <section className="activity-section">
      <div className="section-heading">
        <div>
          <h2>Activity</h2>

          <p className="section-description">
            Security and project activity for this
            workspace.
          </p>
        </div>

        {!activityError && (
          <span>
            {auditLogs.length}{' '}
            {auditLogs.length === 1
              ? 'event'
              : 'events'}
          </span>
        )}
      </div>

      {isLoadingActivity && (
        <p>Loading activity...</p>
      )}

      {activityError && (
        <div className="auth-error">
          {activityError}
        </div>
      )}

      {!isLoadingActivity &&
        !activityError &&
        auditLogs.length === 0 && (
          <div className="empty-state">
            <h3>No activity yet</h3>

            <p>
              Project activity will appear here.
            </p>
          </div>
        )}

      {!isLoadingActivity &&
        !activityError &&
        auditLogs.length > 0 && (
          <div className="audit-list">
            {auditLogs.map((log) => (
              <div
                className="audit-item"
                key={log.id}
              >
                <div className="audit-icon">
                  {getActivityIcon(log.action)}
                </div>

                <div className="audit-content">
                  <div className="audit-header">
                    <strong>
                      {formatActivityAction(
                        log.action,
                      )}
                    </strong>

                    <span>
                      {new Date(
                        log.createdAt,
                      ).toLocaleString()}
                    </span>
                  </div>

                  <p>{log.details}</p>

                  <div className="audit-user">
                    {log.userEmail}
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
    </section>
  )
}

export default ProjectActivity