using UnityEngine;

public class ShapeInteraction : MonoBehaviour
{
    [SerializeField] private Color _highlightColor = Color.yellow;
    [SerializeField] private float _moveDuration = 0.4f;
    [SerializeField] private float _scaleDuration = 0.2f;
    [SerializeField] private float _postClipDelay = 0.3f;

    private Rigidbody2D _rb;
    private SpriteRenderer _sprite;
    private Vector3 _originalPosition;
    private Vector3 _originalScale;
    private Color _originalColor;
    private Vector2 _storedVelocity;
    private float _storedAngularVelocity;
    private bool _isAnimating = false;
    private string _shapeName;
    private string _clipName;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _sprite = GetComponent<SpriteRenderer>();
        _originalColor = _sprite.color;
        _originalScale = transform.localScale;
        _shapeName = gameObject.name;
        _clipName = MapToClipName(_shapeName);
    }

    void Update()
    {
        if (_isAnimating) return;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.touches[0];
            if (touch.phase == TouchPhase.Began)
                CheckTouch(touch.position);
        }
        else if (Input.GetMouseButtonDown(0))
        {
            CheckTouch(Input.mousePosition);
        }
    }

    void CheckTouch(Vector2 screenPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray);
        if (hit.collider != null && hit.collider.gameObject == gameObject)
            StartCoroutine(AnimateShape());
    }

    System.Collections.IEnumerator AnimateShape()
    {
        _isAnimating = true;
        _originalPosition = transform.position;
        _storedVelocity = _rb.linearVelocity;
        _storedAngularVelocity = _rb.angularVelocity;

        _rb.simulated = false;

        Vector3 center = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0));
        center.z = 0;
        yield return StartCoroutine(MoveTo(center, _moveDuration));

        _sprite.color = _highlightColor;
        Vector3 bigScale = _originalScale * 1.3f;
        yield return StartCoroutine(ScaleTo(bigScale, _scaleDuration));

        string clipPath = $"Voices/{_clipName}";
        AudioClip clip = Resources.Load<AudioClip>(clipPath);
        float waitTime = 1f;
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, Vector3.zero);
            waitTime = clip.length + _postClipDelay;
        }
        yield return new WaitForSeconds(waitTime);

        _sprite.color = _originalColor;
        yield return StartCoroutine(ScaleTo(_originalScale, _scaleDuration));

        yield return StartCoroutine(MoveTo(_originalPosition, _moveDuration));

        _rb.simulated = true;
        _rb.linearVelocity = _storedVelocity;
        _rb.angularVelocity = _storedAngularVelocity;
        _isAnimating = false;
    }

    string MapToClipName(string shapeName)
    {
        if (shapeName.StartsWith("Square"))
            return "square";
        return shapeName.ToLowerInvariant();
    }

    System.Collections.IEnumerator MoveTo(Vector3 target, float duration)
    {
        Vector3 start = transform.position;
        float elapsed = 0;
        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(start, target, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = target;
    }

    System.Collections.IEnumerator ScaleTo(Vector3 target, float duration)
    {
        Vector3 start = transform.localScale;
        float elapsed = 0;
        while (elapsed < duration)
        {
            transform.localScale = Vector3.Lerp(start, target, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = target;
    }
}
