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
    private const string INGAME_CHANNEL_GROUP = "ig_";

    // 채널별 최대 메시지 보관 개수
    private const int MAX_MESSAGES_PER_CHANNEL = 100;

    // 채팅 메시지 수신 이벤트 (UI 클래스들이 구독)
    public event Action<MessageInfo> OnChatMessageReceived;

    // 채널별 이벤트
    public event Action<MessageInfo> OnPartyChatReceived;
    public event Action<MessageInfo> OnFriendChatReceived;

    // 파티 초대 이벤트
    public event Action<string, string> OnPartyInviteReceived; // (inviterName, partyId)

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

    protected override void Awake()
    {
        base.Awake();
        // [수정] 자신이 싱글톤 인스턴스가 아니라면(중복 생성된 객체라면) 
        // ChatClient를 초기화하지 않고 여기서 멈춥니다.
        if (Instance != null && Instance != this)
        {
            return;
        }

        // ChatClient 초기화 (로그인 후 Nickname이 설정된 상태)
        InitializeChatClient();
    }

    /// <summary>
    /// ChatClient 초기화 (로비 진입 시 호출)
    /// Nickname은 채널 입장 시점에 검증
    /// </summary>
    public void InitializeChatClient()
    {
        // [추가] 중복 객체이거나 이미 초기화 되었다면 중단
        if (Instance != null && Instance != this) return;
        if (_isChatClientInitialized )
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
        Debug.Log($"[UIChatManager] SendChatMessage 호출: {text}");
        Debug.Log($"[UIChatManager] 현재 채널: Group={_currentChannelGroup}, Name={_currentChannelName}, Number={_currentChannelNumber}");

        if (_chatClient == null)
        {
            Debug.LogError("[UIChatManager] ChatClient가 null입니다!");
            return;
        }

        if (_currentChannelName == string.Empty)
        {
            Debug.LogError("[UIChatManager] 현재 채널이 설정되지 않았습니다!");
            return;
        }

        // Nickname 유효성 검사 (메시지 전송 시점에 검증)
        if (AccountManager.Instance == null ||
            AccountManager.Instance.CurrentAccount == null ||
            string.IsNullOrEmpty(AccountManager.Instance.CurrentAccount.Nickname))
        {
            Debug.LogWarning("[UIChatManager] Nickname이 설정되지 않아 메시지를 전송할 수 없습니다.");
            return;
        }

        if (!_channelList.ContainsKey(_currentChannelGroup))
        {
            Debug.LogError($"[UIChatManager] ChannelGroup '{_currentChannelGroup}'가 채널 리스트에 없습니다!");
            return;
        }

        if (!_channelList[_currentChannelGroup].ContainsKey(_currentChannelName))
        {
            Debug.LogError($"[UIChatManager] ChannelName '{_currentChannelName}'이 채널 리스트에 없습니다!");
            return;
        }

        if (!_channelList[_currentChannelGroup][_currentChannelName].ContainsKey(_currentChannelNumber))
        {
            Debug.LogError($"[UIChatManager] ChannelNumber '{_currentChannelNumber}'가 채널 리스트에 없습니다!");
            return;
        }

        ChannelInfo channelInfo = _channelList[_currentChannelGroup][_currentChannelName][_currentChannelNumber];
        if (channelInfo == null)
        {
            Debug.LogError("[UIChatManager] ChannelInfo가 null입니다!");
            return;
        }

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

            Debug.Log($"[UIChatManager] 귓속말 전송: To={whisper[1]}, Msg={message}");
            _chatClient.SendWhisperMessage(whisper[1], message);
            return;
        }

        // 일반 채팅 메시지 전송
        Debug.Log($"[UIChatManager] 채팅 메시지 전송: Group={channelInfo.ChannelGroup}, Name={channelInfo.ChannelName}, Number={channelInfo.ChannelNumber}, Msg={text}");
        _chatClient.SendChatMessage(channelInfo.ChannelGroup, channelInfo.ChannelName, channelInfo.ChannelNumber, text);
        Debug.Log("[UIChatManager] ChatClient.SendChatMessage 호출 완료");
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

        // 이미 채널이 존재하는지 확인
        bool alreadyExists = false;
        if (_channelList.ContainsKey(channelInfo.ChannelGroup))
        {
            if (_channelList[channelInfo.ChannelGroup].ContainsKey(channelInfo.ChannelName))
            {
                if (_channelList[channelInfo.ChannelGroup][channelInfo.ChannelName].ContainsKey(channelInfo.ChannelNumber))
                {
                    alreadyExists = true;
                }
            }
        }

        // 채널 리스트에 추가 (존재하지 않는 경우만)
        if (!alreadyExists)
        {
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
        }

        string groupLower = channelInfo.ChannelGroup.ToLower();
        bool isInGameChannel = groupLower.StartsWith("ig") || groupLower.Contains("ingame")  || groupLower.Contains("ig_");
        if (isInGameChannel)
        {
            _currentChannelGroup = channelInfo.ChannelGroup;
            _currentChannelName = channelInfo.ChannelName;
            _currentChannelNumber = channelInfo.ChannelNumber;
            Debug.Log($"[UIChatManager] 현재 채널 설정 완료: Group={_currentChannelGroup}, Name={_currentChannelName}, Number={_currentChannelNumber}");
        }
        // 인게임 채널이 아니고, 현재 채널이 설정되지 않은 경우 (첫 입장)
        else if (string.IsNullOrEmpty(_currentChannelGroup))
        {
            _currentChannelGroup = channelInfo.ChannelGroup;
            _currentChannelName = channelInfo.ChannelName;
            _currentChannelNumber = channelInfo.ChannelNumber;
            Debug.Log($"[UIChatManager] 현재 채널 설정 완료 (첫 입장): Group={_currentChannelGroup}, Name={_currentChannelName}, Number={_currentChannelNumber}");
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

    /// <summary>
    /// 채널에 메시지 추가 (최대 100개 제한)
    /// </summary>
    private void AddMessageToChannel(ChannelInfo channelInfo, MessageInfo messageInfo)
    {
        if (channelInfo == null) return;

        // 메시지 추가
        channelInfo.Messages.Add(messageInfo);

        // 100개 초과 시 가장 오래된 메시지 삭제
        if (channelInfo.Messages.Count > MAX_MESSAGES_PER_CHANNEL)
        {
            int removeCount = channelInfo.Messages.Count - MAX_MESSAGES_PER_CHANNEL;
            channelInfo.Messages.RemoveRange(0, removeCount);
            Debug.Log($"[UIChatManager] 채널 '{channelInfo.ChannelGroup}/{channelInfo.ChannelName}' 메시지 {removeCount}개 삭제 (100개 제한)");
        }
    }

    public void OnChatMessage(MessageInfo messageInfo)
    {
        Debug.Log($"[UIChatManager] OnChatMessage 수신: Group={messageInfo.ChannelGroup}, Name={messageInfo.ChannelName}, Number={messageInfo.ChannelNumber}, From={messageInfo.GamerName}, Msg={messageInfo.Message}");
        Debug.Log($"[UIChatManager] 현재 채널: Group={_currentChannelGroup}, Name={_currentChannelName}, Number={_currentChannelNumber}");

        if (!_channelList.ContainsKey(messageInfo.ChannelGroup))
        {
            Debug.LogWarning($"[UIChatManager] ChannelGroup '{messageInfo.ChannelGroup}'가 채널 리스트에 없습니다.");
            return;
        }

        if (!_channelList[messageInfo.ChannelGroup].ContainsKey(messageInfo.ChannelName))
        {
            Debug.LogWarning($"[UIChatManager] ChannelName '{messageInfo.ChannelName}'이 채널 리스트에 없습니다.");
            return;
        }

        if (!_channelList[messageInfo.ChannelGroup][messageInfo.ChannelName].ContainsKey(messageInfo.ChannelNumber))
        {
            Debug.LogWarning($"[UIChatManager] ChannelNumber '{messageInfo.ChannelNumber}'가 채널 리스트에 없습니다.");
            return;
        }

        ChannelInfo channelInfo = _channelList[messageInfo.ChannelGroup][messageInfo.ChannelName][messageInfo.ChannelNumber];
        if (channelInfo == null)
        {
            Debug.LogWarning("[UIChatManager] ChannelInfo가 null입니다.");
            return;
        }

        AddMessageToChannel(channelInfo, messageInfo);

        // 채널 타입별 이벤트 발행
        if (messageInfo.ChannelGroup.Contains("ingame") || messageInfo.ChannelGroup.StartsWith("ig_"))
        {
            // 현재 인게임 채널의 메시지만 처리
            if (_currentChannelGroup == messageInfo.ChannelGroup &&
                _currentChannelName == messageInfo.ChannelName &&
                _currentChannelNumber == messageInfo.ChannelNumber)
            {
                Debug.Log($"[UIChatManager] 인게임 채팅 이벤트 발행: {messageInfo.Message}");
                int subscriberCount = OnChatMessageReceived?.GetInvocationList().Length ?? 0;
                Debug.Log($"[UIChatManager] OnChatMessageReceived 구독자 수: {subscriberCount}");
                OnChatMessageReceived?.Invoke(messageInfo);
            }
            else
            {
                Debug.LogWarning($"[UIChatManager] 채널 불일치로 이벤트 발행 안 함. 현재=({_currentChannelGroup}/{_currentChannelName}/{_currentChannelNumber}), 메시지=({messageInfo.ChannelGroup}/{messageInfo.ChannelName}/{messageInfo.ChannelNumber})");
            }
        }
        else if (messageInfo.ChannelGroup == "party")
        {
            // 파티 채팅 이벤트 발행
            OnPartyChatReceived?.Invoke(messageInfo);
        }
        else if (messageInfo.ChannelGroup == "friend")
        {
            // 친구 채팅 이벤트 발행
            OnFriendChatReceived?.Invoke(messageInfo);
        }
    }

    public void OnWhisperMessage(WhisperMessageInfo messageInfo)
    {
        // 파티 초대 메시지 처리
        if (messageInfo.Message.StartsWith("!partyinvite "))
        {
            string partyId = messageInfo.Message.Substring(13).Trim();
            Debug.Log($"[UIChatManager] 파티 초대 수신: {messageInfo.FromGamerName} → {partyId}");
            OnPartyInviteReceived?.Invoke(messageInfo.FromGamerName, partyId);
            return;
        }

        // 일반 귓속말 처리 (인게임 채팅에만 표시)
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

        AddMessageToChannel(channelInfo, add_messageInfo);

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

                                AddMessageToChannel(channelInfo, messageInfo);
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
        else if (error == ERROR_MESSAGE.CHANNEL_NAME_FILTERED ||
            error == ERROR_MESSAGE.CHANNEL_GROUP_FILTERED)
        {
            ErrorMessageChannelParam errorMessageChannelParam = (ErrorMessageChannelParam)param;
            if (errorMessageChannelParam == null) return;

            // 채널 이름 필터링 에러 - 사용자 친화적 메시지
            messageInfo.Message = "채팅 채널 연결에 실패했습니다. 잠시 후 다시 시도해주세요.";
            Debug.LogWarning($"[UIChatManager] 채널 이름 필터링 감지: {errorMessageChannelParam.ChannelGroup} / {errorMessageChannelParam.ChannelName}");

            // 자동 재시도는 하지 않음 (무한 루프 방지)
        }
        else if (error == ERROR_MESSAGE.CHANNEL_FULL ||
            error == ERROR_MESSAGE.INVALID_PASSWORD ||
            error == ERROR_MESSAGE.ALREADY_CREATED_CHANNEL ||
            error == ERROR_MESSAGE.CHANNEL_GROUP_TOO_SHORT ||
            error == ERROR_MESSAGE.CHANNEL_GROUP_TOO_LONG ||
            error == ERROR_MESSAGE.CHANNEL_NAME_TOO_SHORT ||
            error == ERROR_MESSAGE.CHANNEL_NAME_TOO_LONG ||
            error == ERROR_MESSAGE.DUPLICATE_CHANNEL_GROUP ||
            error == ERROR_MESSAGE.PASSWORD_TOO_LONG)
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

                        AddMessageToChannel(channelInfo, messageInfo);
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

        // ★ 방장은 항상 새로운 타임스탬프 채널 생성 (중복 연결 방지)
        string channelGroup = null;
        string channelName = null;
        ulong channelNumber = 0;

        // ★ 핵심: 채널을 새로 개설해야 하는지 여부를 판단하는 플래그
        bool shouldCreate = false;
        
        if (PhotonNetwork.IsMasterClient)
        {
            // 1. 방 속성에 이미 채널 정보가 있는지 먼저 확인
            bool hasExistingChannel = false;
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(ERoomProperties.ChatChannelGroup.ToString(), out object groupObj))
            {
                // 채널 그룹이 존재하고, 유효한(ingamechat) 값인지 확인
                if (groupObj != null && groupObj.ToString() == "ingamechat")
                {
                    hasExistingChannel = true;
                }
            }
            if (hasExistingChannel)
            {
                // A. 이미 채널이 존재한다면 -> 기존 정보를 가져옴 (새로 생성 X)
                channelGroup = PhotonNetwork.CurrentRoom.CustomProperties[ERoomProperties.ChatChannelGroup.ToString()] as string;
                channelName = PhotonNetwork.CurrentRoom.CustomProperties[ERoomProperties.ChatChannelId.ToString()] as string;
                
                object numObj = PhotonNetwork.CurrentRoom.CustomProperties[ERoomProperties.ChatChannelNumber.ToString()];
                // 타입 변환 안전 장치
                if (numObj is ulong ul) channelNumber = ul;
                else if (numObj is long l) channelNumber = (ulong)l;
                else if (numObj is int i) channelNumber = (ulong)i;

                Debug.Log($"[UIChatManager] (방장) 기존 채널 유지: {channelGroup} / {channelName}");
                
                // ★ 중요: 기존 방이 있으므로 새로 만들지 않음
            }
            else
            {
                long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                channelGroup = $"ingamechat";
                string nameSuffix = ((timestamp + 1) & 0xFFFFFFFF).ToString("X8");
                channelName = $"rm{nameSuffix}";
                channelNumber = (ulong)(timestamp & 0x7FFFFFFF); // int 범위 내로 제한

                // Photon Room 속성에 저장
                ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
                props[ERoomProperties.ChatChannelGroup.ToString()] = channelGroup;
                props[ERoomProperties.ChatChannelId.ToString()] = channelName;
                props[ERoomProperties.ChatChannelNumber.ToString()] = (long)channelNumber; // Photon 직렬화 위해 long 형변환
                PhotonNetwork.CurrentRoom.SetCustomProperties(props);

                Debug.Log($"[UIChatManager] (방장) 새로 채널 정보 생성 완료: {channelGroup} / {channelName} / {channelNumber}");
            }
        }
        else
        {
            // 방장이 아닌 경우, CustomProperties에서 채널 정보 가져오기
            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(ERoomProperties.ChatChannelGroup.ToString()))
                channelGroup = PhotonNetwork.CurrentRoom.CustomProperties[ERoomProperties.ChatChannelGroup.ToString()] as string;

            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(ERoomProperties.ChatChannelId.ToString()))
                channelName = PhotonNetwork.CurrentRoom.CustomProperties[ERoomProperties.ChatChannelId.ToString()] as string;

            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(ERoomProperties.ChatChannelNumber.ToString()))
            {
                object channelNumberObj = PhotonNetwork.CurrentRoom.CustomProperties[ERoomProperties.ChatChannelNumber.ToString()];
                if (channelNumberObj is ulong) channelNumber = (ulong)channelNumberObj;
                else if (channelNumberObj is long) channelNumber = (ulong)(long)channelNumberObj;
                else if (channelNumberObj is int) channelNumber = (ulong)(int)channelNumberObj;
            }

            if (string.IsNullOrEmpty(channelGroup) || string.IsNullOrEmpty(channelName) || channelNumber == 0)
            {
                Debug.LogWarning($"[UIChatManager] 채팅 채널 정보가 아직 동기화되지 않았습니다. 0.5초 후 재시도합니다.");
                StartCoroutine(CoRetryJoinInGameChannel()); // 코루틴 호출
                return;
            }

            Debug.Log($"[UIChatManager] CustomProperties에서 채널 정보 읽음: {channelGroup} / {channelName} / {channelNumber}");
            // ★ 중요: 기존 방이 있으므로 새로 만들지 않음
            shouldCreate = false;
        }

        Debug.Log($"[UIChatManager] 인게임 채팅 채널 접속 시도: {channelGroup} / {channelName} / {channelNumber}");

        // ★ 이미 같은 채널에 접속해 있는지 확인
        if (_currentChannelGroup == channelGroup &&
            _currentChannelName == channelName &&
            _currentChannelNumber == channelNumber)
        {
            Debug.LogWarning($"[UIChatManager] 이미 해당 채널에 접속 중입니다. 입장을 건너뜁니다: {channelGroup} / {channelName}");
            return;
        }

        // ★ 다른 인게임 채널에 접속해 있다면 먼저 퇴장
        if (!string.IsNullOrEmpty(_currentChannelGroup) &&
            !string.IsNullOrEmpty(_currentChannelName) &&
            (_currentChannelGroup.Contains("ingame") || _currentChannelGroup.Contains("game") || _currentChannelGroup.StartsWith("ig")))
        {
            Debug.LogWarning($"[UIChatManager] 다른 인게임 채널에서 먼저 퇴장합니다: {_currentChannelGroup} / {_currentChannelName}");
            LeaveInGameChannel();
        }

        if (PhotonNetwork.IsMasterClient)
        {
            // 방장: "이 방 열어줘(없으면 만들고, 있으면 내가 주인)"
            Debug.Log($"[UIChatManager] (방장) 채널 개설/복구 시도: {channelName}");
            CreateInGameChannel(channelGroup, channelName, channelNumber);
        }
        else
        {
            // 일반 유저: "이 방에 들어갈래"
            Debug.Log($"[UIChatManager] (유저) 채널 참가 시도: {channelName}");
            _chatClient.SendJoinPrivateChannel(channelGroup, channelNumber, "");
        }
    }
    // [추가] 재시도 코루틴
    private System.Collections.IEnumerator CoRetryJoinInGameChannel()
    {
        yield return new WaitForSeconds(0.5f);
        JoinInGameChannel();
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

        Debug.Log($"[UIChatManager] ChannelGroup은 항상 ingamechat이여야함");
        channelGroup = $"ingamechat";
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

    // ==================== 파티 채널 기능 ====================

    /// <summary>
    /// 파티 채널 생성 (파티장만 호출)
    /// </summary>
    public void CreatePartyChannel(string partyId, uint maxCount = 8)
    {
        if (_chatClient == null)
        {
            Debug.LogError("[UIChatManager] ChatClient가 초기화되지 않았습니다.");
            return;
        }

        string channelGroup = "party";
        string channelName = $"party_{partyId}";
        ulong channelNumber = (ulong)Math.Abs(partyId.GetHashCode());
        string password = ""; // 비밀번호 없음

        Debug.Log($"[UIChatManager] 파티 채널 생성: {channelGroup} / {channelName} / {channelNumber}");
        _chatClient.SendCreatePrivateChannel(channelGroup, channelNumber, channelName, maxCount, password);
    }

    /// <summary>
    /// 파티 채널 입장
    /// </summary>
    public void JoinPartyChannel(string partyId)
    {
        if (_chatClient == null)
        {
            Debug.LogError("[UIChatManager] ChatClient가 초기화되지 않았습니다.");
            return;
        }

        string channelGroup = "party";
        ulong channelNumber = (ulong)Math.Abs(partyId.GetHashCode());

        Debug.Log($"[UIChatManager] 파티 채널 입장: {channelGroup} / {channelNumber}");
        _chatClient.SendJoinPrivateChannel(channelGroup, channelNumber, "");
    }

    /// <summary>
    /// 파티 채널 퇴장
    /// </summary>
    public void LeavePartyChannel(string partyId)
    {
        if (_chatClient == null) return;

        string channelGroup = "party";
        string channelName = $"party_{partyId}";
        ulong channelNumber = (ulong)Math.Abs(partyId.GetHashCode());

        Debug.Log($"[UIChatManager] 파티 채널 퇴장: {channelGroup} / {channelName}");
        _chatClient.SendLeaveChannel(channelGroup, channelName, channelNumber);
    }

    /// <summary>
    /// 파티 채널에 메시지 전송
    /// </summary>
    public void SendPartyMessage(string partyId, string text)
    {
        if (_chatClient == null) return;

        string channelGroup = "party";
        string channelName = $"party_{partyId}";
        ulong channelNumber = (ulong)Math.Abs(partyId.GetHashCode());

        SendMessageToChannel(channelGroup, channelName, channelNumber, text);
    }

    // ==================== 친구 채팅 기능 ====================

    /// <summary>
    /// 친구 채팅 채널 생성/입장
    /// </summary>
    public void StartFriendChat(string myUid, string friendUid)
    {
        if (_chatClient == null)
        {
            Debug.LogError("[UIChatManager] ChatClient가 초기화되지 않았습니다.");
            return;
        }

        string channelGroup = "friend";
        string channelName = CreateFriendChannelName(myUid, friendUid);
        ulong channelNumber = 0;

        Debug.Log($"[UIChatManager] 친구 채팅 채널 입장: {channelGroup} / {channelName}");

        // Private Channel 생성 시도 (이미 존재하면 자동으로 입장)
        _chatClient.SendJoinPrivateChannel(channelGroup, channelNumber, "");
    }

    /// <summary>
    /// 친구 채팅 채널명 생성 (항상 정렬된 순서)
    /// </summary>
    private string CreateFriendChannelName(string uid1, string uid2)
    {
        return string.Compare(uid1, uid2) < 0
            ? $"{uid1}_{uid2}"
            : $"{uid2}_{uid1}";
    }

    /// <summary>
    /// 친구 채팅 채널 퇴장
    /// </summary>
    public void LeaveFriendChat(string myUid, string friendUid)
    {
        if (_chatClient == null) return;

        string channelGroup = "friend";
        string channelName = CreateFriendChannelName(myUid, friendUid);
        ulong channelNumber = 0;

        Debug.Log($"[UIChatManager] 친구 채팅 채널 퇴장: {channelGroup} / {channelName}");
        _chatClient.SendLeaveChannel(channelGroup, channelName, channelNumber);
    }

    /// <summary>
    /// 친구에게 메시지 전송
    /// </summary>
    public void SendFriendMessage(string myUid, string friendUid, string text)
    {
        if (_chatClient == null) return;

        string channelGroup = "friend";
        string channelName = CreateFriendChannelName(myUid, friendUid);
        ulong channelNumber = 0;

        SendMessageToChannel(channelGroup, channelName, channelNumber, text);
    }

    // ==================== 파티 초대 기능 (귓속말 사용) ====================

    /// <summary>
    /// 친구에게 파티 초대 전송
    /// </summary>
    public void SendPartyInvite(string friendNickname, string partyId)
    {
        if (_chatClient == null)
        {
            Debug.LogError("[UIChatManager] ChatClient가 초기화되지 않았습니다.");
            return;
        }

        string message = $"!partyinvite {partyId}";
        _chatClient.SendWhisperMessage(friendNickname, message);
        Debug.Log($"[UIChatManager] 파티 초대 전송: {friendNickname} → {partyId}");
    }

    // ==================== 공통 메시지 전송 ====================

    /// <summary>
    /// 특정 채널에 메시지 전송
    /// </summary>
    private void SendMessageToChannel(string channelGroup, string channelName, ulong channelNumber, string text)
    {
        if (_chatClient == null) return;

        if (!_channelList.ContainsKey(channelGroup)) return;
        if (!_channelList[channelGroup].ContainsKey(channelName)) return;
        if (!_channelList[channelGroup][channelName].ContainsKey(channelNumber)) return;

        ChannelInfo channelInfo = _channelList[channelGroup][channelName][channelNumber];
        if (channelInfo == null) return;

        _chatClient.SendChatMessage(channelGroup, channelName, channelNumber, text);
    }

    /// <summary>
    /// 특정 채널의 메시지 기록 가져오기
    /// </summary>
    public List<MessageInfo> GetChannelMessages(string channelGroup, string channelName, ulong channelNumber)
    {
        if (!_channelList.ContainsKey(channelGroup)) return new List<MessageInfo>();
        if (!_channelList[channelGroup].ContainsKey(channelName)) return new List<MessageInfo>();
        if (!_channelList[channelGroup][channelName].ContainsKey(channelNumber)) return new List<MessageInfo>();

        ChannelInfo channelInfo = _channelList[channelGroup][channelName][channelNumber];
        return channelInfo?.Messages ?? new List<MessageInfo>();
    }

    /// <summary>
    /// 현재 채널의 메시지 기록 가져오기
    /// </summary>
    public List<MessageInfo> GetCurrentChannelMessages()
    {
        if (string.IsNullOrEmpty(_currentChannelGroup) ||
            string.IsNullOrEmpty(_currentChannelName) ||
            _currentChannelNumber == 0)
        {
            return new List<MessageInfo>();
        }

        return GetChannelMessages(_currentChannelGroup, _currentChannelName, _currentChannelNumber);
    }

    /// <summary>
    /// 특정 채널의 최근 N개 메시지 가져오기
    /// </summary>
    public List<MessageInfo> GetRecentMessages(string channelGroup, string channelName, ulong channelNumber, int count = 50)
    {
        List<MessageInfo> allMessages = GetChannelMessages(channelGroup, channelName, channelNumber);

        if (allMessages.Count <= count)
        {
            return allMessages;
        }

        // 최근 N개만 반환
        return allMessages.GetRange(allMessages.Count - count, count);
    }

    /// <summary>
    /// 현재 채널의 최근 N개 메시지 가져오기
    /// </summary>
    public List<MessageInfo> GetRecentMessagesFromCurrentChannel(int count = 50)
    {
        if (string.IsNullOrEmpty(_currentChannelGroup) ||
            string.IsNullOrEmpty(_currentChannelName) ||
            _currentChannelNumber == 0)
        {
            return new List<MessageInfo>();
        }

        return GetRecentMessages(_currentChannelGroup, _currentChannelName, _currentChannelNumber, count);
    }

    /// <summary>
    /// 인게임 채널 메시지만 삭제 (게임 종료 시 호출)
    /// </summary>
    public void ClearInGameChannelMessages()
    {
        List<string> inGameChannelGroups = new List<string>();

        // "ingame" 포함하거나 "ig_"로 시작하는 채널 찾기
        foreach (var channelGroupPair in _channelList)
        {
            if (channelGroupPair.Key.Contains("ingame") || channelGroupPair.Key.StartsWith("ig_"))
            {
                inGameChannelGroups.Add(channelGroupPair.Key);
            }
        }

        // 찾은 인게임 채널들 삭제
        foreach (string channelGroup in inGameChannelGroups)
        {
            _channelList.Remove(channelGroup);
            Debug.Log($"[UIChatManager] 인게임 채널 메시지 삭제: {channelGroup}");
        }

        // 현재 채널이 인게임 채널이었다면 초기화
        if (_currentChannelGroup.Contains("ingame") || _currentChannelGroup.StartsWith("ig_"))
        {
            _currentChannelGroup = string.Empty;
            _currentChannelName = string.Empty;
            _currentChannelNumber = 0;
        }
    }
}
