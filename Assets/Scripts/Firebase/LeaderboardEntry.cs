using System;

[Serializable]
public class LeaderboardEntry
{
    public string nickname;
    public float cleartime;
    public long timestamp;

    public LeaderboardEntry()
    {
    }

    public LeaderboardEntry(string nickname, float cleartime, long timestamp)
    {
        this.nickname = nickname;
        this.cleartime = cleartime;
        this.timestamp = timestamp;
    }

    public string ToJson()
    {
        return UnityEngine.JsonUtility.ToJson(this);
    }

    public static LeaderboardEntry FromJson(string json)
    {
        return UnityEngine.JsonUtility.FromJson<LeaderboardEntry>(json);
    }
}
