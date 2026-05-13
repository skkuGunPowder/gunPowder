using UnityEngine;

public class InputHandler
{
    private static bool _blockInput = false;
    public static bool BlockInput { get => _blockInput; set => _blockInput = value; }
    // 시스템용 블락
    private static bool _systemBlock = false;
    public static bool SystemBlock { get => _systemBlock; set => _systemBlock = value; }
    
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
    
    public static bool GetSystemKeyDown(KeyCode key) // 시스템용 인풋
    {
        if (_systemBlock)
        {
            return false;
        }
        
        return Input.GetKeyDown(key);
    }

    public static bool GetKey(KeyCode key)
    {
        if (_blockInput)
        {
            return false;
        }
        return Input.GetKey(key);
    }
}
