using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Animator))]
public class Spawner : MonoBehaviour
{
    private static readonly int SpawnEffect2Hash = Animator.StringToHash("SpawnEffect2");
    private static readonly int SpawnEffectHash = Animator.StringToHash("SpawnEffect");
    [SerializeField] private GameObject boss;
    private Animator animator;
    [SerializeField] private GameObject interactUI;
    [SerializeField] private Transform spawnPoint;
    private bool isInteracted;
    private bool playerInRange = false;
    private PlayerInput playerInput;

    public UnityEvent BossSpawn;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        isInteracted = false;
    }

    private void Update()
    {
        if (playerInRange && !isInteracted && playerInput != null)
        {
            if (playerInput.actions["Interact"].WasPressedThisFrame())
            {
                Spawn();
                interactUI.SetActive(false);
                isInteracted = true;
            }
        }
    }

    private void SpawnEnd()
    {
        boss.transform.position = spawnPoint.position;
        boss.SetActive(true);
        BossSpawn?.Invoke();
        Destroy(gameObject);
    }

    private void Spawn()
    {
        var currScene = SceneManager.GetActiveScene().name;
        switch (currScene)
        {
            case "Boss1":
                animator.Play(SpawnEffectHash);
                break;
            case "Boss2":
                animator.Play(SpawnEffect2Hash);
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
            playerInput = collision.GetComponent<PlayerInput>();
            interactUI.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            interactUI.SetActive(false);
        }
    }
}
