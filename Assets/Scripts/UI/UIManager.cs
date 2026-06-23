using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private BossController boss;
    private Boss2Controller boss2;
    [SerializeField] private Player player;
    [SerializeField] private CinemachineVirtualCamera mainCamera;
    [SerializeField] private CinemachineConfiner2D confiner;
    [SerializeField] private TextMeshProUGUI bossName;
    [SerializeField] private TextMeshProUGUI clearTime;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject optionMenu;
    [SerializeField] private GameObject gameOver;
    [SerializeField] private GameObject clearScreen;
    [SerializeField] private AudioClip buttonSFX;
    [SerializeField] private GameObject SaveRecord;
    [SerializeField] private GameObject SaveRecordButton;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        pauseMenu.SetActive(false);
        optionMenu.SetActive(false);
        gameOver.SetActive(false);
        if (clearScreen) clearScreen.SetActive(false);
        Cursor.visible = false;
    }

    private void OnEnable()
    {
        if (player != null)
        {
            player.OnGameOver.AddListener(ShowGameOver);
        }
    }

    private void OnDisable()
    {
        if (player != null)
        {
            player.OnGameOver.RemoveListener(ShowGameOver);
        }
    }

    public void OnBossSpawn()
    {
        var currScene = SceneManager.GetActiveScene().name;

        Time.timeScale = 0.01f;

        switch (currScene)
        {
            case "Boss1":
                boss = GameObject.FindWithTag("Boss").GetComponent<BossController>();
                bossName.text = LocalizationSettings.StringDatabase.GetLocalizedString("New Table", "Boss1Name");
                mainCamera.m_Lens.OrthographicSize = 3;
                mainCamera.m_LookAt = boss.LookAtZone;
                mainCamera.m_Follow = boss.LookAtZone;
                mainCamera.ForceCameraPosition(new Vector3(boss.LookAtZone.position.x, boss.LookAtZone.position.y, mainCamera.transform.position.z), Quaternion.identity);
                confiner.InvalidateCache();
                animator.Play("BossSpawn");
                break;
            case "Boss2":
                boss2 = GameObject.FindWithTag("Boss").GetComponent<Boss2Controller>();
                bossName.text = LocalizationSettings.StringDatabase.GetLocalizedString("New Table", "Boss2Name");
                mainCamera.m_Lens.OrthographicSize = 3;
                mainCamera.m_LookAt = boss2.LookAtZone;
                mainCamera.m_Follow = boss2.LookAtZone;
                mainCamera.ForceCameraPosition(new Vector3(boss2.LookAtZone.position.x, boss2.LookAtZone.position.y, mainCamera.transform.position.z), Quaternion.identity);
                confiner.InvalidateCache();
                animator.Play("BossSpawn");
                break;
        }
    }

    public void ResetTime()
    {
        Time.timeScale = 1f;
        mainCamera.m_LookAt = player.transform;
        mainCamera.m_Follow = player.transform;
        mainCamera.m_Lens.OrthographicSize = 6;
        confiner.InvalidateCache();
    }

    public void OnPause()
    {
        if (optionMenu.activeSelf)
        {
            optionMenu.SetActive(false);
            EventSystem.current.SetSelectedGameObject(null);
            return;
        }
        bool pausing = !pauseMenu.activeSelf;
        Cursor.visible = pausing;
        pauseMenu.SetActive(pausing);
        Time.timeScale = pausing ? 0f : 1f;

        if (pausing)
        {
            InputSystem.actions.FindActionMap("Player").Disable();
            InputSystem.actions.FindAction("Pause").Enable();
        }
        else
        {
            InputSystem.actions.FindActionMap("Player").Enable();
        }
    }

    public void OnMain()
    {
        SoundManager.Instance.PlaySFX(buttonSFX);
        Time.timeScale = 1f;
        SceneManager.LoadScene("Title");
    }

    public void OnResume()
    {
        SoundManager.Instance.PlaySFX(buttonSFX);
        Time.timeScale = 1f;
        pauseMenu.SetActive(false);
    }

    public void OnOption()
    {
        SoundManager.Instance.PlaySFX(buttonSFX);
        optionMenu.SetActive(true);
    }

    public void ShowGameOver()
    {
        Cursor.visible = true;
        Time.timeScale = 0f;
        gameOver.SetActive(true);
        InputSystem.actions.FindActionMap("Player").Disable();
    }

    public void ShowClear()
    {
        Cursor.visible = true;
        Time.timeScale = 0f;
        SaveManager.SetClear();
        if (clearScreen) clearScreen.SetActive(true);
        if (GameManager.Instance.isExtra) SaveRecordButton.SetActive(false);
        clearTime.text = $"Clear Time: {TimeTracker.PlayTime:f2}";
        InputSystem.actions.FindActionMap("Player").Disable();
    }

    public void OnQuit()
    {
        SoundManager.Instance.PlaySFX(buttonSFX);
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void OnRetry()
    {
        SoundManager.Instance.PlaySFX(buttonSFX);
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnSaveRecord()
    {
        SaveRecord.SetActive(true);
    }
}
