import { Socket } from 'net'
import { ActionType, NetworkResponse } from './protocol'

const SERVER_HOST = '127.0.0.1'
const SERVER_PORT = 8080

type PendingRequest = {
  resolve: (response: NetworkResponse) => void
  reject: (error: Error) => void
}

let socket: Socket | null = null
let buffer = ''
const pendingRequests: PendingRequest[] = []

type NotificationListener = (message: unknown) => void
let notificationListener: NotificationListener | null = null

export function onServerNotification(listener: NotificationListener): void {
  notificationListener = listener
}

export function connectToServer(): Promise<void> {
  return new Promise((resolve, reject) => {
    const newSocket = new Socket()
    newSocket.setEncoding('utf8')
    newSocket.on('data', handleData)
    newSocket.on('close', handleClose)
    newSocket.on('error', (error) => console.log('[Socket error]', error.message))
    newSocket.once('error', reject)

    newSocket.connect(SERVER_PORT, SERVER_HOST, () => {
      socket = newSocket
      console.log('[+] Connected to server')
      resolve()
    })
  })
}

export function sendRequest(action: ActionType, payload: object): Promise<NetworkResponse> {
  return new Promise((resolve, reject) => {
    if (!socket) {
      reject(new Error('Not connected to server'))
      return
    }

    pendingRequests.push({ resolve, reject })
    socket.write(JSON.stringify({ Action: action, Payload: payload }) + '\n')
  })
}

// TCP יכול לחתוך הודעה לחלקים, אז אוספים עד '\n'
function handleData(chunk: string): void {
  buffer += chunk
  let newLineIndex = buffer.indexOf('\n')

  while (newLineIndex >= 0) {
    const line = buffer.slice(0, newLineIndex).trim()
    buffer = buffer.slice(newLineIndex + 1)

    if (line.length > 0) {
      handleLine(line)
    }

    newLineIndex = buffer.indexOf('\n')
  }
}

function handleLine(line: string): void {
  try {
    const message = JSON.parse(line)

    if ('Success' in message) {
      resolveNextRequest(message)
    } else {
      notificationListener?.(message)
    }
  } catch {
    console.log('[!] Invalid JSON from server:', line)
  }
}

// השרת עונה לפי הסדר, אז התשובה שייכת לבקשה הראשונה שמחכה
function resolveNextRequest(response: NetworkResponse): void {
  const pending = pendingRequests.shift()
  pending?.resolve(response)
}

function handleClose(): void {
  console.log('[-] Disconnected from server')
  socket = null
  buffer = ''

  while (pendingRequests.length > 0) {
    pendingRequests.shift()?.reject(new Error('Connection closed'))
  }
}