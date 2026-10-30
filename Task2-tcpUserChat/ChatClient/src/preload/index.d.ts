import { ElectronAPI } from '@electron-toolkit/preload'

type ServerResponse = {
  Success: boolean
  Message: string
  Data: unknown
}

type IncomingMessage = {
  FromUserId: number
  FromUsername: string
  Content: string
  IsGroup: boolean
  GroupId?: number
  SentAt: string
}

declare global {
  interface Window {
    electron: ElectronAPI
    api: {
      login: (username: string, password: string) => Promise<ServerResponse>
      register: (username: string, password: string) => Promise<ServerResponse>
      sendMessage: (targetId: number, isGroup: boolean, content: string) => Promise<ServerResponse>
      onIncomingMessage: (callback: (message: IncomingMessage) => void) => void
    }
  }
}