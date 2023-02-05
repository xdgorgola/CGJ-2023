using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class MainMenu : MonoBehaviour
{
    private CanvasGroup _group = null;

    [SerializeField]
    private GameObject _mainScreen = null;

    private void Awake()
    {
        _group = GetComponent<CanvasGroup>();    
    }


    public void ActivateMenu()
    {
        _mainScreen.SetActive(true);
        StartCoroutine(ActivateMenuRoutine());
    }


    public void DisableMenu()
    {
        StartCoroutine(DisableMenuRoutine());
    }


    private IEnumerator ActivateMenuRoutine()
    {
        yield return StartCoroutine(FadeRoutine(1f));
        _group.interactable = true;
    }

    public IEnumerator DisableMenuRoutine()
    {
        yield return StartCoroutine(FadeRoutine(0f));
        _group.interactable = false;
        _mainScreen.SetActive(false);
    }


    private IEnumerator FadeRoutine(float target)
    {
        Tween fade = _group.DOFade(target, 2f);
        yield return fade.WaitForCompletion();
    }


    public void ExitGame()
    {
        Application.Quit();
    }
}
