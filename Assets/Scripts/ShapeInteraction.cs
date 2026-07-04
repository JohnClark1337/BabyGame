using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ShapeInteraction : MonoBehaviour
{
    private Rigidbody2D _rb;
    private SpriteRenderer _sprite;
    private string _shapeName;
    private string _clipName;

    public bool isCharacter = false;
    public string displayCharacter = "";
    public string characterName = "";

    public bool isQuizTarget = false;
    public bool isCorrectAnswer = false;

    private static bool _showingInfo = false;
    private static int _infoShownFrame = -1;
    private static GameObject _darkOverlay;
    private static GameObject _shapeDisplay;
    private static Text _charText;
    private static Text _nameText;
    private static AudioSource _infoAudio;
    private static Sprite _whiteSprite;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _sprite = GetComponent<SpriteRenderer>();
        if (isCharacter)
        {
            _shapeName = characterName;
            _clipName = characterName.ToLowerInvariant();
        }
        else
        {
            _shapeName = CleanName(gameObject.name);
            _clipName = _shapeName.ToLowerInvariant();
        }
    }

    void Update()
    {
        if (_rb == null) return;

        if (_showingInfo)
        {
            if (_infoShownFrame != Time.frameCount && AnyTouchBeganThisFrame())
                DismissInfo();
            return;
        }

        if (AnyTouchBeganThisFrame())
            CheckTouch(GetTouchPosition());
    }

    bool AnyTouchBeganThisFrame()
    {
        if (Touchscreen.current != null)
        {
            if (Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
                return true;
        }
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
        Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 0));
        Collider2D col = GetComponent<Collider2D>();
        if (col != null && col.OverlapPoint(worldPos))
            ShowShapeInfo();
    }

    void ShowShapeInfo()
    {
        if (isQuizTarget && !isCorrectAnswer)
        {
            _showingInfo = true;
            _infoShownFrame = Time.frameCount;
            ShowQuizFeedback("Try again!", Color.red);
            Invoke(nameof(DismissInfo), 1.2f);
            return;
        }

        _showingInfo = true;
        _infoShownFrame = Time.frameCount;

        foreach (var interaction in FindObjectsByType<ShapeInteraction>(FindObjectsSortMode.None))
        {
            if (interaction._rb != null)
                interaction._rb.simulated = false;
        }

        if (isQuizTarget && isCorrectAnswer)
        {
            ShowQuizFeedback("Congratulations!", Color.green);
            return;
        }

        if (_darkOverlay == null)
            CreateOverlayObjects();

        _darkOverlay.SetActive(true);
        _nameText.gameObject.SetActive(true);

        if (isCharacter)
        {
            _shapeDisplay.SetActive(false);
            _charText.gameObject.SetActive(true);
            _charText.text = displayCharacter;
            _charText.fontSize = 300;
            _charText.color = Color.white;
        }
        else
        {
            _charText.gameObject.SetActive(false);
            _shapeDisplay.SetActive(true);
            _shapeDisplay.GetComponent<SpriteRenderer>().sprite = _sprite.sprite;
            _shapeDisplay.GetComponent<SpriteRenderer>().color = _sprite.color;
        }

        _nameText.text = _shapeName;

        AudioClip clip = Resources.Load<AudioClip>($"Voices/{_clipName}");
        if (clip != null)
            _infoAudio.PlayOneShot(clip);
    }

    void ShowQuizFeedback(string message, Color color)
    {
        if (_darkOverlay == null)
            CreateOverlayObjects();

        _darkOverlay.SetActive(true);
        _shapeDisplay.SetActive(false);
        _charText.gameObject.SetActive(true);
        _nameText.gameObject.SetActive(false);
        _charText.text = message;
        _charText.color = color;
        _charText.fontSize = 200;
    }

    void CreateOverlayObjects()
    {
        Camera cam = Camera.main;
        float camHeight = cam.orthographicSize * 2;
        float camWidth = camHeight * cam.aspect;

        _whiteSprite = CreateWhiteSprite();

        _darkOverlay = new GameObject("DarkOverlay");
        _darkOverlay.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y,
            cam.transform.position.z + 1);
        _darkOverlay.transform.localScale = new Vector3(camWidth, camHeight, 1);
        SpriteRenderer darkSR = _darkOverlay.AddComponent<SpriteRenderer>();
        darkSR.color = Color.black;
        darkSR.sprite = _whiteSprite;
        darkSR.sortingOrder = 100;

        _infoAudio = _darkOverlay.AddComponent<AudioSource>();

        _shapeDisplay = new GameObject("ShapeDisplay");
        _shapeDisplay.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y,
            cam.transform.position.z + 2);
        _shapeDisplay.transform.localScale = new Vector3(6, 6, 1);
        SpriteRenderer shapeSR = _shapeDisplay.AddComponent<SpriteRenderer>();
        shapeSR.sortingOrder = 101;

        _charText = CreateCanvasText("CharText",
            new Vector3(cam.transform.position.x, cam.transform.position.y, cam.transform.position.z + 2),
            new Vector2(12, 10), 300, Color.white, cam, 102);

        _nameText = CreateCanvasText("NameText",
            new Vector3(cam.transform.position.x, cam.transform.position.y - 3f, cam.transform.position.z + 2),
            new Vector2(8, 2), 80, Color.white, cam, 102);

        _darkOverlay.SetActive(false);
        _shapeDisplay.SetActive(false);
        _charText.gameObject.SetActive(false);
        _nameText.gameObject.SetActive(false);
    }

    Text CreateCanvasText(string name, Vector3 position, Vector2 sizeDelta, int fontSize, Color color,
        Camera cam, int sortingOrder)
    {
        GameObject go = new GameObject(name);
        go.transform.position = position;
        Canvas c = go.AddComponent<Canvas>();
        c.renderMode = RenderMode.WorldSpace;
        c.worldCamera = cam;
        c.sortingOrder = sortingOrder;
        RectTransform cr = go.GetComponent<RectTransform>();
        cr.sizeDelta = sizeDelta;

        Text t = go.AddComponent<Text>();
        t.fontSize = fontSize;
        t.color = color;
        t.alignment = TextAnchor.MiddleCenter;
        t.font = Font.CreateDynamicFontFromOSFont("Comic Sans MS", fontSize);
        RectTransform tr = t.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero;
        tr.anchorMax = Vector2.one;
        tr.offsetMin = Vector2.zero;
        tr.offsetMax = Vector2.zero;

        return t;
    }

    Sprite CreateWhiteSprite()
    {
        Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
    }

    void DismissInfo()
    {
        _showingInfo = false;
        CancelInvoke(nameof(DismissInfo));

        if (_darkOverlay != null) _darkOverlay.SetActive(false);
        if (_shapeDisplay != null) _shapeDisplay.SetActive(false);
        if (_charText != null) _charText.gameObject.SetActive(false);
        if (_nameText != null) _nameText.gameObject.SetActive(false);

        if (isCorrectAnswer || !isQuizTarget)
        {
            foreach (var interaction in FindObjectsByType<ShapeInteraction>(FindObjectsSortMode.None))
            {
                if (interaction._rb != null)
                    interaction._rb.simulated = true;
            }
        }

        if (isQuizTarget && isCorrectAnswer)
        {
            if (GameModeManager.Instance != null)
                GameModeManager.Instance.RegenerateQuiz();
        }
    }

    string CleanName(string name)
    {
        if (name.StartsWith("Square"))
            return "Square";
        return name;
    }
}
