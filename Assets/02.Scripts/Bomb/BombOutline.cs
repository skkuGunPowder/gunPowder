using Photon.Pun;
using UnityEngine;

/// <summary>
/// 폭탄에 팀별 Material을 적용한다.
/// - 자신/아군이 발사한 폭탄: _originalMaterial
/// - 적이 발사한 폭탄: _outlineMaterial
/// Bomb.SetOwner 호출 시 ApplyTeamColor가 트리거된다.
/// </summary>
[RequireComponent(typeof(Bomb))]
public class BombOutline : MonoBehaviour
{
    [Header("Outline Renderer")]
    [Tooltip("Sprite Renderer")]
    [SerializeField] private SpriteRenderer _renderer;
    [Tooltip("적군 폭탄에 적용할 외곽선 Material")]
    [SerializeField] private Material _outlineMaterial;
    [Tooltip("아군/자신 폭탄에 적용할 원본 Material")]
    [SerializeField] private Material _originalMaterial;

    [Header("ColorSO")] 
    [SerializeField] private ColorDataSO _allyColorSO;
    [SerializeField] private ColorDataSO _enemyColorSO;

    private Color _allyColor = new Color(0.2f, 1f, 0.3f, 1f);
    private Color _enemyColor = new Color(1f, 0.2f, 0.2f, 1f);

    private void Awake()
    {
        _allyColor = _allyColorSO.Color;
        _enemyColor = _enemyColorSO.Color;
        // SetOwner 도착 전엔 팀을 알 수 없으므로 원본 Material로 시작
        if (_renderer != null && _originalMaterial != null)
        {
            _renderer.material = _originalMaterial;
        }
    }

    /// <summary>
    /// 폭탄 오너의 팀을 로컬 플레이어의 팀과 비교해 Material을 교체한다.
    /// Bomb.SetOwner에서 ownerPhotonView가 결정된 직후 호출된다.
    /// </summary>
    public void ApplyTeamColor(PhotonView ownerView)
    {
        if (_renderer == null)
        {
            return;
        }

        if (ownerView == null)
        {
            return;
        }

        EInGameTeam ownerTeam = GetOwnerTeam(ownerView);
        EInGameTeam localTeam = GetLocalPlayerTeam();

        bool isAlly = ownerTeam == localTeam;
        if (isAlly)
        {
            // 추후 아군 색상 정해지면 적용
            //_outlineMaterial.SetColor("_OutlineColor", _allyColor);
            //_renderer.material = _outlineMaterial;
            _renderer.material = _originalMaterial;
        }
        else
        {
            _outlineMaterial.SetColor("_OutlineColor", _enemyColor);
            _renderer.material = _outlineMaterial;
        }
    }

    private EInGameTeam GetOwnerTeam(PhotonView view)
    {
        PlayerStat stat = view.GetComponent<PlayerStat>();
        return stat != null ? stat.Team : EInGameTeam.Default;
    }

    private EInGameTeam GetLocalPlayerTeam()
    {
        if (PhotonNetwork.LocalPlayer == null)
        {
            return EInGameTeam.Default;
        }
        return PhotonNetwork.LocalPlayer.GetCustomProperty<EInGameTeam>(EProperties.Team);
    }
}
