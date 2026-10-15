using UnityEngine;

// Construit un niveau entier par code (aucun asset necessaire).
public class LevelBuilder : MonoBehaviour
{
    public int level = 1;

    Transform root;
    PlayerController p1;
    float minX, maxX;
    PhysicsMaterial2D noFriction;

    // Theme visuel (change selon le niveau)
    Color bgColor, groundCol, grassCol, floatCol;

    bool preview;   // true = apercu dans l'editeur (objets visibles, sans logique de jeu)

    void SetTheme()
    {
        if (level == 1)
        {
            bgColor = new Color(0.55f, 0.78f, 0.95f);
            groundCol = new Color(0.30f, 0.22f, 0.15f);
            grassCol = new Color(0.25f, 0.6f, 0.25f);
            floatCol = new Color(0.45f, 0.33f, 0.22f);
        }
        else
        {
            bgColor = new Color(0.22f, 0.17f, 0.40f);   // ciel du soir
            groundCol = new Color(0.25f, 0.20f, 0.45f);
            grassCol = new Color(0.55f, 0.85f, 0.95f);
            floatCol = new Color(0.40f, 0.55f, 0.85f);
        }
    }

    // Appelee par l'outil de l'editeur : construit le niveau en objets visibles dans la scene
    public void BuildPreview(int lvl)
    {
        preview = true;
        level = lvl;
        root = new GameObject("World_Niveau" + lvl).transform;
        SetTheme();
        if (level == 1) BuildLevel1(); else BuildLevel2();
        SpawnPlayers();

        var cam = Camera.main;
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = 14f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = bgColor;
            cam.transform.position = new Vector3((minX + maxX) / 2f, 4f, -10f);
        }
    }

    void Awake()
    {
        Time.timeScale = 1f;
        root = new GameObject("World").transform;
        noFriction = new PhysicsMaterial2D("NoFriction") { friction = 0f, bounciness = 0f };
        var gm = gameObject.AddComponent<GameManager>();
        GameManager.Instance = gm;

        SetTheme();
        if (level == 1) BuildLevel1(); else BuildLevel2();
        SpawnPlayers();
        SetupCamera();
        gm.Init(level, "", "");
    }

    // ---------- outils ----------
    GameObject Plat(float left, float top, float w, float thick = 0.5f, bool ground = false)
    {
        var g = Util.Box("Platform", new Vector2(left + w / 2f, top - thick / 2f), new Vector2(w, thick),
            ground ? groundCol : floatCol, true, false, 0, root);
        if (ground || level == 2)
            Util.Box("Top", new Vector2(left + w / 2f, top - 0.1f), new Vector2(w, 0.2f), grassCol, false, false, 1, root);
        return g;
    }

    void Spike(float x, float baseY, float range = 0f)
    {
        var s = Util.Box("Spike", new Vector2(x, baseY + 0.25f), new Vector2(1f, 0.5f),
            new Color(0.9f, 0.1f, 0.1f), true, true, 2, root);
        if (!preview)
        {
            s.AddComponent<Hazard>();
            s.AddComponent<Flash>();
            if (range > 0f) s.AddComponent<Patrol>().range = range;   // obstacle mobile
        }
    }

    GameObject KeyAt(float x, float y)
    {
        var k = Util.Box("Key", new Vector2(x, y), new Vector2(0.5f, 0.5f), new Color(1f, 0.85f, 0.1f), true, true, 3, root);
        if (!preview) { k.AddComponent<KeyPickup>(); k.AddComponent<Bob>(); }
        return k;
    }

    void DoorAt(float x, float groundTop)
    {
        var d = Util.Box("Door", new Vector2(x, groundTop + 1f), new Vector2(1.3f, 2f), new Color(0.45f, 0.25f, 0.1f), true, true, 1, root);
        if (!preview) d.AddComponent<Door>();
    }

    void Walls(float width)
    {
        Util.Box("WallL", new Vector2(-0.5f, 5f), new Vector2(1f, 30f), Color.clear, true, false, 0, root);
        Util.Box("WallR", new Vector2(width + 0.5f, 5f), new Vector2(1f, 30f), Color.clear, true, false, 0, root);
        minX = 0f; maxX = width;
    }

    // ---------- Niveau 1 : la cle partagee ----------
    void BuildLevel1()
    {
        Walls(46f);
        Plat(0, 0, 12, 4, true);
        Plat(14.5f, 0, 8, 4, true);
        Spike(18.5f, 0, 2f);
        Plat(24, 1.5f, 3);
        Plat(28.5f, 3, 3);
        KeyAt(30f, 3.6f);
        Plat(33, 1.5f, 3);
        Plat(37, 0, 9, 4, true);
        Spike(40f, 0, 2f);
        DoorAt(43f, 0);
    }

    // ---------- Niveau 2 : les iles flottantes ----------
    void BuildLevel2()
    {
        Walls(55f);

        Plat(0, 0, 6, 4, true);          // depart
        Plat(8, 1.5f, 3, 1f);            // ile 1
        Plat(13, 3, 3, 1f);              // ile 2

        // Grande ile avec l'enigme des couleurs (sans texte)
        Plat(18, 4.5f, 12, 1f);
        MakeColorButton(20f, 4.5f, 0, new Color(0.9f, 0.1f, 0.1f));   // rouge
        MakeColorButton(22.5f, 4.5f, 1, new Color(0.1f, 0.8f, 0.2f)); // vert
        MakeColorButton(25f, 4.5f, 2, new Color(0.15f, 0.4f, 1f));    // bleu
        Spike(26.6f, 4.5f, 0.6f);

        // Indice visuel : l'ordre des couleurs a suivre (gauche -> droite)
        Util.Box("ClueBoard", new Vector2(23.5f, 7.3f), new Vector2(4.4f, 1.6f), new Color(0.05f, 0.05f, 0.12f), false, false, 1, root);
        Util.Box("Clue1", new Vector2(22f, 7.3f), new Vector2(0.9f, 0.9f), new Color(0.15f, 0.4f, 1f), false, false, 2, root);
        Util.Box("Clue2", new Vector2(23.5f, 7.3f), new Vector2(0.9f, 0.9f), new Color(0.1f, 0.8f, 0.2f), false, false, 2, root);
        Util.Box("Clue3", new Vector2(25f, 7.3f), new Vector2(0.9f, 0.9f), new Color(0.9f, 0.1f, 0.1f), false, false, 2, root);

        // Barriere violette (disparait quand l'enigme est resolue)
        var barrier = Util.Box("Barrier", new Vector2(28f, 7f), new Vector2(0.6f, 5f), new Color(0.75f, 0.3f, 0.95f), true, false, 2, root);
        if (!preview)
        {
            var puzzleObj = new GameObject("Puzzle");
            puzzleObj.transform.SetParent(root);
            puzzle = puzzleObj.AddComponent<ColorPuzzle>();
            puzzle.barrier = barrier;
            foreach (var cb in FindButtons()) cb.puzzle = puzzle;
        }

        Plat(32, 3, 3, 1f);              // ile 4
        Plat(37, 4.5f, 3, 1f);           // ile de la cle
        KeyAt(38.5f, 5.1f);
        Plat(42, 3, 3, 1f);              // ile 6
        Plat(47, 1.5f, 8, 4, true);      // arrivee
        DoorAt(51f, 1.5f);
    }

    ColorPuzzle puzzle;
    System.Collections.Generic.List<ColorButton> buttons = new System.Collections.Generic.List<ColorButton>();
    System.Collections.Generic.List<ColorButton> FindButtons() { return buttons; }

    void MakeColorButton(float x, float top, int id, Color c)
    {
        var b = Util.Box("ColorButton" + id, new Vector2(x, top + 0.15f), new Vector2(1.2f, 0.3f), c, true, true, 2, root);
        if (preview) return;
        var cb = b.AddComponent<ColorButton>();
        cb.id = id;
        buttons.Add(cb);
    }

    // ---------- Joueur et camera ----------
    void SpawnPlayers()
    {
        p1 = MakePlayer("Joueur", new Color(0.2f, 0.4f, 1f), new Vector2(1.5f, 0.6f),
            new[] { KeyCode.LeftArrow, KeyCode.A, KeyCode.Q },
            new[] { KeyCode.RightArrow, KeyCode.D },
            new[] { KeyCode.UpArrow, KeyCode.W, KeyCode.Z, KeyCode.Space });
    }

    PlayerController MakePlayer(string n, Color c, Vector2 pos, KeyCode[] l, KeyCode[] r, KeyCode[] j)
    {
        var go = Util.Box(n, pos, new Vector2(0.8f, 0.8f), c, !preview, false, 5, root);
        foreach (float ex in new[] { -0.2f, 0.2f })
        {
            var e = Util.Box("Eye", Vector2.zero, new Vector2(0.22f, 0.22f), Color.white, false, false, 6, go.transform);
            e.transform.localPosition = new Vector3(ex, 0.15f, 0f);
            if (!preview) e.AddComponent<Blink>();
        }
        if (preview) return null;

        go.GetComponent<BoxCollider2D>().sharedMaterial = noFriction;
        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 3f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        var pc = go.AddComponent<PlayerController>();
        pc.left = l; pc.right = r; pc.jump = j; pc.spawn = pos;
        return pc;
    }

    void SetupCamera()
    {
        var cam = Camera.main;
        if (cam == null)
        {
            var g = new GameObject("Main Camera");
            g.tag = "MainCamera";
            cam = g.AddComponent<Camera>();
            g.AddComponent<AudioListener>();
        }
        cam.orthographic = true;
        cam.orthographicSize = 6.5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = bgColor;
        var cf = cam.gameObject.GetComponent<CameraFollow>();
        if (cf == null) cf = cam.gameObject.AddComponent<CameraFollow>();
        cf.a = p1.transform; cf.b = p1.transform;
        cf.minX = minX; cf.maxX = maxX; cf.y = 3.5f;
        cf.Snap();
    }
}