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
    private GameObject _lineRenderForChoosingTile;
    
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
        _board = GetComponent<BoardComponent>();
        _lineRenderForChoosingTile = new GameObject("Line render for choosing tile");
        LineRenderer lineRender = _lineRenderForChoosingTile.AddComponent<LineRenderer>();
        _board.SetUpLineRender(lineRender);
        lineRender.enabled = false;
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

    IEnumerator StartRootCreation()
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
                StartCoroutine(StartNextTileSelection(mousePos));
                break;
            }

            yield return null;
        }

        // Check if mouse input is valid and start root extension coroutine if so
    }

    IEnumerator StartNextTileSelection(Vector2 startPos)
    {
        _state = RootCreationState.ExtendingRoot;
        _board.MarkRootableTiles(true);

        LineRenderer lineRender = _lineRenderForChoosingTile.GetComponent<LineRenderer>();
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

            if (Input.GetKeyDown(KeyCode.Mouse0) && _board.SetCellToRoot(nextPosition, startPos))
            {
                break;
            }

            yield return null;
        }

        _board.MarkRootableTiles(false);
        _state = RootCreationState.None;
        lineRender.enabled = false;
    }

    private Vector2 GetMousePositionInWorld() => _board.cam.ScreenToWorldPoint(Input.mousePosition);
}
