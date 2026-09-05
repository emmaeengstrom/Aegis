import { useState } from 'react'
import type { FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { apiFetch } from '../api' 

type LoginResponse = {
  id: number
  email: string
  token: string
}

function LoginPage() {
  const navigate = useNavigate()

  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [isLoading, setIsLoading] = useState(false)

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    setError('')
    setIsLoading(true)

    try {
      const response = await apiFetch(
        '/api/auth/login',
        {
          method: 'POST',
          body: JSON.stringify({
            email,
            password,
          }),
        },
      ) 

      if (!response.ok) {
        if (response.status === 401) {
          setError('Invalid email or password.')
          return
        }

        setError('Something went wrong. Please try again.')
        return
      }

      const data: LoginResponse = await response.json()

      sessionStorage.setItem('aegis_token', data.token)
      sessionStorage.setItem('aegis_user_email', data.email)
      sessionStorage.setItem('aegis_user_id', data.id.toString())

      navigate('/dashboard')
    } catch {
      setError('Could not connect to the server.')
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="auth-page">
      <div className="auth-card">
        <div className="auth-brand">
          <div className="brand-mark">A</div>

          <div>
            <strong>Aegis</strong>
            <p>Secure project workspace</p>
          </div>
        </div>

        <div className="auth-heading">
          <h1>Welcome back</h1>
          <p>Sign in to continue to your workspace.</p>
        </div>

        <form className="auth-form" onSubmit={handleSubmit}>
          <label>
            Email
            <input
              type="email"
              value={email}
              onChange={(event) => setEmail(event.target.value)}
              placeholder="you@example.com"
              required
              autoComplete="email"
            />
          </label>

          <label>
            Password
            <input
              type="password"
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              placeholder="Enter your password"
              required
              autoComplete="current-password"
            />
          </label>

          {error && (
            <div className="auth-error">
              {error}
            </div>
          )}

          <button
            className="primary-button auth-submit"
            type="submit"
            disabled={isLoading}
          >
            {isLoading ? 'Signing in...' : 'Sign in'}
          </button>
        </form>

        <p className="auth-footer">
          Don't have an account?{' '}
          <a href="/register">Create account</a>
        </p>
      </div>
    </div>
  )
}

export default LoginPage