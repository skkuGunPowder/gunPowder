using UnityEngine;
using DamageNumbersPro;

public class DamagePopup : MonoBehaviour
{
    public DamageNumber popupPrefab;
    public Transform target;
    
    private void Awake()
    {
        if (target == null)
        {
            target = transform;
        }
    }

    public void SpawnPopup(int damage, int maxDamage)
    {
        if (popupPrefab == null || target == null)
        {
            return;
        }

        DamageNumber newPopup = popupPrefab.Spawn(target.position + new Vector3(0, 0.25f, -1), damage);

        newPopup.SetFollowedTarget(target);

        float absDamage = Mathf.Abs(damage);
        if (absDamage >= maxDamage)
        {
            newPopup.SetScale(3f);
            newPopup.SetColor(new Color(1, 0.2f, 0.2f));
        }
        else
        {
            newPopup.SetScale(2f);
            newPopup.SetColor(new Color(1, 0.7f, 0.5f));
        }
    }
}
