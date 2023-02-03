using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

using DG.Tweening;


public class CardInputUI : MonoBehaviour
{
    // Components
    [SerializeField]
    private Canvas _canvas = null;
    [SerializeField]
    private GraphicRaycaster _raycaster = null;
    [SerializeField]
    private ChooseCardText _chooseText = null;

    // UI Elements
    [SerializeField]
    private RectTransform _handTransform = null;
    [SerializeField]
    private List<RectTransform> _uiContainers = null;
    [SerializeField]
    private List<RectTransform> _uiCards = null;

    // Goals
    [SerializeField]
    private RectTransform _centerGoal = null;

    // Data
    private List<GameCards> _cardHand = new List<GameCards>();
    private Queue<int> _emptyCardsQ = new Queue<int>();

    // Events
    [SerializeField]
    private UnityEvent<GameCards> OnCardUsed = new UnityEvent<GameCards>();

    // Control
    private bool _showing = false;
    private bool _focusing = false;
    private int _focusIndex = 0;
    [SerializeField]
    private bool _isEnabled = false;

    // Timing
    [SerializeField]
    private float _handShowTime = 0.35f;
    [SerializeField]
    private float _handHideTime = 0.35f;
    [SerializeField]
    private float _cardSelectTime = 0.35f;

    // Tweens
    private Tween _hideTween = null;
    private Tween _showTween = null;

    // Routines
    private Coroutine _enlargeRoutine = null;
    private Coroutine _shrinkRoutine = null;


    private void Start()
    {
        for (int i = 0; i < _uiCards.Count; ++i)
        {
            _emptyCardsQ.Enqueue(i);
            _cardHand.Add(null);
        }
    }


    public int MissingCardCount() =>
        _emptyCardsQ.Count;


    public void EnableSystem()
    {
        _isEnabled = true;
        _chooseText.DisplayText();
    }


    public void DisableSystem()
    {
        _isEnabled = false;
    }


    public void ReceiveCard(GameCards card)
    {
        if (!_isEnabled)
            return;

        int target = _emptyCardsQ.Dequeue();
        _cardHand[target] = card;
        StartCoroutine(PlaceCardOnHandRoutine(target));
    }


    private IEnumerator PlaceCardOnHandRoutine(int index)
    {
        RectTransform cont = _uiContainers[index];
        RectTransform card = _uiCards[index];
        RectTransform canv = (RectTransform)_canvas.transform;

        card.position = cont.position + Vector3.right * (canv.sizeDelta.x * _canvas.scaleFactor + 50f);
        yield return null;

        Sequence seq = DOTween.Sequence();
        seq.Append(card.DOMove(cont.position, 0.6f).SetEase(Ease.OutBack, 0.5f));
        seq.Join(card.DORotate(Vector3.zero, 0.01f));
        seq.Join(card.DOScale(1f, 0.01f));

        yield return seq.WaitForCompletion();
    }


    public void ClickCard(BaseEventData click)
    {
        if (!_isEnabled)
            return;

        PointerEventData pointerData = (PointerEventData)click;
        RectTransform cardTrans = (RectTransform)pointerData.pointerClick.transform;
        int index = _uiCards.IndexOf(cardTrans);
        switch (pointerData.button)
        {
            case PointerEventData.InputButton.Right:
                if (!_focusing || index != _focusIndex)
                    return;

                UnfocusCard();
                break;
            case PointerEventData.InputButton.Left:
                if (!_focusing)
                {
                    FocusCard(index);
                    return;
                }

                if (_focusIndex != index)
                    return;

                UseCard(index);
                break;
            default:
                return;
        }
    }


    private void UnfocusCard()
    {
        _focusing = false;
        ReturnCard(_focusIndex);
        _focusIndex = -1;
    }


    private void FocusCard(int index)
    {
        _focusing = true;
        _focusIndex = index;
        EnlargeCard(index);
    }


    private void UseCard(int index)
    {
        if (!_isEnabled)
            return;

        _focusing = false;
        _focusIndex = index;
        StartCoroutine(ConsumeCardRoutine(index));

        if (OnCardUsed == null)
            return;

        OnCardUsed.Invoke(_cardHand[index]);
        _cardHand[index] = null;
        _emptyCardsQ.Enqueue(index);
    }


    private IEnumerator ConsumeCardRoutine(int index)
    {
        RectTransform cardTransform = _uiCards[index];

        Sequence seq = DOTween.Sequence();
        seq.Append(cardTransform.DOScale(0f, 0.5f));
        seq.Join(cardTransform.DORotate(new Vector3(0f, 0f, 80f), 0.5f));

        yield return seq.WaitForCompletion();
    }


    public void EnlargeCard(int index)
    {
        if (!_isEnabled)
            return;

        if (_enlargeRoutine != null)
            StopCoroutine(_enlargeRoutine);

        _enlargeRoutine = StartCoroutine(EnlargeCardRoutine(index));
    }


    private IEnumerator EnlargeCardRoutine(int index)
    {
        RectTransform cardTransform = _uiCards[index];

        Sequence seq = DOTween.Sequence();
        seq.Append(cardTransform.DOMove(_centerGoal.position, _cardSelectTime, false));
        seq.Join(cardTransform.DOScale(2f, _cardSelectTime));

        yield return seq.WaitForCompletion();
    }


    public void ReturnCard(int index)
    {
        if (!_isEnabled)
            return;

        if (_shrinkRoutine != null)
            StopCoroutine(_enlargeRoutine);

        _shrinkRoutine = StartCoroutine(ReturnCardRoutine(index));
    }
    

    private IEnumerator ReturnCardRoutine(int index)
    {
        RectTransform cardTransform = _uiCards[index];

        Sequence seq = DOTween.Sequence();
        seq.Append(cardTransform.DOMove(_uiContainers[index].position, _cardSelectTime, false));
        seq.Join(cardTransform.DOScale(1f, _cardSelectTime));

        yield return seq.WaitForCompletion();
    }


    public void ShowCards()
    {
        if (!_isEnabled)
            return;

        _showing = true;
        if (_hideTween is not null && _hideTween.IsPlaying())
                _hideTween.Kill();

        _showTween = _handTransform.DOMoveY(0f, _handShowTime, true).SetEase(Ease.OutBack);
    }


    public void HideCards()
    {
        if (!_isEnabled)
            return;

        _showing = false;
        if (_showTween is not null && _showTween.IsPlaying())
            _showTween.Kill();

        _hideTween = _handTransform.DOMoveY(_handTransform.sizeDelta.y * -0.8f * _canvas.scaleFactor, _handHideTime, true)
                    .SetEase(Ease.OutBack);
    }
}