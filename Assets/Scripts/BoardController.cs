using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoardComponent))]
public class BoardController : MonoBehaviour
{

    // Board implementing game logic
    private BoardComponent _board;

    private void Awake()
    {
        _board = GetComponent<BoardComponent>();
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
