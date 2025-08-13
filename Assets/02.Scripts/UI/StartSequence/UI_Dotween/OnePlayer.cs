using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;

public class OnePlayer : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnEnable()
    {
        Hashtable hash = new Hashtable()
        {
            {EProperties.IsLoad.ToString(), true}
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        EventManager.Instance.LoadFinished();
    }
    
}
