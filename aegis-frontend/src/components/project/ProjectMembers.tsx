import type { FormEvent } from 'react'
import type { ProjectMember } from '../../types'

type ProjectMembersProps = {
  members: ProjectMember[]
  isLoadingMembers: boolean
  membersError: string
  showAddMemberForm: boolean
  memberEmail: string
  memberRole: string
  isAddingMember: boolean
  addMemberError: string

  onToggleAddMemberForm: () => void
  onCancelAddMemberForm: () => void
  onMemberEmailChange: (email: string) => void
  onMemberRoleChange: (role: string) => void
  onAddMember: (
    event: FormEvent<HTMLFormElement>,
  ) => void
  onRoleChange: (
    userId: number,
    newRole: string,
  ) => void
  onRemoveMember: (
    userId: number,
    email: string,
  ) => void
}

function ProjectMembers({
  members,
  isLoadingMembers,
  membersError,
  showAddMemberForm,
  memberEmail,
  memberRole,
  isAddingMember,
  addMemberError,
  onToggleAddMemberForm,
  onCancelAddMemberForm,
  onMemberEmailChange,
  onMemberRoleChange,
  onAddMember,
  onRoleChange,
  onRemoveMember,
}: ProjectMembersProps) {
  return (
    <section className="members-section">
      <div className="section-heading">
        <div>
          <h2>Project members</h2>

          <p className="section-description">
            Manage who has access to this project.
          </p>
        </div>

        {!membersError && (
          <button
            className="primary-button"
            onClick={onToggleAddMemberForm}
          >
            {showAddMemberForm
              ? 'Cancel'
              : 'Add member'}
          </button>
        )}
      </div>

      {showAddMemberForm && (
        <div className="add-member-card">
          <form
            className="add-member-form"
            onSubmit={onAddMember}
          >
            <label>
              Email

              <input
                type="email"
                value={memberEmail}
                onChange={(event) =>
                  onMemberEmailChange(
                    event.target.value,
                  )
                }
                placeholder="user@example.com"
                required
              />
            </label>

            <label>
              Role

              <select
                value={memberRole}
                onChange={(event) =>
                  onMemberRoleChange(
                    event.target.value,
                  )
                }
              >
                <option value="Viewer">
                  Viewer
                </option>

                <option value="Editor">
                  Editor
                </option>
              </select>
            </label>

            {addMemberError && (
              <div className="auth-error">
                {addMemberError}
              </div>
            )}

            <div className="create-project-actions">
              <button
                type="button"
                className="secondary-button"
                onClick={
                  onCancelAddMemberForm
                }
              >
                Cancel
              </button>

              <button
                type="submit"
                className="primary-button"
                disabled={isAddingMember}
              >
                {isAddingMember
                  ? 'Adding...'
                  : 'Add member'}
              </button>
            </div>
          </form>
        </div>
      )}

      {isLoadingMembers && (
        <p>Loading members...</p>
      )}

      {membersError && (
        <div className="auth-error">
          {membersError}
        </div>
      )}

      {!isLoadingMembers &&
        !membersError &&
        members.length === 0 && (
          <div className="empty-state">
            <h3>No members yet</h3>

            <p>
              This project does not have any
              additional members.
            </p>
          </div>
        )}

      {!isLoadingMembers &&
        !membersError &&
        members.length > 0 && (
          <div className="members-list">
            {members.map((member) => (
              <div
                className="member-row"
                key={member.userId}
              >
                <div className="member-user">
                  <div className="avatar">
                    {member.email
                      .charAt(0)
                      .toUpperCase()}
                  </div>

                  <div>
                    <strong>
                      {member.email}
                    </strong>

                    <p>
                      User ID {member.userId}
                    </p>
                  </div>
                </div>

                <div className="member-actions">
                  <select
                    className={`member-role-select ${member.role.toLowerCase()}`}
                    value={member.role}
                    onChange={(event) =>
                      onRoleChange(
                        member.userId,
                        event.target.value,
                      )
                    }
                  >
                    <option value="Viewer">
                      Viewer
                    </option>

                    <option value="Editor">
                      Editor
                    </option>
                  </select>

                  <button
                    type="button"
                    className="remove-member-button"
                    onClick={() =>
                      onRemoveMember(
                        member.userId,
                        member.email,
                      )
                    }
                  >
                    Remove
                  </button>
                </div>
              </div>
            ))}
          </div>
        )}
    </section>
  )
}

export default ProjectMembers