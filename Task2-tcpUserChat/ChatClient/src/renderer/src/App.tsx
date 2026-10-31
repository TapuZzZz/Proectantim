import { useEffect, useState } from 'react'
import TopBar from './components/TopBar'
import Sidebar from './components/Sidebar'
import ChatPanel from './components/ChatPanel'
import type { Chat, Group, Message, ServerMessage, User } from './types'

function sendToServer(request: object): void {
  window.electron.ipcRenderer.send('chat:send', request)
}

function App(): React.JSX.Element {
  const [user, setUser] = useState<User | null>(null)
  const [error, setError] = useState('')
  const [users, setUsers] = useState<User[]>([])
  const [groups, setGroups] = useState<Group[]>([])
  const [activeChat, setActiveChat] = useState<Chat | null>(null)
  const [messages, setMessages] = useState<Message[]>([])
  const [notice, setNotice] = useState('')

  useEffect(() => {
    function handleAuthResponse(message: ServerMessage): void {
      if (!message.success) {
        setError(message.message ?? 'Login failed')
        return
      }

      setError('')
      setUser({ id: message.userId ?? 0, username: message.username ?? '' })
      sendToServer({ action: 'getUsers' })
      sendToServer({ action: 'getMyGroups' })
    }

    function handleUsersResponse(message: ServerMessage): void {
      setUsers(message.users ?? [])
    }

    function handleGroupsResponse(message: ServerMessage): void {
      setGroups(message.groups ?? [])
    }

    function handleServerMessage(message: ServerMessage): void {
      if (message.action === 'login' || message.action === 'register') {
        handleAuthResponse(message)
      }

      if (message.action === 'getUsers') {
        handleUsersResponse(message)
      }

      if (message.action === 'getMyGroups') {
        handleGroupsResponse(message)
      }

      if (message.action === 'createGroup' && message.success) {
        sendToServer({ action: 'getMyGroups' })
      }

      if (message.action === 'getHistory' && message.success) {
        setMessages(message.messages ?? [])
      }

      if (message.action === 'addGroupMember') {
        setNotice(message.message ?? '')
      }
    }

    return window.electron.ipcRenderer.on('chat:message', (_event, message) =>
      handleServerMessage(message)
    )
  }, [])

  function handleLogin(username: string, password: string): void {
    sendToServer({ action: 'login', username: username, password: password })
  }

  function handleRegister(username: string, password: string): void {
    sendToServer({ action: 'register', username: username, password: password })
  }

  function handleCreateGroup(name: string): void {
    sendToServer({ action: 'createGroup', groupName: name })
  }

  function handleSelectChat(chat: Chat): void {
    setActiveChat(chat)
    setMessages([])
    setNotice('')
    sendToServer({ action: 'getHistory', targetId: chat.id, isGroup: chat.isGroup })
  }

  function handleSendMessage(content: string): void {
    if (activeChat === null || user === null) {
      return
    }

    sendToServer({
      action: 'sendMessage',
      targetId: activeChat.id,
      isGroup: activeChat.isGroup,
      content: content
    })

    const myMessage: Message = {
      senderId: user.id,
      senderName: user.username,
      content: content,
      createdAt: new Date().toISOString()
    }
    setMessages((previous) => [...previous, myMessage])
  }

  function handleAddMember(userId: number): void {
    if (activeChat === null || !activeChat.isGroup) {
      return
    }

    sendToServer({ action: 'addGroupMember', groupId: activeChat.id, targetId: userId })
  }

  return (
    <div className="app">
      <TopBar user={user} error={error} onLogin={handleLogin} onRegister={handleRegister} />
      <Sidebar
        users={users}
        groups={groups}
        onCreateGroup={handleCreateGroup}
        onSelectChat={handleSelectChat}
      />
      <ChatPanel
        chat={activeChat}
        messages={messages}
        myId={user?.id ?? 0}
        users={users}
        notice={notice}
        onSend={handleSendMessage}
        onAddMember={handleAddMember}
      />
    </div>
  )
}

export default App