using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class OptionManager : MonoBehaviour
{
    [SerializeField] private GameObject volume;
    [SerializeField] private GameObject keySetting;

    private void Awake()
    {
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
}
