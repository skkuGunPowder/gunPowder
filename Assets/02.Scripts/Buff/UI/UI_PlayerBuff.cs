using System.Collections.Generic;
using UnityEngine;

public class UI_PlayerBuff : MonoBehaviour
{
    [SerializeField] PlayerBuffHandler _playerBuffHandler;

    [SerializeField] List<UI_BuffSlot> _buffSlotList;


    public void Init(PlayerBuffHandler playerBuffHandler)
    {
        _playerBuffHandler = playerBuffHandler;
        _playerBuffHandler.OnBuffAdded += StartBuffUI;
        _playerBuffHandler.OnBuffRemoved += RemoveBuffUI;
    }

    private void StartBuffUI(Buff newBuff)
    {
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
        for (int i = 0; i < _buffSlotList.Count; i++)
        {
            if (_buffSlotList[i] == null)
            {
                continue;
            }
            
            if (_buffSlotList[i].GetBuff().ID == removedBuff.ID)
            {
                _buffSlotList[i].StopBuffUI();
                break;
            }
        }
    }

    private void OnDestroy()
    {
        if (_playerBuffHandler != null)
        {
            _playerBuffHandler.OnBuffAdded -= StartBuffUI;
            _playerBuffHandler.OnBuffRemoved -= RemoveBuffUI;
        }
    }
}
