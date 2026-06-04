using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class OptionManager : MonoBehaviour
{
    [SerializeField] private GameObject volume;
    [SerializeField] private GameObject keySetting;
    [SerializeField] private Slider sfxVolume;
    [SerializeField] private Slider bgmVolume;

    private void Awake()
    {
        sfxVolume.value = SaveManager.Data.sfxVolume;
        bgmVolume.value = SaveManager.Data.bgmVolume;
        volume.SetActive(true);
        keySetting.SetActive(false);
    }

    public void OnLanguageChanged(int index)
    {
        var locales = LocalizationSettings.AvailableLocales.Locales;
        if (index < locales.Count)
            LocalizationSettings.SelectedLocale = locales[index];
        SaveManager.SetLanguage(index);
    }

    public void OnVolume()
    {
        volume.SetActive(true);
        keySetting.SetActive(false);
    }

    public void OnKeySetting()
    {
        volume.SetActive(false);
        keySetting.SetActive(true);
    }

    public void OnClose()
    {
        gameObject.SetActive(false);
    }

    public void OnSFXChange(float value)
    {
        SoundManager.Instance.SetSFXVolume(value);
        SaveManager.SetSFXVolume(value);
    }

    public void OnBGMChange(float value)
    {
        SoundManager.Instance.SetBGMVolume(value);
        SaveManager.SetBGMVolume(value);
    }
}
