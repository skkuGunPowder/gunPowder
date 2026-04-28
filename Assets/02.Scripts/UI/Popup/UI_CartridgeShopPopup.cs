using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using PhotonPlayer = Photon.Realtime.Player;
public class UI_CartridgeShopPopup : UI_Popup
{
    [SerializeField] private int _cartridgeCount = 4;
    [SerializeField] private List<CartridgeGoods>  _cartridgeGoodList = new List<CartridgeGoods>();
    
    private PhotonView _photonView;
    private List<CartridgeAction> _cartridgeActionList = new List<CartridgeAction>();
    private bool _isMyTurn = false;

    private void Awake()
    {
        if (_photonView == null)
        {
            _photonView = GetComponent<PhotonView>();
        }

        for (int i = 0; i < _cartridgeGoodList.Count; i++)
        {
            CartridgeAction cartridgeAction = _cartridgeGoodList[i].GetComponent<CartridgeAction>();
            _cartridgeActionList.Add(cartridgeAction);
            cartridgeAction.SetSlotNumber(i);
        }

        EventManager.Instance.OnCartridgeStart += SetMyTurn;
    }

    private void OnEnable()
    {
        SetCartridge();
    }

    private void SetCartridge()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        // 마스터 클라이언트가 가중치 랜덤으로 4개 선택 후 전체에 배포
        string[] cartridgeIDs = CartridgeFactory.Instance.GetRandomCartridgeIDs(_cartridgeCount);
        _photonView.RPC(nameof(RPC_RequestSetCartridge), RpcTarget.All, cartridgeIDs);
    }

    [PunRPC]
    public void RPC_RequestSetCartridge(string[] ids)
    {
        for (int i = 0; i < ids.Length; i++)
        {
            Debug.Log("cartridge id" + ids[i]);
            CartridgeData data = CartridgeFactory.Instance.GetCartridgeData(ids[i]);
            if (data == null)
            {
                Debug.LogWarning($"cartridge data is null {ids[i]}");
                continue;
            }
            _cartridgeGoodList[i].Refresh(data);
        }
    }

    public void SetMyTurn(PhotonPlayer player)
    {
        
        bool myTurn = player.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber;
        
        // 원상태 복구 
        foreach (CartridgeAction action in  _cartridgeActionList)
        {
            if (action.Selected)
            {
                continue;
            }

            action.PlayExitAnimation();
        }
        
        SetClickLock(myTurn);
    }

    // 턴 넘기기
    public void OnclickPassTurn()
    {
        EventManager.Instance.ScreenClick();
    }
    
    public void SetClickLock(bool myTurn)
    {
        // 즉시 잠금
        _isMyTurn = myTurn;
    }
    
    public void RequestSelectCartridge(int index)
    {
        if (!_isMyTurn)
        {
            return;
        }

        _photonView.RPC(nameof(RPC_SelectCartridge), RpcTarget.All, index);
    }

    public void RequestHoverCartridge(int index, bool isEnter)
    {
        if (!_isMyTurn)
        {
            return;
        }

        _photonView.RPC(nameof(RPC_HoverCartridge), RpcTarget.All, index, isEnter);
    }

    [PunRPC]
    public void RPC_SelectCartridge(int index)
    {
        _cartridgeActionList[index].PlaySelectAnimation();
    }

    [PunRPC]
    public void RPC_HoverCartridge(int index, bool isEnter)
    {
        if (isEnter)
        {
            _cartridgeActionList[index].PlayEnterAnimation();
        }
        else
        {
            _cartridgeActionList[index].PlayExitAnimation();
        }
    }

    private void OnDestroy()
    {
        EventManager.Instance.OnCartridgeStart -= SetMyTurn;
    }
}
