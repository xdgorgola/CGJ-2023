using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


[RequireComponent(typeof(BoardComponent))]
public class BoardController : MonoBehaviour
{

    [SerializeField]
    private bool _allowFreeBranching = false;

    [Tooltip("Only used when Allowing free branching")]
    [Range(1, 7)]
    [SerializeField]
    private int _nBranches = 1;

    // Board implementing game logic
    private BoardComponent _board;

    public UnityEvent OnRootCreated = new UnityEvent();
    public UnityEvent OnRootCancel = new UnityEvent();

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

    private void Update()
    {
        if (_allowFreeBranching && _state == RootCreationState.None && Input.GetKeyDown(KeyCode.Space))
            StartCoroutine(StartRootCreation(_nBranches));
        if (_allowFreeBranching && _state != RootCreationState.None && Input.GetKeyDown(KeyCode.Escape))
            StopRootCreation();
    }

    public void StopRootCreation()
    {
        if (_state != RootCreationState.None && OnRootCancel != null)
            OnRootCancel.Invoke();

        _state = RootCreationState.None;
    }

    public IEnumerator StartRootCreation(int nBranches = 1, bool allowMid = false)
    {
        // Mark where you can find root endpoints
        _board.MarkRootEndPoints(allowMid : allowMid);
        _state = RootCreationState.SelectingRoot;

        while (_state != RootCreationState.None)
        {
            // Get Mouse input
            Vector2 mousePos = GetMousePositionInWorld();

            if (Input.GetKeyDown(KeyCode.Mouse0) && _board.IsRootEndpoint(mousePos, allowMid)) // Selected a valid position
            {
                // start new coroutine and exit
                _state = RootCreationState.None;
                StartCoroutine(StartNextTileSelection(mousePos, nBranches, allowMid));
                break;
            }

            yield return null;
        }

        _board.MarkRootEndPoints(false);
    }

    public IEnumerator StartNextTileSelection(Vector2 startPos, int nBranchs = 1, bool allowMid = false)
    {
        Debug.Assert(1 <= nBranchs && nBranchs <= 8, "Can't branch more than 8 times or less than 1");
        _state = RootCreationState.ExtendingRoot;
        _board.MarkRootableTiles(true, allowMid);
        HashSet<Vector2Int> positionsToPlaceRoot = new();


        Vector2Int startPosBoardCoords = _board.WorldPosToBoardPos(startPos) ?? Vector2Int.zero;
        for (int i = 0; i < nBranchs && _state != RootCreationState.None; i++)
        {
            LineRenderer lineRender = _lineRenderForChoosingTiles[i].GetComponent<LineRenderer>();
            lineRender.enabled = true;
            lineRender.SetPosition(0, startPos);

            while (_state != RootCreationState.None)
            {
                Vector2 nextPosition = GetMousePositionInWorld();
                lineRender.SetPosition(1, nextPosition);

                Vector2Int? boardPosition = _board.WorldPosToBoardPos(nextPosition);

                if (
                    boardPosition is Vector2Int v &&
                    Input.GetKeyDown(KeyCode.Mouse0) &&
                    _board.CanPlaceRootInCell(nextPosition, startPos, allowMid: allowMid) &&
                    !positionsToPlaceRoot.Contains(v)
                    )
                {
                    Vector3 cellPos = _board.BoardPosToTilemapPos(v.x, v.y);
                    lineRender.SetPosition(1, cellPos + new Vector3(0.5f, 0.5f, 0f));
                    positionsToPlaceRoot.Add(v);
                    break;
                }

                yield return null;
            }
        }

        // Now that we selected every branch position, we actually send them
        foreach (Vector2Int pos in positionsToPlaceRoot)
            _board.SetCellToRoot(pos.x, pos.y, startPosBoardCoords.x, startPosBoardCoords.y, removeOriginalRoot: false, allowMid: allowMid);

        // Turn off old renders so that new ones can replace them
        foreach (var renderHolder in _lineRenderForChoosingTiles)
            renderHolder.GetComponent<LineRenderer>().enabled = false;

        if (positionsToPlaceRoot.Count > 0)
            _board.SetEndpointToRoot(startPosBoardCoords.x, startPosBoardCoords.y);

        _board.MarkRootableTiles(false, allowMid);
        _state = RootCreationState.None;

        if (OnRootCreated != null)
            OnRootCreated.Invoke();
    }

    private Vector2 GetMousePositionInWorld() => _board.cam.ScreenToWorldPoint(Input.mousePosition);
}
