using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoardComponent))]
public class BoardController : MonoBehaviour
{

    // Board implementing game logic
    private BoardComponent _board;
    private Coroutine _startRootCreation;

    // Used to draw a line from a cell to another when creating a new root
    private List<GameObject> _lineRenderForChoosingTiles;
    
    // To control which action is beind done right now
    private enum RootCreationState
    {
        None,
        SelectingRoot,
        ExtendingRoot
    }

    private RootCreationState _state = RootCreationState.None;

    private void Awake()
    {
        _lineRenderForChoosingTiles = new List<GameObject>();
        _board = GetComponent<BoardComponent>();

        for (int i = 0; i < 8; i++)
        {
            _lineRenderForChoosingTiles.Add(new GameObject($"Line render for choosing tile {i}"));
            LineRenderer lineRender = _lineRenderForChoosingTiles[i].AddComponent<LineRenderer>();
            _board.SetUpLineRender(lineRender);
            lineRender.enabled = false;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && _state == RootCreationState.None)
        {
            _startRootCreation = StartCoroutine(StartRootCreation());
        }
    }

    IEnumerator StartRootCreation(int nBranches = 1)
    {
        // Mark where you can find root endpoints
        _board.MarkRootEndPoints();
        _state = RootCreationState.SelectingRoot;

        while (true)
        {
            if (Input.GetKeyDown(KeyCode.Escape)) // cancel
            {
                _board.MarkRootEndPoints(false);
                _state = RootCreationState.None;
                break;
            }

            // Get Mouse input
            Vector2 mousePos = GetMousePositionInWorld();

            if (Input.GetKeyDown(KeyCode.Mouse0) && _board.IsRootEndpoint(mousePos)) // Selected a valid position
            {
                _state = RootCreationState.None;
                // start new coroutine and exit
                StartCoroutine(StartNextTileSelection(mousePos, nBranches));
                break;
            }

            yield return null;
        }

        // Check if mouse input is valid and start root extension coroutine if so
    }

    IEnumerator StartNextTileSelection(Vector2 startPos, int nBranchs = 1)
    {
        Debug.Assert(1 <= nBranchs && nBranchs <= 8, "Can't branch more than 8 times or less than 1");
        _state = RootCreationState.ExtendingRoot;
        _board.MarkRootableTiles(true);
        HashSet<Vector2Int> positionsToPlaceRoot = new();


        Vector2Int startPosBoardCoords = _board.WorldPosToBoardPos(startPos) ?? Vector2Int.zero;
        for (int i = 0; i < nBranchs; i++)
        {
            LineRenderer lineRender = _lineRenderForChoosingTiles[i].GetComponent<LineRenderer>();
            lineRender.enabled = true;
            lineRender.SetPosition(0, startPos);

            while(true)
            {
                Vector2 nextPosition = GetMousePositionInWorld();
                lineRender.SetPosition(1, nextPosition);

                if (Input.GetKeyDown(KeyCode.Escape)) // Go back to select root
                {
                    StartCoroutine(StartRootCreation());
                    break;
                }

                Vector2Int? boardPosition = _board.WorldPosToBoardPos(nextPosition);

                if (
                    boardPosition is Vector2Int v && 
                    Input.GetKeyDown(KeyCode.Mouse0) && 
                    _board.CanPlaceRootInCell(nextPosition, startPos) && 
                    !positionsToPlaceRoot.Contains(v)
                    )
                {
                    Vector3 cellPos = _board.BoardPosToTilemapPos(v.x, v.y);
                    lineRender.SetPosition(1, cellPos + new Vector3(0.5f, 0.5f, 0f ));
                    positionsToPlaceRoot.Add(v);
                    break;
                }

                yield return null;
            }
        }

        // Now that we selected every branch position, we actually send them
        foreach(Vector2Int pos in positionsToPlaceRoot)
            _board.SetCellToRoot(pos.x, pos.y, startPosBoardCoords.x, startPosBoardCoords.y, removeOriginalRoot: false);

        // Turn off old renders so that new ones can replace them
        foreach(var renderHolder in _lineRenderForChoosingTiles)
            renderHolder.GetComponent<LineRenderer>().enabled = false;

        if (positionsToPlaceRoot.Count > 0)
            _board.SetEndpointToRoot(startPosBoardCoords.x, startPosBoardCoords.y);

        _board.MarkRootableTiles(false);
        _state = RootCreationState.None;
    }

    private Vector2 GetMousePositionInWorld() => _board.cam.ScreenToWorldPoint(Input.mousePosition);
}
