using UnityEngine;

public class UI_FindCamera : MonoBehaviour
{
    private Canvas _canvas;
    
    private void Awake()
    {
        _canvas = GetComponent<Canvas>();
        _canvas.worldCamera = Camera.main;
    }
}
