using System.Collections.Generic;
using BackEnd;
using BackndChat;
using UnityEngine;

public class ChatManager : PhotonSingleton<ChatManager>, BackndChat.IChatClientListener
{
    // 뒤끝 Chat 사용
    private ChatClient _chatClient;
    private ulong _currentChannelNumber = 0;
    private bool _isInChannel = false;
    private bool _isChatOpen = false;
    void Start()
    {
        
    }

    void Update()
    {
        _chatClient?.Update();
        
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (!_isChatOpen)
            {
                _isChatOpen = true;
                PopupManager.Instance.Open(EPopupType.UI_IngameChatPopup);
            }
        }
        else if  (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_isChatOpen)
            {
                _isChatOpen = false;
                PopupManager.Instance.Close(EPopupType.UI_IngameChatPopup);    
            }
        }
    }
    
    void OnDestroy()
    {
        _chatClient?.Dispose();
    }
    
    // 채팅방 입장 (Photon Room 입장 시 호출)
    public void JoinChatChannel(string channelGroup, ulong channelNumber = 0, string password = "")
    {
        if(_isInChannel && _currentChannelNumber == channelNumber)
        {
            Debug.Log("이미 해당 채팅방에 있습니다.");
            return;
        }
        _chatClient.SendJoinPrivateChannel(channelGroup, channelNumber, password);
    }
    
    // 채팅방 퇴장 (Photon Room 퇴장 시 호출)
    public void LeaveChatChannel(string channelGroup, string channelName, ulong channelNumber)
    {
        if(!_isInChannel)
        {
            return;
        }
        
        _chatClient.SendLeaveChannel(channelGroup, channelName,  channelNumber);
    }
    
    // 메시지 전송
    public void SendMessage(string channelGroup, string channelName, ulong channelNumber, string message)
    {
        if(!_isInChannel)
        {
            Debug.LogWarning("채팅방에 입장하지 않았습니다.");
            return;
        }

        _chatClient.SendChatMessage(channelGroup, channelName, channelNumber, message);
    }

    public void OnJoinChannel(ChannelInfo channelInfo)
    {
        _currentChannelNumber = channelInfo.ChannelNumber;
        _isInChannel = true;
        Debug.Log($"채팅방 입장 성공: {channelInfo.ChannelNumber}");

    }

    public void OnLeaveChannel(ChannelInfo channelInfo)
    {
        Debug.Log($"채팅방 퇴장 성공: {_currentChannelNumber}");
        _currentChannelNumber = 0;
        _isInChannel = false;
    }

    public void OnJoinChannelPlayer(string channelGroup, string channelName, ulong channelNumber, PlayerInfo player)
    {
        // xx님이 입장하셨습니다
        throw new System.NotImplementedException();
    }

    public void OnLeaveChannelPlayer(string channelGroup, string channelName, ulong channelNumber, PlayerInfo player)
    {
        // xx님이 퇴장하셨습니다.
        throw new System.NotImplementedException();
    }

    public void OnUpdatePlayerInfo(string channelGroup, string channelName, ulong channelNumber, PlayerInfo player)
    {
        throw new System.NotImplementedException();
    }

    public void OnChangeGamerName(string oldGamerName, string newGamerName)
    {
        throw new System.NotImplementedException();
    }

    public void OnChatMessage(MessageInfo messageInfo)
    {
        throw new System.NotImplementedException();
    }

    public void OnWhisperMessage(WhisperMessageInfo messageInfo)
    {
        throw new System.NotImplementedException();
    }

    public void OnTranslateMessage(List<MessageInfo> messages)
    {
        throw new System.NotImplementedException();
    }

    public void OnHideMessage(MessageInfo messageInfo)
    {
        throw new System.NotImplementedException();
    }

    public void OnDeleteMessage(MessageInfo messageInfo)
    {
        throw new System.NotImplementedException();
    }

    public void OnSuccess(SUCCESS_MESSAGE success, object param)
    {
        throw new System.NotImplementedException();
    }

    public void OnError(ERROR_MESSAGE error, object param)
    {
        throw new System.NotImplementedException();
    }
}
