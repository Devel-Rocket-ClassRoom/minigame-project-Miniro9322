using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Title : MonoBehaviour
{
    [SerializeField] private GameObject option;
    [SerializeField] private GameObject tutorial;
    [SerializeField] private GameObject extra;
    [SerializeField] private GameObject extraPanel;

    private void Awake()
    {
        Time.timeScale = 1f;
        option.SetActive(false);
        tutorial.SetActive(false);
        extra.SetActive(SaveManager.Data.isClear);
        InputSystem.actions.FindActionMap("UI").Enable();
    }

    public void OnStart()
    {
        if (SaveManager.Data.isFirstPlay)
        {

            tutorial.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(tutorial.GetComponent<RectTransform>());
            SaveManager.SetFirstPlayDone();
            return;
        }

        SceneManager.LoadScene("Boss1");
    }

    public void OnOptions()
    {
        option.SetActive(true);
    }

    public void OnQuit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void OnExtra()
    {
        extraPanel.SetActive(true);
    }

    public void OnBoss1()
    {
        SceneManager.LoadScene("Boss1");
    }

    public void OnBoss2()
    {
        SceneManager.LoadScene("Boss2");
    }
}
