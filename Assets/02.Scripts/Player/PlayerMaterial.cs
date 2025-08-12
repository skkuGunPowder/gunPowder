using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMaterial : MonoBehaviour
{
    [Header("Material")]
    public Material DefaultMaterial;

    [Serializable]
    public struct MaterialEntry
    {
        public byte Id;         // 네트워크로 전송할 ID
        public Material Mat;    // 실제 머티리얼 참조
    }

    [Header("Registry (ID -> Material)")]
    [SerializeField] private List<MaterialEntry> _registry = new List<MaterialEntry>();

    private readonly Dictionary<byte, Material> _idToMaterial = new Dictionary<byte, Material>();

    private void Awake()
    {
        _idToMaterial.Clear();
        foreach (var entry in _registry)
        {
            if (entry.Mat == null) continue;
            _idToMaterial[entry.Id] = entry.Mat;
        }
        // 기본값이 레지스트리에 없으면 보강
        if (DefaultMaterial != null && !_idToMaterial.ContainsKey(0))
            _idToMaterial[0] = DefaultMaterial;
    }

    public void ChangeMaterial(Material material, List<SpriteRenderer> spriteRendererList)
    {
        if (material == null || spriteRendererList == null) return;

        foreach (SpriteRenderer spriteRenderer in spriteRendererList)
        {
            if (spriteRenderer == null) continue;

            // 교체
            spriteRenderer.material = material;

            // 새 머티리얼에 스프라이트 텍스처/컬러를 즉시 반영
            var tex = spriteRenderer.sprite != null ? spriteRenderer.sprite.texture : null;
            if (tex != null)
            {
                if (spriteRenderer.material.HasProperty("_BaseMap"))
                    spriteRenderer.material.SetTexture("_BaseMap", tex);
                if (spriteRenderer.material.HasProperty("_MainTex"))
                    spriteRenderer.material.SetTexture("_MainTex", tex);
            }

            if (spriteRenderer.material.HasProperty("_Color"))
            {
                // 스프라이트 컬러 유지
                spriteRenderer.material.SetColor("_Color", spriteRenderer.color);
            }

            // 한 프레임 강제 리프레시로 깜빡임 방지
            spriteRenderer.enabled = false;
            spriteRenderer.enabled = true;
        }
    }

    // ID로 머티리얼 적용 (타겟은 외부에서 주입)
    public void ApplyMaterialById(byte id, List<SpriteRenderer> targets)
    {
        if (!_idToMaterial.TryGetValue(id, out var mat))
        {
            mat = DefaultMaterial;
        }
        ChangeMaterial(mat, targets);
    }
}
