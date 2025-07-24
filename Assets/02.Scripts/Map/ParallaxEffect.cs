using UnityEngine;
using UnityEngine.Serialization;

public class ParallaxEffect : MonoBehaviour
{
    public Camera Camera;               // 카메라를 참조하기 위한 공개 변수
    public Transform FollowTarget;   // 팔로우할 대상을 참조하기 위한 공개 변수

    private Vector2 _startingPosition;        // 패럴랙스 게임 오브젝트의 시작 위치를 저장하는 벡터
    private float _startingZ;                 // 패럴랙스 게임 오브젝트의 시작 Z 값
    [SerializeField] private float _multiplier = 1f;
    
    // 카메라 이동량을 계산하여 반환하는 익명 속성 (Property)
    private Vector2 _cameraMoveSinceStart => (Vector2)Camera.transform.position - _startingPosition;

    // 대상과 패럴랙스 오브젝트 간의 Z 거리를 반환하는 익명 속성 (Property)
    private float _zDistanceFromTarget => transform.position.z - FollowTarget.transform.position.z;

    // 클리핑 플레인 (클리핑 평면)을 계산하여 반환하는 익명 속성 (Property)
    // 플레이어와 배경 오브젝트 사이의 거리값에 따라서 다른 움직임 효과를 나타내기 위한 변수
    // 동일한 효과를 주고 싶으면 0.1f로 고정해도 됩니다.
    private float _clippingPlane => (Camera.transform.position.z + (_zDistanceFromTarget) > 0 ? Camera.farClipPlane : Camera.nearClipPlane);

    // 패럴랙스 계수를 계산하여 반환하는 익명 속성 (Property)
    private float _parallaxFactor => Mathf.Abs(_zDistanceFromTarget) / _clippingPlane;
    //[SerializeField] private float _parallaxFactor = 0.2f;
    
    // Start 함수는 첫 번째 프레임 이전에 호출됩니다.
    void Start()
    {
        // 패럴랙스 게임 오브젝트의 시작 위치와 Z 값을 저장합니다.
        _startingPosition = transform.position;
        _startingZ = transform.localPosition.z;
    }

    // Update 함수는 매 프레임마다 호출됩니다.
    void Update()
    {
        // 새로운 위치를 계산하여 패럴랙스 게임 오브젝트의 위치를 업데이트합니다.

        float newX = _startingPosition.x + _cameraMoveSinceStart.x * _multiplier;

        // 패럴랙스 오브젝트의 위치를 새로 계산된 위치로 업데이트하며 Z 값은 시작 Z 값으로 유지합니다.
        transform.position = new Vector3(newX, _startingPosition.y, _startingZ);
    }
}
