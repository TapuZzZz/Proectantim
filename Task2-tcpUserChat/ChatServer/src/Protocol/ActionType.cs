namespace ChatServer.Protocol;

public enum ActionType
{
    Register,
    Login,
    SendMessage,
    CreateGroup,
    AddGroupMember,
    GetChatHistory
}