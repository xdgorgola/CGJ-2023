using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class MainGame : MonoBehaviour
{
    // Main systems
    [SerializeField]
    private BoardComponent _board = null;
    [SerializeField]
    private PlantComponent _plant = null;
    [SerializeField]
    private CardInputUI _cardInput = null;
    [SerializeField]
    private DeckScript _deck = null;
    [SerializeField]
    private SeasonChanger _seasonMng = null;

    [SerializeField]
    private Season _startingSeason = Season.Spring;
    private GameCards _oldCard = null;


    private void Awake()
    {
        _seasonMng.ChangeSeason(_startingSeason);
    }


    private IEnumerator StartGame()
    {
        yield return StartCoroutine(InitalRound());

        while (true)
            yield return StartCoroutine(DoGameRound());
    }


    private IEnumerator InitalRound()
    {
        RequestCardsForDeck();
        yield return new WaitForSeconds(1f);

        _cardInput.EnableSystem();
        yield break;
    }


    private IEnumerator DoGameRound()
    {
        Season season = _seasonMng.TickSeason();

        RequestCardsForDeck();
        yield return new WaitForSeconds(1f);

        _cardInput.EnableSystem();
        yield break;
    }


    private void RequestCardsForDeck()
    {
        int missingCards = _cardInput.MissingCardCount();
        for (int i = 0; i < missingCards; ++i)
            _cardInput.ReceiveCard(_deck.DrawCard());
    }
}
