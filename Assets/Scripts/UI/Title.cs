using UnityEngine;
using UnityEngine.SceneManagement;

public class Title : MonoBehaviour
{
    [SerializeField]
    private GameObject option;

    private void Awake()
    {
        Time.timeScale = 1f;
        option.SetActive(false);
    }

    public void OnStart()
    {
        SceneManager.LoadScene("Boss1");
    }

    public void OnOptions()
    {
        option.SetActive(true);
    }

    public void OnQuit()
    {
        Application.Quit();
    }
}
