export enum ActionType {
  Register = 0,
  Login = 1,
  SendMessage = 2,
  CreateGroup = 3,
  AddGroupMember = 4,
  GetChatHistory = 5
}

export type NetworkResponse<T = unknown> = {
  Success: boolean
  Message: string
  Data: T | null
}