using System.Collections.Generic;
using UnityEngine;

public class UI_PlayerBuff : MonoBehaviour
{
    [SerializeField] PlayerBuffHandler _playerBuffHandler;

    [SerializeField] List<UI_BuffSlot> _buffSlotList;


    public void Init(PlayerBuffHandler playerBuffHandler)
    {
        Debug.LogWarning("UI_PlayerBuff - Init 호출됨");
        _playerBuffHandler = playerBuffHandler;
        _playerBuffHandler.OnBuffAdded += StartBuffUI;
        _playerBuffHandler.OnBuffRemoved += RemoveBuffUI;
    }

    private void StartBuffUI(Buff newBuff)
    {
        Debug.LogWarning("UI_PlayerBuff - StartBuffUI 호출됨");
        for (int i = 0; i < _buffSlotList.Count; i++)
        {
            if (!_buffSlotList[i].gameObject.activeSelf)
            {
                _buffSlotList[i].gameObject.SetActive(true);
                _buffSlotList[i].StartBuffUI(newBuff);
                break;
            }
        }
    }

    private void RemoveBuffUI(Buff removedBuff)
    {
        Debug.LogWarning("UI_PlayerBuff - RemoveBuffUI 호출됨");
        for (int i = 0; i < _buffSlotList.Count; i++)
        {
            if (_buffSlotList[i].GetBuff().ID == removedBuff.ID)
            {
                _buffSlotList[i].StopBuffUI();
                break;
            }
        }
    }

    private void OnDestroy()
    {
        Debug.LogWarning("UI_PlayerBuff - OnDestroy 호출됨");
        if (_playerBuffHandler != null)
        {
            _playerBuffHandler.OnBuffAdded -= StartBuffUI;
            _playerBuffHandler.OnBuffRemoved -= RemoveBuffUI;
        }
    }
}
