using UnityEngine;

public class TutorialTurnOnUltimate : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log(other.TryGetComponent<Player>(out Player a));
            if (other.TryGetComponent<Player>(out Player player))
            {
                player.ForceUltimateChance();
            }
        }
    }
}
