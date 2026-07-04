using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public enum GameMode { Shapes, Numbers, Letters, Quiz }

public class GameModeManager : MonoBehaviour
{
    public static GameModeManager Instance { get; private set; }
    public static GameMode CurrentMode { get; private set; } = GameMode.Shapes;

    private List<GameObject> _sceneShapes = new List<GameObject>();
    private List<GameObject> _dynamicObjects = new List<GameObject>();
    private List<GameObject> _modeButtons = new List<GameObject>();
    private static Sprite _whiteSprite;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        _whiteSprite = CreateWhiteSprite();

        foreach (string name in new[] { "Circle", "Triangle", "Square" })
        {
            GameObject go = GameObject.Find(name);
            if (go != null) _sceneShapes.Add(go);
        }

        CreateModeButtons();
    }

    void Start()
    {
        ActivateShapes();
    }

    public void SetMode(GameMode mode)
    {
        if (mode == CurrentMode && mode != GameMode.Quiz) return;

        foreach (var obj in _dynamicObjects)
            if (obj != null) Destroy(obj);
        _dynamicObjects.Clear();

        foreach (var shape in _sceneShapes)
            if (shape != null) shape.SetActive(false);

        CurrentMode = mode;
        UpdateButtonHighlights();

        switch (mode)
        {
            case GameMode.Shapes: ActivateShapes(); break;
            case GameMode.Numbers: SpawnNumbers(); break;
            case GameMode.Letters: SpawnLetters(); break;
            case GameMode.Quiz: SetupQuiz(); break;
        }
    }

    void ActivateShapes()
    {
        foreach (var shape in _sceneShapes)
        {
            if (shape != null)
            {
                shape.SetActive(true);
                if (shape.GetComponent<ShapeInteraction>() == null)
                    shape.AddComponent<ShapeInteraction>();
            }
        }
    }

    void SpawnNumbers()
    {
        string[] names = { "One", "Two", "Three", "Four", "Five" };
        string[] chars = { "1", "2", "3", "4", "5" };
        for (int i = 0; i < 5; i++)
            _dynamicObjects.Add(CreateBouncer(chars[i], names[i]));
    }

    void SpawnLetters()
    {
        HashSet<char> used = new HashSet<char>();
        while (used.Count < 5)
            used.Add((char)('A' + Random.Range(0, 26)));
        foreach (char c in used)
            _dynamicObjects.Add(CreateBouncer(c.ToString(), c.ToString()));
    }

    GameObject CreateBouncer(string displayChar, string name)
    {
        Camera cam = Camera.main;
        float hh = cam.orthographicSize;
        float hw = hh * cam.aspect;

        GameObject obj = new GameObject(name);
        obj.transform.position = new Vector3(
            Random.Range(-hw + 2, hw - 2),
            Random.Range(-hh + 2, hh - 2), 0);

        Rigidbody2D rb = obj.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.linearDamping = 0;
        rb.freezeRotation = true;
        rb.linearVelocity = Random.insideUnitCircle.normalized * 5f;

        BoxCollider2D col = obj.AddComponent<BoxCollider2D>();
        col.size = new Vector2(2, 2);

        GameObject labelGO = new GameObject("Label");
        labelGO.transform.SetParent(obj.transform, false);
        Canvas canvas = labelGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = cam;
        RectTransform cr = labelGO.GetComponent<RectTransform>();
        cr.sizeDelta = new Vector2(2, 2);

        Text text = labelGO.AddComponent<Text>();
        text.text = displayChar;
        text.fontSize = 120;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.font = Font.CreateDynamicFontFromOSFont("Comic Sans MS", 120);
        RectTransform tr = text.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero;
        tr.anchorMax = Vector2.one;
        tr.offsetMin = Vector2.zero;
        tr.offsetMax = Vector2.zero;

        obj.AddComponent<BounceScript>();
        ShapeInteraction si = obj.AddComponent<ShapeInteraction>();
        si.isCharacter = true;
        si.displayCharacter = displayChar;
        si.characterName = name;

        return obj;
    }

    void SetupQuiz()
    {
        Camera cam = Camera.main;
        float hh = cam.orthographicSize;
        float hw = hh * cam.aspect;

        QuizItem[] items = GenerateQuizItems(3);
        int correctIdx = Random.Range(0, items.Length);

        GameObject qGO = new GameObject("QuizPrompt");
        qGO.transform.position = new Vector3(cam.transform.position.x, hh - 3f, -5);
        Canvas qc = qGO.AddComponent<Canvas>();
        qc.renderMode = RenderMode.WorldSpace;
        qc.worldCamera = cam;
        RectTransform qr = qGO.GetComponent<RectTransform>();
        qr.sizeDelta = new Vector2(14, 2);
        Text qt = qGO.AddComponent<Text>();
        qt.text = $"Find the {items[correctIdx].Name}!";
        qt.fontSize = 80;
        qt.alignment = TextAnchor.MiddleCenter;
        qt.color = Color.yellow;
        qt.font = Font.CreateDynamicFontFromOSFont("Comic Sans MS", 80);
        _dynamicObjects.Add(qGO);

        for (int i = 0; i < items.Length; i++)
        {
            int idx = i;
            GameObject choice = new GameObject($"Choice_{items[i].DisplayChar}");
            float x = i == 0 ? -4f : (i == 1 ? 0f : 4f);
            float y = -hh + 4f;
            choice.transform.position = new Vector3(x, y, -5);
            choice.transform.localScale = new Vector3(3, 3, 1);

            SpriteRenderer bg = choice.AddComponent<SpriteRenderer>();
            bg.sprite = _whiteSprite;
            bg.color = new Color(0.3f, 0.3f, 0.6f, 0.9f);
            bg.sortingOrder = 50;

            GameObject lgo = new GameObject("Label");
            lgo.transform.SetParent(choice.transform, false);
            Canvas lc = lgo.AddComponent<Canvas>();
            lc.renderMode = RenderMode.WorldSpace;
            lc.worldCamera = cam;
            RectTransform lr = lgo.GetComponent<RectTransform>();
            lr.sizeDelta = new Vector2(2.5f, 2.5f);
            Text lt = lgo.AddComponent<Text>();
            lt.text = items[i].DisplayChar;
            lt.fontSize = 150;
            lt.alignment = TextAnchor.MiddleCenter;
            lt.color = Color.white;
            lt.font = Font.CreateDynamicFontFromOSFont("Comic Sans MS", 150);

            BoxCollider2D col = choice.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;

            QuizChoice qc2 = choice.AddComponent<QuizChoice>();
            qc2.isCorrect = (idx == correctIdx);

            _dynamicObjects.Add(choice);
        }
    }

    QuizItem[] GenerateQuizItems(int count)
    {
        List<QuizItem> pool = new List<QuizItem>();
        pool.Add(new QuizItem { DisplayChar = "\u25CB", Name = "Circle" });
        pool.Add(new QuizItem { DisplayChar = "\u25B3", Name = "Triangle" });
        pool.Add(new QuizItem { DisplayChar = "\u25A1", Name = "Square" });
        for (int i = 1; i <= 5; i++)
            pool.Add(new QuizItem { DisplayChar = i.ToString(), Name = NumberWord(i) });
        for (char c = 'A'; c <= 'Z'; c++)
            pool.Add(new QuizItem { DisplayChar = c.ToString(), Name = c.ToString() });

        QuizItem[] result = new QuizItem[count];
        HashSet<int> used = new HashSet<int>();
        for (int i = 0; i < count; i++)
        {
            int idx;
            do idx = Random.Range(0, pool.Count);
            while (used.Contains(idx));
            used.Add(idx);
            result[i] = pool[idx];
        }
        return result;
    }

    static string NumberWord(int n)
    {
        string[] w = { "One", "Two", "Three", "Four", "Five" };
        return w[n - 1];
    }

    void CreateModeButtons()
    {
        Camera cam = Camera.main;
        float hh = cam.orthographicSize;
        float hw = hh * cam.aspect;

        (string label, GameMode mode)[] btns = {
            ("\u25B3", GameMode.Shapes),
            ("1", GameMode.Numbers),
            ("A", GameMode.Letters),
            ("?", GameMode.Quiz),
        };

        for (int i = 0; i < btns.Length; i++)
        {
            GameObject btn = new GameObject($"Btn_{btns[i].label}");
            float x = -hw + 1.5f + i * 3f;
            float y = hh - 1.5f;
            btn.transform.position = new Vector3(x, y, -5);
            btn.transform.localScale = new Vector3(2.5f, 2.5f, 1);

            SpriteRenderer sr = btn.AddComponent<SpriteRenderer>();
            sr.sprite = _whiteSprite;
            sr.color = new Color(0.15f, 0.15f, 0.5f, 0.85f);
            sr.sortingOrder = 60;

            GameObject lgo = new GameObject("Label");
            lgo.transform.SetParent(btn.transform, false);
            Canvas lc = lgo.AddComponent<Canvas>();
            lc.renderMode = RenderMode.WorldSpace;
            lc.worldCamera = cam;
            RectTransform lr = lgo.GetComponent<RectTransform>();
            lr.sizeDelta = new Vector2(2, 2);
            Text lt = lgo.AddComponent<Text>();
            lt.text = btns[i].label;
            lt.fontSize = 100;
            lt.alignment = TextAnchor.MiddleCenter;
            lt.color = Color.white;
            lt.font = Font.CreateDynamicFontFromOSFont("Comic Sans MS", 100);

            BoxCollider2D col = btn.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;

            ModeButton mb = btn.AddComponent<ModeButton>();
            mb.mode = btns[i].mode;

            _modeButtons.Add(btn);
        }
        UpdateButtonHighlights();
    }

    void UpdateButtonHighlights()
    {
        for (int i = 0; i < _modeButtons.Count; i++)
        {
            if (_modeButtons[i] == null) continue;
            ModeButton mb = _modeButtons[i].GetComponent<ModeButton>();
            SpriteRenderer sr = _modeButtons[i].GetComponent<SpriteRenderer>();
            if (mb != null && sr != null)
            {
                bool active = mb.mode == CurrentMode;
                sr.color = active ? new Color(0.3f, 0.3f, 0.7f, 1f) : new Color(0.15f, 0.15f, 0.5f, 0.85f);
            }
        }
    }

    static Sprite CreateWhiteSprite()
    {
        Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
    }

    struct QuizItem
    {
        public string DisplayChar;
        public string Name;
    }
}
