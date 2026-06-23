using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField]
    private GameObject loginPanel;
    [SerializeField]
    private GameObject SaveRecordPanel;

    [Header("Login Form")]
    [SerializeField]
    private TMP_InputField emailInput;

    [SerializeField]
    private TMP_InputField passwordInput;

    [SerializeField]
    private Button loginButton;

    [SerializeField]
    private Button signupButton;

    [SerializeField]
    private Button anonymousButton;

    [SerializeField]
    private TextMeshProUGUI errorText;

    private void OnEnable()
    {
        if (!AuthManager.Instance.IsInitialized) return;

        UpdateUI().Forget();
    }

    private async UniTaskVoid Start()
    {
        await UniTask.WaitUntil(() => AuthManager.Instance.IsInitialized);

        loginButton.onClick.AddListener(() => OnLoginButtonClicked().Forget());
        signupButton.onClick.AddListener(() => OnSignupButtonClicked().Forget());
        anonymousButton.onClick.AddListener(() => OnAnonymousButtonClicked().Forget());
    }

    public async UniTaskVoid UpdateUI()
    {
        if (!AuthManager.Instance.IsInitialized)
            return;

        bool isLogedIn = AuthManager.Instance.IsLoginedIn;
        loginPanel.SetActive(!isLogedIn);
        SaveRecordPanel.SetActive(isLogedIn);
    }

    private async UniTaskVoid OnLoginButtonClicked()
    {
        string email = emailInput.text.Trim();
        string passwd = passwordInput.text;

        if(string.IsNullOrEmpty(email) || string.IsNullOrEmpty(passwd))
        {
            ShowError("이메일과 비밀번호를 입력하세요");
            return;
        }

        SetButtonsInteractable(false);
        var (success, error) = await AuthManager.Instance.SignInUserWithEmailAsync(email, passwd);
        if (success)
        {
            UpdateUI().Forget();
        }
        else
        {
            ShowError(error);
        }
        SetButtonsInteractable(true);
    }

    private async UniTaskVoid OnSignupButtonClicked()
    {
        string email = emailInput.text.Trim();
        string passwd = passwordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(passwd))
        {
            ShowError("이메일과 비밀번호를 입력하세요");
            return;
        }

        SetButtonsInteractable(false);
        var (success, error) = await AuthManager.Instance.CreateUserWithEmailAsync(email, passwd);
        if (success)
        {
            UpdateUI().Forget();
            
        }
        else
        {
            ShowError(error);
        }
        SetButtonsInteractable(true); ;
    }

    private async UniTaskVoid OnAnonymousButtonClicked()
    {
        SetButtonsInteractable(false);

        var (success, error) = await AuthManager.Instance.SignInAnonymouslyAsync();

        if (success)
        {
            UpdateUI().Forget();
        }
        else
        {
            ShowError(error);
        }
        SetButtonsInteractable(true);
    }

    private void ShowError(string message)
    {
        errorText.text = message;
        errorText.color = Color.red;
    }

    private void SetButtonsInteractable(bool interactable)
    {
        loginButton.interactable = interactable;
        signupButton.interactable = interactable;
        anonymousButton.interactable = interactable;
    }
}
