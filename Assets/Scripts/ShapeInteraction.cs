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

    private static bool _showingInfo = false;
    private static int _infoShownFrame = -1;
    private static GameObject _darkOverlay;
    private static GameObject _shapeDisplay;
    private static GameObject _charCanvas;
    private static GameObject _nameCanvas;
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
        _showingInfo = true;
        _infoShownFrame = Time.frameCount;

        foreach (var interaction in FindObjectsByType<ShapeInteraction>(FindObjectsSortMode.None))
        {
            if (interaction._rb != null)
                interaction._rb.simulated = false;
        }

        if (_darkOverlay == null)
            CreateOverlayObjects();

        _darkOverlay.SetActive(true);
        _nameCanvas.SetActive(true);

        if (isCharacter)
        {
            _shapeDisplay.SetActive(false);
            _charCanvas.SetActive(true);
            _charCanvas.GetComponentInChildren<Text>().text = displayCharacter;
        }
        else
        {
            _charCanvas.SetActive(false);
            _shapeDisplay.SetActive(true);
            _shapeDisplay.GetComponent<SpriteRenderer>().sprite = _sprite.sprite;
            _shapeDisplay.GetComponent<SpriteRenderer>().color = _sprite.color;
        }

        _nameCanvas.GetComponentInChildren<Text>().text = _shapeName;

        AudioClip clip = Resources.Load<AudioClip>($"Voices/{_clipName}");
        if (clip != null)
            _infoAudio.PlayOneShot(clip);
    }

    void CreateOverlayObjects()
    {
        Camera cam = Camera.main;
        float camHeight = cam.orthographicSize * 2;
        float camWidth = camHeight * cam.aspect;

        _whiteSprite = CreateWhiteSprite();

        _infoAudio = _darkOverlay.AddComponent<AudioSource>();

        _darkOverlay = new GameObject("DarkOverlay");
        _darkOverlay.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, cam.transform.position.z + 1);
        _darkOverlay.transform.localScale = new Vector3(camWidth, camHeight, 1);
        SpriteRenderer darkSR = _darkOverlay.AddComponent<SpriteRenderer>();
        darkSR.color = Color.black;
        darkSR.sprite = _whiteSprite;
        darkSR.sortingOrder = 100;

        _shapeDisplay = new GameObject("ShapeDisplay");
        _shapeDisplay.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, cam.transform.position.z + 2);
        _shapeDisplay.transform.localScale = new Vector3(6, 6, 1);
        SpriteRenderer shapeSR = _shapeDisplay.AddComponent<SpriteRenderer>();
        shapeSR.sortingOrder = 101;

        _charCanvas = new GameObject("CharCanvas");
        _charCanvas.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, cam.transform.position.z + 2);
        Canvas charC = _charCanvas.AddComponent<Canvas>();
        charC.renderMode = RenderMode.WorldSpace;
        charC.worldCamera = cam;
        RectTransform charRect = _charCanvas.GetComponent<RectTransform>();
        charRect.sizeDelta = new Vector2(12, 10);

        GameObject charTextObj = new GameObject("CharText");
        charTextObj.transform.SetParent(_charCanvas.transform, false);
        Text charText = charTextObj.AddComponent<Text>();
        charText.alignment = TextAnchor.MiddleCenter;
        charText.fontSize = 300;
        charText.color = Color.white;
        charText.font = Font.CreateDynamicFontFromOSFont("Comic Sans MS", 300);
        RectTransform charTextRect = charTextObj.GetComponent<RectTransform>();
        charTextRect.anchorMin = Vector2.zero;
        charTextRect.anchorMax = Vector2.one;
        charTextRect.offsetMin = Vector2.zero;
        charTextRect.offsetMax = Vector2.zero;

        _nameCanvas = new GameObject("NameCanvas");
        _nameCanvas.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y - 3f, cam.transform.position.z + 2);
        Canvas canvas = _nameCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = cam;
        RectTransform canvasRect = _nameCanvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(8, 2);

        GameObject nameTextObj = new GameObject("NameText");
        nameTextObj.transform.SetParent(_nameCanvas.transform, false);
        Text nameText = nameTextObj.AddComponent<Text>();
        nameText.alignment = TextAnchor.MiddleCenter;
        nameText.fontSize = 80;
        nameText.color = Color.white;
        nameText.font = Font.CreateDynamicFontFromOSFont("Comic Sans MS", 80);
        RectTransform textRect = nameTextObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        _darkOverlay.SetActive(false);
        _shapeDisplay.SetActive(false);
        _charCanvas.SetActive(false);
        _nameCanvas.SetActive(false);
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

        if (_darkOverlay != null) _darkOverlay.SetActive(false);
        if (_shapeDisplay != null) _shapeDisplay.SetActive(false);
        if (_charCanvas != null) _charCanvas.SetActive(false);
        if (_nameCanvas != null) _nameCanvas.SetActive(false);

        foreach (var interaction in FindObjectsByType<ShapeInteraction>(FindObjectsSortMode.None))
        {
            if (interaction._rb != null)
                interaction._rb.simulated = true;
        }
    }

    string CleanName(string name)
    {
        if (name.StartsWith("Square"))
            return "Square";
        return name;
    }
}
