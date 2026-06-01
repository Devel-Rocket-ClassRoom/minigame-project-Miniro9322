using UnityEngine;

public class OptionManager : MonoBehaviour
{
    [SerializeField] private GameObject volume;
    [SerializeField] private GameObject keySetting;

    private void Awake()
    {
        volume.SetActive(true);
        keySetting.SetActive(false);
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
