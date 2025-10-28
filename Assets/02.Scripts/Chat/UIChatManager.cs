using BackEnd;
using BackndChat;

using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class UIChatManager : MonoBehaviour, BackndChat.IChatClientListener
{
    // public GameObject ChannelContent = null;

    public GameObject ChatContent = null;

    // public GameObject UserContent = null;

    // public Text ChannelUserCount = null;

    public InputField ChatInput = null;

    public Button SendButton = null;

    // public GameObject ReportPopup = null;

    // public Button JoinChannelButton = null;

    // public GameObject JoinChannelPopup = null;

    private string _currentChannelGroup = string.Empty;

    private string _currentChannelName = string.Empty;

    private UInt64 _currentChannelNumber = 0;

    private List<string> _selectMessageKey = new List<string>();

    private ChatClient _chatClient = null;

    private Dictionary<string, Dictionary<string, Dictionary<UInt64, ChannelInfo>>> _channelList =
        new Dictionary<string, Dictionary<string, Dictionary<UInt64, ChannelInfo>>>();

    // Start is called before the first frame update
    void Start()
    {
        if (SendButton != null)
        {
            SendButton.onClick.AddListener(SendChatMessage);
        }

        
        if (ChatInput != null)
        {
            ChatInput.onEndEdit.AddListener((string text) =>
            {
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    SendChatMessage();
                }
            });
        }

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

        _chatClient = new ChatClient(this, new ChatClientArguments
        {
            Avatar = avatar,
            CustomAccessToken = "",
        });
    }

    // Update is called once per frame
    void Update()
    {
        _chatClient?.Update();
    }

    private void SendChatMessage()
    {
        if (ChatInput == null) return;

        if (ChatInput.text.Length == 0) return;

        string text = ChatInput.text;

        ChatInput.text = string.Empty;

        if (string.IsNullOrEmpty(text)) return;

        if (_chatClient == null) return;

        if (_currentChannelName == string.Empty) return;

        if (!_channelList.ContainsKey(_currentChannelGroup)) return;

        if (!_channelList[_currentChannelGroup].ContainsKey(_currentChannelName)) return;

        if (!_channelList[_currentChannelGroup][_currentChannelName].ContainsKey(_currentChannelNumber)) return;

        ChannelInfo channelInfo = _channelList[_currentChannelGroup][_currentChannelName][_currentChannelNumber];
        if (channelInfo == null) return;

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
        else if (text.IndexOf("/nickname") == 0)
        {
            string[] strings = text.Split(' ');

            if (strings.Length < 2) return;

            var returnObject = Backend.BMember.UpdateNickname(strings[1]);
            if (!returnObject.IsSuccess())
            {
                Debug.LogError("닉네임 변경 실패 : " + returnObject);
                return;
            }

            _chatClient.UpdateNickname(strings[1]);
        }
        else if (text.IndexOf("/meta") == 0)
        {
            string[] strings = text.Split(' ');

            if (strings.Length < 4) return;

            if (!channelInfo.Players.ContainsKey(AccountManager.Instance.CurrentAccount.Nickname)) return;

            PlayerInfo player = channelInfo.Players[AccountManager.Instance.CurrentAccount.Nickname];

            if (strings[1] == "add")
            {
                if (player.Metadata.ContainsKey(strings[2])) return;

                player.Metadata.Add(strings[2], strings[3]);
            }
            else if (strings[1] == "remove")
            {
                if (!player.Metadata.ContainsKey(strings[2])) return;

                player.Metadata.Remove(strings[2]);
            }
            else if (strings[1] == "update")
            {
                if (!player.Metadata.ContainsKey(strings[2])) return;

                player.Metadata[strings[2]] = strings[3];
            }

            _chatClient.UpdateMetadata(player.Metadata);
        }
        else if (text.IndexOf("/language") == 0)
        {
            string[] strings = text.Split(' ');

            if (strings.Length < 2) return;

            _chatClient.UpdateLanguage(strings[1]);
        }
        else if (text.IndexOf("/avatar") == 0)
        {
            string[] strings = text.Split(' ');

            if (strings.Length < 2) return;

            _chatClient.UpdateAvatar(strings[1]);
        }
        else
        {
            _chatClient.SendChatMessage(channelInfo.ChannelGroup, channelInfo.ChannelName, channelInfo.ChannelNumber, text);
        }
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

    private void SendReportChat(UInt64 index, string tag, string keyword, string reason)
    {
        if (_chatClient == null) return;

        _chatClient.SendReportChatMessage(index, tag, keyword, reason);
    }

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

        if (_currentChannelGroup == messageInfo.ChannelGroup && _currentChannelName == messageInfo.ChannelName && _currentChannelNumber == messageInfo.ChannelNumber)
        {
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

    private void OnApplicationQuit()
    {
        _chatClient?.Dispose();
    }
}
