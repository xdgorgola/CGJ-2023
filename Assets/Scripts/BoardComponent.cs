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
        Invisible = 3,
        Root = 4,
        RootEndpoint = 5,
        White = 6
    }

    public enum Layers
    {
        Ground = 0,
        Roots = 1, 
        Visibility = 2,
        Playable = 3,
        Overlays = 4
    }

    // -- < Internal logic > ------------------------------------------------
    private TileTypes[,,] _board;

    // Size of board
    private int _boardWidth;
    private int _boardHeight;

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
    [SerializeField]
    private Tilemap _overLays;

    public bool _hidePlayableArea = true;


    [SerializeField]
    private Tile _defaultDirtTile;
    [SerializeField]
    private Tile[] _dirtTiles;
    private HashSet<Tile> _dirtSet;

    [SerializeField]
    private Tile _defaultRockTile;
    [SerializeField]
    private Tile[] _rockTiles;
    private HashSet<Tile> _rockSet;

    [SerializeField]
    private Tile _defaultObscureTile;  // Tile style when you're not able to see actual content of layer
    [SerializeField]
    private Tile[] _obscureTiles;
    private HashSet<Tile> _obscureSet;

    [SerializeField]
    private Tile _rootTile;
    [SerializeField]
    private Tile _rootTileEndpoint;

    [SerializeField]
    private Tile _whiteTile;

    [SerializeField]
    private GameObject _grid;         // Grid containing tilemap
    [SerializeField]
    private Camera _cam;

    private List<(int, int)> _rootEndpoints; // Endpoint positions inside the matrix
 

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
        Debug.Assert(_defaultDirtTile != null, "Missing Dirt Tile property in board object");
        Debug.Assert(_defaultRockTile != null, "Missing Rock Tile property in board object");
        Debug.Assert(_rootTile != null, "Missing Root Tile property in board object");
        Debug.Assert(_rootTileEndpoint != null, "Missing Root Tile Endpoint property in board object");
        Debug.Assert(_whiteTile != null, "Missing white Tile property in board object");
        Debug.Assert(_grid != null, "Missing Grid property in board object");
        Debug.Assert(_cam != null, "Missing camera property in board object");

        // Hide playable area, the player doesn't need to know about it
        if (_hidePlayableArea)
            _playableTilemap.gameObject.SetActive(false);

        // Initialize systems
        InitBoard();
        InitRootSystem();
    }

    // Update is called once per frame
    void Update()
    {
        foreach(var (rootI, rootJ) in _rootEndpoints)
            for (int i = 0; i < _boardHeight; i++)
                for(int j = 0; j < _boardWidth; j++)
                    if(CanPlaceRootInCell(i,j, rootI, rootJ))
                    {
                        var position = BoardIndexToTilemapPosition(i, j);
                        SetTileAndRenderIt(i, j, TileTypes.White, Layers.Overlays);
                        _overLays.SetTileFlags(position, TileFlags.None);
                        _overLays.SetColor(position, new Color(0,1,0,0.5f));
                    }
    }

    private void InitBoard()
    {
        // Init set of tiles
        _dirtSet = new HashSet<Tile>();
        foreach (var tile in _dirtTiles)
            _dirtSet.Add(tile);

        _rockSet = new HashSet<Tile>();
        foreach (var tile in _rockTiles)
            _rockSet.Add(tile);

        _obscureSet = new HashSet<Tile>();
        foreach (var tile in _obscureTiles)
            _obscureSet.Add(tile);


        // We need to initialize board height and width by querying the playable area what size does it have
        Vector3Int size = _playableTilemap.size;
        _boardHeight = size.y;
        _boardWidth = size.x;

        _origin = new Vector3Int(_playableTilemap.cellBounds.xMin, _playableTilemap.cellBounds.yMax, 0) + Vector3Int.down;
        _playableTilemap.SetTile(_origin, _defaultRockTile);

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

    private bool DestroyRock(Vector2 worldPosition)
    {
        return false;
    }

    private bool DiscoverCell(Vector2 worldPositon)
    {
        return false;
    }


    private void SetUpTilemap()
    {
        // antes aqui hacia lo de cuadrar la escala, aqui creo que haremos el 
        // tema del aspect ratio

        
    }

    private Tile TypeToTile(TileTypes type)
    {
        switch (type)
        {
            case TileTypes.Dirt:
                return _defaultDirtTile;
            case TileTypes.Rock:
                return _defaultRockTile;
            case TileTypes.Nothing:
                return null;
            case TileTypes.Invisible:
                return _defaultObscureTile;
            case TileTypes.White:
                return _whiteTile;
            default:
                Debug.LogError("Unknown Type of Tile. Maybe you forgot to handle a new type of tile?");
                break;
        }

        return null;
    }

    private TileTypes TileToType(Tile tile)
    {
        if (_dirtSet.Contains(tile))
            return TileTypes.Dirt;
        if (tile == null)
            return TileTypes.Nothing;
        if (_rockSet.Contains(tile))
            return TileTypes.Rock;
        if (_obscureSet.Contains(tile))
            return TileTypes.Invisible;
        if (tile == _rootTile)
            return TileTypes.Root;
        if (tile == _rootTileEndpoint)
            return TileTypes.RootEndpoint;
        if (tile == _whiteTile)
            return TileTypes.White;


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
            case Layers.Overlays:
                return _overLays;
            default:
                Debug.LogError("Unknown layer type. Maybe you forgot to handle a new type of tile?");
                break;
        }

        return null;
    }

    private Vector3Int BoardIndexToTilemapPosition(int i, int j) => new(j + _origin.x, -i + _origin.y, _origin.z);


    /// <summary>
    /// Transform from world position to matrix position
    /// </summary>
    /// <param name="worldPosition"> Position in world coordinates, z coordinate is not important </param>
    /// <returns> null if outside of game matrix, (i,j) indices otherwise </returns>
    private Vector2Int? WorldPosToBoardPosition(Vector2 worldPosition)
    {
        Vector3Int cellPosition = _origin - _playableTilemap.WorldToCell(worldPosition);
        cellPosition.x *= -1; // PQC lol
        if (0 <= cellPosition.x && cellPosition.x < _boardWidth && 0 <= cellPosition.y && cellPosition.y < _boardHeight)
        {
            return new Vector2Int(cellPosition.y, cellPosition.x);
        }

        return null;
    }


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

        Tilemap tilemap = GetTilemapOfLayer(layer);

        tilemap.SetTile(position, tile);
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

    /// <summary>
    /// Obscure all board, setting the entire board of obscure tiles filled
    /// </summary>
    private void ObscureBoard()
    {
        for (int i = 0; i < _boardHeight; i++)
            for (int j = 0; j < _boardWidth; j++)
                SetTileAndRenderIt(i, j, TileTypes.Invisible, Layers.Visibility);
    }

    private bool IsRoot(TileTypes type) => type == TileTypes.RootEndpoint || type == TileTypes.Root;

    private bool IsRoot(Tile tile) => tile == _rootTile || tile == _rootTileEndpoint;

    private bool InsideBoard(int i, int j) => 0 <= i && i < _boardHeight && 0 <= j && j < _boardWidth;

    /// <summary>
    /// Initialize root system once every game starts
    /// </summary>
    private void InitRootSystem()
    {
        // Set all cells as not visible
        ObscureBoard();

        _rootEndpoints = new List<(int, int)>();
        // Clear invisible cells where there's root
        for(int i = 0; i < _boardHeight; i++)
            for(int j = 0; j < _boardWidth; j++)
            {
                if (IsRoot(_board[(int) Layers.Roots,i,j]))
                {
                    for (int a = -1; a < 2; a++)
                        for (int b = -1; b < 2; b++)
                            if (InsideBoard(a + i, b + j))
                                SetTileAndRenderIt(a + i, b + j, TileTypes.Nothing, Layers.Visibility);


                }

                // Find all root starting points
                if (_board[(int)Layers.Roots, i,j] == TileTypes.RootEndpoint)
                    _rootEndpoints.Add((i, j));
            }
    }


    private bool CanPlaceRootInCell(int i, int j, int fromI, int fromJ, int reach = 1)
    {
        // from position is inside board and is a root endpoint?
        if (!InsideBoard(fromI, fromJ) || _board[(int)Layers.Roots, fromI, fromJ] != TileTypes.RootEndpoint)
            return false; // origin is not a valid start

        // Cell is empty and inside the board?
        if (_board[(int)Layers.Roots, i, j] != TileTypes.Nothing && InsideBoard(i,j))
            return false; // not empty, can't place it there

        // Now we have to check if the specified position can be reached from the
        // starting position using the specified reach
        for(int a = -1; a < 2; a ++)
            for(int b = -1; b < 2; b++)
            {
                if (a == b && a == 0 || !InsideBoard(i + a, j + b))
                    continue;

                // The following computation comes from solving the following equation:
                // (i,j) = (fromI, fromJ) + k * d
                // Where k is only integer when i,j it's a valid cell.
                // d is a direction in the board

                for (int r = 1; r <= reach; r++)
                {
                    int newI, newJ;
                    newI = fromI + a * r;
                    newJ = fromJ + b * r;
                    if (InsideBoard(newI, newJ) && newI == i && newJ == j && _board[(int)Layers.Roots, newI, newJ] == TileTypes.Nothing)
                        return true;
                }

            }

        return false;
    }

    private bool SetCellToRoot(int i,int j, int fromI, int fromJ, int reach = 1)
    {
        if (!CanPlaceRootInCell(i, j, fromI, fromJ, reach))
            return false; // if can't place root here, just don't

        SetTileAndRenderIt(i, j, TileTypes.RootEndpoint, Layers.Roots);
        SetTileAndRenderIt(fromI, fromJ, TileTypes.Root, Layers.Roots);

        // Now we have to clear cells around new root 
        for (int a = -1; a < 2; a++)
            for(int b = -1; b < 2; b++)
            {
                int newI, newJ;
                newI = i + a;
                newJ = j + b;

                if (InsideBoard(newI, newJ))
                    SetTileAndRenderIt(newI, newJ, TileTypes.Invisible, Layers.Visibility);
            }

        return true;
    }

}
