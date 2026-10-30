import { app, shell, BrowserWindow, ipcMain } from 'electron'
import { join } from 'path'
import { electronApp, optimizer, is } from '@electron-toolkit/utils'
import icon from '../../resources/icon.png?asset'
import { connectToServer, sendRequest, onServerNotification } from './serverConnection'
import { ActionType } from './protocol' // חדש

function createWindow(): void {
  const mainWindow = new BrowserWindow({
    width: 900,
    height: 670,
    show: false,
    autoHideMenuBar: true,
    ...(process.platform === 'linux' ? { icon } : {}),
    webPreferences: {
      preload: join(__dirname, '../preload/index.js'),
      sandbox: false
    }
  })

  mainWindow.on('ready-to-show', () => {
    mainWindow.show()
  })

  mainWindow.webContents.setWindowOpenHandler((details) => {
    shell.openExternal(details.url)
    return { action: 'deny' }
  })

  if (is.dev && process.env['ELECTRON_RENDERER_URL']) {
    mainWindow.loadURL(process.env['ELECTRON_RENDERER_URL'])
  } else {
    mainWindow.loadFile(join(__dirname, '../renderer/index.html'))
  }
}

// חדש: הבקשות שהחלון יכול לשלוח ל-main
function registerIpcHandlers(): void {
  ipcMain.handle('login', (_event, username: string, password: string) =>
    sendRequest(ActionType.Login, { Username: username, Password: password })
  )

  ipcMain.handle('register', (_event, username: string, password: string) =>
    sendRequest(ActionType.Register, { Username: username, Password: password })
  )

  ipcMain.handle('send-message', (_event, targetId: number, isGroup: boolean, content: string) =>
    sendRequest(ActionType.SendMessage, { TargetId: targetId, IsGroup: isGroup, Content: content })
  )
}

function forwardServerNotifications(): void {
  onServerNotification((message) => {
    for (const window of BrowserWindow.getAllWindows()) {
      window.webContents.send('incoming-message', message)
    }
  })
}

// חדש: אם השרת כבוי, האפליקציה לא קורסת
async function connectSafely(): Promise<void> {
  try {
    await connectToServer()
  } catch {
    console.log('[!] Could not connect to server. Is it running?')
  }
}

app.whenReady().then(async () => {
  electronApp.setAppUserModelId('com.electron')

  app.on('browser-window-created', (_, window) => {
    optimizer.watchWindowShortcuts(window)
  })

  registerIpcHandlers() // חדש
  forwardServerNotifications()
  createWindow()

  await connectSafely() // חדש

  app.on('activate', function () {
    if (BrowserWindow.getAllWindows().length === 0) createWindow()
  })
})

app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') {
    app.quit()
  }
})