using UnityEngine;
using Photon.Pun;
using Cysharp.Threading.Tasks;

public class DestroyCollector: DontDestroySingleton<DestroyCollector>
{
    public void PhotonLazyDestory(GameObject gameObject, PhotonView photonView)
    {
        gameObject.transform.SetParent(transform);
        gameObject.SetActive(false);

        if(photonView.IsMine)
        {
            LazyDestroy(gameObject, 5f);
        }
    }

    private async UniTaskVoid LazyDestroy(GameObject gameObject, float seconds)
    {
        await UniTask.WaitForSeconds(seconds);
        if(gameObject != null)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
