using UnityEngine;

public class GameModeBase : MonoBehaviour
{
    /// <summary>
    /// 게임 모드 모두가 사용할 공통 함수
    /// 1. 게임 종료
    /// 2.  
    /// </summary>
    
    protected virtual void Awake()
    {
        
    }
    
    protected virtual void Start()
    {
        
    }
    
    protected virtual void Update()
    {
        
    }

    /// <summary>
    /// 게임 종료 시키기 : 라이프 감소외 다른 게임 종료 조건이 있다면 오버라이드
    /// </summary>
    public virtual void GameOver()
    {
        // 게임 종료 :  GameManager.Instance.RequestGameOver();
    }    
}
