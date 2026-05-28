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
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

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
                Debug.Log(boss.LookAtZone.position);
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
}
