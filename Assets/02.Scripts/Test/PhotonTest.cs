using System;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
public class PhotonTest : MonoBehaviour
{
    public Transform myTransform;
    private void Start()
    {
        myTransform.DOMove(Vector3.zero, 3f).SetEase(Ease.InCirc);   
    }
}
