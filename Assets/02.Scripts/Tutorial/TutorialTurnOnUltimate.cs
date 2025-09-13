using UnityEngine;

public class TutorialTurnOnUltimate : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent<Player>(out Player player))
            {
                player.ForceUltimateChance();
            }
        }
    }
}
