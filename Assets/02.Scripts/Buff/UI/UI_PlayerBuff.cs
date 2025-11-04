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
        _playerBuffHandler.OnBuffAdded += UpdateBuffUI;
    }

    private void UpdateBuffUI(Buff newBuff)
    {
        Debug.LogWarning("UI_PlayerBuff - UpdateBuffUI 호출됨");
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
}
