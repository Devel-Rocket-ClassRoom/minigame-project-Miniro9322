using System.IO;
using UnityEngine;
using UnityEngine.Localization.Settings;

public static class SaveManager
{
    [System.Serializable]
    public class SaveData
    {
        public float bgmVolume = 1f;
        public float sfxVolume = 1f;
        public int languageIndex = 0;
        public bool isFirstPlay = true;
        public bool isClear = false;
    }

    private static string Path => Application.persistentDataPath + "/settings.json";

    private static SaveData _data;
    public static SaveData Data
    {
        get
        {
            if (_data == null) Load();
            return _data;
        }
    }

    public static void Save()
    {
        File.WriteAllText(Path, JsonUtility.ToJson(_data, true));
    }

    public static void Load()
    {
        if (File.Exists(Path))
            _data = JsonUtility.FromJson<SaveData>(File.ReadAllText(Path));
        else
            _data = new SaveData();

        Apply();
    }

    private static void Apply()
    {
        var locales = LocalizationSettings.AvailableLocales.Locales;
        if (_data.languageIndex < locales.Count)
            LocalizationSettings.SelectedLocale = locales[_data.languageIndex];
    }

    public static void SetBGMVolume(float volume)
    {
        _data.bgmVolume = volume;
        Save();
    }

    public static void SetSFXVolume(float volume)
    {
        _data.sfxVolume = volume;
        Save();
    }

    public static void SetLanguage(int index)
    {
        _data.languageIndex = index;
        var locales = LocalizationSettings.AvailableLocales.Locales;
        if (index < locales.Count)
            LocalizationSettings.SelectedLocale = locales[index];
        Save();
    }

    public static void SetFirstPlayDone()
    {
        _data.isFirstPlay = false;
        Save();
    }

    public static void SetClear()
    {
        _data.isClear = true;
        Save();
    }
}
