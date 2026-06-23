using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField]
    private Transform leaderboardListParent;

    [SerializeField]
    private GameObject leaderboardEntryPrefab;

    [SerializeField]
    private Button refreshButton;

    [SerializeField]
    private Button closeButton;

    [Header("Settings")]
    [SerializeField]
    private int topCount = 10;

    private async UniTaskVoid Start()
    {
        refreshButton.onClick.AddListener(() => LoadAndDisplayLeaderboardAsync().Forget());
        closeButton.onClick.AddListener(() => gameObject.SetActive(false));
    }

    private async UniTaskVoid OnEnable()
    {
        await UniTask.WaitUntil(() => LeaderBoardManager.Instance.isInstanced);
        LoadAndDisplayLeaderboardAsync().Forget();
    }

    private async UniTaskVoid LoadAndDisplayLeaderboardAsync()
    {
        var leaderboard = await LeaderBoardManager.Instance.LoaadLeaderboardAsync(topCount);
        DisplayLeaderboard(leaderboard);
    }

    private void DisplayLeaderboard(List<LeaderboardEntry> leaderboard)
    {
        foreach (Transform child in leaderboardListParent)
        {
            Destroy(child.gameObject);
        }

        int rank = 1;
        foreach (LeaderboardEntry entry in leaderboard)
        {
            GameObject item = Instantiate(leaderboardEntryPrefab, leaderboardListParent);

            TextMeshProUGUI[] texts = item.GetComponentsInChildren<TextMeshProUGUI>();

            if (texts.Length >= 3)
            {
                texts[0].text = $"{rank}";
                texts[1].text = entry.nickname;
                texts[2].text = $"Time: {(int)entry.cleartime / 60:d2} : {entry.cleartime % 60:f2}";
            }

            rank++;
        }

        Debug.Log($"[LeaderboardUI] 리더보드 표시 완료: {leaderboard.Count}명");
    }

    private void OnDestroy()
    {
        refreshButton.onClick.RemoveListener(() => LoadAndDisplayLeaderboardAsync().Forget());
        closeButton.onClick.RemoveListener(() => gameObject.SetActive(false));
    }
}
