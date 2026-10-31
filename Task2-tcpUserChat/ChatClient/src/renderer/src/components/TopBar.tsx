import { useState } from 'react'
import type { User } from '../types'

type TopBarProps = {
  user: User | null
  error: string
  onLogin: (username: string, password: string) => void
  onRegister: (username: string, password: string) => void
}

function TopBar({ user, error, onLogin, onRegister }: TopBarProps): React.JSX.Element {
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')

  if (user !== null) {
    return (
      <header className="topbar panel">
        <div className="logo">💬</div>
        <div className="topbar-user">
          <span>
            Connected as <b>{user.username}</b>
          </span>
          <span className="user-id">User ID: {user.id}</span>
        </div>
      </header>
    )
  }

  return (
    <header className="topbar panel">
      <div className="logo">💬</div>

      <div className="topbar-fields">
        <input
          placeholder="Username"
          value={username}
          onChange={(e) => setUsername(e.target.value)}
        />
        <input
          placeholder="Password"
          type="password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
        />
      </div>

      {error && <span className="topbar-error">{error}</span>}

      <div className="topbar-actions">
        <button className="btn" onClick={() => onLogin(username, password)}>
          Log in
        </button>
        <button className="btn btn-outline" onClick={() => onRegister(username, password)}>
          Register
        </button>
      </div>
    </header>
  )
}

export default TopBar