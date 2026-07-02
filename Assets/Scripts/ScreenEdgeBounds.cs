using UnityEngine;

public class ScreenEdgeBounds : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void AutoInitialize()
    {
        GameObject go = new GameObject("ScreenEdgeBounds");
        go.AddComponent<ScreenEdgeBounds>();
    }

    [SerializeField] private float _thickness = 2f;
    [SerializeField] private float _bounciness = 0.8f;
    [SerializeField] private float _friction = 0.4f;

    void Awake()
    {
        RemoveOldWalls();
        TryCreateWalls();
    }

    void Start()
    {
        if (transform.childCount == 0)
            TryCreateWalls();
    }

    void TryCreateWalls()
    {
        CreateEdgeColliders();
    }

    void RemoveOldWalls()
    {
        string[] wallNames = { "Square", "Square (1)", "Square (2)", "Square (3)" };
        foreach (string name in wallNames)
        {
            GameObject wall = GameObject.Find(name);
            if (wall != null)
                Destroy(wall);
        }
    }

    void CreateEdgeColliders()
    {
        Camera cam = Camera.main;
        if (cam == null || !cam.orthographic) return;

        PhysicsMaterial2D bouncyMat = new PhysicsMaterial2D();
        bouncyMat.bounciness = _bounciness;
        bouncyMat.friction = _friction;

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        CreateEdge("Top", new Vector2(0, halfHeight + _thickness * 0.5f), new Vector2(halfWidth * 2, _thickness), bouncyMat);
        CreateEdge("Bottom", new Vector2(0, -halfHeight - _thickness * 0.5f), new Vector2(halfWidth * 2, _thickness), bouncyMat);
        CreateEdge("Left", new Vector2(-halfWidth - _thickness * 0.5f, 0), new Vector2(_thickness, halfHeight * 2), bouncyMat);
        CreateEdge("Right", new Vector2(halfWidth + _thickness * 0.5f, 0), new Vector2(_thickness, halfHeight * 2), bouncyMat);
    }

    void CreateEdge(string side, Vector2 position, Vector2 size, PhysicsMaterial2D mat)
    {
        GameObject edge = new GameObject($"ScreenEdge_{side}");
        edge.transform.SetParent(transform);
        edge.transform.position = position;

        BoxCollider2D collider = edge.AddComponent<BoxCollider2D>();
        collider.size = size;
        collider.sharedMaterial = mat;
    }
}
