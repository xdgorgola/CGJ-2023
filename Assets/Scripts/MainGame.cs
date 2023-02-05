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
    private BoardController _boardController = null;
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

    // Components
    [SerializeField]
    private Camera _cam = null;

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
        //_plant.OnUsedCard.AddListener(ReceiveUsedCard);
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

        _weatherMng.RestartWeather();
        _seasonMng.ChangeSeason(_startingSeason);
        _particles.UpdateParticles(Weather.NEUTRAL, _startingSeason);
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
        _failedCard = false;
        _successCard = true;
        _usedCard = true;
    }


    public void ReceiveCanceledCard(GameCards card)
    {
        _receivedCard = card;
        _successCard = false;
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

            _successCard = false; // fua cosa boba aca
            _cardInput.DisableSystem();
            yield return StartCoroutine(ProcessCard(_receivedCard));

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
        BoardComponent.Collected plantCollected = _board.Tick(1, 1);

        _plant.ChangeWaterCount(plantCollected.water);
        _plant.UpdateNutrients(plantCollected.nutrients);

        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(CardUseRoutine());
        yield return new WaitForSeconds(2f);

        _particles.UpdateParticles(weather, season);
    }


    private IEnumerator RockDestroyRoutine(GameCards card)
    {
        while (true)
        {
            yield return null;
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                Vector3 wpos = _cam.ScreenToWorldPoint(Input.mousePosition);
                if (_board.DestroyRock(wpos))
                {
                    ReceiveUsedCard(card);
                    yield break;
                }
                continue;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ReceiveCanceledCard(card);
                yield break;
            }
        }
    }


    private IEnumerator DiscoverRoutine(GameCards card)
    {
        while (true)
        {
            yield return null;
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                Vector3 wpos = _cam.ScreenToWorldPoint(Input.mousePosition);
                Debug.Log(wpos);
                if (_board.DiscoverCell(wpos))
                {
                    ReceiveUsedCard(card);
                    yield break;
                }
                continue;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ReceiveCanceledCard(card);
                yield break;
            }
        }
    }


    private IEnumerator RootCreate(GameCards card)
    {
        UnityAction succAct = () => ReceiveUsedCard(card);
        UnityAction cancAct = () => ReceiveCanceledCard(card);

        _boardController.OnRootCreated.AddListener(succAct);
        _boardController.OnRootCancel.AddListener(cancAct);

        yield return StartCoroutine(_boardController.StartRootCreation(1));
        _usedCard = false; // cosa rara

        yield return new WaitUntil(() => _usedCard);

        Debug.Log(_usedCard);
        Debug.Log(_successCard);

        _boardController.OnRootCreated.RemoveListener(succAct);
        _boardController.OnRootCancel.RemoveListener(cancAct);
    }


    private IEnumerator ProcessCard(GameCards card)
    {
        switch (card.CardEffect)
        {
            case CardEffects.Move:
                yield return StartCoroutine(RootCreate(card));
                if (_successCard)
                    _plant.ChangeWaterCount(-card.WaterCost);
                break;
            case CardEffects.DivideRoot:
                yield return StartCoroutine(RootCreate(card));
                if (_successCard)
                    _plant.ChangeWaterCount(-card.WaterCost);
                break;
            case CardEffects.DiscoverMap:
                yield return StartCoroutine(DiscoverRoutine(card));
                if (_successCard)
                    _plant.ChangeWaterCount(-card.WaterCost);
                break;
            case CardEffects.BreakRock:
                yield return StartCoroutine(RockDestroyRoutine(card));
                if (_successCard)
                    _plant.ChangeWaterCount(-card.WaterCost);
                break;
            case CardEffects.QueueRain:
            case CardEffects.QueueCloudy:
            case CardEffects.QueueSunny:
            case CardEffects.QueueNeutral:
                _weatherMng.ReceiveCard(card);
                _plant.ChangeWaterCount(-card.WaterCost);
                _successCard = true;
                break;
            case CardEffects.GainWater:
            case CardEffects.GainNutrient:
            case CardEffects.BetterWaterAbs:
            case CardEffects.GainLeaf:
                _plant.ReceiveCard(card);
                _successCard = true;
                yield break;
        }
    }
}
