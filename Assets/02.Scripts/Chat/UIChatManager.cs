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
    private const string INGAME_CHANNEL_GROUP = "ingame";

    // 채팅 메시지 수신 이벤트 (UI 클래스들이 구독)
    public event Action<MessageInfo> OnChatMessageReceived;

    // 채널 퇴장 이벤트 (UI 클래스들이 구독)
    public event Action OnChannelLeft;

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

    private bool _isChatClientInitialized = false;

    /// <summary>
    /// ChatClient 초기화 (로비 진입 시 호출)
    /// Nickname은 채널 입장 시점에 검증
    /// </summary>
    public void InitializeChatClient()
    {
        if (_isChatClientInitialized)
        {
            Debug.LogWarning("[UIChatManager] ChatClient가 이미 초기화되었습니다.");
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

        Debug.Log($"[UIChatManager] ChatClient 초기화 시작 - Avatar: {avatar}");

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

        // Nickname 유효성 검사 (메시지 전송 시점에 검증)
        if (AccountManager.Instance == null ||
            AccountManager.Instance.CurrentAccount == null ||
            string.IsNullOrEmpty(AccountManager.Instance.CurrentAccount.Nickname))
        {
            Debug.LogWarning("[UIChatManager] Nickname이 설정되지 않아 메시지를 전송할 수 없습니다.");
            return;
        }

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
            return;
        }

        // 일반 채팅 메시지 전송
        _chatClient.SendChatMessage(channelInfo.ChannelGroup, channelInfo.ChannelName, channelInfo.ChannelNumber, text);
    }

    public void OnJoinChannel(ChannelInfo channelInfo)
    {
        Debug.Log($"[UIChatManager] 채널 입장 성공: Group={channelInfo.ChannelGroup}, Name={channelInfo.ChannelName}, Number={channelInfo.ChannelNumber}");

        // "global" 채널은 SDK 기본 채널이므로 자동으로 나가기
        if (channelInfo.ChannelGroup.ToLower() == "global")
        {
            Debug.Log($"[UIChatManager] 기본 채널({channelInfo.ChannelGroup}/{channelInfo.ChannelName})에서 자동으로 나갑니다.");
            _chatClient.SendLeaveChannel(channelInfo.ChannelGroup, channelInfo.ChannelName, channelInfo.ChannelNumber);
            return;
        }

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

        // 현재 채널 정보 설정 (첫 번째 채널인 경우)
        if (string.IsNullOrEmpty(_currentChannelGroup))
        {
            _currentChannelGroup = channelInfo.ChannelGroup;
            _currentChannelName = channelInfo.ChannelName;
            _currentChannelNumber = channelInfo.ChannelNumber;
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

        // 현재 채널에서 퇴장한 경우, 현재 채널 정보 초기화
        if (_currentChannelGroup == channelInfo.ChannelGroup &&
            _currentChannelName == channelInfo.ChannelName &&
            _currentChannelNumber == channelInfo.ChannelNumber)
        {
            _currentChannelGroup = string.Empty;
            _currentChannelName = string.Empty;
            _currentChannelNumber = 0;
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

        // 현재 채널의 메시지만 처리하여 이벤트 발행
        if (_currentChannelGroup == messageInfo.ChannelGroup &&
            _currentChannelName == messageInfo.ChannelName &&
            _currentChannelNumber == messageInfo.ChannelNumber)
        {
            // UI 클래스들에게 메시지 수신 이벤트 발생
            OnChatMessageReceived?.Invoke(messageInfo);
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
    }

    public void OnTranslateMessage(List<MessageInfo> messages)
    {
        // 인게임에서는 번역 기능을 사용하지 않음
    }

    public void OnHideMessage(MessageInfo messageInfo)
    {
        if (!_channelList.ContainsKey(messageInfo.ChannelGroup)) return;

        if (!_channelList[messageInfo.ChannelGroup].ContainsKey(messageInfo.ChannelName)) return;

        if (!_channelList[messageInfo.ChannelGroup][messageInfo.ChannelName].ContainsKey(messageInfo.ChannelNumber)) return;

        ChannelInfo channelInfo = _channelList[messageInfo.ChannelGroup][messageInfo.ChannelName][messageInfo.ChannelNumber];
        if (channelInfo == null) return;

        // 메시지 내용 업데이트
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

        // 메시지 삭제
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

                // if (ChatContent != null)
                // {
                //     GameObject chatList = Instantiate(Resources.Load<GameObject>("Prefabs/ChatList"), ChatContent.transform);
                //     chatList.GetComponent<UIChatList>().SetData(messageInfo.Index, messageInfo.Avatar, messageInfo.GamerName, messageInfo.Message, messageInfo.Time, messageInfo.Tag, OnReportButton, OnTranslateCheckButton);
                // }
            }
            break;
        }
    }

    public void OnError(ERROR_MESSAGE error, object param)
    {
        Debug.LogError($"[UIChatManager] Backend Chat 에러 발생: {error}, Param: {param}");

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

        // if (ChatContent != null)
        // {
        //     GameObject chatList = Instantiate(Resources.Load<GameObject>("Prefabs/ChatList"), ChatContent.transform);
        //     chatList.GetComponent<UIChatList>().SetData(messageInfo.Index, messageInfo.Avatar, messageInfo.GamerName, messageInfo.Message, messageInfo.Time, messageInfo.Tag, OnReportButton, OnTranslateCheckButton);
        // }
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

        // Nickname 유효성 검사 (채널 입장 시점에 검증)
        if (AccountManager.Instance == null ||
            AccountManager.Instance.CurrentAccount == null ||
            string.IsNullOrEmpty(AccountManager.Instance.CurrentAccount.Nickname))
        {
            Debug.LogError("[UIChatManager] Nickname이 설정되지 않았습니다. 채팅 채널 입장을 중단합니다.");
            return;
        }

        if (!PhotonNetwork.InRoom)
        {
            Debug.LogError("[UIChatManager] Photon 방에 접속되어 있지 않습니다.");
            return;
        }

        // CustomProperties에서 채팅 채널 정보 가져오기
        string channelGroup = null;
        string channelName = null;
        ulong channelNumber = 0;

        // 채널 그룹 가져오기
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(ERoomProperties.ChatChannelGroup.ToString()))
        {
            channelGroup = PhotonNetwork.CurrentRoom.CustomProperties[ERoomProperties.ChatChannelGroup.ToString()] as string;
        }

        // 채널 이름 가져오기
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(ERoomProperties.ChatChannelId.ToString()))
        {
            channelName = PhotonNetwork.CurrentRoom.CustomProperties[ERoomProperties.ChatChannelId.ToString()] as string;
        }

        // 채널 번호 가져오기
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(ERoomProperties.ChatChannelNumber.ToString()))
        {
            object channelNumberObj = PhotonNetwork.CurrentRoom.CustomProperties[ERoomProperties.ChatChannelNumber.ToString()];

            if (channelNumberObj is ulong)
            {
                channelNumber = (ulong)channelNumberObj;
            }
            else if (channelNumberObj is long)
            {
                channelNumber = (ulong)(long)channelNumberObj;
            }
            else if (channelNumberObj is int)
            {
                channelNumber = (ulong)(int)channelNumberObj;
            }
        }

        // Fallback: CustomProperties에 없으면 HashCode 사용 (Backend Chat SDK 길이 제한: 2~20자)
        if (string.IsNullOrEmpty(channelGroup) || string.IsNullOrEmpty(channelName) || channelNumber == 0)
        {
            int roomHash = Math.Abs(PhotonNetwork.CurrentRoom.Name.GetHashCode());
            channelGroup = $"ig_{roomHash}";  // "ig_" + 숫자 (최대 12자)
            channelName = $"rm_{roomHash}";   // "rm_" + 숫자 (최대 12자)
            channelNumber = (ulong)roomHash;
            Debug.LogWarning($"[UIChatManager] 채팅 채널 정보가 CustomProperties에 없어 Fallback 사용: {channelGroup} / {channelName} / {channelNumber}");
        }

        Debug.Log($"[UIChatManager] 인게임 채팅 채널 접속 시도: {channelGroup} / {channelName} / {channelNumber}");

        // 방장인 경우 Private Channel 생성, 다른 플레이어는 참가만
        if (PhotonNetwork.IsMasterClient)
        {
            CreateInGameChannel(channelGroup, channelName, channelNumber);
        }
        else
        {
            // Private Channel로 참가 (비밀번호 없음)
            _chatClient.SendJoinPrivateChannel(channelGroup, channelNumber, "");
        }
    }

    /// <summary>
    /// 인게임 채팅 Private Channel 생성 (방장만 호출)
    /// </summary>
    private void CreateInGameChannel(string channelGroup, string channelName, ulong channelNumber)
    {
        if (_chatClient == null)
        {
            Debug.LogError("[UIChatManager] ChatClient가 null입니다!");
            return;
        }

        uint maxCount = (uint)PhotonNetwork.CurrentRoom.MaxPlayers;
        string password = ""; // 비밀번호 없음 (공개 방처럼 사용)

        Debug.Log($"[UIChatManager] Private Channel 생성 요청: Group={channelGroup}, Number={channelNumber}, Name={channelName}, MaxCount={maxCount}");

        // Private Channel 생성
        _chatClient.SendCreatePrivateChannel(channelGroup, channelNumber, channelName, maxCount, password);
    }

    /// <summary>
    /// 인게임 채팅 채널에서 퇴장
    /// </summary>
    public void LeaveInGameChannel()
    {
        if (_chatClient == null) return;

        // 현재 채널이 인게임 채널인지 확인
        if (!string.IsNullOrEmpty(_currentChannelGroup) &&
            !string.IsNullOrEmpty(_currentChannelName))
        {
            Debug.Log($"[UIChatManager] 인게임 채팅 채널 퇴장: {_currentChannelGroup} / {_currentChannelName}");
            _chatClient.SendLeaveChannel(_currentChannelGroup, _currentChannelName, _currentChannelNumber);

            // 채널 퇴장 이벤트 발행
            OnChannelLeft?.Invoke();
        }
    }

    private void OnApplicationQuit()
    {
        LeaveInGameChannel();
        _chatClient?.Dispose();
    }
}
