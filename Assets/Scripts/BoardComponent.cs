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
        Visibility = 2,
        Playable = 3
    }

    // -- < Internal logic > ------------------------------------------------
    private TileTypes[,,] _board;

    // Size of board
    private int _boardWidth;
    private int _boardHeight;

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
    private Tilemap _playableTilemap;
    public bool _hidePlayableArea = true;


    [SerializeField]
    private Tile _dirtTile;
    [SerializeField]
    private Tile _rockTile;
    [SerializeField]
    private Tile _obscureTile;  // Tile style when you're not able to see actual content of layer
    [SerializeField]
    private GameObject _grid;         // Grid containing tilemap
    [SerializeField]
    private Camera _cam;
    private Rect _cameraRect;


    private Vector3Int _origin; // Top left corner of tilemap

    private void Awake()
    {
        // Properly set up origin first thing
        SetUpTilemap();
    }

    // Start is called before the first frame update
    void Start()
    {
        // Sanity checks
        Debug.Assert(_groundTilemap != null, "Missing Ground Tilemap component from board object ");
        Debug.Assert(_rootTilemap != null, "Missing Root Tilemap component from board object");
        Debug.Assert(_visibilityTilemap != null, "Missing visibility Tilemap component from board object");
        Debug.Assert(_playableTilemap != null, "Missing playable Tilemap component from board object");
        Debug.Assert(_dirtTile != null, "Missing Dirt Tile property in board object");
        Debug.Assert(_rockTile != null, "Missing Rock Tile property in board object");
        Debug.Assert(_grid != null, "Missing Grid property in board object");
        Debug.Assert(_cam != null, "Missing camera property in board object");

        // Hide playable area, the player doesn't need to know about it
        if (_hidePlayableArea)
            _playableTilemap.gameObject.SetActive(false);

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
        // We need to initialize board height and width by querying the playable area what size does it have
        Vector3Int size = _playableTilemap.size;
        _boardHeight = size.y;
        _boardWidth = size.x;

        _origin = new Vector3Int(_playableTilemap.cellBounds.xMin, _playableTilemap.cellBounds.yMax, 0) + Vector3Int.down;
        _playableTilemap.SetTile(_origin, _rockTile);

        _nTileTypes = Enum.GetNames(typeof(TileTypes)).Length;
        _nLayers = Enum.GetNames(typeof(Layers)).Length;
        _board = new TileTypes[_nLayers, _boardHeight, _boardWidth];

        // Now we have to fill the internal board with the initial state of tilemaps
        for (int i = 0; i < _boardHeight; i++)
            for (int j = 0; j < _boardWidth; j++)
            {
                Tile groundTile = (Tile) _groundTilemap.GetTile(BoardIndexToTilemapPosition(i, j));
                Tile rootTile   = (Tile) _rootTilemap.GetTile(BoardIndexToTilemapPosition(i, j));
                Tile visibilityTile  = (Tile) _visibilityTilemap.GetTile(BoardIndexToTilemapPosition(i, j));
                _board[(int)Layers.Ground, i,j] = TileToType(groundTile);
                _board[(int)Layers.Roots, i, j] = TileToType(rootTile);
                _board[(int)Layers.Visibility, i, j] = TileToType(visibilityTile);
            }
    }

    private void SetUpTilemap()
    {
        
        
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

    private TileTypes TileToType(Tile tile)
    {
        if (tile == _dirtTile)
            return TileTypes.Dirt;
        if (tile == null)
            return TileTypes.Nothing;
        if (tile == _rockTile)
            return TileTypes.Rock;
        if (tile == _obscureTile)
            return TileTypes.Invisible;

        Debug.LogError("Unknown tile");
        return TileTypes.Nothing;
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
            case Layers.Playable:
                return _playableTilemap;
            default:
                Debug.LogError("Unknown layer type. Maybe you forgot to handle a new type of tile?");
                break;
        }

        return null;
    }

    private Vector3Int BoardIndexToTilemapPosition(int i, int j) => new(j + _origin.x, -i + _origin.y, _origin.z);

    /// <summary>
    /// Synch tilemap to match state of board
    /// </summary>
    private void UpdateTileMap()
    {
        for (int layer = 0; layer < _nLayers; layer++)
            for(int i = 0; i < _boardHeight; i++)
                for (int j = 0; j < _boardWidth; j++)
                {
                    if (layer == (int)Layers.Playable) continue;

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

    private void OnDrawGizmos()
    {
        var camRect = _cam.rect;
        var bottonLeftCamRect = _cam.ViewportToWorldPoint(new Vector3(camRect.xMin, camRect.yMin, -10));
        var topRightCamRect = _cam.ViewportToWorldPoint(new Vector3(camRect.xMax, camRect.yMax, -10));
        var sizeOfRect = topRightCamRect - bottonLeftCamRect;
        sizeOfRect.z = 2;
        Gizmos.color = Color.red;
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(_groundTilemap.CellToWorld(_origin), 0.2f);
    }

}
