using UnityEngine;

public class SidePlatform : MonoBehaviour
{
    private bool _isRightSide;
    private RotationPlatform _owner;

    public void Init(RotationPlatform rotationPlatform)
    {
        _owner = rotationPlatform;
        if (_owner.transform.position.x < transform.position.x)
        {
            _isRightSide = true;
            return;
        }

        _isRightSide = false;
    }

    public void Update()
    {
        if (_owner.transform.position.x < transform.position.x)
        {
            _isRightSide = true;
            return;
        }

        _isRightSide = false;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player" && Input.GetKeyDown(KeyCode.DownArrow))
        {
            _owner.Rotate(_isRightSide);
        }
    }
}
