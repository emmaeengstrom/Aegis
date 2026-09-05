import type { FormEvent } from 'react'
import type { ProjectTask } from '../../types'

type ProjectTasksProps = {
  tasks: ProjectTask[]
  isLoadingTasks: boolean
  tasksError: string
  showAddTaskForm: boolean
  taskTitle: string
  taskDescription: string
  isAddingTask: boolean
  addTaskError: string
  onToggleAddTaskForm: () => void
  onCancelAddTaskForm: () => void
  onTaskTitleChange: (value: string) => void
  onTaskDescriptionChange: (value: string) => void
  onAddTask: (
    event: FormEvent<HTMLFormElement>,
  ) => void
  onTaskStatusChange: (
    task: ProjectTask,
    newStatus: ProjectTask['status'],
  ) => void
  onDeleteTask: (task: ProjectTask) => void
}

function ProjectTasks({
  tasks,
  isLoadingTasks,
  tasksError,
  showAddTaskForm,
  taskTitle,
  taskDescription,
  isAddingTask,
  addTaskError,
  onToggleAddTaskForm,
  onCancelAddTaskForm,
  onTaskTitleChange,
  onTaskDescriptionChange,
  onAddTask,
  onTaskStatusChange,
  onDeleteTask,
}: ProjectTasksProps) {
  return (
    <section className="panel">
      <div className="section-heading">
        <div>
          <p className="eyebrow">
            Project work
          </p>

          <h2>Tasks</h2>

          <p className="subtitle">
            Track the work that belongs to this project.
          </p>
        </div>

        <button
          type="button"
          className="primary-button"
          onClick={onToggleAddTaskForm}
        >
          New task
        </button>
      </div>

      {showAddTaskForm && (
        <form
          className="task-form"
          onSubmit={onAddTask}
        >
          <label>
            Title

            <input
              type="text"
              value={taskTitle}
              onChange={(event) =>
                onTaskTitleChange(
                  event.target.value,
                )
              }
              maxLength={150}
              required
            />
          </label>

          <label>
            Description

            <textarea
              value={taskDescription}
              onChange={(event) =>
                onTaskDescriptionChange(
                  event.target.value,
                )
              }
              maxLength={1000}
              rows={4}
            />
          </label>

          {addTaskError && (
            <div className="auth-error">
              {addTaskError}
            </div>
          )}

          <div className="form-actions">
            <button
              type="submit"
              className="primary-button"
              disabled={isAddingTask}
            >
              {isAddingTask
                ? 'Creating...'
                : 'Create task'}
            </button>

            <button
              type="button"
              className="secondary-button"
              onClick={onCancelAddTaskForm}
              disabled={isAddingTask}
            >
              Cancel
            </button>
          </div>
        </form>
      )}

      {isLoadingTasks && (
        <p>Loading tasks...</p>
      )}

      {tasksError && (
        <div className="auth-error">
          {tasksError}
        </div>
      )}

      {!isLoadingTasks &&
        !tasksError &&
        tasks.length === 0 && (
          <p>
            No tasks have been created yet.
          </p>
        )}

      {!isLoadingTasks &&
        !tasksError &&
        tasks.length > 0 && (
          <div className="task-list">
            {tasks.map((task) => (
              <article
                className="task-card"
                key={task.id}
              >
                <div>
                  <h3>{task.title}</h3>

                  <p>
                    {task.description ||
                      'No description.'}
                  </p>
                </div>

                <label>
                  Status

                  <select
                    value={task.status}
                    onChange={(event) =>
                      onTaskStatusChange(
                        task,
                        event.target.value as ProjectTask['status'],
                      )
                    }
                  >
                    <option value="Todo">
                      Todo
                    </option>

                    <option value="InProgress">
                      In Progress
                    </option>

                    <option value="Done">
                      Done
                    </option>
                  </select>
                </label>

                <button
                  type="button"
                  className="secondary-button"
                  onClick={() =>
                    onDeleteTask(task)
                  }
                >
                  Delete
                </button>
              </article>
            ))}
          </div>
        )}
    </section>
  )
}

export default ProjectTasks 