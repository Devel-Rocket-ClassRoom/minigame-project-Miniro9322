using Cysharp.Threading.Tasks;
using Firebase.Database;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfileUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField]
    private GameObject saveRecordPanel;

    [Header("Profile Info")]
    [SerializeField]
    private TMP_InputField nicknameInput;

    [Header("Buttons")]
    [SerializeField]
    private Button saveButton;

    private DatabaseReference scoresRef;

    private async UniTaskVoid Start()
    {
        if (!await FirebaseInitializer.Instance.WaitForInitializationAsync())
        {
            Debug.Log("[Score] 파이어 베이스 초기화 X");
            return;
        }

        await UniTask.WaitUntil(() => AuthManager.Instance.IsInitialized);

        scoresRef = FirebaseInitializer.Instance.Database.RootReference.Child("record");

        Debug.Log("[Score] 초기화 완료");

        saveButton.onClick.AddListener(OnSaveButtonClicked);
    }

    private void OnSaveButtonClicked()
    {
        SaveInfo().Forget();
    }

    private async UniTaskVoid SaveInfo()
    {
        string nickname = string.IsNullOrEmpty(nicknameInput.text) ? AuthManager.Instance.UserId.Substring(0, 6) : nicknameInput.text;

        try
        {
            Debug.Log("[Record] 기록 저장 시도");
            DatabaseReference newHistoryRef = scoresRef.Push();
            Dictionary<string, object> scoreData = new()
            {
                {"nickname",  nickname},
                {"cleartime", TimeTracker.PlayTime },
                {"timestamp", ServerValue.Timestamp },
            };
            await newHistoryRef.UpdateChildrenAsync(scoreData);

            Debug.Log($"[Record] 기록 저장 성공");

            saveRecordPanel.SetActive(false);

            return;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Record] 기록 저장 실패 {ex.Message}");
            return;
        }
    }
}
