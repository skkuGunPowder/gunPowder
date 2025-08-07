using UnityEngine;

public class OneWayPlatform : MonoBehaviour
{
    void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<Player>().PlayerStat.IsDownJump = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<Player>().PlayerStat.IsDownJump = false;
        }
    }
}
