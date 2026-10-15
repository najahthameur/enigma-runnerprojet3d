using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public KeyCode[] left, right, jump;
    public float speed = 5f;
    public float jumpSpeed = 11f;
    public Vector2 spawn;

    Rigidbody2D rb;
    bool grounded, jumpRequested;
    float h;

    Vector2 Vel
    {
#if UNITY_6000_0_OR_NEWER
        get { return rb.linearVelocity; }
        set { rb.linearVelocity = value; }
#else
        get { return rb.velocity; }
        set { rb.velocity = value; }
#endif
    }

    void Awake() { rb = GetComponent<Rigidbody2D>(); }

    void Update()
    {
        h = 0f;
        if (GameManager.Instance != null && GameManager.Instance.Playing)
        {
            if (Util.Any(left)) h -= 1f;
            if (Util.Any(right)) h += 1f;
            if (Util.Any(jump, true)) jumpRequested = true;
        }
    }

    void FixedUpdate()
    {
        Vector2 v = Vel;
        v.x = h * speed;
        if (jumpRequested && grounded) { v.y = jumpSpeed; Util.Sfx("saut", 0.6f); }
        jumpRequested = false;
        grounded = false;

        // Les joueurs ne peuvent pas sortir de l'ecran (jeu cooperatif)
        var cam = Camera.main;
        if (cam != null)
        {
            float half = cam.orthographicSize * cam.aspect;
            float minX = cam.transform.position.x - half + 0.5f;
            float maxX = cam.transform.position.x + half - 0.5f;
            Vector2 p = rb.position;
            if (p.x < minX) { p.x = minX; if (v.x < 0) v.x = 0; rb.position = p; }
            if (p.x > maxX) { p.x = maxX; if (v.x > 0) v.x = 0; rb.position = p; }
        }
        Vel = v;

        if (rb.position.y < -7f && GameManager.Instance != null)
            GameManager.Instance.KillPlayer(this);
    }

    void OnCollisionStay2D(Collision2D c)
    {
        foreach (var contact in c.contacts)
            if (contact.normal.y > 0.5f) { grounded = true; break; }
    }

    public void Respawn()
    {
        rb.position = spawn;
        transform.position = spawn;
        Vel = Vector2.zero;
    }
}