using UnityEngine;
using System.Collections;
using Photon.Pun;

public class DestroyCollector: DontDestroySingleton<DestroyCollector>
{
    public void PhotonLazyDestory(GameObject gameObject, PhotonView photonView)
    {
        gameObject.transform.SetParent(transform);
        gameObject.SetActive(false);

        if(photonView.IsMine)
        {
            StartCoroutine(LazyDestroy(gameObject, 5f));
        }
    }

    private IEnumerator LazyDestroy(GameObject gameObject, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if(gameObject != null)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
