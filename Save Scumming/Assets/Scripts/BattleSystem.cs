using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum BattleState { START, PLACE, PLAYERTURN, ENEMYTURN, WON, LOST}

public class BattleSystem : MonoBehaviour
{
    [SerializeField] private BattleState _state;
    [SerializeField] private GridManager _gridManager;

    [Space(10)] [Header("Characters")]
    [SerializeField] List<GameObject> _characters;
    [SerializeField] List<GameObject> _enemies;

    [Space(10)] [Header("UI")]
    public TMP_Text startText;
    public TMP_Text turnText;
    public Button endTurnButton;


    void Start()
    {
        _state = BattleState.START;
        StartCoroutine(StartBattle());
    }

    IEnumerator StartBattle()
    {
        yield return StartCoroutine(_gridManager.CreateMap());

        //Place Enemies on the board
        PlaceEnemies(_enemies);

        //Battle Start UI animation
        yield return StartCoroutine(ShowText("Start"));

        //Highlight start area
        _gridManager.HighlightStartArea();
        StartPlaceState();
    }

    void PlaceEnemies(List<GameObject> enemies)
    {
        foreach (GameObject enemy in enemies)
        {
            _gridManager.PlaceEnemyCharacter(enemy);
        }
    }

    public void StartPlaceState()
    {
        _state = BattleState.PLACE;
        endTurnButton.gameObject.SetActive(true);
        turnText.text = "Placement Phase";
        _gridManager.AddCharacters(_characters);
    }

    public void StartPlayerTurn()
    {
        _state = BattleState.PLAYERTURN;
        turnText.text = "Player Turn";
        
        // Give action token to player units
        foreach (GameObject playerUnit in _gridManager.Characters)
        {
            playerUnit.GetComponent<PlayerUnitBehaviour>().GiveTurnTokens();
        }

        _gridManager.ResetTurn();
        endTurnButton.interactable = true;
    }

    public void EndTurn()
    {
        StartCoroutine(StartEnemyTurn());
        endTurnButton.interactable = false;
    }

    public IEnumerator StartEnemyTurn()
    {
        _state = BattleState.ENEMYTURN;
        turnText.text = "Enemy Turn";

        // Enemies do stuff
        foreach (GameObject enemy in _gridManager.Enemies)
        {
            var map = _gridManager.GenerateArrayWithUnits();
            Vector2Int start = enemy.GetComponent<UnitMovement>().GetGridCoordinates();
            Vector2Int end = enemy.GetComponent<EnemyBehaviourEngine>().GenerateEnemyTurn(map);
            var path = PathFinder.CalculateShortestPath(map, start, end);
            _gridManager.UpdateUnitPosition(enemy, end);
            yield return StartCoroutine(_gridManager.MoveCharacter(enemy, path));
        }

        StartPlayerTurn();
    }

    public BattleState GetBattleState()
    {
        return _state;
    }

    IEnumerator ShowText(string message)
    {
        startText.text = message;
        startText.enabled = true;
        yield return new WaitForSeconds(.5f);
        startText.enabled = false;
    }
}