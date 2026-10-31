import { createConnection, Socket } from 'net'

const SERVER_HOST = '127.0.0.1'
const SERVER_PORT = 8080

export class ChatConnection {
  private socket: Socket | null = null
  private buffer = ''
  private onMessage: (message: object) => void

  constructor(onMessage: (message: object) => void) {
    this.onMessage = onMessage
  }

  connect(): void {
    this.socket = createConnection({ host: SERVER_HOST, port: SERVER_PORT })
    this.socket.setEncoding('utf8')

    this.socket.on('connect', () => console.log('[tcp] connected'))
    this.socket.on('data', (chunk: string) => this.onData(chunk))
    this.socket.on('close', () => console.log('[tcp] disconnected'))
    this.socket.on('error', (error) => console.log('[tcp] error:', error.message))
  }

  send(request: object): void {
    if (this.socket === null) {
      return
    }

    const json = JSON.stringify(request)
    console.log('[tcp] >>', json)
    this.socket.write(json + '\n')
  }

  private onData(chunk: string): void {
    this.buffer += chunk

    let index = this.buffer.indexOf('\n')

    while (index !== -1) {
      const line = this.buffer.slice(0, index)
      this.buffer = this.buffer.slice(index + 1)

      if (line.trim() !== '') {
        this.onLine(line)
      }

      index = this.buffer.indexOf('\n')
    }
  }

  private onLine(line: string): void {
    console.log('[tcp] <<', line)

    try {
      this.onMessage(JSON.parse(line))
    } catch {
      console.log('[tcp] invalid JSON')
    }
  }
}