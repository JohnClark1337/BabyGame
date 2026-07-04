using UnityEngine;
using System.Collections.Generic;
using TMPro;

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
    }

    void Start()
    {
        _whiteSprite = CreateWhiteSprite();

        string[] shapeNames = { "Circle", "Triangle", "Square" };
        Color[] shapeColors = {
            new Color(0.2f, 0.5f, 0.9f, 1f),
            new Color(0.9f, 0.3f, 0.2f, 1f),
            new Color(0.2f, 0.8f, 0.3f, 1f),
        };
        ShapeType[] shapeTypes = { ShapeType.Circle, ShapeType.Triangle, ShapeType.Square };

        for (int i = 0; i < shapeNames.Length; i++)
        {
            GameObject go = GameObject.Find(shapeNames[i]);
            if (go != null)
            {
                SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sprite = GenerateShapeSprite(shapeTypes[i], shapeColors[i]);
                }
                _sceneShapes.Add(go);
            }
        }

        CreateModeButtons();
        ActivateShapes();
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

        TextMeshPro tmp = obj.AddComponent<TextMeshPro>();
        tmp.text = displayChar;
        tmp.fontSize = 3;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

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
        float hw = hh * cam.aspect;

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

        GameObject promptGO = new GameObject("QuizPrompt");
        promptGO.transform.position = new Vector3(cam.transform.position.x, hh - 2f, -5);
        TextMeshPro promptTMP = promptGO.AddComponent<TextMeshPro>();
        promptTMP.text = $"Find the {correct.Name}!";
        promptTMP.fontSize = 1.5f;
        promptTMP.alignment = TextAlignmentOptions.Center;
        promptTMP.color = Color.yellow;
        _dynamicObjects.Add(promptGO);

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

            TextMeshPro tmp = btn.AddComponent<TextMeshPro>();
            tmp.text = btns[i].label;
            tmp.fontSize = 1.5f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

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
