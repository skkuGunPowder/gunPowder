using UnityEngine;

public class UI_Ping : MonoBehaviour
{
    public GameObject Content;
    public GameObject Arm;

    [SerializeField] private int _margin = 100;

    private Camera _camera;
    private Camera _renderTexCamera;
    [SerializeField] private Transform _playerTransform;
    public Transform PlayerTransform => _playerTransform;

    private Vector3 _playerScreenPoint;
    private Vector3 _pingScreenPoint;

    public void Init(Transform playerTransform, int margin)
    {
        _playerTransform = playerTransform;
        _margin = margin;
    }

    private void Awake()
    {
        _camera = Camera.main;
    }

    private void Update()
    {
        if (_playerTransform == null || _renderTexCamera == null || !_playerTransform.gameObject.activeSelf)
        {
            Content.SetActive(false);
            return;
        }
        _renderTexCamera.transform.position = new Vector3(_playerTransform.position.x, _playerTransform.position.y, _renderTexCamera.transform.position.z);
        _playerScreenPoint = _camera.WorldToScreenPoint(_playerTransform.position);


        if (CheckPlayerIsInCamera())
        {
            Content.SetActive(false);
            return;
        }
        Content.SetActive(true);

        SetPingPosition(_margin);

        Vector3 dir = (transform.position - _playerTransform.position).normalized;
        Arm.transform.localRotation = Quaternion.LookRotation(transform.forward, dir);
    }

    public void SetPlayerTransform(Transform playerTransform)
    {
        _playerTransform = playerTransform;
    }

    public void SetMargin(int margin)
    {
        _margin = margin;
    }

    public void SetRenderTexCamera(Camera camera)
    {
        _renderTexCamera = camera;
    }

    private void SetPingPosition(int margin)
    {
        _pingScreenPoint = _playerScreenPoint;

        _pingScreenPoint.x = _pingScreenPoint.x < 0 ? margin : _pingScreenPoint.x;
        _pingScreenPoint.y = _pingScreenPoint.y < 0 ? margin : _pingScreenPoint.y;
        _pingScreenPoint.x = _pingScreenPoint.x > Screen.width ? Screen.width - margin : _pingScreenPoint.x;
        _pingScreenPoint.y = _pingScreenPoint.y > Screen.height ? Screen.height - margin : _pingScreenPoint.y;

        transform.position = _camera.ScreenToWorldPoint(_pingScreenPoint);
    }

    private bool CheckPlayerIsInCamera()
    {
        return _playerScreenPoint.y > 0 && _playerScreenPoint.x > 0 && _playerScreenPoint.x < Screen.width && _playerScreenPoint.y < Screen.height;
    }
}
