using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIAnimation : MonoBehaviour
{
    private BossController boss;
    private Boss2Controller boss2;
    [SerializeField] private Player player;
    [SerializeField] private CinemachineVirtualCamera mainCamera;
    [SerializeField] private CinemachineConfiner2D confiner;
    [SerializeField] private TextMeshProUGUI bossName;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject optionMenu;
    [SerializeField] private GameObject gameOver;
    [SerializeField] private GameObject clearScreen;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        pauseMenu.SetActive(false);
        optionMenu.SetActive(false);
        gameOver.SetActive(false);
        if (clearScreen) clearScreen.SetActive(false);
    }

    private void OnEnable()
    {
        if (player != null)
        {
            player.OnGameOver.AddListener(ShowGameOver);
            player.OnHit.AddListener(OnPlayerHit);
        }
    }

    private void OnDisable()
    {
        if (player != null)
        {
            player.OnGameOver.RemoveListener(ShowGameOver);
            player.OnHit.RemoveListener(OnPlayerHit);
        }
    }

    private void OnPlayerHit() { }  // 필요 시 피격 UI 처리용

    public void OnBossSpawn()
    {
        var currScene = SceneManager.GetActiveScene().name;

        Time.timeScale = 0.01f;

        switch (currScene)
        {
            case "Boss1":
                boss = GameObject.FindWithTag("Boss").GetComponent<BossController>();
                bossName.text = boss.Data.bossName;
                mainCamera.m_Lens.OrthographicSize = 3;
                mainCamera.m_LookAt = boss.LookAtZone;
                mainCamera.m_Follow = boss.LookAtZone;
                mainCamera.ForceCameraPosition(new Vector3(boss.LookAtZone.position.x, boss.LookAtZone.position.y, mainCamera.transform.position.z), Quaternion.identity);
                confiner.InvalidateCache();
                animator.Play("BossSpawn");
                break;
            case "Boss2":
                boss2 = GameObject.FindWithTag("Boss").GetComponent<Boss2Controller>();
                bossName.text = boss2.Data.bossName;
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
        // 옵션창이 열려있으면 옵션창만 닫고 일시정지 유지
        if (optionMenu.activeSelf)
        {
            optionMenu.SetActive(false);
            return;
        }

        bool pausing = !pauseMenu.activeSelf;
        pauseMenu.SetActive(pausing);
        Time.timeScale = pausing ? 0f : 1f;
    }

    public void OnMain()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Title");
    }

    public void OnResume()
    {
        Time.timeScale = 1f;
        pauseMenu.SetActive(false);
    }

    public void OnOption()
    {
        optionMenu.SetActive(true);
    }

    public void ShowGameOver()
    {
        Time.timeScale = 0f;
        gameOver.SetActive(true);
    }

    public void ShowClear()
    {
        Time.timeScale = 0f;
        if (clearScreen) clearScreen.SetActive(true);
    }

    public void OnQuit()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void OnRetry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
