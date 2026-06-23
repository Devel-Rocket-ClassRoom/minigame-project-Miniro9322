using UnityEngine;

public class TimeTracker : MonoBehaviour
{
    private static TimeTracker insctance;
    public static TimeTracker Instance => insctance;

    public static float PlayTime { get; private set; }
    private bool isTimer = false;

    private void Awake()
    {
        if(insctance == null)
        {
            insctance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void StartTimer()
    {
        PlayTime = 0f;
        isTimer = true;
    }

    public void EndTimer()
    {
        isTimer = false;
    }

    private void Update()
    {
        if (isTimer)
        {
            PlayTime += Time.deltaTime;
        }
    }


}
