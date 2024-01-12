using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private GameObject[] prefabs; // Array of prefabs to instantiate
    [SerializeField]
    private int[,] grid = new int[10, 10] // Bidimensional array representing the grid
    {
        {0,0,0,0,0,0,0,0,0,0},
        {0,0,0,0,0,0,0,0,0,0},
        {0,1,1,0,0,0,0,0,0,0},
        {0,0,0,0,0,0,0,0,0,0},
        {0,0,0,0,0,0,0,0,0,0},
        {0,0,0,0,0,0,0,0,0,0},
        {0,0,0,0,0,0,0,0,0,0},
        {0,0,0,1,1,0,1,0,0,0},
        {0,1,1,1,0,0,0,0,0,0},
        {0,0,0,0,0,0,0,0,0,0}
    };

    [SerializeField] private float gridSpacing = 1f; // Spacing between grid elements
    [SerializeField] private bool centerMap = true; // Whether to center the map or not

    private float offsetX;
    private float offsetZ;

    private int rows;
    private int cols;

    private void CreateMap ()
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

                    if (instantiatedPrefab.GetComponent<ClickScript>() != null)
                    {
                        instantiatedPrefab.GetComponent<ClickScript>().SetGrid(row, col);
                    }
                }
                else
                {
                    Debug.LogWarning("Invalid prefab index at row " + row + ", col " + col);
                }
            }
        }
    }

    [SerializeField] List<GameCharacter> _characters;
    [SerializeField][Range(0f, 5f)] float CharacterYOffset; // 1.25 funciona por agora

    private void PlaceCharacters()
    {
        int p = Mathf.FloorToInt(grid.GetLength(1)/2 - _characters.Count/2) - 1;
        int r = grid.GetLength(0) - 1;
        foreach (var c in _characters)
        {
            // posição definida à serralheiro
            Vector3 vec = GridToWorldPosition(new(r, p));
            vec.y += CharacterYOffset;
            c.SetGrid(r, p, vec);
            p++;
        }
    }

    private void Start()
    {
        CreateMap();
        PlaceCharacters();
    }

    private Vector2Int startClickPos; // Start click position
    private Vector2Int destinationClickPos; // Destination click position
    private bool isMovementInProgress; // Flag to indicate if movement is in progress

    private List<GameObject> highlight = new List<GameObject>();
    [SerializeField] GameObject _highlightSquare;

    [SerializeField] Material _highlightPlace;
    [SerializeField] Material _highlightFar;
    [SerializeField] [Range(0f, 1f)] float HighlightYOffset; // 0.51f funciona por agora


    private void DestroyHighlight ()
    {
        foreach (var g in highlight)
        {
            Destroy(g);
        }
        highlight.Clear();
    }

    private void Highlight(int x, int y)
    {
        print(x + " " + y);

        // Destroy o highlight anterior
        DestroyHighlight();

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

                // mostra caminhos com tamanho até 4 (4 hard coded à sapateiro)
                var pathToThisPlace = PathFinder.CalculateShortestPath(AddPlayerToGrid(), new(x, y), new(row, col));

                if (pathToThisPlace == null)
                {
                    continue;
                }

                var distance = pathToThisPlace.Count;

                if (distance <= _selectedCharacter.CharacterClass.MovementRange)
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

    private GameCharacter _selectedCharacter;
    private bool isMoving = false;

    public void MapClick(int x, int y)
    {
        if (isMoving)
        {
            return;
        }

        if (!isMovementInProgress)
        {
            // se não houver boneco neste sitio ignora o click
            _selectedCharacter = _characters.Find(c => c.GridX == x && c.GridY == y);

            if (_selectedCharacter == null)
            {
                return;
            }

            // First click, set the initial position
            startClickPos = new Vector2Int(x, y);
            isMovementInProgress = true;
            Debug.Log("Initial position set: " + startClickPos);

            // Highlight all neighbour cells in a range
            Highlight(x, y);

        }
        else if (startClickPos == new Vector2Int(x, y))
        {
            // Second click is the same as the first, cancel movement
            isMovementInProgress = false;
            Debug.Log("Movement canceled");
            GetComponent<PathDrawer>().UpdatePath(null);
            DestroyHighlight();
        }
        else if (destinationClickPos == new Vector2Int(x, y))
        {
            // Third click, same as the destination, confirm movement
            Debug.Log("Movement confirmed to destination position: " + destinationClickPos);
            // Perform additional actions or initiate movement to the destination
            // ...

            // Há sempre _selectedCharacter?
            // Se estiver null foi por alguma estupidez
            //_selectedCharacter.SetGrid(x, y, GridToWorldPosition(new(x, y)));
 
            pathAnimation = PathFinder.CalculateShortestPath(AddPlayerToGrid(), startClickPos, destinationClickPos);
            StartCoroutine(MoveCharacter());

            GetComponent<PathDrawer>().UpdatePath(null);
            DestroyHighlight();
            isMovementInProgress = false;
            destinationClickPos = new Vector2Int(-1, -1);
        }
        else
        {
            var characterInDestination = _characters.Find(c => c.GridX == x && c.GridY == y);

            // Player clicked in a different character
            if (characterInDestination != null)
            {
                // Change to new character
                _selectedCharacter = characterInDestination;
                startClickPos = new Vector2Int(x, y);
                Debug.Log("Initial position set: " + startClickPos);

                // Highlight all neighbour cells in a range
                Highlight(x, y);

                return;
            }

            var pathToThisPlace = PathFinder.CalculateShortestPath(AddPlayerToGrid(), startClickPos, new(x, y));

            if (pathToThisPlace == null)
            {
                return;
            }

            var distance = pathToThisPlace.Count;

            // If the click is out of the character's range, cancel movement
            if (distance > _selectedCharacter.CharacterClass.MovementRange)
            {
                GetComponent<PathDrawer>().UpdatePath(null);
                DestroyHighlight();
                isMovementInProgress = false;
                destinationClickPos = new Vector2Int(-1, -1);
                return;
            }

            // Second click, set the destination position
            destinationClickPos = new Vector2Int(x, y);
            //Debug.Log("Destination position set: " + destinationClickPos);

            var path = PathFinder.CalculateShortestPath(AddPlayerToGrid(), startClickPos, destinationClickPos);
            path.Insert(0, startClickPos);
            GetComponent<PathDrawer>().UpdatePath(ConvertPathToWorld(path));

            //Debug.Log(PathFinder.PathListToString(path));
        }
    }

    List<Vector2Int> pathAnimation;

    private int[,] AddPlayerToGrid ()
    {
        var newArray = DuplicateArray(grid);

        foreach (var c in _characters)
        {
            newArray[c.GridX, c.GridY] = -1;
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

    private IEnumerator MoveCharacter ()
    {
        isMoving = true;
        foreach (var p in pathAnimation)
        {
            Vector3 vec = GridToWorldPosition(new(p.x, p.y));
            vec.y += CharacterYOffset;
            _selectedCharacter.SetGrid(p.x, p.y, vec);
            yield return new WaitForSeconds(0.2f);
        }

        isMoving = false;
        yield return null;
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