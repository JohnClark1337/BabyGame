using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class QuizChoice : MonoBehaviour
{
    public bool isCorrect;
    private static bool _showingFeedback = false;
    private static int _feedbackFrame = -1;
    private static GameObject _feedbackOverlay;

    void Update()
    {
        if (_showingFeedback)
        {
            if (_feedbackFrame != Time.frameCount && AnyTouchBeganThisFrame())
                DismissFeedback();
            return;
        }

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
            ShowFeedback();
    }

    void ShowFeedback()
    {
        _showingFeedback = true;
        _feedbackFrame = Time.frameCount;

        string message = isCorrect ? "Congratulations!" : "Try again!";
        Color color = isCorrect ? Color.green : Color.red;

        if (_feedbackOverlay == null)
            CreateFeedbackOverlay();

        _feedbackOverlay.SetActive(true);
        _feedbackOverlay.GetComponentInChildren<Text>().text = message;
        _feedbackOverlay.GetComponentInChildren<Text>().color = color;
    }

    void CreateFeedbackOverlay()
    {
        Camera cam = Camera.main;
        float ch = cam.orthographicSize * 2;
        float cw = ch * cam.aspect;

        _feedbackOverlay = new GameObject("QuizFeedback");
        _feedbackOverlay.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, cam.transform.position.z + 1);
        _feedbackOverlay.transform.localScale = new Vector3(cw, ch, 1);
        SpriteRenderer sr = _feedbackOverlay.AddComponent<SpriteRenderer>();
        sr.color = new Color(0, 0, 0, 0.85f);
        sr.sprite = CreateWhiteSprite();
        sr.sortingOrder = 200;

        GameObject textGO = new GameObject("FeedbackText");
        textGO.transform.SetParent(_feedbackOverlay.transform, false);
        textGO.transform.localPosition = Vector3.zero;
        Canvas canvas = textGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = cam;
        RectTransform cr = textGO.GetComponent<RectTransform>();
        cr.sizeDelta = new Vector2(12, 4);

        Text text = textGO.AddComponent<Text>();
        text.fontSize = 120;
        text.alignment = TextAnchor.MiddleCenter;
        text.font = Font.CreateDynamicFontFromOSFont("Comic Sans MS", 120);
        RectTransform tr = text.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero;
        tr.anchorMax = Vector2.one;
        tr.offsetMin = Vector2.zero;
        tr.offsetMax = Vector2.zero;

        _feedbackOverlay.SetActive(false);
    }

    void DismissFeedback()
    {
        _showingFeedback = false;
        if (_feedbackOverlay != null)
            _feedbackOverlay.SetActive(false);

        if (GameModeManager.Instance != null)
            GameModeManager.Instance.SetMode(GameModeManager.CurrentMode);
    }

    Sprite CreateWhiteSprite()
    {
        Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
    }
}
