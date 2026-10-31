import { useState } from 'react'
import CreateGroupModal from './CreateGroupModal'
import type { Chat, Group, User } from '../types'

type SidebarProps = {
  users: User[]
  groups: Group[]
  onCreateGroup: (name: string) => void
  onSelectChat: (chat: Chat) => void
}

function Sidebar({ users, groups, onCreateGroup, onSelectChat }: SidebarProps): React.JSX.Element {
  const [userSearch, setUserSearch] = useState('')
  const [groupSearch, setGroupSearch] = useState('')
  const [showCreateGroup, setShowCreateGroup] = useState(false)

  const filteredUsers = users.filter((user) =>
    user.username.toLowerCase().includes(userSearch.toLowerCase())
  )

  const filteredGroups = groups.filter((group) =>
    group.name.toLowerCase().includes(groupSearch.toLowerCase())
  )

  return (
    <aside className="sidebar panel">
      <section className="side-section">
        <h3>Users</h3>
        <input
          placeholder="Search users..."
          value={userSearch}
          onChange={(e) => setUserSearch(e.target.value)}
        />
        <ul className="list">
          {filteredUsers.map((user) => (
            <li
              key={user.id}
              onClick={() => onSelectChat({ id: user.id, name: user.username, isGroup: false })}
            >
              {user.username}
            </li>
          ))}
        </ul>
      </section>

      <section className="side-section">
        <div className="section-header">
          <h3>My Groups</h3>
          <button className="icon-btn" onClick={() => setShowCreateGroup(true)}>
            +
          </button>
        </div>
        <input
          placeholder="Search groups..."
          value={groupSearch}
          onChange={(e) => setGroupSearch(e.target.value)}
        />
        <ul className="list">
          {filteredGroups.map((group) => (
            <li
              key={group.id}
              onClick={() => onSelectChat({ id: group.id, name: group.name, isGroup: true })}
            >
              {group.name}
            </li>
          ))}
        </ul>
      </section>

      {showCreateGroup && (
        <CreateGroupModal onCreate={onCreateGroup} onClose={() => setShowCreateGroup(false)} />
      )}
    </aside>
  )
}

export default Sidebar