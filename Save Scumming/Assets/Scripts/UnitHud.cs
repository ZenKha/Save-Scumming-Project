using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UnitHud : MonoBehaviour
{
    [SerializeField] private Healthbar _healthbar;
    [SerializeField] private TMP_Text _accText;

    [SerializeField] private bool _keepActive = false;

    private UnitMovement _movement;

    private void Start()
    {
        _movement = GetComponent<UnitMovement>();
    }

    public void ShowUnitHud(int acc)
    {
        _healthbar.ShowHealthbar(true);
        _accText.text = acc + "%";
        _keepActive = true;
    }

    public void HideUnitHud()
    {
        _healthbar.ShowHealthbar(false);
        _accText.text = "";
        _keepActive = false;
    }

    private void OnMouseEnter()
    {
        _healthbar.ShowHealthbar(true);
        GridManager.instance.MapHover(_movement.GridX, _movement.GridY);
    }

    private void OnMouseExit()
    {
        if (!_keepActive)
        {
            _healthbar.ShowHealthbar(false);
        }

        GridManager.instance.MapHover(-1, -1);
    }

    private void OnMouseUp()
    {
        GridManager.instance.MapClick(_movement.GridX, _movement.GridY);
    }
}
