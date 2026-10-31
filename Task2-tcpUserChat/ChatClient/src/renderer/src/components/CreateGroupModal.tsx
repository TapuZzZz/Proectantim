import { useState } from 'react'

type CreateGroupModalProps = {
  onCreate: (name: string) => void
  onClose: () => void
}

function CreateGroupModal({ onCreate, onClose }: CreateGroupModalProps): React.JSX.Element {
  const [name, setName] = useState('')

  function handleCreate(): void {
    if (name.trim() === '') {
      return
    }

    onCreate(name.trim())
    onClose()
  }

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal panel" onClick={(e) => e.stopPropagation()}>
        <h3>New group</h3>
        <input
          placeholder="Group name"
          value={name}
          onChange={(e) => setName(e.target.value)}
          autoFocus
        />
        <div className="modal-actions">
          <button className="btn btn-outline" onClick={onClose}>
            Cancel
          </button>
          <button className="btn" onClick={handleCreate}>
            Create
          </button>
        </div>
      </div>
    </div>
  )
}

export default CreateGroupModal