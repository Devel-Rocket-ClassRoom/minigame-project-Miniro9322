using Cysharp.Threading.Tasks;
using Firebase.Database;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class LeaderBoardManager : MonoBehaviour
{
    private static LeaderBoardManager instance;
    public static LeaderBoardManager Instance => instance;

    private DatabaseReference leaderboardRef;

    public bool isInstanced = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnDestroy()
    {

    }

    private async UniTaskVoid Start()
    {
        if(!await FirebaseInitializer.Instance.WaitForInitializationAsync())
        {
            Debug.LogError("[Leaderboard] Firebase 초기화 실패");
            return;
        }

        leaderboardRef = FirebaseInitializer.Instance.Database.RootReference.Child("record");
        isInstanced = true;
    }

    public async UniTask<List<LeaderboardEntry>> LoaadLeaderboardAsync(int limit = 10)
    {
        if(leaderboardRef == null)
        {
            return new List<LeaderboardEntry>();
        }

        try
        {
            Debug.Log("[Leaderboard] 로드 시도");
            Query query = leaderboardRef.OrderByChild("cleartime").LimitToLast(limit);
            DataSnapshot snapshot = await query.GetValueAsync();
            var leaderboard = ParseEntries(snapshot);

            Debug.Log("[Leaderboard] 로드 성공");
            return leaderboard;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Leaderboard] 저장 실패: {ex.Message}");
            return new List<LeaderboardEntry>();
        }
    }

    public List<LeaderboardEntry> ParseEntries(DataSnapshot snapshot)
    {
        var list = new List<LeaderboardEntry>();

        if(snapshot != null)
        {
            foreach(var child in snapshot.Children)
            {
                list.Add(LeaderboardEntry.FromJson(child.GetRawJsonValue()));
            }
        }

        list.Sort((a, b) => a.cleartime.CompareTo(b.cleartime));

        return list;
    }
}
