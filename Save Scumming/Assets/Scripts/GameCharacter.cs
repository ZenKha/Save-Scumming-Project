using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCharacter : MonoBehaviour
{
    [SerializeField] private int _gridX;
    [SerializeField] private int _gridY;
    [SerializeField] private CharacterClass _characterClass;


    public CharacterClass CharacterClass => _characterClass;

    public int GridX => _gridX;

    public int GridY => _gridY;


    public void SetGrid(int x, int y, Vector3 worldPosition)
    {
        _gridX = x;
        _gridY = y;
        transform.position = worldPosition;
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
