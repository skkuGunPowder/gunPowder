using UnityEngine;

public class PlayerDiePart : MonoBehaviour
{
    private float _timer = 0f;
    private Transform _originalParent;
    private Vector3 _originalLocalPosition;
    private Quaternion _originalLocalRotation;

    private void OnEnable()
    {
        // 활성화될 때 부모와 해제
        _originalParent = transform.parent;
        _originalLocalPosition = transform.localPosition;
        _originalLocalRotation = transform.localRotation;
        
        transform.SetParent(null);
        _timer = 0f;
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        if(_timer > 3f)
        {
            // 비활성화될 때 다시 부모에게 들어가기
        if(_originalParent != null)
        {
            transform.SetParent(_originalParent);
            transform.localPosition = _originalLocalPosition;
            transform.localRotation = _originalLocalRotation;
        }
            gameObject.SetActive(false);
        }
    }
}
