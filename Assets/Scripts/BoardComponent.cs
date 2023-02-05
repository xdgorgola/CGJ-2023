using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;

[RequireComponent(typeof(LineRenderer))]
public class BoardComponent : MonoBehaviour
{
    
    public struct Collected
    {
        public Collected(int water, int nutrients)
        {
            this.water = water;
            this.nutrients = nutrients;
        }

        public readonly int water; 
        public readonly int nutrients; 
    }

    public enum TileTypes
    {
        Dirt = 0,
        Rock = 1,
        Nothing = 2,
        Invisible = 3,
        Root = 4,
        RootEndpoint = 5,
        White = 6,
        Water = 7, 
        Nutrients = 8
    }

    public enum Layers
    {
        Ground = 0,
        Roots = 1, 
        Visibility = 2,
        Playable = 3,
        Overlays = 4, 
        Pickables = 5 // Water, nutrients
    }

    // -- < Internal logic > ------------------------------------------------
    private TileTypes[,,] _board;
    private int[,] _pickableAmount;

    [Tooltip("Amount of content of water/nutrients from the start")]
    [Range(1, 100000)]
    [SerializeField]
    private int _defaultPickableAmount = 100;

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
    [SerializeField]
    private Tilemap _pickables;

    [SerializeField]
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
    private Tile _waterTile;
    [SerializeField]
    private Tile _nutrientsTile;

    [SerializeField]
    private GameObject _grid;         // Grid containing tilemap
    [SerializeField]
    private Camera _cam;
    public Camera cam { get { return _cam;  } }

    private HashSet<(int, int)> _rootEndpoints; // Endpoint positions inside the matrix
    private Vector3Int _origin; // Top left corner of tilemap

    [Tooltip("Color the tiles will be tinted to when they can be used to add a new root")]
    [SerializeField]
    private Color _rootableTilesColor = new Color(0, 1, 0, 0.25f);

    // Used to be cloned and rendered when putting a new root
    private LineRenderer _lineRender;

    // Line renders generated for each position, they're null when there's no root in that cell
    private GameObject[,] _lineRenderObjects; 

    private void Awake()
    {
        _lineRender = GetComponent<LineRenderer>();
    }

    // Start is called before the first frame update
    void Start()
    {
        // Sanity checks
        Debug.Assert(_groundTilemap != null, "Missing Ground Tilemap component from board object ");
        Debug.Assert(_rootTilemap != null, "Missing Root Tilemap component from board object");
        Debug.Assert(_visibilityTilemap != null, "Missing visibility Tilemap component from board object");
        Debug.Assert(_playableTilemap != null, "Missing playable Tilemap component from board object");
        Debug.Assert(_pickables != null, "Missing pickable Tilemap component from board object");
        Debug.Assert(_defaultDirtTile != null, "Missing Dirt Tile property in board object");
        Debug.Assert(_defaultRockTile != null, "Missing Rock Tile property in board object");
        Debug.Assert(_rootTile != null, "Missing Root Tile property in board object");
        Debug.Assert(_rootTileEndpoint != null, "Missing Root Tile Endpoint property in board object");
        Debug.Assert(_whiteTile != null, "Missing white Tile property in board object");
        Debug.Assert(_waterTile != null, "Missing water Tile property in board object");
        Debug.Assert(_nutrientsTile != null, "Missing nutrients Tile property in board object");
        Debug.Assert(_grid != null, "Missing Grid property in board object");
        Debug.Assert(_cam != null, "Missing camera property in board object");


        // Hide playable area, the player doesn't need to know about it
        if (_hidePlayableArea)
            _playableTilemap.gameObject.SetActive(false);

        // Initialize systems
        InitBoard();
        InitRootSystem();
    }

    /// <summary>
    /// Update state of pickables in board. Take a specified amount of water and nutrients in every
    /// position with some pickable. Return collected resources at the end.
    /// </summary>
    /// <param name="waterToCollect">Amount of water to collect</param>
    /// <param name="nutrientToCollect">Amount of nutrients to collect</param>
    /// <returns>Collected resources</returns>
    public Collected Tick(int waterToCollect, int nutrientToCollect)
    {
        int collectedWater = 0;
        int collectedNutr = 0;
        for(int i = 0; i < _boardHeight; i ++)
            for(int j = 0; j < _boardWidth; j++)
            {
                TileTypes typeOfCell = GetTypeOfCell(i, j, Layers.Pickables);
                if ( typeOfCell == TileTypes.Nothing || !IsRootCell(i,j))
                    continue;

                // Choose how much we will take from this cell
                int amountToCollect = 0;
                switch (typeOfCell)
                {
                    case TileTypes.Water:
                        amountToCollect = waterToCollect;
                        break;
                    case TileTypes.Nutrients:
                        amountToCollect = nutrientToCollect;
                        break;
                    default:
                        Debug.LogError("Invalid type of tile");
                        break;
                }

                // This min prevents result from going bellow zero
                int pickedAmount = Mathf.Min(_pickableAmount[i, j], amountToCollect);
                _pickableAmount[i,j] -= pickedAmount;

                // choose where to store the picked amount
                switch (typeOfCell)
                {
                    case TileTypes.Water:
                        collectedWater += pickedAmount;
                        break;
                    case TileTypes.Nutrients:
                        collectedNutr += pickedAmount;
                        break;
                    default:
                        Debug.LogError("Invalid type of tile");
                        break;
                }

                // Clear this cell if there are no resources remaining
                if (_pickableAmount[i, j] == 0)
                    SetTileAndRenderIt(i, j, TileTypes.Nothing, Layers.Pickables);
            }

        return new Collected(collectedWater, collectedNutr);
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
        _pickableAmount = new int[_boardHeight, _boardWidth];

        // Now we have to fill the internal board with the initial state of tilemaps
        for (int i = 0; i < _boardHeight; i++)
            for (int j = 0; j < _boardWidth; j++)
            {
                Tile groundTile = (Tile) _groundTilemap.GetTile(BoardPosToTilemapPos(i, j));
                Tile rootTile   = (Tile) _rootTilemap.GetTile(BoardPosToTilemapPos(i, j));
                Tile visibilityTile  = (Tile) _visibilityTilemap.GetTile(BoardPosToTilemapPos(i, j));
                Tile pickableTile  = (Tile) _pickables.GetTile(BoardPosToTilemapPos(i, j));
                _board[(int)Layers.Ground, i,j] = TileToType(groundTile);
                _board[(int)Layers.Roots, i, j] = TileToType(rootTile);
                _board[(int)Layers.Visibility, i, j] = TileToType(visibilityTile);
                _board[(int)Layers.Pickables, i, j] = TileToType(pickableTile);

                // Also if this cell has a pickable, add its corresponding amount to the 
                if (GetTypeOfCell(i, j, Layers.Pickables) != TileTypes.Nothing)
                    _pickableAmount[i,j] = _defaultPickableAmount;
            }
    }

    /// <summary>
    /// Try to destroy a Rock in the specified position. 
    /// </summary>
    /// <param name="worldPosition">position of the world matching a cell in the grid</param>
    /// <returns>false if no action was performed or position is invalid, true otherwise</returns>
    public bool DestroyRock(Vector2 worldPosition)
    {
        Vector2Int? boardPos = WorldPosToBoardPos(worldPosition);
        if (boardPos is Vector2Int v && GetTypeOfCell(v.x, v.y) == TileTypes.Rock && GetTypeOfCell(v.x, v.y, Layers.Visibility) != TileTypes.Invisible)
        {
            SetTile(v.x, v.y, TileTypes.Dirt);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Try to discover a hidden cell in the specified position. 
    /// </summary>
    /// <param name="worldPosition">position of the world matching a cell in the grid</param>
    /// <returns>false if no action was performed or position is invalid, true otherwise</returns>
    public bool DiscoverCell(Vector2 worldPosition)
    {
        Vector2Int? boardPos = WorldPosToBoardPos(worldPosition);
        if (boardPos is Vector2Int v && GetTypeOfCell(v.x, v.y, Layers.Visibility) == TileTypes.Invisible)
        {
            SetTile(v.x, v.y, TileTypes.Nothing, Layers.Visibility);
            return true;
        }

        return false;
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
            case TileTypes.RootEndpoint:
                return _rootTileEndpoint;
            case TileTypes.Root:
                return _rootTile;
            case TileTypes.Water:
                return _waterTile;
            case TileTypes.Nutrients:
                return _nutrientsTile;
            default:
                Debug.LogError("Unknown Type of Tile. Maybe you forgot to handle a new type of tile?");
                break;
        }

        return null;
    }

    private TileTypes GetTypeOfCell(int i, int j, Layers layer = Layers.Ground) =>_board[(int)layer, i, j];    

    private TileTypes TileToType(Tile tile)
    {
        if (_dirtSet.Contains(tile))
            return TileTypes.Dirt;
        else if (tile == null)
            return TileTypes.Nothing;
        else if (_rockSet.Contains(tile))
            return TileTypes.Rock;
        else if (_obscureSet.Contains(tile))
            return TileTypes.Invisible;
        else if (tile == _rootTile)
            return TileTypes.Root;
        else if (tile == _rootTileEndpoint)
            return TileTypes.RootEndpoint;
        else if (tile == _whiteTile)
            return TileTypes.White;
        else if (tile == _waterTile)
            return TileTypes.Water;
        else if (tile == _nutrientsTile)
            return TileTypes.Nutrients;

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
            case Layers.Pickables:
                return _pickables;
            default:
                Debug.LogError("Unknown layer type. Maybe you forgot to handle a new type of tile?");
                break;
        }

        return null;
    }

    public Vector3Int BoardPosToTilemapPos(int i, int j) => new Vector3Int(j + _origin.x, -i + _origin.y, _origin.z);

    /// <summary>
    /// Transform from world position to matrix position
    /// </summary>
    /// <param name="worldPosition"> Position in world coordinates, z coordinate is not important </param>
    /// <returns> null if outside of game matrix, (i,j) indices otherwise </returns>
    public Vector2Int? WorldPosToBoardPos(Vector2 worldPosition)
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

                    Vector3Int p = BoardPosToTilemapPos(i,j);
                    var type = _board[layer, i, j];
                    var tile = TypeToTile(type);
                    var tilemap = GetTilemapOfLayer((Layers) layer);
                    tilemap.SetTile(p, tile);
                }
    }

    private void SetTileAndRenderIt(int i, int j, TileTypes type, Layers layer = Layers.Ground)
    {
        SetTile(i, j, type, layer);

        var position = BoardPosToTilemapPos(i, j);
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
        _lineRenderObjects = new GameObject[_boardHeight, _boardWidth];

        // Use line render to define where's the first root, use it's endpoint in particular
        Debug.Assert(_lineRender.positionCount == 2, "Initial line render for root rendering should have just two positions, one of them being the starting point");
        Vector3 worldPosOfRoot = _lineRender.GetPosition(1);
        Vector2Int? positionOfRoot = WorldPosToBoardPos(worldPosOfRoot);
        Vector2Int boardPosOfRoot = positionOfRoot ?? Vector2Int.zero;

        // Sanity check
        Debug.Assert(positionOfRoot != null, "Default root position specified by line render's second point is not a valid board position");

        // Set root to the specified position
        SetTileAndRenderIt(boardPosOfRoot.x, boardPosOfRoot.y, TileTypes.RootEndpoint, Layers.Roots);

        // Set all cells as not visible
        ObscureBoard();

        _rootEndpoints = new HashSet<(int, int)>();
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
        
        // make root tilemap invisible
        for(int i = 0; i < _boardHeight; i++)
            for(int j = 0; j < _boardWidth; j++)
            {
                var position = BoardPosToTilemapPos(i, j);
                _rootTilemap.SetTileFlags(position, TileFlags.None);
                _rootTilemap.SetColor(position, new Color(0,0,0,0));
            }
    }

    public bool ValidPosition(Vector2 worldPos)
    {
        Vector2Int? boardCoords = WorldPosToBoardPos(worldPos);

        if (boardCoords is Vector2Int v)
            return InsideBoard(v.x, v.y);

        return false;
    }

    public bool IsRootEndpoint(Vector2 worldPos)
    {
        Vector2Int? boardCoords = WorldPosToBoardPos(worldPos);

        if (boardCoords is Vector2Int v)
            return IsRootEndpoint(v.x, v.y);

        return false;
    }
    
    public bool IsRootCell(int i, int j)
    {
        TileTypes typeOfTile = GetTypeOfCell(i, j, Layers.Roots);
        return typeOfTile == TileTypes.Root || typeOfTile == TileTypes.RootEndpoint;
    }

    public bool IsRootEndpoint(int i, int j) => InsideBoard(i, j) && GetTypeOfCell(i, j, Layers.Roots) == TileTypes.RootEndpoint;

    public bool IsBlocked(int i, int j)
    {
        return _board[(int) Layers.Ground,i,j] == TileTypes.Rock || _board[(int) Layers.Roots,i,j] != TileTypes.Nothing;
    }

    public bool CanPlaceRootInCell(Vector2 worldPosTo, Vector2 worldPosFrom, int reach = 1)
    {
        // convert both positions to board coordinates
        Vector2Int? boardPosTo = WorldPosToBoardPos(worldPosTo);
        Vector2Int? boardPosFrom = WorldPosToBoardPos(worldPosFrom);

        if (boardPosTo is Vector2Int v0 && boardPosFrom is Vector2Int v1)
            return CanPlaceRootInCell(v0.x, v0.y, v1.x, v1.y, reach);

        return false;
    }

    public bool CanPlaceRootInCell(int i, int j, int fromI, int fromJ, int reach = 1)
    {
        // from position is inside board and is a root endpoint?
        if (!InsideBoard(fromI, fromJ) || !IsRootEndpoint(fromI, fromJ))
            return false; // origin is not a valid start

        // Cell is empty and inside the board?
        if (!InsideBoard(i,j) || IsBlocked(i,j) || IsRootCell(i,j))
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

    public void ClearCellsAround(int i, int j, int reach = 1)
    {
        for(int a = -reach; a <= reach; a++)
            for(int b = -reach; b <= reach; b++)
            {
                int newI, newJ;
                newI = i + a;
                newJ = j + b;
                if (InsideBoard(newI, newJ))
                    SetTileAndRenderIt(newI, newJ, TileTypes.Nothing, Layers.Visibility);
            }
    }

    public bool SetCellToRoot(Vector2 worldPosTo, Vector2 worldPosFrom, int reach = 1)
    {
        if (WorldPosToBoardPos(worldPosTo) is Vector2Int v0 && WorldPosToBoardPos(worldPosFrom) is Vector2Int v1)
            return SetCellToRoot(v0.x, v0.y, v1.x, v1.y, reach);

        return false;
    }

    public bool SetCellToRoot(int i,int j, int fromI, int fromJ, int reach = 1, bool removeOriginalRoot = true)
    {
        if (!CanPlaceRootInCell(i, j, fromI, fromJ, reach))
            return false; // if can't place root here, just don't

        SetTile(i, j, TileTypes.RootEndpoint, Layers.Roots);

        // TODO this is gonna crash when reach > 1 since we're not checking for intermediate cells

        if (removeOriginalRoot && false)
        {
            SetTile(fromI, fromJ, TileTypes.Root, Layers.Roots);
            _rootEndpoints.Remove((fromI, fromJ));
        }

        _rootEndpoints.Add((i, j));

        // Now we have to clear cells around new root 
        ClearCellsAround(i, j);

        // Finally, we will render a new line as root. Such line will start in the 
        // from position.
        GameObject newLineHolder = new GameObject($"Line holder ({fromI}, {fromJ})");
        LineRenderer lineRenderer = newLineHolder.AddComponent<LineRenderer>();

        _lineRenderObjects[fromI, fromJ] = newLineHolder;

        SetUpLineRender(lineRenderer);

        // Specify endpoints for new line renderer
        Vector3 startingPosition = BoardPosToTilemapPos(fromI, fromJ) + new Vector3(0.5f, 0.5f, 0);
        Vector3 endPosition = BoardPosToTilemapPos(i, j) + new Vector3(0.5f, 0.5f, 0);
        
        lineRenderer.SetPositions(new Vector3[] { startingPosition, endPosition });

        return true;
    }

    public void SetEndpointToRoot(int i, int j)
    {
        if (GetTypeOfCell(i, j, Layers.Roots) != TileTypes.RootEndpoint)
            return;

        SetTile(i, j, TileTypes.Root, Layers.Roots);
        _rootEndpoints.Remove((i, j));
    }

    /// <summary>
    /// Mark tiles as rootable when requested so 
    /// </summary>
    /// <param name="active">if tiles should be colored as active or turn invisible as unactive</param>
    public void MarkRootableTiles(bool active = true)
    {
        Color color = _rootableTilesColor;
        if (!active)
            color.a = 0;

        foreach (var (rootI, rootJ) in _rootEndpoints)
            for (int i = 0; i < _boardHeight; i++)
                for (int j = 0; j < _boardWidth; j++)
                    if (!active || CanPlaceRootInCell(i, j, rootI, rootJ)) // when active es false, this will be true for any cell
                    {
                        var position = BoardPosToTilemapPos(i, j);
                        SetTileAndRenderIt(i, j, TileTypes.White, Layers.Overlays);
                        _overLays.SetTileFlags(position, TileFlags.None);
                        _overLays.SetColor(position, color);
                    }
    }

    public void MarkRootEndPoints(bool active = true)
    {
        Color color = _rootableTilesColor;
        if (!active)
            color.a = 0;

        foreach (var (rootI, rootJ) in _rootEndpoints)
        {
            var position = BoardPosToTilemapPos(rootI, rootJ);
            SetTileAndRenderIt(rootI, rootJ, TileTypes.White, Layers.Overlays);
            _overLays.SetTileFlags(position, TileFlags.None);
            _overLays.SetColor(position, color);
        }
    }

    /// <summary>
    /// Try to split a root in position i,j in board
    /// </summary>
    /// <returns>If could actually split</returns>
    public bool SplitRoot(int rootI, int rootJ, int newRoot0i, int newRoot0j, int newRoot1i, int newRoot1j, int reach = 1)
    {
        // Check if all positions are inside the board and two new positions are possible and different
        if (!InsideBoard(rootI, rootJ) || !InsideBoard(newRoot0i, newRoot0j) || !InsideBoard(newRoot1i, newRoot1j) || (newRoot0i == newRoot1i && newRoot0j == newRoot1j))
            return false;

        // Check that splitting in root i,j is possible: In root i,j there's a root endpoint
        // and new root positions are rootable
        if (
            GetTypeOfCell(rootI, rootJ, Layers.Roots) != TileTypes.RootEndpoint || 
            !CanPlaceRootInCell(newRoot0i, newRoot0i, rootI, rootJ, reach) || 
            !CanPlaceRootInCell(newRoot1i, newRoot1i, rootI, rootJ, reach)
            )
            return false;

        // Now that we're sure that these are valid positions to place new roots, then place it and replace old root
        SetTile(rootI, rootJ, TileTypes.Root, Layers.Roots);
        _rootEndpoints.Remove((rootI, rootJ));

        SetTile(newRoot0i, newRoot0j, TileTypes.RootEndpoint);
        _rootEndpoints.Add((newRoot0i, newRoot0j));
        SetTile(newRoot1i, newRoot1j, TileTypes.RootEndpoint);
        _rootEndpoints.Add((newRoot1i, newRoot1j));

        ClearCellsAround(newRoot0i, newRoot0j);
        ClearCellsAround(newRoot1i, newRoot1j);

        return true;
    }

    /// <summary>
    /// Set up the provided line render so that its configs match the root line render
    /// </summary>
    /// <param name="lineRenderer"></param>
    public void SetUpLineRender(LineRenderer lineRenderer)
    {
        var myLineRender = GetComponent<LineRenderer>();

        lineRenderer.colorGradient = myLineRender.colorGradient;
        lineRenderer.widthCurve = myLineRender.widthCurve;
        lineRenderer.numCapVertices = myLineRender.numCapVertices;
        lineRenderer.numCornerVertices = myLineRender.numCornerVertices;
        lineRenderer.shadowCastingMode = myLineRender.shadowCastingMode;
        lineRenderer.sortingOrder = myLineRender.sortingOrder;
        lineRenderer.material = myLineRender.material;
    }
}
