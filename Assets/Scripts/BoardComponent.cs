using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;

public class BoardComponent : MonoBehaviour
{

    public enum TileTypes
    {
        Dirt = 0,
        Rock = 1,
    }

    public enum Layers
    {
        Ground = 0,
        Roots = 1, 
        Visibility = 2
    }

    // -- < Internal logic > ------------------------------------------------
    private TileTypes[,,] _board;

    // Size of board
    [SerializeField]
    private int _boardWidth = 20;
    [SerializeField]
    private int _boardHeight = 20;

    private int _nTileTypes;
    private int _nLayers;


    // -- < Render > --------------------------------------------------------
    [SerializeField]
    private Tilemap _tilemap;

    [SerializeField]
    private Tile _dirtTile;
    [SerializeField]
    private Tile _rockTile;
    

    // Start is called before the first frame update
    void Start()
    {
        // Sanity checks
        Debug.Assert(_tilemap != null, "Missing Grid component from board object");
        Debug.Assert(_boardWidth > 0 && _boardHeight > 0, "Board size should be positive");
        Debug.Assert(_dirtTile != null, "Missing Ground Tile property in board object");
        Debug.Assert(_rockTile != null, "Missing Rock Tile property in board object");

        // Init board
        InitBoard();
        UpdateTileMap();
        

    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void InitBoard()
    {
        _nTileTypes = Enum.GetNames(typeof(TileTypes)).Length;
        _nLayers = Enum.GetNames(typeof(Layers)).Length;
        _board = new TileTypes[_nLayers, _boardHeight, _boardWidth];


        for (int i = 0; i < _boardHeight; i++)
            for (int j = 0; j < _boardWidth; j++)
            {
                _board[(int)Layers.Ground, i,j] = TileTypes.Dirt;
                // TODO rellenar la capa de overlays como raices, agua, nutrientes


            }
    }

    private Tile TypeToTile(TileTypes type)
    {
        switch (type)
        {
            case TileTypes.Dirt:
                return _dirtTile;
            case TileTypes.Rock:
                return _rockTile;
            default:
                Debug.LogError("Unknown Type of Tile");
                break;

        }

        return null;
    }

    private void UpdateTileMap()
    {
        for (int layer = 0; layer < _nLayers; layer++)
            for(int i = 0; i < _boardHeight; i++)
                for (int j = 0; j < _boardWidth; j++)
                {
                    Vector3Int p = new(i - _boardHeight/2,j - _boardWidth/2, 0);
                    var type = _board[layer, i, j];
                    var tile = TypeToTile(type);

                    Debug.Assert(tile != null, "Could not retrieve Tile from tile type. Maybe you forgot to add a new tile?");

                    _tilemap.SetTile(p, tile);
                }
    }

    private void SetTile()
    {

    }

}
