using Photon.Pun;
using UnityEngine;

public class TutorialEnd : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PhotonNetwork.LeaveRoom();
        }
    }
}
