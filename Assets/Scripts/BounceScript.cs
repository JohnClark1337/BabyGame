using UnityEngine;

public class BounceScript : MonoBehaviour
{
    public float initialSpeed = 4f;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        ClampToCameraBounds();

        if (GetComponent<ShapeInteraction>() == null)
            gameObject.AddComponent<ShapeInteraction>();

        if (rb.linearVelocity.magnitude < 0.01f)
            rb.linearVelocity = Random.insideUnitCircle.normalized * initialSpeed;
    }

    void ClampToCameraBounds()
    {
        Camera cam = Camera.main;
        if (cam == null || !cam.orthographic) return;

        float hh = cam.orthographicSize;
        float aspect = (float)Mathf.Max(Screen.width, Screen.height) / Mathf.Min(Screen.width, Screen.height);
        float hw = hh * aspect;

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -hw, hw);
        pos.y = Mathf.Clamp(pos.y, -hh, hh);
        transform.position = pos;
    }
}
