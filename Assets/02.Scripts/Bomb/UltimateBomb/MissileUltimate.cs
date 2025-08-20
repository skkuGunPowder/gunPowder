using Photon.Pun;
using UnityEngine;
using DG.Tweening;
using System.Collections;

public class MissileUltimate : Ultimate
{
    private const string ID = "BO0006";

    [SerializeField] private GameObject BomberJetPrefab;
    [SerializeField] private GameObject BackgroundJet;
    [SerializeField] private GameObject ForegroundJet;
    [SerializeField] private AudioClip ForegroundJetSound;

    [SerializeField] private float _backgroundDuration = 2f;
    [SerializeField] private float _foregroundDuration = 1f;

    private Transform _startTransform;
    private Transform _endTransform;

    public override void Init()
    {
        _ownerBombID = "BO0005";
        _startTransform = GameObject.FindWithTag("StartTransform").transform;
        _endTransform = GameObject.FindWithTag("EndTransform").transform;

        _bombStat = ItemDatabase.Instance.GetStat<BombStat>(ID);
    }

    public override void ExcuteUltimate()
    {
        SoundManager.Instance.PlayGlobalSound(ForegroundJetSound.name);

        GameObject backgroundJet = PhotonNetwork.Instantiate(BackgroundJet.name, _startTransform.position, Quaternion.identity);

        Sequence seq = DOTween.Sequence();
        seq.Append(backgroundJet.transform.DOMove(_endTransform.position, _backgroundDuration)
        .OnComplete(() => PhotonNetwork.Destroy(backgroundJet)));

        seq.AppendCallback(() =>
        {
            GameObject foregroundJet = PhotonNetwork.Instantiate(ForegroundJet.name, _endTransform.position, Quaternion.identity);
            foregroundJet.transform.DOMove(_startTransform.position, _foregroundDuration)
            .OnComplete(() => PhotonNetwork.Destroy(foregroundJet));
        });

        seq.AppendCallback(() =>
        {
            GameObject bomberjet = PhotonNetwork.Instantiate(BomberJetPrefab.name, transform.position, Quaternion.identity);
            bomberjet.GetComponent<BomberJet>().SetPlayer(_owner);
        });
    }
}