using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionWall : MonoBehaviour
{
    [SerializeField] private string nextScene;
    [SerializeField] private GameObject arrowUI;

    private Collider2D col;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        col.isTrigger = false;
        if (arrowUI) arrowUI.SetActive(false);
    }

    public void Activate()
    {
        col.isTrigger = true;
        if (arrowUI) arrowUI.SetActive(true);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SoundManager.Instance.StopBGM();
            SceneManager.LoadScene(nextScene);
        }
    }
}
