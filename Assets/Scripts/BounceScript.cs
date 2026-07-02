using UnityEngine;

public class BounceScript : MonoBehaviour
{
    public float initialSpeed = 5f;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        ClampToCameraBounds();

        if (GetComponent<ShapeInteraction>() == null)
            gameObject.AddComponent<ShapeInteraction>();

        var direction = (transform.right + transform.up).normalized;
        rb.linearVelocity = direction * initialSpeed;
    }

    void ClampToCameraBounds()
    {
        Camera cam = Camera.main;
        if (cam == null || !cam.orthographic) return;

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -halfWidth, halfWidth);
        pos.y = Mathf.Clamp(pos.y, -halfHeight, halfHeight);
        transform.position = pos;
    }
}
