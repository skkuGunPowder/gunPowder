using UnityEngine;

public class PlayerZFix : MonoBehaviour
{
    void LateUpdate()
    {
        Vector3 pos = transform.position;
        if (pos.z != 0f)
        {
            pos.z = 0f;
            transform.position = pos;
        }
    }
}
