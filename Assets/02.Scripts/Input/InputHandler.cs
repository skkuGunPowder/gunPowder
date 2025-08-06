using UnityEngine;

public class InputHandler
{
    private static bool _blockInput = false;
    public static bool BlockInput { get => _blockInput; set => _blockInput = value; }

    public static float GetAxis(string axisName)
    {
        if (_blockInput)
        {
            return 0;
        }

        return Input.GetAxis(axisName);
    }
    
    public static float GetAxisRaw(string axisName)
    {
        if (_blockInput)
        {
            return 0;
        }

        return Input.GetAxisRaw(axisName);
    }
    
    public static bool GetMouseButtonDown(int button)
    {
        if (_blockInput)
        {
            return false;
        }
        
        return Input.GetMouseButtonDown(button);
    }

    public static bool GetMouseButton(int button)
    {
        if (_blockInput)
        {
            return false;
        }
        
        return Input.GetMouseButton(button);
    }
    
    public static bool GetMouseButtonUp(int button)
    {
        if (_blockInput)
        {
            return false;
        }
        
        return Input.GetMouseButtonUp(button);
    }

    public static bool GetKeyDown(KeyCode key)
    {
        if (_blockInput)
        {
            return false;
        }
        
        return Input.GetKeyDown(key);
    }
}
