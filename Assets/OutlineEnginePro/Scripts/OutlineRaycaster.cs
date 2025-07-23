using UnityEngine;

namespace OutlineEnginePro
{
    public class OutlineRaycaster : MonoBehaviour
    {
        public Camera playerCamera;
        public float maxDistance = 10f;
        private OutlineTarget lastTarget;

        void Update()
        {
            if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, maxDistance))
            {
                OutlineTarget target = hit.collider.GetComponent<OutlineTarget>();

                if (target != null)
                {
                    if (lastTarget != null && lastTarget != target)
                        lastTarget.SetOutline(false);

                    target.SetOutline(true);
                    lastTarget = target;
                }
                else
                {
                    if (lastTarget != null)
                    {
                        lastTarget.SetOutline(false);
                        lastTarget = null;
                    }
                }
            }
            else
            {
                if (lastTarget != null)
                {
                    lastTarget.SetOutline(false);
                    lastTarget = null;
                }
            }
        }
    }
}