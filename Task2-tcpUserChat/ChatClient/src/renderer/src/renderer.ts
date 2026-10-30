// ---- מוצאים את האלמנטים במסך ----
const usernameInput = document.getElementById('username') as HTMLInputElement
const passwordInput = document.getElementById('password') as HTMLInputElement
const loginButton = document.getElementById('login-btn') as HTMLButtonElement
const registerButton = document.getElementById('register-btn') as HTMLButtonElement
const result = document.getElementById('result') as HTMLParagraphElement

const targetIdInput = document.getElementById('target-id') as HTMLInputElement
const contentInput = document.getElementById('content') as HTMLInputElement
const sendButton = document.getElementById('send-btn') as HTMLButtonElement
const messagesList = document.getElementById('messages') as HTMLUListElement

// ---- פונקציות עזר ----
function showResult(text: string): void {
  result.textContent = text
}

function addMessageToList(text: string): void {
  const item = document.createElement('li')
  item.textContent = text
  messagesList.appendChild(item)
}

// ---- התחברות והרשמה ----
async function onLoginClick(): Promise<void> {
  try {
    const response = await window.api.login(usernameInput.value, passwordInput.value)
    showResult(response.Message)
  } catch (error) {
    showResult(`Error: ${(error as Error).message}`)
  }
}

async function onRegisterClick(): Promise<void> {
  try {
    const response = await window.api.register(usernameInput.value, passwordInput.value)
    showResult(response.Message)
  } catch (error) {
    showResult(`Error: ${(error as Error).message}`)
  }
}

// ---- שליחת הודעה ----
async function onSendClick(): Promise<void> {
  try {
    const targetId = Number(targetIdInput.value)
    const response = await window.api.sendMessage(targetId, false, contentInput.value)

    if (response.Success) {
      addMessageToList(`Me -> ${targetId}: ${contentInput.value}`)
      contentInput.value = ''
    } else {
      showResult(response.Message)
    }
  } catch (error) {
    showResult(`Error: ${(error as Error).message}`)
  }
}

// ---- מחברים כפתורים לפונקציות ----
loginButton.addEventListener('click', onLoginClick)
registerButton.addEventListener('click', onRegisterClick)
sendButton.addEventListener('click', onSendClick)

// ---- הודעה חיה שמגיעה מהשרת ----
window.api.onIncomingMessage((message) => {
  addMessageToList(`${message.FromUsername}: ${message.Content}`)
})