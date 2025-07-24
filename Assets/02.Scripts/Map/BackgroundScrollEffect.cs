using System.Collections.Generic;
using UnityEngine;


public class BackgroundScrollEffect : MonoBehaviour
{
    public SpriteRenderer MySpriteRenderer;
    private Material _material;
    private MaterialPropertyBlock _materialPropertyBlock;
    private Vector2 _currentOffset = Vector2.zero;

    public float ScrollSpeed = 1f;

    private void Awake()
    {
        MySpriteRenderer = GetComponent<SpriteRenderer>();
        _materialPropertyBlock = new MaterialPropertyBlock();
    }

    void Update()
    {
        Vector2 direction = Vector2.right;
        
        _currentOffset += direction * ScrollSpeed * Time.deltaTime;
        MySpriteRenderer.GetPropertyBlock(_materialPropertyBlock);
        _materialPropertyBlock.SetVector("_MainTex_ST", new Vector4(1, 1, _currentOffset.x, _currentOffset.y));
        MySpriteRenderer.SetPropertyBlock(_materialPropertyBlock);
    }
}
