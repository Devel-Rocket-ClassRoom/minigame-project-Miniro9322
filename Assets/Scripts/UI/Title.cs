using UnityEngine;
using UnityEngine.SceneManagement;

public class Title : MonoBehaviour
{
    public void OnStart()
    {
        SceneManager.LoadScene("Boss1");
    }

    public void OnQuit()
    {
        Application.Quit();
    }
}
