using BackEnd;
using BackndChat;
using Photon.Pun;

using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class UIChatManager : DontDestroySingleton<UIChatManager>, BackndChat.IChatClientListener
{
    // 인게임 채팅 채널 설정
    private const string INGAME_CHANNEL_GROUP = "InGame";

    // 채팅 메시지 수신 이벤트 (UI 클래스들이 구독)
    public event Action<MessageInfo> OnChatMessageReceived;

    public GameObject ChatContent = null;
    public InputField ChatInput = null;
    public Button SendButton = null;

    public string CurrentChannelGroup => _currentChannelGroup;
    private string _currentChannelGroup = string.Empty;
    
    public string CurrentChannelName => _currentChannelName;
    private string _currentChannelName = string.Empty;
    
    public UInt64 CurrentChannelNumber => _currentChannelNumber;
    private UInt64 _currentChannelNumber = 0;
    
    public ChatClient ChatClient => _chatClient;
    private ChatClient _chatClient = null;

    public Dictionary<string, Dictionary<string, Dictionary<UInt64, ChannelInfo>>> ChannelList => _channelList;
    private Dictionary<string, Dictionary<string, Dictionary<UInt64, ChannelInfo>>> _channelList =
        new Dictionary<string, Dictionary<string, Dictionary<UInt64, ChannelInfo>>>();

    private List<string> _selectMessageKey = new List<string>();
    private bool _isChatClientInitialized = false;

    /// <summary>
    /// ChatClient 초기화 (로그인 후 PhotonServerManager.Connect()에서 호출)
    /// </summary>
    public void InitializeChatClient()
    {
        if (_isChatClientInitialized)
        {
            Debug.LogWarning("[UIChatManager] ChatClient가 이미 초기화되었습니다.");
            return;
        }

        if (AccountManager.Instance == null ||
            AccountManager.Instance.CurrentAccount == null ||
            string.IsNullOrEmpty(AccountManager.Instance.CurrentAccount.Nickname))
        {
            Debug.LogError("[UIChatManager] AccountManager의 Nickname이 설정되지 않았습니다. ChatClient 초기화를 중단합니다.");
            return;
        }

        // 예시 이미지들, Resources 폴더의 경로를 string으로 지정, 추후 이미지 생기면 수정
        // 아마 Firebase나 뒤끝에 저장된 커스텀 사진(미리 저장된 이미지 중에서 선택한 사진)을 사용하게 될듯?
        List<string> avatars = new List<string>
        {
            "Boy_1",
            "Boy_2",
            "Boy_3",
            "Boy_4",
            "Girl_1",
            "Girl_2",
            "Girl_3",
            "Girl_4"
        };

        string avatar = avatars[UnityEngine.Random.Range(0, avatars.Count)];
        string nickname = AccountManager.Instance.CurrentAccount.Nickname;

        Debug.Log($"[UIChatManager] ChatClient 초기화 시작 - Nickname: {nickname}, Avatar: {avatar}");

        _chatClient = new ChatClient(this, new ChatClientArguments
        {
            
            Avatar = avatar,
        });

        _isChatClientInitialized = true;
        Debug.Log("[UIChatManager] ChatClient 초기화 완료");
    }

    // Update is called once per frame
    private void Update()
    {
        _chatClient?.Update();
    }

    public void SendChatMessage(string text)
    {
        if (_chatClient == null) return;
        if (_currentChannelName == string.Empty) return;
        if (!_channelList.ContainsKey(_currentChannelGroup)) return;
        if (!_channelList[_currentChannelGroup].ContainsKey(_currentChannelName)) return;
        if (!_channelList[_currentChannelGroup][_currentChannelName].ContainsKey(_currentChannelNumber)) return;

        ChannelInfo channelInfo = _channelList[_currentChannelGroup][_currentChannelName][_currentChannelNumber];
        if (channelInfo == null) return;

        // 귓속말
        if (text.IndexOf("/w") == 0)
        {
            string[] whisper = text.Split(' ');

            if (whisper.Length < 3) return;

            string message = string.Empty;
            for (int i = 2; i < whisper.Length; ++i)
            {
                if (i == whisper.Length - 1)
                {
                    message += whisper[i];
                    break;
                }

                message += whisper[i] + " ";
            }

            _chatClient.SendWhisperMessage(whisper[1], message);
        }
        // 번역기능?
        else if (text.IndexOf("/translate") == 0)
        {
            if (_selectMessageKey.Count == 0) return;

            List<string> langaues = new List<string>();

            string[] translate = text.Split(' ');
            for (int i = 1; i < translate.Length; ++i)
            {
                langaues.Add(translate[i]);
            }

            List<MessageInfo> messages = new List<MessageInfo>();
            foreach (var key in _selectMessageKey)
            {
                string[] keys = key.Split(',');
                if (keys.Length < 2) continue;

                string tag = keys[0];
                UInt64 index = Convert.ToUInt64(keys[1]);

                foreach (var message in channelInfo.Messages)
                {
                    if (message.Index == index && message.Tag == tag)
                    {
                        messages.Add(message);
                        break;
                    }
                }
            }

            for (int i = 0; i < messages.Count; ++i)
            {
                _chatClient.SendTranslateChatMessage(messages[i], langaues);
            }
        }
        
        // 차단 기능? 혹은 신고?
        else if (text.IndexOf("/block") == 0)
        {
            string[] strings = text.Split(' ');

            if (strings.Length < 2)
            {
                List<string> blocks = _chatClient?.GetBlockGamers();

                if (blocks.Count == 0)
                {
                    MessageInfo messageInfo = new MessageInfo
                    {
                        Index = 0,
                        GamerName = "SYSTEM",
                        Avatar = "Girl_5",
                        Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        Tag = "",
                        ChannelGroup = _currentChannelGroup,
                        ChannelName = _currentChannelName,
                        ChannelNumber = _currentChannelNumber,
                        Message = "Empty Block Gamer"
                    };

                    channelInfo.Messages.Add(messageInfo);

                    OnChatMessage(messageInfo);
                }
                else
                {
                    for (int i = 0; i < blocks.Count; ++i)
                    {
                        MessageInfo messageInfo = new MessageInfo
                        {
                            Index = 0,
                            GamerName = "SYSTEM",
                            Avatar = "Girl_5",
                            Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            Tag = "",
                            ChannelGroup = _currentChannelGroup,
                            ChannelName = _currentChannelName,
                            ChannelNumber = _currentChannelNumber,
                            Message = i + ") Block Gamer : " + blocks[i]
                        };

                        channelInfo.Messages.Add(messageInfo);

                        OnChatMessage(messageInfo);
                    }
                }
            }
            else
            {
                if (strings[1] == "add")
                {
                    if (strings.Length < 3) return;

                    _chatClient.SendAddBlockGamer(strings[2]);

                }
                else if (strings[1] == "remove")
                {
                    if (strings.Length < 3) return;

                    _chatClient.SendRemoveBlockGamer(strings[2]);
                }
                else
                {
                    MessageInfo messageInfo = new MessageInfo
                    {
                        Index = 0,
                        GamerName = "SYSTEM",
                        Avatar = "Girl_5",
                        Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        Tag = "",
                        ChannelGroup = _currentChannelGroup,
                        ChannelName = _currentChannelName,
                        ChannelNumber = _currentChannelNumber,
                        Message = "Invalid Command"
                    };

                    channelInfo.Messages.Add(messageInfo);

                    OnChatMessage(messageInfo);
                }
            }
        }
        
        // 내 정보?
        else if (text.IndexOf("/info") == 0)
        {
            string[] strings = text.Split(' ');

            if (strings.Length < 2) return;

            if (channelInfo.Players.ContainsKey(strings[1]))
            {
                PlayerInfo player = channelInfo.Players[strings[1]];

                MessageInfo messageInfo = new MessageInfo
                {
                    Index = 0,
                    GamerName = "SYSTEM",
                    Avatar = "Girl_5",
                    Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Tag = "",
                    ChannelGroup = _currentChannelGroup,
                    ChannelName = _currentChannelName,
                    ChannelNumber = _currentChannelNumber,
                    Message = player.GamerName + " / " + player.Avatar + " / " + player.Language + " / "
                };

                foreach (var item in player.Metadata)
                {
                    messageInfo.Message += item.Key + " : " + item.Value + " / ";
                }

                channelInfo.Messages.Add(messageInfo);

                OnChatMessage(messageInfo);
            }
        }
        // 닉네임 변경은 챗에서 사용할 수 없음
        // else if (text.IndexOf("/nickname") == 0)
        // {
        //     string[] strings = text.Split(' ');
        //
        //     if (strings.Length < 2) return;
        //
        //     var returnObject = Backend.BMember.UpdateNickname(strings[1]);
        //     if (!returnObject.IsSuccess())
        //     {
        //         Debug.LogError("닉네임 변경 실패 : " + returnObject);
        //         return;
        //     }
        //
        //     _chatClient.UpdateNickname(strings[1]);
        // }
        // 메타데이터 수정도 안댐
        // else if (text.IndexOf("/meta") == 0)
        // {
        //     string[] strings = text.Split(' ');
        //
        //     if (strings.Length < 4) return;
        //
        //     if (!channelInfo.Players.ContainsKey(AccountManager.Instance.CurrentAccount.Nickname)) return;
        //
        //     PlayerInfo player = channelInfo.Players[AccountManager.Instance.CurrentAccount.Nickname];
        //
        //     if (strings[1] == "add")
        //     {
        //         if (player.Metadata.ContainsKey(strings[2])) return;
        //
        //         player.Metadata.Add(strings[2], strings[3]);
        //     }
        //     else if (strings[1] == "remove")
        //     {
        //         if (!player.Metadata.ContainsKey(strings[2])) return;
        //
        //         player.Metadata.Remove(strings[2]);
        //     }
        //     else if (strings[1] == "update")
        //     {
        //         if (!player.Metadata.ContainsKey(strings[2])) return;
        //
        //         player.Metadata[strings[2]] = strings[3];
        //     }
        //
        //     _chatClient.UpdateMetadata(player.Metadata);
        // }
        // 언어변경은 아직 미지원
        // else if (text.IndexOf("/language") == 0)
        // {
        //     string[] strings = text.Split(' ');
        //
        //     if (strings.Length < 2) return;
        //
        //     _chatClient.UpdateLanguage(strings[1]);
        // }
        // 아바타 수정도 아직
        // else if (text.IndexOf("/avatar") == 0)
        // {
        //     string[] strings = text.Split(' ');
        //
        //     if (strings.Length < 2) return;
        //
        //     _chatClient.UpdateAvatar(strings[1]);
        // }
   
        _chatClient.SendChatMessage(channelInfo.ChannelGroup, channelInfo.ChannelName, channelInfo.ChannelNumber, text);
    }

    private void OnChannelSelected(string channelGroup, string channelName, UInt64 channelNumber)
    {
        if (!_channelList.ContainsKey(channelGroup)) return;
        if (!_channelList[channelGroup].ContainsKey(channelName)) return;
        if (!_channelList[channelGroup][channelName].ContainsKey(channelNumber)) return;

        ChannelInfo channelInfo = _channelList[channelGroup][channelName][channelNumber];
        if (channelInfo == null) return;

        _selectMessageKey.Clear();

        if (ChatContent != null)
        {
            foreach (Transform child in ChatContent.transform)
            {
                Destroy(child.gameObject);
            }
        }
        //
        //
        //
        // if (UserContent != null)
        // {
        //     foreach (var player in channelInfo.Players)
        //     {
        //         GameObject userList = Instantiate(Resources.Load<GameObject>("Prefabs/UserList"), UserContent.transform);
        //         userList.name = player.Value.GamerName;
        //
        //         if (player.Value.GamerName == AccountManager.Instance.CurrentAccount.Nickname)
        //         {
        //             userList.GetComponent<UIUserList>().SetData(player.Value.Avatar, player.Value.GamerName, true);
        //         }
        //         else
        //         {
        //             userList.GetComponent<UIUserList>().SetData(player.Value.Avatar, player.Value.GamerName);
        //         }
        //     }
        // }

        if (ChatContent != null)
        {
            foreach (var message in channelInfo.Messages)
            {
                GameObject chatList = Instantiate(Resources.Load<GameObject>("Prefabs/ChatList"), ChatContent.transform);

                if (message.GamerName == AccountManager.Instance.CurrentAccount.Nickname)
                {
                    chatList.GetComponent<UIChatList>().SetData(message.Index, message.Avatar, message.GamerName, message.Message, message.Time, message.Tag, OnReportButton, OnTranslateCheckButton, true);
                }
                else
                {
                    chatList.GetComponent<UIChatList>().SetData(message.Index, message.Avatar, message.GamerName, message.Message, message.Time, message.Tag, OnReportButton, OnTranslateCheckButton);
                }
            }
        }

        _currentChannelGroup = channelGroup;
        _currentChannelName = channelName;
        _currentChannelNumber = channelNumber;
    }

    // Report는 미구현
    // private void SendReportChat(UInt64 index, string tag, string keyword, string reason)
    // {
    //     if (_chatClient == null) return;
    //
    //     _chatClient.SendReportChatMessage(index, tag, keyword, reason);
    // }

    private void OnReportButton(UInt64 index, string tag)
    {
        Debug.Log("[UIChatManager] OnReportButton은 미구현");
    }

    private void OnTranslateCheckButton(bool isOn, string messageKey)
    {
        if (isOn)
        {
            for (int i = 0; i < _selectMessageKey.Count; ++i)
            {
                if (_selectMessageKey[i] == messageKey) return;
            }

            _selectMessageKey.Add(messageKey);
        }
        else
        {
            for (int i = 0; i < _selectMessageKey.Count; ++i)
            {
                if (_selectMessageKey[i] == messageKey)
                {
                    _selectMessageKey.RemoveAt(i);
                    break;
                }
            }
        }
    }

    private void SendCreatePrivateChannel(string channelGroup, UInt64 channelNumber, string channelName, uint maxCount, string password)
    {
        if (_chatClient == null) return;

        _chatClient.SendCreatePrivateChannel(channelGroup, channelNumber, channelName, maxCount, password);
    }

    private void SendJoinOpenChannel(string channelGroup, string channelName)
    {
        if (_chatClient == null) return;

        _chatClient.SendJoinOpenChannel(channelGroup, channelName);
    }

    private void SendJoinPrivateChannel(string channelGroup, UInt64 channelNumber, string password)
    {
        if (_chatClient == null) return;

        _chatClient.SendJoinPrivateChannel(channelGroup, channelNumber, password);
    }

    public void OnClickJoinChannel()
    {
        // if (JoinChannelPopup)
        // {
        //     JoinChannelPopup.GetComponent<UIJoinChannelManager>().SetData(SendCreatePrivateChannel, SendJoinOpenChannel, SendJoinPrivateChannel);
        // }
    }

    public void OnJoinChannel(ChannelInfo channelInfo)
    {
        if (_channelList.ContainsKey(channelInfo.ChannelGroup))
        {
            if (_channelList[channelInfo.ChannelGroup].ContainsKey(channelInfo.ChannelName))
            {
                if (_channelList[channelInfo.ChannelGroup][channelInfo.ChannelName].ContainsKey(channelInfo.ChannelNumber)) return;
            }
        }

        if (!_channelList.ContainsKey(channelInfo.ChannelGroup))
        {
            _channelList.Add(channelInfo.ChannelGroup, new Dictionary<string, Dictionary<UInt64, ChannelInfo>>());
            _channelList[channelInfo.ChannelGroup].Add(channelInfo.ChannelName, new Dictionary<UInt64, ChannelInfo>());
        }
        else
        {
            if (!_channelList[channelInfo.ChannelGroup].ContainsKey(channelInfo.ChannelName))
            {
                _channelList[channelInfo.ChannelGroup].Add(channelInfo.ChannelName, new Dictionary<UInt64, ChannelInfo>());
            }
        }

        _channelList[channelInfo.ChannelGroup][channelInfo.ChannelName].Add(channelInfo.ChannelNumber, channelInfo);

        // GameObject channelList = Instantiate(Resources.Load<GameObject>("Prefabs/ChannelList"), ChannelContent.transform);
        // channelList.name = channelInfo.ChannelGroup + "_" + channelInfo.ChannelName + "_" + channelInfo.ChannelNumber.ToString();
        // channelList.GetComponent<UIChannelList>().AddChannel(channelInfo.ChannelGroup, channelInfo.ChannelName, channelInfo.ChannelNumber, OnChannelSelected);

        if (_channelList.Count == 1)
        {
            OnChannelSelected(channelInfo.ChannelGroup, channelInfo.ChannelName, channelInfo.ChannelNumber);
        }
    }

    public void OnLeaveChannel(ChannelInfo channelInfo)
    {
        if (!_channelList.ContainsKey(channelInfo.ChannelGroup)) return;

        if (!_channelList[channelInfo.ChannelGroup].ContainsKey(channelInfo.ChannelName)) return;

        if (!_channelList[channelInfo.ChannelGroup][channelInfo.ChannelName].ContainsKey(channelInfo.ChannelNumber)) return;

        _channelList[channelInfo.ChannelGroup][channelInfo.ChannelName].Remove(channelInfo.ChannelNumber);

        if (_channelList[channelInfo.ChannelGroup][channelInfo.ChannelName].Count == 0)
        {
            _channelList[channelInfo.ChannelGroup].Remove(channelInfo.ChannelName);
        }

        if (_channelList[channelInfo.ChannelGroup].Count == 0)
        {
            _channelList.Remove(channelInfo.ChannelGroup);
        }
        //
        // if (ChannelContent != null)
        // {
        //     foreach (Transform child in ChannelContent.transform)
        //     {
        //         if (child.name == channelInfo.ChannelGroup + "_" + channelInfo.ChannelName + "_" + channelInfo.ChannelNumber.ToString())
        //         {
        //             Destroy(child.gameObject);
        //             break;
        //         }
        //     }
        // }

        if (_currentChannelGroup == channelInfo.ChannelGroup && _currentChannelName == channelInfo.ChannelName && _currentChannelNumber == channelInfo.ChannelNumber)
        {
            if (_channelList.Count > 0)
            {
                foreach (var channel in _channelList)
                {
                    foreach (var channelName in channel.Value)
                    {
                        foreach (var channelNumber in channelName.Value)
                        {
                            OnChannelSelected(channel.Key, channelName.Key, channelNumber.Key);
                            return;
                        }
                    }
                }
            }
            else
            {

                if (ChatContent != null)
                {
                    foreach (Transform child in ChatContent.transform)
                    {
                        Destroy(child.gameObject);
                    }
                }
                //
                // if (UserContent != null)
                // {
                //     foreach (Transform child in UserContent.transform)
                //     {
                //         Destroy(child.gameObject);
                //     }
                // }
                //
                // if (ChannelUserCount != null)
                // {
                //     ChannelUserCount.text = "0 / 0";
                // }
            }
        }
    }

    public void OnJoinChannelPlayer(string channelGroup, string channelName, UInt64 channelNumber, PlayerInfo player)
    {
        if (!_channelList.ContainsKey(channelGroup)) return;

        if (!_channelList[channelGroup].ContainsKey(channelName)) return;

        if (!_channelList[channelGroup][channelName].ContainsKey(channelNumber)) return;

        ChannelInfo channelInfo = _channelList[channelGroup][channelName][channelNumber];
        if (channelInfo == null) return;

        if (channelInfo.Players.ContainsKey(player.GamerName)) return;

        channelInfo.Players.Add(player.GamerName, player);

        if (_currentChannelGroup == channelGroup && _currentChannelName == channelName && _currentChannelNumber == channelNumber)
        {
            // if (ChannelUserCount != null)
            // {
            //     ChannelUserCount.text = string.Format("{0} / {1}", channelInfo.Players.Count, channelInfo.MaxCount);
            // }
            //
            // if (UserContent != null)
            // {
            //     GameObject userList = Instantiate(Resources.Load<GameObject>("Prefabs/UserList"), UserContent.transform);
            //     userList.name = player.GamerName;
            //
            //     if (player.GamerName == AccountManager.Instance.CurrentAccount.Nickname)
            //     {
            //         userList.GetComponent<UIUserList>().SetData(player.Avatar, player.GamerName, true);
            //     }
            //     else
            //     {
            //         userList.GetComponent<UIUserList>().SetData(player.Avatar, player.GamerName);
            //     }
            // }
        }
    }

    public void OnLeaveChannelPlayer(string channelGroup, string channelName, UInt64 channelNumber, PlayerInfo player)
    {
        if (!_channelList.ContainsKey(channelGroup)) return;

        if (!_channelList[channelGroup].ContainsKey(channelName)) return;

        if (!_channelList[channelGroup][channelName].ContainsKey(channelNumber)) return;

        ChannelInfo channelInfo = _channelList[channelGroup][channelName][channelNumber];
        if (channelInfo == null) return;

        if (!channelInfo.Players.ContainsKey(player.GamerName)) return;

        channelInfo.Players.Remove(player.GamerName);

        if (_currentChannelGroup == channelGroup && _currentChannelName == channelName && _currentChannelNumber == channelNumber)
        {
            // if (ChannelUserCount != null)
            // {
            //     ChannelUserCount.text = string.Format("{0} / {1}", channelInfo.Players.Count, channelInfo.MaxCount);
            // }
            //
            // if (UserContent != null)
            // {
            //     foreach (Transform child in UserContent.transform)
            //     {
            //         if (child.name == player.GamerName)
            //         {
            //             Destroy(child.gameObject);
            //             break;
            //         }
            //     }
            // }
        }
    }

    public void OnUpdatePlayerInfo(string channelGroup, string channelName, UInt64 channelNumber, PlayerInfo player)
    {
        if (!_channelList.ContainsKey(channelGroup)) return;

        if (!_channelList[channelGroup].ContainsKey(channelName)) return;

        if (!_channelList[channelGroup][channelName].ContainsKey(channelNumber)) return;

        ChannelInfo channelInfo = _channelList[channelGroup][channelName][channelNumber];
        if (channelInfo == null) return;

        if (!channelInfo.Players.ContainsKey(player.GamerName)) return;

        channelInfo.Players[player.GamerName] = player;

        if (_currentChannelGroup == channelGroup && _currentChannelName == channelName && _currentChannelNumber == channelNumber)
        {
            // if (UserContent != null)
            // {
            //     foreach (Transform child in UserContent.transform)
            //     {
            //         UIUserList userList = child.GetComponent<UIUserList>();
            //         if (userList != null)
            //         {
            //             string name = userList.Name.text;
            //             name = name.Replace(" (You)", string.Empty);
            //
            //             if (name == player.GamerName)
            //             {
            //                 if (name == AccountManager.Instance.CurrentAccount.Nickname)
            //                 {
            //                     userList.SetData(player.Avatar, player.GamerName, true);
            //                 }
            //                 else
            //                 {
            //                     userList.SetData(player.Avatar, player.GamerName);
            //                 }
            //                 break;
            //             }
            //         }
            //     }
            // }

            MessageInfo messageInfo = new MessageInfo
            {
                Index = 0,
                GamerName = "SYSTEM",
                Avatar = "Girl_5",
                Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                Tag = "",
                ChannelGroup = _currentChannelGroup,
                ChannelName = _currentChannelName,
                ChannelNumber = _currentChannelNumber,
                Message = player.GamerName + "님의 정보가 갱신되었습니다."
            };

            channelInfo.Messages.Add(messageInfo);

            OnChatMessage(messageInfo);
        }
    }

    public void OnChangeGamerName(string oldGamerName, string newGamerName)
    {
        // 모든 채널을 확인하여 변경된 닉네임을 갱신한다.
        foreach(var channelGroup in _channelList)
        {
            foreach (var channelName in channelGroup.Value)
            {
                foreach (var channelNumber in channelName.Value)
                {
                    ChannelInfo channelInfo = channelNumber.Value;
                    if (channelInfo == null) continue;

                    if (!channelInfo.Players.ContainsKey(oldGamerName)) continue;

                    PlayerInfo player = channelInfo.Players[oldGamerName];

                    channelInfo.Players.Remove(oldGamerName);

                    player.GamerName = newGamerName;

                    channelInfo.Players.Add(newGamerName, player);

                    if (_currentChannelGroup == channelGroup.Key && _currentChannelName == channelName.Key && _currentChannelNumber == channelNumber.Key)
                    {
                        // if (UserContent != null)
                        // {
                        //     foreach (Transform child in UserContent.transform)
                        //     {
                        //         UIUserList userList = child.GetComponent<UIUserList>();
                        //         if (userList != null)
                        //         {
                        //             string name = userList.Name.text;
                        //             name = name.Replace(" (You)", string.Empty);
                        //
                        //             if (name == oldGamerName)
                        //             {
                        //                 if (name == AccountManager.Instance.CurrentAccount.Nickname)
                        //                 {
                        //                     userList.SetData(player.Avatar, player.GamerName, true);
                        //                 }
                        //                 else
                        //                 {
                        //                     userList.SetData(player.Avatar, player.GamerName);
                        //                 }
                        //
                        //                 child.name = player.GamerName;
                        //
                        //                 break;
                        //             }
                        //         }
                        //     }
                        // }

                        MessageInfo messageInfo = new MessageInfo
                        {
                            Index = 0,
                            GamerName = "SYSTEM",
                            Avatar = "Girl_5",
                            Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            Tag = "",
                            ChannelGroup = _currentChannelGroup,
                            ChannelName = _currentChannelName,
                            ChannelNumber = _currentChannelNumber,
                            Message = oldGamerName + "님의 닉네임이 " + newGamerName + "으로 변경되었습니다."
                        };

                        channelInfo.Messages.Add(messageInfo);

                        OnChatMessage(messageInfo);
                    }
                }
            }
        }
    }

    public void OnChatMessage(MessageInfo messageInfo)
    {
        if (!_channelList.ContainsKey(messageInfo.ChannelGroup)) return;

        if (!_channelList[messageInfo.ChannelGroup].ContainsKey(messageInfo.ChannelName)) return;

        if (!_channelList[messageInfo.ChannelGroup][messageInfo.ChannelName].ContainsKey(messageInfo.ChannelNumber)) return;

        ChannelInfo channelInfo = _channelList[messageInfo.ChannelGroup][messageInfo.ChannelName][messageInfo.ChannelNumber];
        if (channelInfo == null) return;

        channelInfo.Messages.Add(messageInfo);

        // 현재 채널의 메시지만 처리
        if (_currentChannelGroup == messageInfo.ChannelGroup && _currentChannelName == messageInfo.ChannelName && _currentChannelNumber == messageInfo.ChannelNumber)
        {
            // UI 클래스들에게 메시지 수신 이벤트 발생
            OnChatMessageReceived?.Invoke(messageInfo);

            // ChatContent가 설정되어 있으면 기본 UI 표시 (로비 등에서 사용)
            if (ChatContent != null)
            {
                GameObject chatList = Instantiate(Resources.Load<GameObject>("Prefabs/ChatList"), ChatContent.transform);

                if (messageInfo.GamerName == AccountManager.Instance.CurrentAccount.Nickname)
                {
                    chatList.GetComponent<UIChatList>().SetData(messageInfo.Index, messageInfo.Avatar, messageInfo.GamerName, messageInfo.Message, messageInfo.Time, messageInfo.Tag, OnReportButton, OnTranslateCheckButton, true);
                }
                else
                {
                    chatList.GetComponent<UIChatList>().SetData(messageInfo.Index, messageInfo.Avatar, messageInfo.GamerName, messageInfo.Message, messageInfo.Time, messageInfo.Tag, OnReportButton, OnTranslateCheckButton);
                }
            }
        }
    }

    public void OnWhisperMessage(WhisperMessageInfo messageInfo)
    {
        if (!_channelList.ContainsKey(_currentChannelGroup)) return;

        if (!_channelList[_currentChannelGroup].ContainsKey(_currentChannelName)) return;

        if (!_channelList[_currentChannelGroup][_currentChannelName].ContainsKey(_currentChannelNumber)) return;

        ChannelInfo channelInfo = _channelList[_currentChannelGroup][_currentChannelName][_currentChannelNumber];
        if (channelInfo == null) return;

        MessageInfo add_messageInfo = new MessageInfo()
        {
            ChannelGroup = channelInfo.ChannelGroup,
            ChannelName = channelInfo.ChannelName,
            ChannelNumber = channelInfo.ChannelNumber,
            Index = messageInfo.Index,
            GamerName = messageInfo.FromGamerName,
            Avatar = messageInfo.FromAvatar,
            Message = "[귓속말] " + messageInfo.Message,
            Time = messageInfo.Time,
            Tag = messageInfo.Tag
        };

        channelInfo.Messages.Add(add_messageInfo);

        // UI 클래스들에게 귓속말 수신 이벤트 발생
        OnChatMessageReceived?.Invoke(add_messageInfo);

        // ChatContent가 설정되어 있으면 기본 UI 표시
        if (ChatContent != null)
        {
            GameObject chatList = Instantiate(Resources.Load<GameObject>("Prefabs/ChatList"), ChatContent.transform);

            if (messageInfo.FromGamerName == AccountManager.Instance.CurrentAccount.Nickname)
            {
                chatList.GetComponent<UIChatList>().SetData(add_messageInfo.Index, add_messageInfo.Avatar, add_messageInfo.GamerName, add_messageInfo.Message, add_messageInfo.Time, add_messageInfo.Tag, OnReportButton, OnTranslateCheckButton, true);
            }
            else
            {
                chatList.GetComponent<UIChatList>().SetData(add_messageInfo.Index, add_messageInfo.Avatar, add_messageInfo.GamerName, add_messageInfo.Message, add_messageInfo.Time, add_messageInfo.Tag, OnReportButton, OnTranslateCheckButton);
            }
        }
    }

    public void OnTranslateMessage(List<MessageInfo> messages)
    {
        if (!_channelList.ContainsKey(_currentChannelGroup)) return;

        if (!_channelList[_currentChannelGroup].ContainsKey(_currentChannelName)) return;

        if (!_channelList[_currentChannelGroup][_currentChannelName].ContainsKey(_currentChannelNumber)) return;

        ChannelInfo channelInfo = _channelList[_currentChannelGroup][_currentChannelName][_currentChannelNumber];
        if (channelInfo == null) return;

        if (ChatContent != null)
        {
            foreach (var message in messages)
            {
                message.GamerName = message.GamerName + " (번역)";

                GameObject chatList = Instantiate(Resources.Load<GameObject>("Prefabs/ChatList"), ChatContent.transform);

                if (message.GamerName == AccountManager.Instance.CurrentAccount.Nickname)
                {
                    chatList.GetComponent<UIChatList>().SetData(message.Index, message.Avatar, message.GamerName, message.Message, message.Time, message.Tag, null, null, true);
                }
                else
                {
                    chatList.GetComponent<UIChatList>().SetData(message.Index, message.Avatar, message.GamerName, message.Message, message.Time, message.Tag, null, null);
                }
            }
        }
    }

    public void OnHideMessage(MessageInfo messageInfo)
    {
        if (!_channelList.ContainsKey(messageInfo.ChannelGroup)) return;

        if (!_channelList[messageInfo.ChannelGroup].ContainsKey(messageInfo.ChannelName)) return;

        if (!_channelList[messageInfo.ChannelGroup][messageInfo.ChannelName].ContainsKey(messageInfo.ChannelNumber)) return;

        ChannelInfo channelInfo = _channelList[messageInfo.ChannelGroup][messageInfo.ChannelName][messageInfo.ChannelNumber];
        if (channelInfo == null) return;

        if (_currentChannelGroup == messageInfo.ChannelGroup && _currentChannelName == messageInfo.ChannelName && _currentChannelNumber == messageInfo.ChannelNumber)
        {
            if (ChatContent != null)
            {
                foreach (Transform child in ChatContent.transform)
                {
                    UIChatList chatList = child.GetComponent<UIChatList>();
                    if (chatList != null)
                    {
                        if (chatList.IsEqual(messageInfo.Index, messageInfo.Tag))
                        {
                            chatList.SetMessage(messageInfo.Message);
                            break;
                        }
                    }
                }
            }
        }

        foreach (var message in channelInfo.Messages)
        {
            if (message.Index == messageInfo.Index && message.Tag == messageInfo.Tag)
            {
                message.Message = messageInfo.Message;
                break;
            }
        }
    }

    public void OnDeleteMessage(MessageInfo messageInfo)
    {
        if (!_channelList.ContainsKey(messageInfo.ChannelGroup)) return;

        if (!_channelList[messageInfo.ChannelGroup].ContainsKey(messageInfo.ChannelName)) return;

        if (!_channelList[messageInfo.ChannelGroup][messageInfo.ChannelName].ContainsKey(messageInfo.ChannelNumber)) return;

        ChannelInfo channelInfo = _channelList[messageInfo.ChannelGroup][messageInfo.ChannelName][messageInfo.ChannelNumber];
        if (channelInfo == null) return;

        if (_currentChannelGroup == messageInfo.ChannelGroup && _currentChannelName == messageInfo.ChannelName && _currentChannelNumber == messageInfo.ChannelNumber)
        {
            if (ChatContent != null)
            {
                foreach (Transform child in ChatContent.transform)
                {
                    UIChatList chatList = child.GetComponent<UIChatList>();
                    if (chatList != null)
                    {
                        if (chatList.IsEqual(messageInfo.Index, messageInfo.Tag))
                        {
                            Destroy(child.gameObject);
                            break;
                        }
                    }
                }
            }
        }

        foreach (var message in channelInfo.Messages)
        {
            if (message.Index == messageInfo.Index && message.Tag == messageInfo.Tag)
            {
                channelInfo.Messages.Remove(message);
                break;
            }
        }
    }

    public void OnSuccess(SUCCESS_MESSAGE success, object param)
    {
        switch (success)
        {
            case SUCCESS_MESSAGE.REPORT:
            case SUCCESS_MESSAGE.ADD_BLOCK_PLAYER:
            case SUCCESS_MESSAGE.REMOVE_BLOCK_PLAYER:
            {
                MessageInfo messageInfo = new MessageInfo
                {
                    Index = 0,
                    GamerName = "SYSTEM",
                    Avatar = "Girl_5",
                    Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Tag = ""
                };

                if (success == SUCCESS_MESSAGE.REPORT)
                {
                    messageInfo.Message = "신고 처리가 완료되었습니다.";
                }
                else if (success == SUCCESS_MESSAGE.ADD_BLOCK_PLAYER)
                {
                    messageInfo.Message = "차단 처리가 완료되었습니다.";
                }
                else if (success == SUCCESS_MESSAGE.REMOVE_BLOCK_PLAYER)
                {
                    messageInfo.Message = "차단 해제 처리가 완료되었습니다.";
                }

                if (_channelList.ContainsKey(_currentChannelGroup))
                {
                    if (_channelList[_currentChannelGroup].ContainsKey(_currentChannelName))
                    {
                        if (_channelList[_currentChannelGroup][_currentChannelName].ContainsKey(_currentChannelNumber))
                        {
                            ChannelInfo channelInfo = _channelList[_currentChannelGroup][_currentChannelName][_currentChannelNumber];
                            if (channelInfo != null)
                            {
                                messageInfo.ChannelGroup = channelInfo.ChannelGroup;
                                messageInfo.ChannelName = channelInfo.ChannelName;
                                messageInfo.ChannelNumber = channelInfo.ChannelNumber;

                                channelInfo.Messages.Add(messageInfo);
                            }
                        }
                    }
                }

                if (ChatContent != null)
                {
                    GameObject chatList = Instantiate(Resources.Load<GameObject>("Prefabs/ChatList"), ChatContent.transform);
                    chatList.GetComponent<UIChatList>().SetData(messageInfo.Index, messageInfo.Avatar, messageInfo.GamerName, messageInfo.Message, messageInfo.Time, messageInfo.Tag, OnReportButton, OnTranslateCheckButton);
                }
            }
            break;
        }
    }

    public void OnError(ERROR_MESSAGE error, object param)
    {
        MessageInfo messageInfo = new MessageInfo
        {
            Index = 0,
            GamerName = "SYSTEM",
            Avatar = "Girl_5",
            Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            Tag = ""
        };

        if (error == ERROR_MESSAGE.CHAT_BAN)
        {
            ErrorMessageChatBanParam errorMessageChatBanParam = (ErrorMessageChatBanParam)param;
            if (errorMessageChatBanParam == null) return;

            var banTime = DateTime.Now.AddSeconds(errorMessageChatBanParam.RemainSeconds);

            messageInfo.Message = error.ToString() + " : " + banTime.ToString("yyyy-MM-dd HH:mm:ss") + " 까지";
        }
        else if (error == ERROR_MESSAGE.CHANNEL_FULL ||
            error == ERROR_MESSAGE.INVALID_PASSWORD ||
            error == ERROR_MESSAGE.ALREADY_CREATED_CHANNEL ||
            error == ERROR_MESSAGE.CHANNEL_GROUP_TOO_SHORT ||
            error == ERROR_MESSAGE.CHANNEL_GROUP_TOO_LONG ||
            error == ERROR_MESSAGE.CHANNEL_NAME_TOO_SHORT ||
            error == ERROR_MESSAGE.CHANNEL_NAME_TOO_LONG ||
            error == ERROR_MESSAGE.DUPLICATE_CHANNEL_GROUP ||
            error == ERROR_MESSAGE.PASSWORD_TOO_LONG ||
            error == ERROR_MESSAGE.CHANNEL_GROUP_FILTERED ||
            error == ERROR_MESSAGE.CHANNEL_NAME_FILTERED)
        {
            ErrorMessageChannelParam errorMessageChannelParam = (ErrorMessageChannelParam)param;
            if (errorMessageChannelParam == null) return;

            messageInfo.Message = error.ToString() + " : " + errorMessageChannelParam.ChannelGroup + " / " + errorMessageChannelParam.ChannelName + " / " + errorMessageChannelParam.ChannelNumber;
        }
        else
        {
            messageInfo.Message = error.ToString();
        }

        if (_channelList.ContainsKey(_currentChannelGroup))
        {
            if (_channelList[_currentChannelGroup].ContainsKey(_currentChannelName))
            {
                if (_channelList[_currentChannelGroup][_currentChannelName].ContainsKey(_currentChannelNumber))
                {
                    ChannelInfo channelInfo = _channelList[_currentChannelGroup][_currentChannelName][_currentChannelNumber];
                    if (channelInfo != null)
                    {
                        messageInfo.ChannelGroup = channelInfo.ChannelGroup;
                        messageInfo.ChannelName = channelInfo.ChannelName;
                        messageInfo.ChannelNumber = channelInfo.ChannelNumber;

                        channelInfo.Messages.Add(messageInfo);
                    }
                }
            }
        }

        if (ChatContent != null)
        {
            GameObject chatList = Instantiate(Resources.Load<GameObject>("Prefabs/ChatList"), ChatContent.transform);
            chatList.GetComponent<UIChatList>().SetData(messageInfo.Index, messageInfo.Avatar, messageInfo.GamerName, messageInfo.Message, messageInfo.Time, messageInfo.Tag, OnReportButton, OnTranslateCheckButton);
        }
    }

    /// <summary>
    /// Photon 방 입장 시 인게임 채팅 채널에 자동으로 접속
    /// RoomManager에서 호출
    /// </summary>
    public void JoinInGameChannel()
    {
        if (_chatClient == null)
        {
            Debug.LogError("[UIChatManager] ChatClient가 초기화되지 않았습니다.");
            return;
        }

        if (!PhotonNetwork.InRoom)
        {
            Debug.LogError("[UIChatManager] Photon 방에 접속되어 있지 않습니다.");
            return;
        }

        string roomName = PhotonNetwork.CurrentRoom.Name;

        Debug.Log($"[UIChatManager] 인게임 채팅 채널 접속 시도: {INGAME_CHANNEL_GROUP} / {roomName}");

        // 오픈 채널로 참가 (같은 Photon 방에 있는 플레이어끼리 채팅)
        _chatClient.SendJoinOpenChannel(INGAME_CHANNEL_GROUP, roomName);
    }

    /// <summary>
    /// 인게임 채팅 채널에서 퇴장
    /// </summary>
    public void LeaveInGameChannel()
    {
        if (_chatClient == null) return;

        if (!string.IsNullOrEmpty(_currentChannelGroup) &&
            _currentChannelGroup == INGAME_CHANNEL_GROUP &&
            !string.IsNullOrEmpty(_currentChannelName))
        {
            Debug.Log($"[UIChatManager] 인게임 채팅 채널 퇴장: {_currentChannelGroup} / {_currentChannelName}");
            _chatClient.SendLeaveChannel(_currentChannelGroup, _currentChannelName, _currentChannelNumber);
        }
    }

    private void OnApplicationQuit()
    {
        LeaveInGameChannel();
        _chatClient?.Dispose();
    }
}
