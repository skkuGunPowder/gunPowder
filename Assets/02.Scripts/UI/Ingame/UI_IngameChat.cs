using System.Collections.Generic;
using BackndChat;
using UnityEngine;

public class UI_IngameChat : UI_Popup
{
    
    void Start()
    {
        
    }

    

    public void OnJoinChannel(ChannelInfo channelInfo)
    {
        throw new System.NotImplementedException();
    }

    public void OnLeaveChannel(ChannelInfo channelInfo)
    {
        throw new System.NotImplementedException();
    }

    public void OnJoinChannelPlayer(string channelGroup, string channelName, ulong channelNumber, PlayerInfo player)
    {
        throw new System.NotImplementedException();
    }

    public void OnLeaveChannelPlayer(string channelGroup, string channelName, ulong channelNumber, PlayerInfo player)
    {
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
