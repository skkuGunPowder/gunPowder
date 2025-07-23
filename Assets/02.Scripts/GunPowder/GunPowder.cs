using Photon.Pun;
using UnityEngine;

public class GunPowder : MonoBehaviourPun, IPunInstantiateMagicCallback
{
    private BoxCollider2D _collider;
    private float _timer = 0f;
    private float _colliderOnTime = 0.2f;
    private Transform _target;
    public Transform Target => _target;
    private bool _isFallingOut;
    public bool IsFallingOut => _isFallingOut;

    private void Start()
    {
        _collider = GetComponent<BoxCollider2D>();
        _collider.enabled = false;
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        if(_timer > _colliderOnTime)
        {
            _collider.enabled = true;
        }
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] instData = photonView.InstantiationData;
        if (instData != null && instData.Length >= 2)
        {
            int attackerViewId = (int)instData[0];
            _isFallingOut = (bool)instData[1];
            PhotonView attackerView = PhotonView.Find(attackerViewId);
            if (attackerView != null)
                _target = attackerView.transform;

            // 컴포넌트 활성화/비활성화 처리
            var release = GetComponent<GunPowderRelease>();
            var bezier = GetComponent<GunPowderBezierCurve>();
            if (_isFallingOut)
            {
                if (release != null) release.enabled = true;
                if (bezier != null) bezier.enabled = false;
            }
            else
            {
                if (release != null) release.enabled = false;
                if (bezier != null) bezier.enabled = true;
            }
        }
    }

    [PunRPC]
    public void SetTarget(Transform target)
    {
        _target = target;
    }
}
