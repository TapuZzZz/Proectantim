import { contextBridge, ipcRenderer } from 'electron'
import { electronAPI } from '@electron-toolkit/preload'

const api = {
  login: (username: string, password: string) => ipcRenderer.invoke('login', username, password),
  register: (username: string, password: string) => ipcRenderer.invoke('register', username, password),

  sendMessage: (targetId: number, isGroup: boolean, content: string) =>
    ipcRenderer.invoke('send-message', targetId, isGroup, content),

  onIncomingMessage: (callback: (message: unknown) => void) => {
    ipcRenderer.on('incoming-message', (_event, message) => callback(message))
  }
}

if (process.contextIsolated) {
  try {
    contextBridge.exposeInMainWorld('electron', electronAPI)
    contextBridge.exposeInMainWorld('api', api)
  } catch (error) {
    console.error(error)
  }
} else {
  // @ts-ignore (define in dts)
  window.electron = electronAPI
  // @ts-ignore (define in dts)
  window.api = api
}