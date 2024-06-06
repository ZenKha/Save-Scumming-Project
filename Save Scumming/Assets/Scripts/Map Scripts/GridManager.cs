using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using Random = UnityEngine.Random;
using static UnityEngine.GraphicsBuffer;

public enum SelectState { None, Select, Attack, Block, Rest }

public class GridManager : MonoBehaviour
{
    public static GridManager instance;

    void Awake()
    {
        if (instance == null) 
        { 
            instance = this; 
        }
    }

    [SerializeField] private BattleSystem battleSystem;

    [Space(5)]
    [Header("UI")]
    [SerializeField] TMP_Text _actionText;

    [Space(5)]
    [Header("Prefabs")]
    [SerializeField] private GameObject[] floorPrefabs; // Array of prefabs to instantiate
    [SerializeField] private GameObject[] obstaclePrefabs; // Array of prefabs to instantiate
    [SerializeField] private GameObject floatingText;
    [SerializeField]
    private int[,] grid = new int[10, 10] // Bidimensional array representing the grid
    {
        {0,0,0,0,0,0,0,0,0,0},
        {0,1,0,0,0,0,0,0,0,0},
        {0,1,1,0,0,0,1,1,1,0},
        {0,0,0,0,0,0,0,1,0,0},
        {0,0,0,0,0,0,0,0,0,0},
        {0,0,1,1,0,0,0,0,0,0},
        {0,0,1,0,0,0,0,1,0,0},
        {0,1,1,0,0,0,0,1,0,0},
        {0,0,0,0,0,0,0,0,0,0},
        {0,0,0,0,0,0,0,0,0,0}
    };

    private readonly List<Block> _blocks = new(); // 2D array of created blocks

    [SerializeField] private float gridSpacing = 1f; // Spacing between grid elements
    [SerializeField] private bool centerMap = true; // Whether to center the map or not

    private float offsetX;
    private float offsetZ;

    private int rows;
    private int cols;

    void Update()
    {
        // Ignore all inputs during movement
        if (isMoving) return;

        if (selectState == SelectState.Select)
        {
            if (Input.GetButtonUp("Ability1") && _selectedCharacter.GetComponent<PlayerUnitBehaviour>().ActionToken)
            {
                selectState = SelectState.Attack;
                _actionText.text = "Attack";
                DestroyHighlight();
                GetComponent<PathDrawer>().UpdatePath(null);
                HighlightCharacterAttack();
            }

            if (Input.GetButtonUp("Ability2") && _selectedCharacter.GetComponent<PlayerUnitBehaviour>().ActionToken)
            {
                selectState = SelectState.Block;
                _actionText.text = "Block";
                DestroyHighlight();
                GetComponent<PathDrawer>().UpdatePath(null);
                HighlightCharacterSelfTarget();
            }

            if (Input.GetButtonUp("Ability3") && _selectedCharacter.GetComponent<PlayerUnitBehaviour>().ActionToken)
            {
                selectState = SelectState.Rest;
                _actionText.text = "Rest";
                DestroyHighlight();
                GetComponent<PathDrawer>().UpdatePath(null);
                HighlightCharacterSelfTarget();
            }
        }

        // Right click, cancel all selections
        if (Input.GetMouseButtonUp(1))
        {
            // If in attack, block or rest state, revert to move select with the same character
            if (selectState == SelectState.Attack || selectState == SelectState.Block || selectState == SelectState.Rest)
            {
                Vector2Int coords = _selectedCharacter.GetComponent<UnitMovement>().GetGridCoordinates();
                ResetSelects();
                MapClick(coords.x, coords.y);
            }
            else // else, just reset selects
            {
                ResetSelects();
            }
        }
    }

    public IEnumerator CreateMap ()
    {
        rows = grid.GetLength(0);
        cols = grid.GetLength(1);

        // Calculate the offset based on centering the map or not
        offsetX = centerMap ? -((cols - 1) * gridSpacing) / 2f : 0f;
        offsetZ = centerMap ? -((rows - 1) * gridSpacing) / 2f : 0f;

        // Loop through the grid array
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                int tileType = grid[row, col];

                GameObject prefab = null;

                if (tileType == 0)
                {
                    int prefabIndex = Random.Range(0, floorPrefabs.Length);

                    prefab = floorPrefabs[prefabIndex];
                }
                else if (tileType == 1)
                {
                    int prefabIndex = Random.Range(0, obstaclePrefabs.Length);

                    prefab = obstaclePrefabs[prefabIndex];
                }
                else
                {
                    Debug.LogWarning("Invalid prefab index at row " + row + ", col " + col);
                }

                // Calculate the position for the prefab based on the grid spacing and offset
                Vector3 position = GridToWorldPosition(new Vector2Int(row, col));
                position += new Vector3(0, 0.5f, 0);

                // Instantiate the prefab at the calculated position
                GameObject instantiatedPrefab = Instantiate(prefab, position, Quaternion.identity);

                // Parent the instantiated prefab to the game object of this script for organization
                instantiatedPrefab.transform.SetParent(transform);

                if (instantiatedPrefab.GetComponent<Block>() != null)
                {
                    instantiatedPrefab.GetComponent<Block>().SetGrid(row, col);
                }

                _blocks.Add(instantiatedPrefab.GetComponent<Block>());
            }
        }

        yield return null;
    }

    public void HighlightStartArea()
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                if (grid[row, col] != 0)
                {
                    continue;
                }

                if (row >= 7)
                {
                    Block block = _blocks.Find(b => b.GridX == row && b.GridY == col);
                    block.HighlightBlock(HighlightType.Close);
                    highlight.Add(block);
                }
            }
        }
    }

    [Space(5)] [Header("Character")]
    [SerializeField] List<GameObject> _characters;
    [SerializeField] List<GameObject> _enemies;
    [SerializeField] GameObject semiTransparentCharacter;
    [SerializeField][Range(0f, 5f)] float CharacterYOffset; // 0.51 funciona por agora

    public List<GameObject> Characters => _characters;
    public List<GameObject> Enemies => _enemies;

    public void AddCharacters(List<GameObject> characters)
    {
        _characters.AddRange(characters);
    }

    public void PlaceEnemyCharacter(GameObject enemy)
    {
        int x, y;
        Block block;

        do
        {
            x = Random.Range(0, 3);
            y = Random.Range(0, cols);
            //Debug.Log("(" + x + ", " + y + ")");
            block = _blocks.Find(b => b.GridX == x && b.GridY == y).GetComponent<Block>();
        } while (grid[x, y] != 0 || block.GetCharacterInBlock() != null);

        Vector3 vec = GridToWorldPosition(new(x, y));
        vec.y += CharacterYOffset;
        GameObject character = Instantiate(enemy, vec, Quaternion.identity);
        character.GetComponent<UnitMovement>().SetGrid(x, y, vec);
        character.transform.GetChild(0).transform.forward = Vector3.right;
        block.SetCharacterInBlock(character);
        _enemies.Add(character);
    }

    [SerializeField] private GameObject _selectedCharacter;
    List<Vector2Int> pathAnimation;
    private bool isMoving = false;
    private SelectState selectState = SelectState.None;

    private int placeIndex = 0;

    private Vector2Int startClickPos; // Start click position

    public void MapClick(int x, int y)
    {
        if (battleSystem.GetBattleState() == BattleState.PLACE)
        {
            // Check if block is highlighted, ignore click if it's not
            Block block = _blocks.Find(b => b.GridX == x && b.GridY == y).GetComponent<Block>();
            if (!block.IsHighlighted)
            {
                return;
            }

            // Another character already placed in that spot, ignore command
            if (_characters.Find(c => c.GetComponent<UnitMovement>().GridX == x && c.GetComponent<UnitMovement>().GridY == y) != null)
            {
                return;
            }

            // Get next character in list
            GameObject character = _characters[placeIndex];

            // Place Character
            Vector3 vec = GridToWorldPosition(new(x, y));
            vec.y += CharacterYOffset;
            character.GetComponent<UnitMovement>().SetGrid(x, y, vec);
            character.transform.GetChild(0).transform.forward = Vector3.left;
            // Update block info
            block.SetCharacterInBlock(character);

            // Update index to place
            placeIndex++;

            // If list is now empty, proceed to next battle state
            if (_characters.Count == placeIndex)
            {
                DestroyHighlight();
                Destroy(semi);
                battleSystem.StartPlayerTurn();
                return;
            }
        }

        if (battleSystem.GetBattleState() == BattleState.PLAYERTURN)
        {
            if (isMoving)
            {
                return;
            }

            if (selectState == SelectState.None)
            {
                // Find character in clicked tile and set it as _selectedCharacter
                _selectedCharacter = _characters.Find(c => c.GetComponent<UnitMovement>().GridX == x && c.GetComponent<UnitMovement>().GridY == y);

                // If there was no character, ignore click
                if (_selectedCharacter == null)
                {
                    return;
                }

                // Set SelectState to Select
                selectState = SelectState.Select;

                // Update starting position
                startClickPos = new Vector2Int(x, y);
                Debug.Log("Initial position set: " + startClickPos);

                // If there is a character in clicked tile AND it has the move token...
                if (_selectedCharacter.GetComponent<PlayerUnitBehaviour>().MoveToken)
                {
                    // ... highlight all neighbour cells in range
                    HighlightCharacterMovement(x, y);
                }
            }
            else
            {
                if (selectState == SelectState.Select)
                {
                    // If the click is on the currently selected character, ignore it
                    if (startClickPos == new Vector2Int(x, y))
                    {
                        return;
                    }

                    // Checks if clicked block already has a character in it and...
                    Block block = _blocks.Find(b => b.GridX == x && b.GridY == y).GetComponent<Block>();
                    var charInBlock = block.GetCharacterInBlock();

                    // ... if it does, check type of character
                    if (charInBlock != null)
                    {
                        // If it's a player character, change to that character
                        if (charInBlock.GetComponent<PlayerUnitBehaviour>() != null)
                        {
                            ResetSelects();
                            Debug.Log("Movement canceled");

                            // Change to new character
                            selectState = SelectState.Select;
                            _selectedCharacter = charInBlock;

                            startClickPos = new Vector2Int(x, y);
                            Debug.Log("Initial position set: " + startClickPos);

                            if (charInBlock.GetComponent<PlayerUnitBehaviour>().MoveToken)
                            {
                                // Highlight all neighbour cells in a range
                                HighlightCharacterMovement(x, y);
                            }

                            return;
                        }
                    }

                    // Second click
                    if (!_selectedCharacter.GetComponent<PlayerUnitBehaviour>().MoveToken)
                    {
                        return;
                    }

                    var pathToThisPlace = PathFinder.CalculateShortestPath(GenerateArrayWithUnits(), startClickPos, new(x, y));

                    if (pathToThisPlace == null)
                    {
                        return;
                    }

                    var distance = pathToThisPlace.Count;

                    // If the click is out of the character's range, cancel movement
                    if (distance > _selectedCharacter.GetComponent<UnitInfo>().MovementRange)
                    {
                        ResetSelects();
                        return;
                    }

                    // Second click, set the destination position

                    UpdateUnitPosition(_selectedCharacter, x, y);
                    pathAnimation = pathToThisPlace;
                    StartCoroutine(MoveSelectedCharacter());
                    _selectedCharacter.GetComponent<PlayerUnitBehaviour>().RemoveMoveToken();

                    // Not using ResetSelects() since _selectedCharacter is still needed
                    GetComponent<PathDrawer>().UpdatePath(null);
                    DestroyHighlight();
                }
                
                if (selectState == SelectState.Attack)
                {
                    // If the click is on the currently selected character, ignore it
                    if (startClickPos == new Vector2Int(x, y))
                    {
                        return;
                    }

                    Block block = _blocks.Find(b => b.GridX == x && b.GridY == y).GetComponent<Block>();
                    
                    // If block is highlighted, get info for attack
                    if (block.IsHighlighted)
                    {
                        var posInfo = _selectedCharacter.GetComponent<UnitMovement>();

                        // There's probably an easier way of doing this
                        // Attack up
                        if (block.GridX < posInfo.GridX && block.GridY == posInfo.GridY)
                        {
                            // Grid is rotated 90º counter-clockwise
                            StartCoroutine(Attack(_selectedCharacter, Vector2Int.left));
                        }
                        
                        // Attack down
                        if (block.GridX > posInfo.GridX && block.GridY == posInfo.GridY)
                        {
                            StartCoroutine(Attack(_selectedCharacter, Vector2Int.right));
                        }

                        // Attack left
                        if (block.GridX == posInfo.GridX && block.GridY < posInfo.GridY)
                        {
                            StartCoroutine(Attack(_selectedCharacter, Vector2Int.down));
                        }

                        // Attack right
                        if (block.GridX == posInfo.GridX && block.GridY > posInfo.GridY)
                        {
                            StartCoroutine(Attack(_selectedCharacter, Vector2Int.up));
                        }
                    }

                    // Conclude character turn
                    _selectedCharacter.GetComponent<PlayerUnitBehaviour>().RemoveActionToken();
                    ResetSelects();
                }
                
                if (selectState == SelectState.Block)
                {
                    Block block = _blocks.Find(b => b.GridX == x && b.GridY == y).GetComponent<Block>();
                    var charInBlock = block.GetCharacterInBlock();

                    // If the click is on the currently selected character, activate block
                    if (startClickPos == new Vector2Int(x, y))
                    {
                        Block(_selectedCharacter);

                        // Conclude character turn
                        _selectedCharacter.GetComponent<PlayerUnitBehaviour>().RemoveActionToken();
                        ResetSelects();
                        return;
                    }
                }
                
                if (selectState == SelectState.Rest)
                {
                    Block block = _blocks.Find(b => b.GridX == x && b.GridY == y).GetComponent<Block>();
                    var charInBlock = block.GetCharacterInBlock();

                    // If the click is on the currently selected character, activate block
                    if (startClickPos == new Vector2Int(x, y))
                    {
                        Rest(_selectedCharacter);

                        // Conclude character turn
                        _selectedCharacter.GetComponent<PlayerUnitBehaviour>().RemoveActionToken();
                        ResetSelects();

                        return;
                    }
                }
            } 
        }
    }

    public IEnumerator Attack(GameObject unit, Vector2Int direction)
    {
        var stats = unit.GetComponent<UnitInfo>();
        var pierce = stats.Pierce;
        var acc = stats.Accuracy;
        Vector2Int start = unit.GetComponent<UnitMovement>().GetGridCoordinates();
        Block block;

        Vector3 direction3 = new(direction.x, 0, direction.y);
        if (unit.transform.GetChild(0).transform.forward != direction3)
        {
            yield return StartCoroutine(RotateUnit(unit, direction3));
        }

        var unitAnimator = unit.GetComponent<AnimatorControllerScript>();
        unitAnimator.SetAttack();

        for (int i = 1; i <= stats.AttackRange; i++)
        {
            Vector2Int coords = start + (direction * i);

            //Check if coordinates are inside limits of grid
            if (coords.x < 0 || coords.x > rows-1 || coords.y < 0 || coords.y > cols-1)
            {
                yield break;
            }

            block = _blocks.Find(b => b.GridX == coords.x && b.GridY == coords.y).GetComponent<Block>();

            var target = block.GetCharacterInBlock();

            if (target != null)
            {
                StartCoroutine(RotateUnit(target, direction3 * -1));
            }
        }
    }

    public void Block(GameObject unit)
    {
        unit.GetComponent<UnitInfo>().SetBlockingState(true);
        unit.GetComponent<AnimatorControllerScript>().SetBlocking(true);
    }

    public void Rest(GameObject unit)
    {
        var info = unit.GetComponent<UnitInfo>();
        unit.GetComponent<AnimatorControllerScript>().SetResting(true);
        info.ModifyHp(info.HealPower);
    }

    public void KillUnit(GameObject unit)
    {
        // Player unit
        if (unit.GetComponent<PlayerUnitBehaviour>() != null)
        {
            var unitMove = unit.GetComponent<UnitMovement>();
            
            // Remove unit from block
            Block block = _blocks.Find(b => b.GridX == unitMove.GridX && b.GridY == unitMove.GridY).GetComponent<Block>();
            block.SetCharacterInBlock(null);
            
            _characters.Remove(unit);
            unit.GetComponent<AnimatorControllerScript>().SetDeath();
            unit.GetComponent<Collider>().enabled = false;
            unit.transform.GetChild(1).gameObject.SetActive(false);
            //unit.SetActive(false);

            if (_characters.Count == 0)
            {
                battleSystem.Lose();
            }
        }

        // Enemy unit
        if (unit.GetComponent<EnemyBehaviourEngine>() != null)
        {
            var unitMove = unit.GetComponent<UnitMovement>();

            // Remove unit from block
            Block block = _blocks.Find(b => b.GridX == unitMove.GridX && b.GridY == unitMove.GridY).GetComponent<Block>();
            block.SetCharacterInBlock(null);

            _enemies.Remove(unit);
            unit.GetComponent<AnimatorControllerScript>().SetDeath();
            unit.GetComponent<Collider>().enabled = false;
            unit.transform.GetChild(1).gameObject.SetActive(false);
            //unit.SetActive(false);

            if (_enemies.Count == 0)
            {
                battleSystem.Win();
            }
        }
    }

    public void KillDebug()
    {
        KillUnit(_selectedCharacter);
        ResetSelects();
    }

    private GameObject semi;
    private readonly List<GameObject> _highlightedUnits = new();

    public void MapHover(int x, int y)
    {
        if (x == -1 && y == -1)
        {
            if (selectState == SelectState.Attack)
            {
                DestroyHighlight();
                RemoveHighlightAttackHover();
                HighlightCharacterAttack();
            }

            GetComponent<PathDrawer>().UpdatePath(null);
            return;
        }

        if (battleSystem.GetBattleState() == BattleState.PLACE)
        {
            Block block = _blocks.Find(b => b.GridX == x && b.GridY == y).GetComponent<Block>();
            if (!block.IsHighlighted) return;

            Destroy(semi);
            Vector3 vec = GridToWorldPosition(new(x, y));
            vec.y += 2f;
            semi = Instantiate(semiTransparentCharacter, vec, Quaternion.identity);
            return;
        }

        if (battleSystem.GetBattleState() == BattleState.PLAYERTURN)
        {
            if (selectState == SelectState.Select)
            {
                if (_selectedCharacter == null || isMoving)
                {
                    return;
                }

                if (!_selectedCharacter.GetComponent<PlayerUnitBehaviour>().MoveToken)
                {
                    return;
                }

                Block block = _blocks.Find(b => b.GridX == x && b.GridY == y).GetComponent<Block>();

                // Player hovered over a different character
                if (block.GetCharacterInBlock() != null)
                {
                    // Erase path
                    GetComponent<PathDrawer>().UpdatePath(null);
                    return;
                }

                var path = PathFinder.CalculateShortestPath(GenerateArrayWithUnits(), startClickPos, new(x, y));

                if (path == null)
                {
                    return;
                }

                var distance = path.Count;

                // If the hover is out of the character's range, erase path
                if (distance > _selectedCharacter.GetComponent<UnitInfo>().MovementRange)
                {
                    GetComponent<PathDrawer>().UpdatePath(null);
                    return;
                }

                path.Insert(0, startClickPos);
                GetComponent<PathDrawer>().UpdatePath(ConvertPathToWorld(path)); 
            }

            if (selectState == SelectState.Attack)
            {
                Block block = _blocks.Find(b => b.GridX == x && b.GridY == y).GetComponent<Block>();

                // If block is highlighted, get info for attack
                if (block.IsHighlighted)
                {
                    var posInfo = _selectedCharacter.GetComponent<UnitMovement>();

                    // There's probably an easier way of doing this
                    // Attack up
                    if (block.GridX < posInfo.GridX && block.GridY == posInfo.GridY)
                    {
                        // Grid is rotated 90º counter-clockwise
                        HighlightAttackHover(Vector2Int.left);
                    }

                    // Attack down
                    if (block.GridX > posInfo.GridX && block.GridY == posInfo.GridY)
                    {
                        HighlightAttackHover(Vector2Int.right);
                    }

                    // Attack left
                    if (block.GridX == posInfo.GridX && block.GridY < posInfo.GridY)
                    {
                        HighlightAttackHover(Vector2Int.down);
                    }

                    // Attack right
                    if (block.GridX == posInfo.GridX && block.GridY > posInfo.GridY)
                    {
                        HighlightAttackHover(Vector2Int.up);
                    }
                }
            }
        }
    }

    private void HighlightAttackHover(Vector2Int direction)
    {
        var stats = _selectedCharacter.GetComponent<UnitInfo>();
        int acc = stats.Accuracy;
        Vector2Int start = _selectedCharacter.GetComponent<UnitMovement>().GetGridCoordinates();
        Block block;

        for (int i = 1; i <= stats.AttackRange; i++)
        {
            Vector2Int coords = start + (direction * i);

            //Check if coordinates are inside limits of grid
            if (coords.x < 0 || coords.x > rows - 1 || coords.y < 0 || coords.y > cols - 1)
            {
                return;
            }

            block = _blocks.Find(b => b.GridX == coords.x && b.GridY == coords.y).GetComponent<Block>();
            block.HighlightBlock(HighlightType.AttackHover);

            if (!block.IsClickable)
            {
                acc -= 20;
            }

            var unit = block.GetCharacterInBlock();

            if (unit != null)
            {
                unit.GetComponent<UnitHud>().ShowUnitHud(acc);
                _highlightedUnits.Add(unit);
            }

            acc -= 10;
        }
    }

    private void RemoveHighlightAttackHover()
    {
        foreach (GameObject unit in _highlightedUnits)
        {
            unit.GetComponent<UnitHud>().HideUnitHud();
        }
        _highlightedUnits.Clear();
    }

    public void UpdateUnitPosition(GameObject unit, int x, int y)
    {
        // Remove character from starting block
        Block block = _blocks.Find(b => b.GridX == unit.GetComponent<UnitMovement>().GridX && b.GridY == unit.GetComponent<UnitMovement>().GridY).GetComponent<Block>();
        block.SetCharacterInBlock(null);
        // Add character to destination block
        block = _blocks.Find(b => b.GridX == x && b.GridY == y).GetComponent<Block>();
        block.SetCharacterInBlock(unit);
    }

    public void UpdateUnitPosition(GameObject unit, Vector2Int vec)
    {
        // Remove character from starting block
        Block block = _blocks.Find(b => b.GridX == unit.GetComponent<UnitMovement>().GridX && b.GridY == unit.GetComponent<UnitMovement>().GridY).GetComponent<Block>();
        block.SetCharacterInBlock(null);
        // Add character to destination block
        block = _blocks.Find(b => b.GridX == vec.x && b.GridY == vec.y).GetComponent<Block>();
        block.SetCharacterInBlock(unit);
    }

    public int[,] GenerateArrayWithUnits()
    {
        int rows = grid.GetLength(0);
        int columns = grid.GetLength(1);
        int[,] newArray = new int[rows, columns];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                Block block = _blocks.Find(b => b.GridX == i && b.GridY == j).GetComponent<Block>();
                var unit = block.GetCharacterInBlock();

                if (unit != null)
                {
                    if (unit.GetComponent<PlayerUnitBehaviour>() != null)
                    {
                        newArray[i, j] = 2;
                    }
                    else if (unit.GetComponent<EnemyBehaviourEngine>() != null)
                    {
                        newArray[i, j] = 3;
                    }
                }
                else
                {
                    newArray[i, j] = grid[i, j];
                }
            }
        }

        return newArray;
    }

    public void ResetSelects()
    {
        _selectedCharacter = null;
        GetComponent<PathDrawer>().UpdatePath(null);
        DestroyHighlight();
        RemoveHighlightAttackHover();
        isMoving = false;
        selectState = SelectState.None;
        _actionText.text = "";
    }

    private readonly List<Block> highlight = new();

    private void DestroyHighlight()
    {
        foreach (var g in highlight)
        {
            g.DestroyHighlight();
        }
        highlight.Clear();
    }

    private void HighlightCharacterMovement(int x, int y)
    {
        Block block;

        // Por agora vai à bruta e procura no mapa todo
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                if (row == x && col == y)
                {
                    block = _blocks.Find(b => b.GridX == row && b.GridY == col);
                    block.HighlightBlock(HighlightType.Close);
                    highlight.Add(block);
                }

                if (grid[row, col] != 0)
                {
                    continue;
                }

                block = _blocks.Find(b => b.GridX == row && b.GridY == col).GetComponent<Block>();

                if (block.GetCharacterInBlock() != null)
                {
                    continue;
                }

                var pathToThisPlace = PathFinder.CalculateShortestPath(GenerateArrayWithUnits(), new(x, y), new(row, col));

                if (pathToThisPlace == null)
                {
                    continue;
                }

                var distance = pathToThisPlace.Count;
                var range = _selectedCharacter.GetComponent<UnitInfo>().MovementRange;

                if (distance == range)
                {
                    block.HighlightBlock(HighlightType.Far);
                }
                else if (distance < range)
                {
                    block.HighlightBlock(HighlightType.Close);
                }

                highlight.Add(block);
            }
        }
    }

    private void HighlightCharacterAttack()
    {
        UnitInfo unitInfo = _selectedCharacter.GetComponent<UnitInfo>();
        int range = unitInfo.AttackRange;
        UnitMovement unitMovement = _selectedCharacter.GetComponent<UnitMovement>();
        bool up, down, left, right;
        up = down = left = right = true;
        
        Block block;

        for (int i = 1; i <= range; i++)
        {
            if (up)
            {
                block = _blocks.Find(b => b.GridX == unitMovement.GridX - i && b.GridY == unitMovement.GridY);

                if (block != null)
                {
                    block.HighlightBlock(HighlightType.Attack);
                    highlight.Add(block);
                }
                else
                {
                    up = false;
                }
            }

            if (down)
            {
                block = _blocks.Find(b => b.GridX == unitMovement.GridX + i && b.GridY == unitMovement.GridY);

                if (block != null)
                {
                    block.HighlightBlock(HighlightType.Attack);
                    highlight.Add(block);
                }
                else
                {
                    down = false;
                }
            }

            if (right)
            {
                block = _blocks.Find(b => b.GridX == unitMovement.GridX && b.GridY == unitMovement.GridY + i);

                if (block != null)
                {
                    block.HighlightBlock(HighlightType.Attack);
                    highlight.Add(block);
                }
                else
                {
                    right = false;
                }
            }

            if (left)
            {
                block = _blocks.Find(b => b.GridX == unitMovement.GridX && b.GridY == unitMovement.GridY - i);

                if (block != null)
                {
                    block.HighlightBlock(HighlightType.Attack);
                    highlight.Add(block);
                }
                else
                {
                    left = false;
                }
            }
        }
    }

    private void HighlightCharacterSelfTarget()
    {
        UnitMovement unitMovement = _selectedCharacter.GetComponent<UnitMovement>();
        Block block = _blocks.Find(b => b.GridX == unitMovement.GridX && b.GridY == unitMovement.GridY).GetComponent<Block>();
        block.HighlightBlock(HighlightType.Far); 
        highlight.Add(block);
    }

    private int[,] AddPlayerToGrid()
    {
        var newArray = DuplicateArray(grid);

        foreach (var c in _characters)
        {
            newArray[c.GetComponent<UnitMovement>().GridX, c.GetComponent<UnitMovement>().GridY] = -1;
        }

        return newArray;
    }

    private static int[,] DuplicateArray (int[,] originalArray)
    {
        int rows = originalArray.GetLength(0);
        int columns = originalArray.GetLength(1);
        int[,] newArray = new int[rows, columns];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                newArray[i, j] = originalArray[i, j];
            }
        }

        return newArray;
    }

    public IEnumerator MoveCharacter(GameObject unit, List<Vector2Int> path)
    {
        unit.GetComponent<AnimatorControllerScript>().SetMoving(true);
        isMoving = true;
        foreach (var p in path)
        {
            Vector3 vec = GridToWorldPosition(new(p.x, p.y));
            vec.y += CharacterYOffset;
            yield return StartCoroutine(unit.GetComponent<UnitMovement>().SetGridLerp(p.x, p.y, vec));
        }
        unit.GetComponent<AnimatorControllerScript>().SetMoving(false);

        ResetSelects();
    }

    private IEnumerator MoveSelectedCharacter()
    {
        _selectedCharacter.GetComponent<AnimatorControllerScript>().SetMoving(true);
        isMoving = true;
        foreach (var p in pathAnimation)
        {
            Vector3 vec = GridToWorldPosition(new(p.x, p.y));
            vec.y += CharacterYOffset;
            yield return StartCoroutine(_selectedCharacter.GetComponent<UnitMovement>().SetGridLerp(p.x, p.y, vec));
        }
        _selectedCharacter.GetComponent<AnimatorControllerScript>().SetMoving(false);

        ResetSelects();
    }

    private List<Vector3> ConvertPathToWorld (List<Vector2Int> path)
    {
        var worldPath = new List<Vector3>();
        foreach (var p in path)
        {
            worldPath.Add(GridToWorldPosition(p));
        }
        return worldPath;
    }

    public Vector3 GridToWorldPosition(Vector2Int p)
    {
        float worldX = offsetX + p.x * gridSpacing;
        float worldY = 0;
        float worldZ = offsetZ + p.y * gridSpacing;

        return new Vector3(worldX, worldY, worldZ);
    }

    public Vector2Int WorldPositionToGrid(Vector3 p)
    {
        int worldX = Mathf.FloorToInt(p.x - offsetX);
        int worldZ = Mathf.FloorToInt(p.z - offsetZ);

        return new Vector2Int(worldX, worldZ);
    }

    public IEnumerator RotateUnit(GameObject unit, Vector3 dirOfRotate)
    {
        float timeElapsed = 0;
        float rotationTime = 0.5f;
        var mesh = unit.transform.GetChild(0);

        Vector3 initialDir = mesh.transform.forward;

        while (timeElapsed < rotationTime)
        {
            mesh.transform.forward = Vector3.Lerp(initialDir, dirOfRotate, timeElapsed / rotationTime);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
    }
}