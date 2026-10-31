export type User = {
  id: number
  username: string
}

export type ServerMessage = {
  action: string
  success?: boolean
  message?: string
  userId?: number
  username?: string
  users?: User[]
  groups?: Group[]
  messages?: Message[]
}

export type Group = {
  id: number
  name: string
}

export type Chat = {
  id: number
  name: string
  isGroup: boolean
}

export type Message = {
  senderId: number
  senderName: string
  content: string
  createdAt: string
}