using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Healthbar : MonoBehaviour
{
    [SerializeField] private Image _background;
    [SerializeField] private Image _foreground;
    
    public void UpdateHealthBar(float maxHp, float hp)
    {
        _foreground.fillAmount = hp/maxHp;
    }

    public void ShowHealthbar(bool show)
    {
        _background.gameObject.SetActive(show);
    }
}
