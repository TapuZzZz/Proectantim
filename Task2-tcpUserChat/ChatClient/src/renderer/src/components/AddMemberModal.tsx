import { useState } from 'react'
import type { User } from '../types'

type AddMemberModalProps = {
  users: User[]
  onAdd: (userId: number) => void
  onClose: () => void
}

function AddMemberModal({ users, onAdd, onClose }: AddMemberModalProps): React.JSX.Element {
  const [selectedId, setSelectedId] = useState(users.length > 0 ? users[0].id : 0)

  function handleAdd(): void {
    if (selectedId === 0) {
      return
    }

    onAdd(selectedId)
    onClose()
  }

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal panel" onClick={(e) => e.stopPropagation()}>
        <h3>Add member</h3>
        <select value={selectedId} onChange={(e) => setSelectedId(Number(e.target.value))}>
          {users.map((user) => (
            <option key={user.id} value={user.id}>
              {user.username}
            </option>
          ))}
        </select>
        <div className="modal-actions">
          <button className="btn btn-outline" onClick={onClose}>
            Cancel
          </button>
          <button className="btn" onClick={handleAdd}>
            Add
          </button>
        </div>
      </div>
    </div>
  )
}

export default AddMemberModal