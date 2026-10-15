using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

// Outils : creation de sprites carres et d'elements d'interface par code.
public static class Util
{
    static Sprite _sq;
    public static Sprite Square
    {
        get
        {
#if UNITY_EDITOR
            // Dans l'editeur (hors Play) : utilise le vrai asset Assets/Sprites/Carre.png
            if (!Application.isPlaying)
            {
                var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Carre.png");
                if (asset != null) return asset;
            }
#endif
            if (_sq == null)
            {
                var t = new Texture2D(4, 4);
                var px = new Color[16];
                for (int i = 0; i < 16; i++) px[i] = Color.white;
                t.SetPixels(px);
                t.Apply();
                t.filterMode = FilterMode.Point;
                t.hideFlags = HideFlags.HideAndDontSave;
                _sq = Sprite.Create(t, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
                _sq.hideFlags = HideFlags.HideAndDontSave;
            }
            return _sq;
        }
    }

    public static GameObject Box(string name, Vector2 pos, Vector2 size, Color color,
        bool collider = true, bool trigger = false, int order = 0, Transform parent = null)
    {
        var go = new GameObject(name);
        if (parent != null) go.transform.SetParent(parent);
        go.transform.position = pos;
        go.transform.localScale = new Vector3(size.x, size.y, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Square;
        sr.color = color;
        sr.sortingOrder = order;
        if (collider)
        {
            var bc = go.AddComponent<BoxCollider2D>();
            bc.size = Vector2.one;
            bc.isTrigger = trigger;
        }
        return go;
    }

    // ---------- Sons (fichiers dans Assets/Resources/Sounds) ----------
    static GameObject _music;

    // Joue un effet sonore court : Util.Sfx("saut")
    public static void Sfx(string name, float vol = 1f)
    {
        var clip = Resources.Load<AudioClip>("Sounds/" + name);
        if (clip == null) return;
        var go = new GameObject("Sfx_" + name);
        UnityEngine.Object.DontDestroyOnLoad(go);
        var src = go.AddComponent<AudioSource>();
        src.clip = clip;
        src.volume = vol;
        src.spatialBlend = 0f;
        src.Play();
        UnityEngine.Object.Destroy(go, clip.length + 0.2f);
    }

    // Musique de fond en boucle (une seule a la fois, continue entre les scenes)
    public static void Music(string name, float vol = 0.25f)
    {
        if (_music != null) return;
        var clip = Resources.Load<AudioClip>("Sounds/" + name);
        if (clip == null) return;
        _music = new GameObject("Musique");
        UnityEngine.Object.DontDestroyOnLoad(_music);
        var src = _music.AddComponent<AudioSource>();
        src.clip = clip;
        src.loop = true;
        src.volume = vol;
        src.spatialBlend = 0f;
        src.Play();
    }

    public static bool Any(KeyCode[] keys, bool down = false)
    {
        foreach (var k in keys)
            if (down ? Input.GetKeyDown(k) : Input.GetKey(k)) return true;
        return false;
    }

    // ---------- Interface ----------
    static Font _font;
    public static Font UIFont
    {
        get
        {
            if (_font == null) _font = Font.CreateDynamicFontFromOSFont("Arial", 32);
            return _font;
        }
    }

    public static Canvas MakeCanvas()
    {
        var go = new GameObject("Canvas");
        var c = go.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        var s = go.AddComponent<CanvasScaler>();
        s.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        s.referenceResolution = new Vector2(1920, 1080);
        s.matchWidthOrHeight = 0.5f;
        go.AddComponent<GraphicRaycaster>();
        if (EventSystem.current == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }
        return c;
    }

    public static Text MakeText(Transform parent, string txt, int size, Color col, TextAnchor align,
        Vector2 anchor, Vector2 pos, Vector2 sizeDelta)
    {
        var go = new GameObject("Text");
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<Text>();
        t.font = UIFont;
        t.text = txt;
        t.fontSize = size;
        t.color = col;
        t.alignment = align;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        var rt = t.rectTransform;
        rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = sizeDelta;
        return t;
    }

    public static Image MakePanel(Transform parent, Color c)
    {
        var go = new GameObject("Panel");
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = c;
        var rt = img.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return img;
    }

    public static Button MakeButton(Transform parent, string label, Vector2 pos, Vector2 size, UnityAction onClick)
    {
        var go = new GameObject("Btn_" + label);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = new Color(0.2f, 0.45f, 0.9f);
        var rt = img.rectTransform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        var b = go.AddComponent<Button>();
        b.targetGraphic = img;
        b.onClick.AddListener(onClick);
        b.onClick.AddListener(() => Sfx("clic", 0.6f));
        MakeText(go.transform, label, 42, Color.white, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), Vector2.zero, size);
        return b;
    }
}