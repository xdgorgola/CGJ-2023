using System.Collections;
using UnityEngine;
using DG.Tweening;


public class ChooseCardText : MonoBehaviour
{
    // Transform
    [SerializeField]
    private RectTransform _textTransform = null;

    // Tween params
    [SerializeField]
    private float _appearTime = 0.7f;
    [SerializeField]
    private float _stayTime = 1f;
    [SerializeField]
    private float _dissapearTime = 0.7f;


    public void DisplayText()
    {
        StartCoroutine(DisplayTextRoutine());
    }


    private IEnumerator DisplayTextRoutine()
    {
        _textTransform.anchoredPosition = Vector3.right * 2000f;
        _textTransform.rotation = Quaternion.identity;
        yield return null;

        Tween entryPos = _textTransform.DOAnchorPos(Vector3.zero, _appearTime, true).SetEase(Ease.OutBack);
        yield return entryPos.WaitForCompletion();
        yield return new WaitForSeconds(_stayTime);

        Tween exitPos = _textTransform.DOAnchorPos(Vector3.left * 2000f, _dissapearTime).SetEase(Ease.InBack);
        yield return exitPos.WaitForCompletion();
    }
}
