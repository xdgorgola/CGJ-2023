using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainGame : MonoBehaviour
{
    [SerializeField]
    private BoardComponent _board = null;
    [SerializeField]
    private PlantComponent _plant = null;
    [SerializeField]
    private CardInputUI _cardInput = null;


    private IEnumerator StartGame()
    {
        yield break;
    }


    private IEnumerator DoGameRound()
    {
        yield break;
    }
}
