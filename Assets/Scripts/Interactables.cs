using UnityEngine;
using System.Collections.Generic;

// Piques : le joueur perd une vie et revient au depart.
public class Hazard : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D o)
    {
        var p = o.GetComponent<PlayerController>();
        if (p != null) GameManager.Instance.KillPlayer(p);
    }
}

// Cle a ramasser.
public class KeyPickup : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D o)
    {
        if (o.GetComponent<PlayerController>() == null) return;
        GameManager.Instance.KeyCollected();
        Destroy(gameObject);
    }
}

// Porte : s'ouvre avec la cle, il faut que les 2 joueurs soient dedans.
public class Door : MonoBehaviour
{
    int inside;
    SpriteRenderer sr;
    static readonly Color locked = new Color(0.45f, 0.25f, 0.1f);
    static readonly Color open = new Color(0.2f, 0.85f, 0.35f);

    void Awake() { sr = GetComponent<SpriteRenderer>(); }

    void OnTriggerEnter2D(Collider2D o)
    {
        if (o.GetComponent<PlayerController>() == null) return;
        inside++;
        if (!GameManager.Instance.HasKey)
            GameManager.Instance.ShowMessage("La porte est verrouillee : il faut la cle !");
    }

    void OnTriggerExit2D(Collider2D o)
    {
        if (o.GetComponent<PlayerController>() != null) inside = Mathf.Max(0, inside - 1);
    }

    void Update()
    {
        var gm = GameManager.Instance;
        sr.color = gm.HasKey ? open : locked;
        if (gm.Playing && gm.HasKey && inside >= 1) gm.LevelComplete();
    }
}

// Grille ouverte tant qu'au moins une plaque est pressee.
public class Gate : MonoBehaviour
{
    int pressed;
    BoxCollider2D col;
    SpriteRenderer sr;
    Color closedColor;

    void Awake()
    {
        col = GetComponent<BoxCollider2D>();
        sr = GetComponent<SpriteRenderer>();
        closedColor = sr.color;
    }

    public void Add(int d)
    {
        pressed = Mathf.Max(0, pressed + d);
        bool open = pressed > 0;
        col.enabled = !open;
        sr.color = open ? new Color(closedColor.r, closedColor.g, closedColor.b, 0.2f) : closedColor;
        if (d > 0 && pressed == 1) GameManager.Instance.ShowMessage("La grille s'ouvre !");
    }
}

public class Plate : MonoBehaviour
{
    public Gate gate;
    public bool permanent;   // la grille reste ouverte apres le passage
    int count;

    void OnTriggerEnter2D(Collider2D o)
    {
        if (o.GetComponent<PlayerController>() == null) return;
        count++;
        if (count == 1) gate.Add(1);
    }

    void OnTriggerExit2D(Collider2D o)
    {
        if (o.GetComponent<PlayerController>() == null) return;
        if (permanent) return;
        count = Mathf.Max(0, count - 1);
        if (count == 0) gate.Add(-1);
    }
}

// Enigme des couleurs : appuyer dans le bon ordre.
public class ColorPuzzle : MonoBehaviour
{
    public int[] solution = { 2, 1, 0 };   // 0 = rouge, 1 = vert, 2 = bleu  -> ciel, herbe, sang
    public GameObject barrier;
    int progress;
    bool solved;

    public void Press(int id)
    {
        if (solved) return;
        if (id == solution[progress])
        {
            progress++;
            if (progress >= solution.Length)
            {
                solved = true;
                if (barrier != null) barrier.SetActive(false);
                GameManager.Instance.ShowMessage("Bravo ! La barriere violette disparait !");
                Util.Sfx("victoire");
            }
            else { GameManager.Instance.ShowMessage("Bien ! " + progress + " / " + solution.Length); Util.Sfx("cle"); }
        }
        else
        {
            progress = (id == solution[0]) ? 1 : 0;
            GameManager.Instance.ShowMessage("Mauvais ordre ! Reflechissez a l'enigme...");
            Util.Sfx("erreur");
        }
    }
}

public class ColorButton : MonoBehaviour
{
    public int id;
    public ColorPuzzle puzzle;

    void OnTriggerEnter2D(Collider2D o)
    {
        if (o.GetComponent<PlayerController>() != null) puzzle.Press(id);
    }
}

// Petites animations faites par code (aucun fichier .anim necessaire).

// La cle monte et descend doucement.
public class Bob : MonoBehaviour
{
    public float height = 0.15f;
    public float speed = 3f;
    Vector3 start;

    void Start() { start = transform.position; }

    void Update()
    {
        transform.position = start + Vector3.up * Mathf.Sin(Time.time * speed) * height;
    }
}

// Les yeux du joueur clignent de temps en temps.
public class Blink : MonoBehaviour
{
    float next;
    Vector3 open;

    void Start()
    {
        open = transform.localScale;
        next = Time.time + Random.Range(2f, 4f);
    }

    void Update()
    {
        if (Time.time < next) return;
        if (Time.time - next < 0.12f)
            transform.localScale = new Vector3(open.x, open.y * 0.15f, open.z);   // yeux fermes
        else
        {
            transform.localScale = open;                                          // yeux ouverts
            next = Time.time + Random.Range(2f, 4f);
        }
    }
}

// Le titre du menu "respire" (grossit et rapetisse un peu).
public class Pulse : MonoBehaviour
{
    public float amount = 0.05f;
    public float speed = 2f;

    void Update()
    {
        float s = 1f + Mathf.Sin(Time.time * speed) * amount;
        transform.localScale = new Vector3(s, s, 1f);
    }
}

// Le pique clignote (rouge <-> orange) pour avertir le joueur.
public class Flash : MonoBehaviour
{
    public float speed = 4f;
    SpriteRenderer sr;
    Color a = new Color(0.9f, 0.1f, 0.1f);
    Color b = new Color(1f, 0.5f, 0.1f);

    void Start() { sr = GetComponent<SpriteRenderer>(); }

    void Update()
    {
        sr.color = Color.Lerp(a, b, (Mathf.Sin(Time.time * speed) + 1f) / 2f);
    }
}

// L'obstacle va et vient : gauche <-> droite autour de sa position de depart.
public class Patrol : MonoBehaviour
{
    public float range = 2f;    // distance parcourue de chaque cote (en unites)
    public float speed = 1.5f;  // vitesse du va-et-vient
    Rigidbody2D rb;
    Vector2 start;

    void Start()
    {
        start = transform.position;
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;   // se deplace sans etre pousse par la gravite
    }

    void FixedUpdate()
    {
        float x = start.x + Mathf.Sin(Time.time * speed) * range;
        rb.MovePosition(new Vector2(x, start.y));
    }
}