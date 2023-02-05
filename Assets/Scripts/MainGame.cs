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
    private WeatherMgr _weatherMng = null;

    // Visuals
    [SerializeField]
    private WeatherParticlesSystem _particles = null;

    [SerializeField]
    private Season _startingSeason = Season.Spring;
    private GameCards _oldCard = null;
    private GameCards _receivedCard = null;

    // Control variables
    private bool _inGame = false;

    private bool _usedCard = false;
    private bool _successCard = false;
    private bool _failedCard = false;


    public UnityEvent OnGameOver = new UnityEvent();

    private void Awake()
    {
        _plant.OnUsedCard.AddListener(ReceiveUsedCard);
        _seasonMng.ChangeSeason(_startingSeason);
        _cardInput.ListenOnCardUsed(ReceiveUsedCard);
    }


    public void StartGame()
    {
        _usedCard = false;
        _successCard = false;
        _failedCard = false;
        _inGame = true;

        _plant.resetPlant();
        StartCoroutine(StartGameRoutine());
    }


    public void GameOver()
    {
        _inGame = false;
        if (OnGameOver != null)
            OnGameOver.Invoke();
    }


    private void EndGame()
    {
        _cardInput.DiscardHand();
        _cardInput.DisableSystem();

        _particles.UpdateParticles(Weather.NEUTRAL, _startingSeason);
        _seasonMng.ChangeSeason(_startingSeason);
    }


    private void RequestInitialCards()
    {
        for (int i = 0; i < _deck.cartasIniciales.Count; ++i)
            _cardInput.ReceiveCard(_deck.cartasIniciales[i]);
    }


    private void RequestCardsForDeck()
    {
        int missingCards = _cardInput.MissingCardCount();
        for (int i = 0; i < missingCards; ++i)
            _cardInput.ReceiveCard(_deck.DrawCard());
    }


    public void ReceiveUsedCard(GameCards card)
    {
        _receivedCard = card;
        _successCard = true;
        _usedCard = true;
    }


    public void ReceiveCanceledCard(GameCards card)
    {
        _receivedCard = card;
        _failedCard = true;
        _usedCard = true;
    }


    private IEnumerator StartGameRoutine()
    {
        RequestInitialCards();
        yield return StartCoroutine(InitalRound());

        while (_inGame)
            yield return StartCoroutine(DoGameRound());

        _cardInput.DiscardHand();
        EndGame();
    }


    private IEnumerator CardUseRoutine()
    {
        while (true)
        {
            _cardInput.EnableSystem();
            yield return new WaitUntil(() => _usedCard);

            _cardInput.DisableSystem();
            ProcessCard(_receivedCard);

            yield return new WaitUntil(() => _successCard || _failedCard);

            if (_successCard)
            {
                _usedCard = false;
                _successCard = false;
                break;
            }

            _cardInput.ReceiveCard(_receivedCard);
            _usedCard = false;
            _failedCard = false;
        }
    }


    private IEnumerator InitalRound()
    {
        RequestCardsForDeck();
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(CardUseRoutine());
        yield return new WaitForSeconds(2f);
    }


    private IEnumerator DoGameRound()
    {
        _plant.Tick();
        if (!_inGame)
            yield break;

        RequestCardsForDeck();
        Season season = _seasonMng.TickSeason();
        Weather weather = _weatherMng.Tick(season);

        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(CardUseRoutine());
        yield return new WaitForSeconds(2f);

        _particles.UpdateParticles(weather, season);
    }


    private void ProcessCard(GameCards card)
    {
        switch (card.CardEffect)
        {
            case CardEffects.Move:
            case CardEffects.DivideRoot:
            case CardEffects.DiscoverMap:
            case CardEffects.BreakRock:
                break;
            case CardEffects.QueueRain:
            case CardEffects.QueueCloudy:
            case CardEffects.QueueNeutral:
                break;
            case CardEffects.GainWater:
            case CardEffects.GainNutrient:
            case CardEffects.BetterWaterAbs:
            case CardEffects.GainLeaf:
                _plant.ReceiveCard(card);
                break;
        }
    }
}
