using UnityEngine;
using UnityEngine.SceneManagement;

public class ExplosionVFXNotifier : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float notifyRadius = 3f;
    [SerializeField] private LayerMask targetLayers = ~0;
    [SerializeField] private bool waitOneFrame = true;
    [SerializeField] private int maxDamage = 4;

    [Header("Scene Gate")]
    [SerializeField] private bool onlyInTutorial = true;
    [SerializeField] private string[] allowedSceneNames = new[] { "Tutorial" };

    

    private void OnEnable()
    {
        if (onlyInTutorial && !IsInAllowedScene())
        {
            return;
        }

        if (waitOneFrame)
        {
            StartCoroutine(NotifyNextFrame());
        }
        else
        {
            NotifyNow();
        }
    }

    private bool IsInAllowedScene()
    {
        var active = SceneManager.GetActiveScene().name;
        for (int i = 0; i < allowedSceneNames.Length; i++)
        {
            if (!string.IsNullOrEmpty(allowedSceneNames[i]) && active == allowedSceneNames[i])
            {
                return true;
            }
        }
        return false;
    }

    private System.Collections.IEnumerator NotifyNextFrame()
    {
        yield return null;
        NotifyNow();
    }

    private void NotifyNow()
    {
        Vector2 origin = transform.position;
        var colliders = Physics2D.OverlapCircleAll(origin, notifyRadius, targetLayers);
        foreach (var col in colliders)
        {
            if (col == null)
            {
                continue;
            }

            Dummy dummy = col.GetComponentInParent<Dummy>();
            if (dummy == null)
            {
                continue;
            }

            if (!dummy.gameObject.CompareTag("Enemy"))
            {
                continue;
            }

            if (dummy.isActiveAndEnabled)
            {
                Vector2 closest = col.ClosestPoint(origin);
                float distance = Vector2.Distance(origin, closest);
                distance = Mathf.Min(distance, notifyRadius);

                int damage = 0;
                if (distance < notifyRadius)
                {
                    float t = 1f - (distance / notifyRadius);
                    float damagePerDistance = maxDamage * t;
                    damage = Mathf.CeilToInt(Mathf.Max(0f, damagePerDistance));
                }

                dummy.TriggerExplosionEffect(maxDamage, damage);
            }
        }
    }
}


