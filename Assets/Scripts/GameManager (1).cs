using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static int Lives = 3;
    public const int MaxLives = 3;
    public const int TotalLevels = 2;

    public bool Playing = true;
    public bool HasKey;
    int level;
    Text livesTxt, msgTxt;
    float msgTimer;
    GameObject winPanel, overPanel, finalPanel;

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
        if (Lives <= 0) Lives = MaxLives;
    }

    public void Init(int lvl, string title, string hint)
    {
        level = lvl;
        Util.Music("musique");
        var canvas = Util.MakeCanvas();
        var t = canvas.transform;

        Color hudCol = (lvl == 2) ? Color.white : Color.black;   // lisible sur fond sombre
        Util.MakeText(t, "Niveau " + lvl, 38, hudCol,
            TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(30, -25), new Vector2(1350, 160));
        livesTxt = Util.MakeText(t, "", 44, new Color(0.8f, 0.05f, 0.05f),
            TextAnchor.UpperRight, new Vector2(1, 1), new Vector2(-30, -25), new Vector2(400, 80));
        msgTxt = Util.MakeText(t, "", 44, new Color(1f, 0.9f, 0.1f),
            TextAnchor.MiddleCenter, new Vector2(0.5f, 0), new Vector2(0, 110), new Vector2(1500, 90));

        // Ecran Niveau reussi
        winPanel = EndPanel(t, "REUSSI !", new Color(0.3f, 1f, 0.4f), "Bravo, vous avez reussi ce niveau !");
        Util.MakeButton(winPanel.transform, "Niveau suivant", new Vector2(0, -60), new Vector2(520, 110), NextLevel);
        Util.MakeButton(winPanel.transform, "Menu", new Vector2(0, -200), new Vector2(520, 110), ToMenu);
        winPanel.SetActive(false);

        // Ecran Perdu
        overPanel = EndPanel(t, "GAME OVER", new Color(1f, 0.3f, 0.3f), "");
        Util.MakeButton(overPanel.transform, "Reessayer", new Vector2(0, -60), new Vector2(520, 110), Retry);
        Util.MakeButton(overPanel.transform, "Menu", new Vector2(0, -200), new Vector2(520, 110), ToMenu);
        overPanel.SetActive(false);

        // Ecran Victoire finale
        finalPanel = EndPanel(t, "VICTOIRE !", new Color(1f, 0.85f, 0.2f), "Bravo ! Vous avez reussi tous les niveaux !");
        Util.MakeButton(finalPanel.transform, "Rejouer", new Vector2(0, -60), new Vector2(520, 110), Restart);
        Util.MakeButton(finalPanel.transform, "Menu", new Vector2(0, -200), new Vector2(520, 110), ToMenu);
        finalPanel.SetActive(false);

        RefreshLives();
    }

    GameObject EndPanel(Transform parent, string title, Color col, string sub)
    {
        var p = Util.MakePanel(parent, new Color(0, 0, 0, 0.75f));
        Util.MakeText(p.transform, title, 110, col, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0, 230), new Vector2(1500, 160));
        Util.MakeText(p.transform, sub, 44, Color.white, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0, 110), new Vector2(1500, 90));
        return p.gameObject;
    }

    void RefreshLives() { livesTxt.text = "Vies : " + Lives; }

    public void ShowMessage(string m, float seconds = 2.5f)
    {
        if (msgTxt == null) return;
        msgTxt.text = m;
        msgTimer = seconds;
    }

    void Update()
    {
        if (msgTimer > 0f)
        {
            msgTimer -= Time.deltaTime;
            if (msgTimer <= 0f) msgTxt.text = "";
        }
        if (Input.GetKeyDown(KeyCode.R)) Retry();
        if (Input.GetKeyDown(KeyCode.Escape)) ToMenu();
    }

    public void KillPlayer(PlayerController p)
    {
        if (!Playing) return;
        Lives--;
        RefreshLives();
        if (Lives <= 0)
        {
            Playing = false;
            Util.Sfx("gameover");
            overPanel.SetActive(true);
        }
        else
        {
            p.Respawn();
            Util.Sfx("perte");
            ShowMessage("Aie ! Vies restantes : " + Lives);
        }
    }

    public void KeyCollected()
    {
        HasKey = true;
        Util.Sfx("cle");
        ShowMessage("Cle recuperee ! Allez a la porte.");
    }

    public void LevelComplete()
    {
        if (!Playing) return;
        Playing = false;
        Util.Sfx("victoire");
        if (level >= TotalLevels) finalPanel.SetActive(true);
        else winPanel.SetActive(true);
    }

    void NextLevel() { SceneManager.LoadScene("Level" + (level + 1)); }
    void Retry() { Lives = MaxLives; SceneManager.LoadScene("Level" + level); }
    void Restart() { Lives = MaxLives; SceneManager.LoadScene("Level1"); }
    void ToMenu() { SceneManager.LoadScene("Menu"); }
}