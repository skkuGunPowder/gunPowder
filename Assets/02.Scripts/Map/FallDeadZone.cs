using UnityEngine;

public class FallDeadZone : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            if(collision.TryGetComponent<PlayerStat>(out PlayerStat playerStat))
            {
                if(playerStat.IsFallingDead)
                    return;
            }

            PlayerFSM playerFSM = collision.GetComponent<PlayerFSM>();
            playerFSM.ChangeState<PlayerFallDeadState>();
        }
    }
}
