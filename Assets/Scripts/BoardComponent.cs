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
        Nothing = 2,
        Invisible = 3
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
    private Tilemap _groundTilemap;
    [SerializeField]
    private Tilemap _rootTilemap;
    [SerializeField]
    private Tilemap _visibilityTilemap;

    [SerializeField]
    private Tile _dirtTile;
    [SerializeField]
    private Tile _rockTile;
    [SerializeField]
    private Tile _obscureTile; // Tile style when you're not able to see actual content of layer

    // Start is called before the first frame update
    void Start()
    {
        // Sanity checks
        Debug.Assert(_groundTilemap != null, "Missing Ground Tilemap component from board object ");
        Debug.Assert(_rootTilemap != null, "Missing Root Tilemap component from board object");
        Debug.Assert(_visibilityTilemap != null, "Missing visibility Tilemap component from board object");
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
                _board[(int)Layers.Roots, i, j] = TileTypes.Nothing;
                _board[(int)Layers.Visibility, i, j] = TileTypes.Nothing;
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
            case TileTypes.Nothing:
                return null;
            case TileTypes.Invisible:
                return _obscureTile;
            default:
                Debug.LogError("Unknown Type of Tile. Maybe you forgot to handle a new type of tile?");
                break;
        }

        return null;
    }

    private Tilemap GetTilemapOfLayer(Layers layer)
    {
        switch(layer)
        {
            case Layers.Ground:
                return _groundTilemap;
            case Layers.Roots:
                return _rootTilemap;
            case Layers.Visibility:
                return _visibilityTilemap;
            default:
                Debug.LogError("Unknown layer type. Maybe you forgot to handle a new type of tile?");
                break;
        }

        return null;
    }

    private Vector3Int BoardIndexToTilemapPosition(int i, int j) => new(i - _boardHeight/2,j - _boardWidth/2, 0);

    /// <summary>
    /// Synch tilemap to match state of board
    /// </summary>
    private void UpdateTileMap()
    {
        for (int layer = 0; layer < _nLayers; layer++)
            for(int i = 0; i < _boardHeight; i++)
                for (int j = 0; j < _boardWidth; j++)
                {
                    Vector3Int p = BoardIndexToTilemapPosition(i,j);
                    var type = _board[layer, i, j];
                    var tile = TypeToTile(type);
                    var tilemap = GetTilemapOfLayer((Layers) layer);
                    tilemap.SetTile(p, tile);
                }
    }

    private void SetTileAndRenderIt(int i, int j, TileTypes type, Layers layer = Layers.Ground)
    {
        SetTile(i, j, type, layer);

        var position = BoardIndexToTilemapPosition(i, j);
        var tile = TypeToTile(type);

        _board[(int)layer, i, j] = type;
        _groundTilemap.SetTile(position, tile);
    }

    /// <summary>
    /// Set tile in board matrix.
    /// </summary>
    /// <param name="i">vertical index in board matrix</param>
    /// <param name="j">horizontal index in board matrix</param>
    /// <param name="type">Type of tile to set</param>
    /// <param name="layer">Layer to set</param>
    private void SetTile(int i, int j, TileTypes type, Layers layer = Layers.Ground) => _board[(int)layer, i, j] = type;
    


}
