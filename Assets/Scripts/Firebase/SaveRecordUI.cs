using Cysharp.Threading.Tasks;
using Firebase.Database;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
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

    [SerializeField]
    private Button logoutButton;

    [Header("References")]
    [SerializeField]
    private LoginUI loginUI;

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
        logoutButton.onClick.AddListener(OnLogoutButtonClicked);
    }

    private void OnSaveButtonClicked()
    {
        SaveInfo().Forget();
    }

    private async UniTaskVoid SaveInfo()
    {
        if (!AuthManager.Instance.IsLoginedIn)
        {
            Debug.LogError($"로그인 필요");
            return;
        }

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
            Debug.LogError($"[Score] 점수 저장 실패 {ex.Message}");
            return;
        }
    }

    private void OnLogoutButtonClicked()
    {
        AuthManager.Instance.SignOut();
        saveRecordPanel.SetActive(false);

        loginUI.UpdateUI().Forget();
    }
}
