using UnityEngine;

public class UI_Ping : MonoBehaviour
{
    [SerializeField] private GameObject _arm;
    [SerializeField] private Transform _playerTransform;
    private Camera _camera;

    private Vector3 _playerScreenPoint;
    private Vector3 _pingScreenPoint;

    private void Awake()
    {
        _camera = Camera.main;
    }

    private void Update()
    {
        if (_playerTransform == null)
        {
            GameObject enemy = GameObject.FindWithTag("Enemy");
            if (enemy == null)
            {
                return;
            }

            _playerTransform = enemy.transform;
        }

        _playerScreenPoint = _camera.WorldToScreenPoint(_playerTransform.position);

        // if (!CheckPlayerIsInCamera())
        // {
        //     gameObject.SetActive(false);
        // }
        // else
        // {
            gameObject.SetActive(true);
            
            _pingScreenPoint = _playerScreenPoint;
            _pingScreenPoint.x = _pingScreenPoint.x < 0 ? 100 : _pingScreenPoint.x;
            _pingScreenPoint.x = _pingScreenPoint.x > Screen.width ? Screen.width - 100 : _pingScreenPoint.x;
            _pingScreenPoint.y = _pingScreenPoint.y < 0 ? 100 : _pingScreenPoint.y;
            _pingScreenPoint.y = _pingScreenPoint.y > Screen.height ? Screen.height - 100 : _pingScreenPoint.y;
            transform.position = _camera.ScreenToWorldPoint(_pingScreenPoint);

            Vector3 dir = (_playerTransform.position - transform.position).normalized;
            _arm.transform.localRotation = Quaternion.LookRotation(transform.forward, -dir);
        // }
    }

    private bool CheckPlayerIsInCamera()
    {
        return _playerScreenPoint.y > 0 && _playerScreenPoint.x > 0 && _playerScreenPoint.x < Screen.width && _playerScreenPoint.y < Screen.height;
    }
}
