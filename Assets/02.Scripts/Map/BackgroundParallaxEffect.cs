using UnityEngine;

public class BackgroundParallaxEffect : MonoBehaviour
{
    public Camera Camera;

    private Vector2 _startingPosition;
    private float _startingZ;
    [SerializeField] private float _multiplier = 1f;

    private Vector2 _cameraMoveSinceStart => (Vector2)Camera.transform.position - _startingPosition;

    void Start()
    {
        _startingPosition = transform.position;
        _startingZ = transform.localPosition.z;
    }

    void Update()
    {
        float newX = _startingPosition.x + _cameraMoveSinceStart.x * _multiplier;
        transform.position = new Vector3(newX, _startingPosition.y, _startingZ);
    }
}