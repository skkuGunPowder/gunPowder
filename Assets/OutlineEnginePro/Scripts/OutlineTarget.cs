using UnityEngine;

namespace OutlineEnginePro
{
    [RequireComponent(typeof(Renderer))]
    public class OutlineTarget : MonoBehaviour
    {
        private Renderer objectRenderer;
        private MaterialPropertyBlock propertyBlock;

        private static readonly int ShowOutlineProperty = Shader.PropertyToID("_ShowOutline");

        void Awake()
        {
            objectRenderer = GetComponent<Renderer>();
            propertyBlock = new MaterialPropertyBlock();
        }

        public void SetOutline(bool enable)
        {
            float value = enable ? 1f : 0f;

            objectRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat(ShowOutlineProperty, value);
            objectRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}