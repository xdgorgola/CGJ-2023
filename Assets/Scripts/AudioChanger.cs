using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AudioChanger : MonoBehaviour
{
    [SerializeField]
    private AudioSource _newSource = null;
    [SerializeField]
    private AudioSource _oldSource = null;


    [SerializeField]
    private AudioClip _springClip = null;
    [SerializeField]
    private AudioClip _summerClip = null;
    [SerializeField]
    private AudioClip _fallClip = null;
    [SerializeField]
    private AudioClip _winterClip = null;
    [SerializeField]
    private AudioClip _loseClip = null;
    [SerializeField]
    private AudioClip _winClip = null;

    private void Start()
    {
        _oldSource.Play();
        _oldSource.loop = true;
        _newSource.loop = true;
    }


    public void ChangeAudioSeason(Season newSeason)
    {
        AudioClip targetClip = newSeason switch
        {
            Season.Spring => _springClip,
            Season.Summer => _summerClip,
            Season.Fall => _fallClip,
            Season.Winter => _winterClip,
            _ => null
        };

        StartCoroutine(FadeAudiosRoutine(targetClip));
    }

    public void ChangeAudioLose()
    {
        StartCoroutine(FadeAudiosRoutine(_loseClip));
    }

    public void ChangeAudioWin()
    {
        StartCoroutine(FadeAudiosRoutine(_winClip));
    }

    private IEnumerator FadeAudiosRoutine(AudioClip targetClip)
    {
        yield return _oldSource.DOFade(0f, 1f).WaitForCompletion();

        _oldSource.Stop();
        _newSource.clip = targetClip;
        _newSource.Play();
        yield return _newSource.DOFade(1f, 1f).WaitForCompletion();

        AudioSource tmp = _oldSource;
        _oldSource = _newSource;
        _newSource = tmp;
    }
}
