using UnityEngine;

public class AirDropItemSpeed : AirDropItemBase, IAirDropItem
{
    private float _duration = 10f;
    private float _increaseAmount = 4f;

    private float _originalMoveSpeed;
    private float _originalRunSpeed;
    

    private float _timer = 0f;
    private bool _isBuffOn = true;

    private void Update()
    {
        if (!_isBuffOn)
        {
            return;
        }

        _timer += Time.deltaTime;
        if (_timer > _duration)
        {
            EndBuff();
        }
    }

    public override void Use()
    {       
        _isBuffOn = true;
        _originalMoveSpeed = _owner.PlayerStat.MoveSpeed;
        _originalRunSpeed = _owner.PlayerStat.RunSpeed;

        _owner.PlayerStat.MoveSpeed += _increaseAmount;
        _owner.PlayerStat.RunSpeed += _increaseAmount;
    }

    private void EndBuff()
    {
        _owner.PlayerStat.MoveSpeed = _originalMoveSpeed;
        _owner.PlayerStat.RunSpeed = _originalRunSpeed;
        _isBuffOn = false;
    }
}
