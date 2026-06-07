using System.IO;
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
    [SerializeField] private GameObject formatButton;
    [SerializeField] private AudioClip titleBgm;
    [SerializeField] private AudioClip buttonSFX;
    private InputAction escAction;

    private string path;

    private void Awake()
    {
        path = Application.persistentDataPath + "/keybindings.json";

        if (File.Exists(path))
            InputSystem.actions.LoadBindingOverridesFromJson(File.ReadAllText(path));

        Time.timeScale = 1f;
        option.SetActive(false);
        tutorial.SetActive(false);
        extra.SetActive(SaveManager.Data.isClear);
        InputSystem.actions.FindActionMap("UI").Enable();
        escAction = InputSystem.actions.FindAction("Cancel");
        SoundManager.Instance.PlayBGM(titleBgm);
        Cursor.visible = true;
    }

    private void Start()
    {
        SaveManager.Load();
    }

    private void OnEnable()
    {
        if (escAction != null)
            escAction.performed += OnEsc;
    }

    private void OnDisable()
    {
        if (escAction != null)
            escAction.performed -= OnEsc;
    }

    private void OnEsc(InputAction.CallbackContext _)
    {
        if (option.activeSelf)
        {
            if(formatButton != null) formatButton.SetActive(true);
            option.SetActive(false);
        }
        else if (extraPanel.activeSelf)
        {
            if(formatButton != null) formatButton.SetActive(true);
            extraPanel.SetActive(false);
        }
    }

    public void OnStart()
    {
        if (SaveManager.Data.isFirstPlay)
        {
            SoundManager.Instance.PlaySFX(buttonSFX);
            if(formatButton != null) formatButton.SetActive(false);
            tutorial.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(tutorial.GetComponent<RectTransform>());
            SaveManager.SetFirstPlayDone();
            return;
        }

        SoundManager.Instance.PlaySFX(buttonSFX);
        SoundManager.Instance.StopBGM();
        SceneManager.LoadScene("Boss1");
    }

    public void OnOptions()
    {
        SoundManager.Instance.PlaySFX(buttonSFX);
        if(formatButton != null) formatButton.SetActive(false);
        option.SetActive(true);
    }

    public void OnQuit()
    {
        SoundManager.Instance.PlaySFX(buttonSFX);
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void OnExtra()
    {
        SoundManager.Instance.PlaySFX(buttonSFX);
        if(formatButton != null) formatButton.SetActive(false);
        extraPanel.SetActive(true);
    }

    public void OnBoss1()
    {
        SoundManager.Instance.PlaySFX(buttonSFX);
        SceneManager.LoadScene("Boss1");
    }

    public void OnBoss2()
    {
        SoundManager.Instance.PlaySFX(buttonSFX);
        SceneManager.LoadScene("Boss2");
    }

    public void OnRemoveSave()
    {
        SoundManager.Instance.PlaySFX(buttonSFX);
        SaveManager.RemoveSave();
        extra.SetActive(SaveManager.Data.isClear);
    }
}
