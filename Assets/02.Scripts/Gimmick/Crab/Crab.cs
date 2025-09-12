using System.Collections;
using UnityEngine;

public abstract class Crab : MonoBehaviour
{
    [SerializeField] private float _activeDelay = 1.5f;
    [SerializeField] private float _waitingTime = 5f;
    [SerializeField] private float _maxMoveDistance = 10f;
    [SerializeField] private float _minMoveDistance = 3f;
    [SerializeField] private float _moveSpeed = 2f;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private Transform _rigthGroundChecker;
    [SerializeField] private Transform _leftGroundChecker;

    protected Coroutine _moveCoroutineInstance;
    protected Animator _animator;
    protected SpriteRenderer _spriteRenderer;
    protected bool _isNeedToMove = true;
    protected bool _isActive = false;

    private float _moveDistance;
    private float _movingDirection; // 1 : 오른쪽, -1 왼쪽
    private float _timer = 0f;

    protected virtual void Awake()
    {
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (!_isNeedToMove)
        {
            return;
        }

        _timer += Time.deltaTime;
        if (_timer >= _waitingTime)
        {
            Move();
            _timer = 0f;
        }
    }

    private void Move()
    {
        _moveDistance = Random.Range(_minMoveDistance, _maxMoveDistance);
        _movingDirection = Random.Range(0, 2) == 0 ? -1 : 1;

        if (_movingDirection != 1)
        {
            _spriteRenderer.flipX = true;
        }
        else
        {
            _spriteRenderer.flipX = false;
        }

        _moveCoroutineInstance = StartCoroutine(MoveCoroutine());
    }

    private IEnumerator MoveCoroutine()
    {
        _animator.SetBool("IsMoving", true);
        float movedDistance = 0f;
        while (movedDistance < _moveDistance)
        {
            bool isGrounded = _movingDirection == 1 ?
                Physics2D.OverlapCircle(_rigthGroundChecker.position, 0.1f, _groundLayer) :
                Physics2D.OverlapCircle(_leftGroundChecker.position, 0.1f, _groundLayer);
            if (!isGrounded)
            {
                _movingDirection *= -1;
                _spriteRenderer.flipX = !_spriteRenderer.flipX;
            }

            float step = Time.deltaTime * _moveSpeed; // 이동 속도 조절
            transform.Translate(Vector2.right * _movingDirection * step);
            movedDistance += step;
            yield return null;
        }
        _animator.SetBool("IsMoving", false);
    }

    protected IEnumerator DeactiveCoroutine()
    {
        _animator.SetBool("IsAttack", false);
        _animator.SetBool("IsMoving", false);

        yield return new WaitForSeconds(_activeDelay);
        
        _isNeedToMove = true;
        _isActive = false;
    }
}
