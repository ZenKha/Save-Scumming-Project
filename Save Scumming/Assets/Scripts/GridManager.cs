using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using Random = UnityEngine.Random;

public class GridManager : MonoBehaviour
{
    [SerializeField] private BattleSystem battleSystem;
    [Space(10)]
    [SerializeField] private GameObject[] prefabs; // Array of prefabs to instantiate
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

    private List<Block> _blocks = new(); // 2D array of created blocks

    [SerializeField] private float gridSpacing = 1f; // Spacing between grid elements
    [SerializeField] private bool centerMap = true; // Whether to center the map or not

    private float offsetX;
    private float offsetZ;

    private int rows;
    private int cols;

    void Update()
    {
        if (_selectedCharacter != null)
        {
            if (Input.GetButton("Ability1"))
            {

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
                int prefabIndex = grid[row, col];

                // Check if the prefab index is valid
                if (prefabIndex >= 0 && prefabIndex < prefabs.Length)
                {
                    GameObject prefab = prefabs[prefabIndex];

                    // Calculate the position for the prefab based on the grid spacing and offset
                    Vector3 position = GridToWorldPosition(new Vector2Int(row, col));

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
                else
                {
                    Debug.LogWarning("Invalid prefab index at row " + row + ", col " + col);
                }
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
                    Vector3 position = GridToWorldPosition(new Vector2Int(row, col));
                    position.y += HighlightYOffset;
                    GameObject instantiatedPrefab = Instantiate(_highlightSquare, position, Quaternion.identity);
                    highlight.Add(instantiatedPrefab);
                }
            }
        }
    }

    [Space(10)] [Header("Character")]
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
        block.SetCharacterInBlock(character);
        _enemies.Add(character);
    }

    private GameObject _selectedCharacter;
    List<Vector2Int> pathAnimation;
    private bool isMoving = false;

    private int placeIndex = 0;

    private Vector2Int startClickPos; // Start click position
    private Vector2Int destinationClickPos; // Destination click position

    public void MapClick(int x, int y)
    {
        if (battleSystem.GetBattleState() == BattleState.PLACE)
        {
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
            // Update block info
            Block block = _blocks.Find(b => b.GridX == x && b.GridY == y).GetComponent<Block>();
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

            if (_selectedCharacter == null)
            {
                // Find character in clicked tile and set it as _selectedCharacter
                _selectedCharacter = _characters.Find(c => c.GetComponent<UnitMovement>().GridX == x && c.GetComponent<UnitMovement>().GridY == y);

                // If there is a character in clicked tile AND it has the move token...
                if (_selectedCharacter != null && _selectedCharacter.GetComponent<PlayerUnitBehaviour>().MoveToken)
                {
                    // ...set the initial position...
                    startClickPos = new Vector2Int(x, y);
                    Debug.Log("Initial position set: " + startClickPos);

                    // ...and highlight all neighbour cells in range
                    HighlightCharacterMovement(x, y);
                }
            }
            else if (startClickPos == new Vector2Int(x, y))
            {
                // Second click is the same as the first, cancel movement
                _selectedCharacter = null;
                Debug.Log("Movement canceled");
                GetComponent<PathDrawer>().UpdatePath(null);
                DestroyHighlight();
            }
            else
            {
                // Player clicked in a different character
                var characterInDestination = _characters.Find(c => c.GetComponent<UnitMovement>().GridX == x && c.GetComponent<UnitMovement>().GridY == y);

                if (characterInDestination != null)
                {
                    if (!characterInDestination.GetComponent<PlayerUnitBehaviour>().ActionToken)
                    {
                        // Second character has no action, cancel move
                        _selectedCharacter = null;
                        Debug.Log("Movement canceled");
                        GetComponent<PathDrawer>().UpdatePath(null);
                        DestroyHighlight();
                        return;
                    }

                    // Change to new character
                    _selectedCharacter = characterInDestination;
                    startClickPos = new Vector2Int(x, y);
                    Debug.Log("Initial position set: " + startClickPos);
                    DestroyHighlight();

                    if (characterInDestination.GetComponent<PlayerUnitBehaviour>().MoveToken )
                    {
                        // Highlight all neighbour cells in a range
                        HighlightCharacterMovement(x, y);
                    }

                    return;
                }

                // TODO: Mudar verificação de character para diferenciar entre jogador e inimigo em vez de ter dois ifs
                // Checks if clicked block already has a character in it
                // Por agora, como o if anterior verifica characters do jogador, isto acaba só por verificar inimigos
                Block block = _blocks.Find(b => b.GridX == x && b.GridY == y).GetComponent<Block>();

                if (block.GetCharacterInBlock() != null)
                {
                    //if (character é aliado) {
                    //    faz isto
                    //}
                    //else 
                    //{
                    //    outra cena
                    //}
                    return;
                }

                // Second click
                if (!_selectedCharacter.GetComponent<PlayerUnitBehaviour>().MoveToken)
                {
                    return;
                }

                var pathToThisPlace = PathFinder.CalculateShortestPath(AddPlayerToGrid(), startClickPos, new(x, y));

                if (pathToThisPlace == null)
                {
                    return;
                }

                var distance = pathToThisPlace.Count;

                // If the click is out of the character's range, cancel movement
                if (distance > _selectedCharacter.GetComponent<UnitInfo>().MovementRange)
                {
                    _selectedCharacter = null;
                    destinationClickPos = new Vector2Int(-1, -1);
                    GetComponent<PathDrawer>().UpdatePath(null);
                    DestroyHighlight();
                    return;
                }

                // Second click, set the destination position
                destinationClickPos = new Vector2Int(x, y);

                UpdateUnitPosition(_selectedCharacter, x, y);
                pathAnimation = PathFinder.CalculateShortestPath(AddPlayerToGrid(), startClickPos, destinationClickPos);
                StartCoroutine(MoveSelectedCharacter());
                _selectedCharacter.GetComponent<PlayerUnitBehaviour>().RemoveMoveToken();

                destinationClickPos = new Vector2Int(-1, -1);
                GetComponent<PathDrawer>().UpdatePath(null);
                DestroyHighlight();
            }
        }
    }

    private GameObject semi;

    public void MapHover(int x, int y)
    {
        if (battleSystem.GetBattleState() == BattleState.PLACE)
        {
            // TODO: Mudar sistema de highlights para ser linked ao bloco
            if (x < 7) return;

            Destroy(semi);
            Vector3 vec = GridToWorldPosition(new(x, y));
            vec.y += 2f;
            semi = Instantiate(semiTransparentCharacter, vec, Quaternion.identity);
            return;
        }

        if (battleSystem.GetBattleState() == BattleState.PLAYERTURN)
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

            var path = PathFinder.CalculateShortestPath(AddPlayerToGrid(), startClickPos, new(x, y));

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
    }

    public void UpdateUnitPosition(GameObject unit, int x, int y)
    {
        // Remove character from starting block
        Block block = _blocks.Find(b => b.GridX == unit.GetComponent<UnitMovement>().GridX && b.GridY == unit.GetComponent<UnitMovement>().GridY).GetComponent<Block>();
        block.SetCharacterInBlock(null);
        // Add character to destination block
        block = _blocks.Find(b => b.GridX == x && b.GridY == y).GetComponent<Block>();
        block.SetCharacterInBlock(_selectedCharacter);
    }

    public void UpdateUnitPosition(GameObject unit, Vector2Int vec)
    {
        // Remove character from starting block
        Block block = _blocks.Find(b => b.GridX == unit.GetComponent<UnitMovement>().GridX && b.GridY == unit.GetComponent<UnitMovement>().GridY).GetComponent<Block>();
        block.SetCharacterInBlock(null);
        // Add character to destination block
        block = _blocks.Find(b => b.GridX == vec.x && b.GridY == vec.y).GetComponent<Block>();
        block.SetCharacterInBlock(_selectedCharacter);
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
                // TODO: Mudar quando distinção entre inimigos e jogadores for feita para não haver dois ifs
                if (_characters.Find(c => c.GetComponent<UnitMovement>().GridX == i && c.GetComponent<UnitMovement>().GridY == j) != null)
                {
                    newArray[i, j] = 2;
                }
                else if (_enemies.Find(c => c.GetComponent<UnitMovement>().GridX == i && c.GetComponent<UnitMovement>().GridY == j) != null)
                {
                    newArray[i, j] = 3;
                }
                else
                {
                    newArray[i, j] = grid[i, j];
                }
            }
        }

        return newArray;
    }

    public void ResetTurn()
    {
        _selectedCharacter = null;
    }

    private readonly List<GameObject> highlight = new();

    [Space(10)] [Header("Highlight Tiles")]
    [SerializeField] GameObject _highlightSquare;
    [SerializeField] Material _highlightPlace;
    [SerializeField] Material _highlightFar;
    [SerializeField] [Range(0f, 1f)] float HighlightYOffset; // 0.51f funciona por agora

    private void DestroyHighlight()
    {
        foreach (var g in highlight)
        {
            Destroy(g);
        }
        highlight.Clear();
    }

    private void HighlightCharacterMovement(int x, int y)
    {
        // Por agora vai à bruta e procura no mapa todo
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                if (row == x && col == y)
                {
                    Vector3 position = GridToWorldPosition(new Vector2Int(row, col));
                    // offset em Y feito com magic number mas à calceteiro
                    position.y += HighlightYOffset;
                    GameObject instantiatedPrefab = Instantiate(_highlightSquare, position, Quaternion.identity);
                    instantiatedPrefab.GetComponent<MeshRenderer>().material = _highlightPlace;
                    highlight.Add(instantiatedPrefab);
                }

                if (grid[row, col] != 0)
                {
                    continue;
                }

                // falta ver se há personagem
                Block block = _blocks.Find(b => b.GridX == row && b.GridY == col).GetComponent<Block>();

                if (block.GetCharacterInBlock() != null)
                {
                    continue;
                }

                var pathToThisPlace = PathFinder.CalculateShortestPath(AddPlayerToGrid(), new(x, y), new(row, col));

                if (pathToThisPlace == null)
                {
                    continue;
                }

                var distance = pathToThisPlace.Count;

                if (distance <= _selectedCharacter.GetComponent<UnitInfo>().MovementRange)
                {
                    Vector3 position = GridToWorldPosition(new Vector2Int(row, col));
                    // offset em Y feito com magic number mas à carpinteiro
                    position.y += HighlightYOffset;
                    GameObject instantiatedPrefab = Instantiate(_highlightSquare, position, Quaternion.identity);

                    //if (distance < 4)
                    //{
                    //    instantiatedPrefab.GetComponent<MeshRenderer>().material = _highlightFar;
                    //}
                    
                    highlight.Add(instantiatedPrefab);
                }
            }
        }
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
        isMoving = true;
        foreach (var p in path)
        {
            Vector3 vec = GridToWorldPosition(new(p.x, p.y));
            vec.y += CharacterYOffset;
            yield return StartCoroutine(unit.GetComponent<UnitMovement>().SetGridLerp(p.x, p.y, vec));
        }

        isMoving = false;
        _selectedCharacter = null;
    }

    private IEnumerator MoveSelectedCharacter()
    {
        isMoving = true;
        foreach (var p in pathAnimation)
        {
            Vector3 vec = GridToWorldPosition(new(p.x, p.y));
            vec.y += CharacterYOffset;
            yield return StartCoroutine(_selectedCharacter.GetComponent<UnitMovement>().SetGridLerp(p.x, p.y, vec));
        }

        isMoving = false;
        _selectedCharacter = null;
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

    private Vector3 GridToWorldPosition(Vector2Int p)
    {
        float worldX = offsetX + p.x * gridSpacing;
        float worldY = 0;
        float worldZ = offsetZ + p.y * gridSpacing;

        return new Vector3(worldX, worldY, worldZ);
    }
}