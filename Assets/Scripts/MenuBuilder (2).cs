using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class MenuBuilder : MonoBehaviour
{
    const string GameTitle = "ENIGMA RUNNER";  
    GameObject quitPanel;
    Text quitTitle;
    GameObject yesBtn, noBtn;

    void Awake()
    {
        Time.timeScale = 1f;
        Util.Music("musique");
        var cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.1f, 0.15f, 0.3f);
        }

        var canvas = Util.MakeCanvas();
        var t = canvas.transform;
        var c = new Vector2(0.5f, 0.5f);

        var titleTxt = Util.MakeText(t, GameTitle, 130, new Color(1f, 0.85f, 0.2f), TextAnchor.MiddleCenter, c, new Vector2(0, 340), new Vector2(1600, 180));
        titleTxt.gameObject.AddComponent<Pulse>();   // animation du titre

        Util.MakeButton(t, "Jouer", new Vector2(0, 30), new Vector2(560, 120), Play);
        Util.MakeButton(t, "Quitter", new Vector2(0, -130), new Vector2(560, 120), () => quitPanel.SetActive(true));

        // ----- Panneau de confirmation pour quitter -----
        var q = Util.MakePanel(t, new Color(0, 0, 0, 0.88f));
        quitPanel = q.gameObject;
        quitTitle = Util.MakeText(q.transform, "Voulez-vous vraiment quitter le jeu ?", 64, Color.white,
            TextAnchor.MiddleCenter, c, new Vector2(0, 150), new Vector2(1500, 200));
        yesBtn = Util.MakeButton(q.transform, "Oui, quitter", new Vector2(-300, -80), new Vector2(520, 120), Quit).gameObject;
        noBtn = Util.MakeButton(q.transform, "Non, rester", new Vector2(300, -80), new Vector2(520, 120), () => quitPanel.SetActive(false)).gameObject;
        quitPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && !quitPanel.activeSelf) Play();
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            quitPanel.SetActive(false);
        }
    }

    void Quit() { StartCoroutine(QuitRoutine()); }

    IEnumerator QuitRoutine()
    {
        // Message d'au revoir, puis fermeture
        yesBtn.SetActive(false);
        noBtn.SetActive(false);
        quitTitle.text = "Merci d'avoir joue !\nA bientot !";
        yield return new WaitForSeconds(1.5f);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;   
#else
        Application.Quit();                                
#endif
    }

    void Play()
    {
        GameManager.Lives = GameManager.MaxLives;
        SceneManager.LoadScene("Level1");
    }
}