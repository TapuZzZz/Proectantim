import { useState } from 'react'
import AddMemberModal from './AddMemberModal'
import type { Chat, Message, User } from '../types'

type ChatPanelProps = {
  chat: Chat | null
  messages: Message[]
  myId: number
  users: User[]
  notice: string
  onSend: (content: string) => void
  onAddMember: (userId: number) => void
}

function formatTime(createdAt: string): string {
  return new Date(createdAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
}

function ChatPanel({
  chat,
  messages,
  myId,
  users,
  notice,
  onSend,
  onAddMember
}: ChatPanelProps): React.JSX.Element {
  const [text, setText] = useState('')
  const [showAddMember, setShowAddMember] = useState(false)

  function handleSend(): void {
    if (text.trim() === '') {
      return
    }

    onSend(text.trim())
    setText('')
  }

  if (chat === null) {
    return <section className="chat panel chat-empty">Select a user or group to start chatting</section>
  }

  // אי אפשר להוסיף את עצמי
  const otherUsers = users.filter((user) => user.id !== myId)

  return (
    <section className="chat panel">
      <div className="chat-header">
        <h2>{chat.name}</h2>
        {notice && <span className="notice">{notice}</span>}
        {chat.isGroup && (
          <button className="btn btn-outline" onClick={() => setShowAddMember(true)}>
            Add member
          </button>
        )}
      </div>

      <div className="messages">
        {messages.map((message, index) => (
          <div key={index} className={message.senderId === myId ? 'bubble mine' : 'bubble'}>
            {message.senderId !== myId && <div className="sender">{message.senderName}</div>}
            <div>{message.content}</div>
            <div className="time">{formatTime(message.createdAt)}</div>
          </div>
        ))}
      </div>

      <div className="composer">
        <input
          placeholder="Type a message..."
          value={text}
          onChange={(e) => setText(e.target.value)}
          onKeyDown={(e) => {
            if (e.key === 'Enter') handleSend()
          }}
        />
        <button className="btn" onClick={handleSend}>
          Send
        </button>
      </div>

      {showAddMember && (
        <AddMemberModal
          users={otherUsers}
          onAdd={onAddMember}
          onClose={() => setShowAddMember(false)}
        />
      )}
    </section>
  )
}

export default ChatPanel