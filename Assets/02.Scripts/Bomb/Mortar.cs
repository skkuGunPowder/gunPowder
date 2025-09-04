using Photon.Pun;
using UnityEngine;
using Heathen.UnityPhysics.API;
using Heathen.UnityPhysics;


public class Mortar : Bomb
{
    public const string ID = "BO0009";

    [SerializeField] private GameObject _mortarShellPrefab;
    [SerializeField] private AudioClip _mortarFireSound;
    [SerializeField] private AudioClip _mortarInstallSound;

    [SerializeField] private float _mortarDuration = 8f;

    private float _distance;

    protected override void Init()
    {
        base.Init();
        SetStat(ID);
    }

    protected override void Update()
    {
        // (축) 아무것도 안함 (하)
        if (Input.GetKey(KeyCode.RightArrow))
        {
            _distance += Time.deltaTime * 5f;
            _distance = Mathf.Clamp(_distance, 0f, 20f);

        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            _distance -= Time.deltaTime * 5f;
            _distance = Mathf.Clamp(_distance, 0f, 20f);
        }
    }

    [PunRPC]
    public override void PlaceBomb(Vector3 fireRightDirection, Vector3 fireUpDrection, Vector3 fireFowordDirection)
    {
        if (_mortarFireSound != null)
        {
            SoundManager.Instance.PlayLocalSound(nameof(_mortarInstallSound), transform);
        }

        // TODO
        if (PhotonView.IsMine)
        {
            InputHandler.BlockInput = true;
        }
    }

    [PunRPC]
    public override void ThrowBomb(Vector3 fireRightDirection, Vector3 fireUpDrection, Vector3 fireFowordDirection)
    {
        PlaceBomb(fireRightDirection, fireUpDrection, fireFowordDirection);
    }

    [PunRPC]
    public override void ThrowBombStraight(Vector3 fireRightDirection, Vector3 fireUpDrection, Vector3 fireFowordDirection)
    {
        PlaceBomb(fireRightDirection, fireUpDrection, fireFowordDirection);
    }

    [PunRPC]
    public override void BoostBomb(Vector3 fireRightDirection, Vector3 fireUpDrection, Vector3 fireFowordDirection)
    {
        PlaceBomb(fireRightDirection, fireUpDrection, fireFowordDirection);
    }

    [PunRPC]
    public override void SmashBomb(Vector3 fireRightDirection, Vector3 fireUpDrection, Vector3 fireFowordDirection)
    {
        PlaceBomb(fireRightDirection, fireUpDrection, fireFowordDirection);
    }
}
