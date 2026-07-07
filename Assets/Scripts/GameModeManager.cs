using UnityEngine;
using System.Collections.Generic;

public enum ShapeType { Circle, Triangle, Square }

public enum GameMode { Shapes, Numbers, Letters, Quiz }

public class GameModeManager : MonoBehaviour
{
    public static GameModeManager Instance { get; private set; }
    public static GameMode CurrentMode { get; private set; } = GameMode.Shapes;

    private List<GameObject> _sceneShapes = new List<GameObject>();
    private List<GameObject> _dynamicObjects = new List<GameObject>();
    private List<GameObject> _modeButtons = new List<GameObject>();
    private static Sprite _whiteSprite;
    private static Font _defaultFont;

    static float LandscapeHW(Camera cam)
    {
        float aspect = (float)Mathf.Max(Screen.width, Screen.height) / Mathf.Min(Screen.width, Screen.height);
        return cam.orthographicSize * aspect;
    }

    static Font DefaultFont()
    {
        if (_defaultFont != null) return _defaultFont;
        _defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (_defaultFont == null)
            _defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return _defaultFont;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoInitialize()
    {
        if (Instance != null) return;
        GameObject go = new GameObject("GameModeManager");
        go.AddComponent<GameModeManager>();
    }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        Screen.orientation = ScreenOrientation.LandscapeLeft;
    }

    void Start()
    {
        _whiteSprite = CreateWhiteSprite();

        RemoveOldWalls();

        float scale = 2.4f;

        (string name, Color color, ShapeType st)[] shapeDefs = {
            ("Circle", new Color(0f, 0.7f, 1f, 1f), ShapeType.Circle),
            ("Triangle", new Color(1f, 0.2f, 0f, 1f), ShapeType.Triangle),
            ("Square (4)", new Color(0f, 1f, 0.2f, 1f), ShapeType.Square),
        };

        foreach (var (sName, sColor, sType) in shapeDefs)
        {
            GameObject go = GameObject.Find(sName);
            if (go != null)
            {
                SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
                Rigidbody2D rb = go.GetComponent<Rigidbody2D>();
                if (sr != null)
                    sr.sprite = GenerateShapeSprite(sType, sColor);
                if (rb != null)
                {
                    rb.gravityScale = 0;
                    rb.linearDamping = 0;
                    rb.angularDamping = 0;
                }
                go.transform.localScale = Vector3.one * scale;
                _sceneShapes.Add(go);
            }
        }

        CreateModeButtons();
        ActivateShapes();
    }

    void RemoveOldWalls()
    {
        string[] oldWalls = { "Square", "Square (1)", "Square (2)", "Square (3)" };
        foreach (string name in oldWalls)
        {
            GameObject go = GameObject.Find(name);
            if (go != null)
                Destroy(go);
        }
    }

    public void RegenerateQuiz()
    {
        SetMode(GameMode.Quiz);
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
                Rigidbody2D rb = shape.GetComponent<Rigidbody2D>();
                if (rb != null)
                    rb.linearVelocity = Random.insideUnitCircle.normalized * 4f;
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

    GameObject CreateBouncer(string displayChar, string name, bool isQuiz = false, bool isCorrect = false)
    {
        Camera cam = Camera.main;
        float hh = cam.orthographicSize;
        float hw = LandscapeHW(cam);

        GameObject obj = new GameObject(name);
        obj.transform.position = new Vector3(
            Random.Range(-hw + 2, hw - 2),
            Random.Range(-hh + 2, hh - 2), 0);
        obj.transform.localScale = Vector3.one * 2.4f;

        Rigidbody2D rb = obj.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.linearDamping = 0;
        rb.freezeRotation = true;
        rb.linearVelocity = Random.insideUnitCircle.normalized * 4f;

        BoxCollider2D col = obj.AddComponent<BoxCollider2D>();
        col.size = Vector2.one;

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = _whiteSprite;
        Color[] bgColors = {
            new Color(0f, 0.7f, 1f, 0.9f),
            new Color(1f, 0.2f, 0f, 0.9f),
            new Color(0f, 1f, 0.2f, 0.9f),
            new Color(1f, 0.9f, 0f, 0.9f),
            new Color(1f, 0f, 0.7f, 0.9f),
        };
        sr.color = bgColors[Random.Range(0, bgColors.Length)];
        sr.sortingOrder = 0;

        Font font = DefaultFont();
        TextMesh tmp = obj.AddComponent<TextMesh>();
        if (font != null) tmp.font = font;
        tmp.text = displayChar;
        tmp.fontSize = 20;
        tmp.alignment = TextAlignment.Center;
        tmp.anchor = TextAnchor.MiddleCenter;
        tmp.color = Color.white;
        MeshRenderer mr = tmp.GetComponent<MeshRenderer>();
        if (mr != null) mr.sortingOrder = 1;

        obj.AddComponent<BounceScript>();
        ShapeInteraction si = obj.AddComponent<ShapeInteraction>();
        si.isCharacter = true;
        si.displayCharacter = displayChar;
        si.characterName = name;
        si.isQuizTarget = isQuiz;
        si.isCorrectAnswer = isCorrect;

        return obj;
    }

    void SetupQuiz()
    {
        Camera cam = Camera.main;
        float hh = cam.orthographicSize;
        float hw = LandscapeHW(cam);

        QuizItem[] pool = GenerateQuizPool();

        int correctIdx = Random.Range(0, pool.Length);
        QuizItem correct = pool[correctIdx];

        List<QuizItem> distractors = new List<QuizItem>();
        for (int i = 0; i < pool.Length; i++)
            if (i != correctIdx) distractors.Add(pool[i]);

        for (int i = 0; i < distractors.Count; i++)
        {
            var t = distractors[i];
            int j = Random.Range(i, distractors.Count);
            distractors[i] = distractors[j];
            distractors[j] = t;
        }

        List<QuizItem> choices = new List<QuizItem> { correct };
        choices.AddRange(distractors.GetRange(0, 4));

        for (int i = 0; i < choices.Count; i++)
        {
            var t = choices[i];
            int j = Random.Range(i, choices.Count);
            choices[i] = choices[j];
            choices[j] = t;
        }

        Font font = DefaultFont();
        GameObject promptGO = new GameObject("QuizPrompt");
        promptGO.transform.position = new Vector3(cam.transform.position.x, hh - 3f, 0);
        TextMesh pt = promptGO.AddComponent<TextMesh>();
        if (font != null) pt.font = font;
        pt.text = $"Find the {correct.Name}!";
        pt.fontSize = 20;
        pt.alignment = TextAlignment.Center;
        pt.anchor = TextAnchor.MiddleCenter;
        pt.color = Color.yellow;
        MeshRenderer pmr = pt.GetComponent<MeshRenderer>();
        if (pmr != null) pmr.sortingOrder = 20;
        _dynamicObjects.Add(promptGO);
        NativeTTS.Speak($"Find the {correct.Name}");

        foreach (var item in choices)
        {
            _dynamicObjects.Add(CreateBouncer(
                item.DisplayChar, item.Name,
                true, item.DisplayChar == correct.DisplayChar && item.Name == correct.Name));
        }
    }

    QuizItem[] GenerateQuizPool()
    {
        List<QuizItem> pool = new List<QuizItem>();
        pool.Add(new QuizItem { DisplayChar = "C", Name = "Circle" });
        pool.Add(new QuizItem { DisplayChar = "T", Name = "Triangle" });
        pool.Add(new QuizItem { DisplayChar = "S", Name = "Square" });
        for (int i = 1; i <= 5; i++)
            pool.Add(new QuizItem { DisplayChar = i.ToString(), Name = NumberWord(i) });
        for (char c = 'A'; c <= 'Z'; c++)
            pool.Add(new QuizItem { DisplayChar = c.ToString(), Name = c.ToString() });
        return pool.ToArray();
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
        float hw = LandscapeHW(cam);

        (string label, GameMode mode)[] btns = {
            ("S", GameMode.Shapes),
            ("1", GameMode.Numbers),
            ("A", GameMode.Letters),
            ("?", GameMode.Quiz),
        };

        float btnSize = 2f;
        float gap = 0.5f;
        float totalW = btns.Length * btnSize + (btns.Length - 1) * gap;
        float startX = -totalW / 2 + btnSize / 2;
        float y = hh - btnSize / 2 - 0.5f;

        for (int i = 0; i < btns.Length; i++)
        {
            GameObject btn = new GameObject($"Btn_{btns[i].label}");
            btn.transform.position = new Vector3(startX + i * (btnSize + gap), y, 0);
            btn.transform.localScale = Vector3.one * btnSize;

            SpriteRenderer sr = btn.AddComponent<SpriteRenderer>();
            sr.sprite = _whiteSprite;
            sr.color = new Color(0.15f, 0.15f, 0.7f, 1f);
            sr.sortingOrder = 60;

            TextMesh tmp = btn.AddComponent<TextMesh>();
            tmp.text = btns[i].label;
            tmp.fontSize = 48;
            tmp.alignment = TextAlignment.Center;
            tmp.anchor = TextAnchor.MiddleCenter;
            tmp.color = Color.white;
            MeshRenderer bmr = tmp.GetComponent<MeshRenderer>();
            if (bmr != null) bmr.sortingOrder = 61;

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
                sr.color = active ? new Color(0.4f, 0.4f, 1f, 1f) : new Color(0.15f, 0.15f, 0.7f, 0.85f);
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

    static Sprite GenerateShapeSprite(ShapeType type, Color color)
    {
        int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float cx = size / 2f;
        float cy = size / 2f;
        float r = size / 2f - 2;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                bool inside = false;
                float fx = x - cx;
                float fy = y - cy;

                switch (type)
                {
                    case ShapeType.Circle:
                        inside = (fx * fx + fy * fy) <= r * r;
                        break;
                    case ShapeType.Triangle:
                        inside = PointInTriangle(fx, fy, 0, r, -r, -r * 0.6f, r, -r * 0.6f);
                        break;
                    case ShapeType.Square:
                        inside = Mathf.Abs(fx) <= r && Mathf.Abs(fy) <= r;
                        break;
                }
                tex.SetPixel(x, y, inside ? color : Color.clear);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100);
    }

    static bool PointInTriangle(float px, float py, float x1, float y1, float x2, float y2, float x3, float y3)
    {
        float d1 = Sign(px, py, x1, y1, x2, y2);
        float d2 = Sign(px, py, x2, y2, x3, y3);
        float d3 = Sign(px, py, x3, y3, x1, y1);
        bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);
        return !(hasNeg && hasPos);
    }

    static float Sign(float px, float py, float x1, float y1, float x2, float y2)
    {
        return (px - x2) * (y1 - y2) - (x1 - x2) * (py - y2);
    }

    struct QuizItem
    {
        public string DisplayChar;
        public string Name;
    }
}
