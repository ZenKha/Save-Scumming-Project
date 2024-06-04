using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public enum BattleState { START, PLACE, PLAYERTURN, ENEMYTURN, WON, LOST}

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleState _state;

    [Space(10)] [Header("Characters")]
    [SerializeField] List<GameObject> _characters;
    [SerializeField] List<GameObject> _enemies;

    [Space(10)]
    [Header("UI")]
    [SerializeField] TMP_Text startText;
    [Space(5)]
    [SerializeField] GameObject turnUI;
    [SerializeField] TMP_Text turnText;
    [SerializeField] Button endTurnButton;
    [Space(5)]
    [SerializeField] GameObject endScreenUI;
    [SerializeField] TMP_Text endText;


    void Start()
    {
        _state = BattleState.START;
        StartCoroutine(StartBattle());
    }

    IEnumerator StartBattle()
    {
        yield return StartCoroutine(GridManager.instance.CreateMap());

        //Place Enemies on the board
        PlaceEnemies(_enemies);

        //Battle Start UI animation
        yield return StartCoroutine(ShowText("Start"));

        //Highlight start area
        GridManager.instance.HighlightStartArea();
        StartPlaceState();
    }

    void PlaceEnemies(List<GameObject> enemies)
    {
        foreach (GameObject enemy in enemies)
        {
            GridManager.instance.PlaceEnemyCharacter(enemy);
        }
    }

    public void StartPlaceState()
    {
        _state = BattleState.PLACE;
        turnUI.SetActive(true);
        GridManager.instance.AddCharacters(_characters);
    }

    public void StartPlayerTurn()
    {
        _state = BattleState.PLAYERTURN;
        turnText.text = "Player Turn";

        // Give action token to player units
        foreach (GameObject playerUnit in GridManager.instance.Characters)
        {
            // Give action tokens
            playerUnit.GetComponent<PlayerUnitBehaviour>().GiveTurnTokens();

            // Remove blocking and resting state in case it was used last turn
            playerUnit.GetComponent<UnitInfo>().SetBlockingState(false);
            playerUnit.GetComponent<AnimatorControllerScript>().SetBlocking(false);
            playerUnit.GetComponent<AnimatorControllerScript>().SetResting(false);
        }

        GridManager.instance.ResetSelects();
        endTurnButton.interactable = true;
    }

    public void EndTurn()
    {
        GridManager.instance.ResetSelects();
        StartCoroutine(StartEnemyTurn());
        endTurnButton.interactable = false;
    }

    public IEnumerator StartEnemyTurn()
    {
        _state = BattleState.ENEMYTURN;
        turnText.text = "Enemy Turn";

        // Enemies do stuff
        foreach (GameObject enemy in GridManager.instance.Enemies)
        {
            // Remove blocking state in case it was used last turn
            enemy.GetComponent<UnitInfo>().SetBlockingState(false);
            enemy.GetComponent<AnimatorControllerScript>().SetBlocking(false);

            var map = GridManager.instance.GenerateArrayWithUnits();
            var characters = GridManager.instance.Characters;

            // Generate turn of enemy
            EnemyTurn turn = enemy.GetComponent<EnemyBehaviourEngine>().GenerateEnemyTurn(map, characters);

            // Move enemy to destination
            Vector2Int start = enemy.GetComponent<UnitMovement>().GetGridCoordinates();
            GridManager.instance.UpdateUnitPosition(enemy, turn.Destination);
            var path = PathFinder.CalculateShortestPath(map, start, turn.Destination);
            yield return StartCoroutine(GridManager.instance.MoveCharacter(enemy, path));
            yield return new WaitForSeconds(.5f);

            // Execute Action
            switch (turn.Action)
            {
                case Actions.Attack:
                    StartCoroutine(GridManager.instance.Attack(enemy, turn.Direction));
                    if (turn.AttacksTarget)
                    {
                        enemy.GetComponent<EnemyBehaviourEngine>().target = null;
                    }
                    break;
                case Actions.Block:
                    GridManager.instance.Block(enemy);
                    break;
                case Actions.Rest:
                    GridManager.instance.Rest(enemy);
                    break;
            }

            yield return new WaitForSeconds(2);
        }

        StartPlayerTurn();
    }

    public void Win()
    {
        foreach (var character in GridManager.instance.Characters)
        {
            character.GetComponent<AnimatorControllerScript>().SetVictory();
        }
        turnUI.SetActive(false);
        endScreenUI.SetActive(true);
        endText.text = "You Win!";
    }

    public void Lose()
    {
        foreach (var character in GridManager.instance.Enemies)
        {
            character.GetComponent<AnimatorControllerScript>().SetVictory();
        }
        turnUI.SetActive(false);
        endScreenUI.SetActive(true);
        endText.text = "You Lose...";
    }

    public BattleState GetBattleState()
    {
        return _state;
    }

    IEnumerator ShowText(string message)
    {
        startText.text = message;
        startText.enabled = true;
        yield return new WaitForSeconds(1.5f);
        startText.enabled = false;
    }
}