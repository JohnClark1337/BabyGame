using UnityEngine;
using UnityEngine.InputSystem;

public class ModeButton : MonoBehaviour
{
    public GameMode mode;

    void Update()
    {
        if (AnyTouchBeganThisFrame())
            CheckTouch(GetTouchPosition());
    }

    bool AnyTouchBeganThisFrame()
    {
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            return true;
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            return true;
        return false;
    }

    Vector2 GetTouchPosition()
    {
        if (Touchscreen.current != null)
            return Touchscreen.current.primaryTouch.position.ReadValue();
        if (Mouse.current != null)
            return Mouse.current.position.ReadValue();
        return Vector2.zero;
    }

    void CheckTouch(Vector2 screenPosition)
    {
        Camera cam = Camera.main;
        if (cam == null) return;
        Vector3 worldPos = cam.ScreenToWorldPoint(screenPosition);
        Collider2D col = GetComponent<Collider2D>();
        if (col != null && col.OverlapPoint(worldPos))
        {
            if (GameModeManager.Instance != null)
                GameModeManager.Instance.SetMode(mode);
        }
    }
}
