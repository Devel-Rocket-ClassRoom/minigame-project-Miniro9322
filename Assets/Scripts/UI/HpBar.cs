using UnityEngine;
using UnityEngine.UI;

public class HpBar : MonoBehaviour
{
    [SerializeField] private Slider hpBar;

    public void OnHpChanged(int hp, int maxhp)
    {
        hpBar.value = (float)hp / maxhp;
    }
}
