using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class WeatherParticlesSystem : MonoBehaviour
{
    // Particles
    [SerializeField]
    private ParticleSystem _fallSystem = null;
    [SerializeField]
    private ParticleSystem _rainSystem = null;
    private ParticleSystem _currParticles = null;

    // Materials
    [SerializeField]
    private Material _fallMat = null;
    [SerializeField]
    private Material _rainMat = null;
    private Material _curMaterial = null;

    // Times
    [SerializeField]
    private float _fadeTime = 2f;

    private Weather _currentWeather = Weather.NEUTRAL;
    private Season _currentSeason = Season.Spring;


    private void Awake()
    {
        _rainMat.SetColor("_BaseColor", new Color(1f, 1f,1f, 0f));
        _fallMat.SetColor("_BaseColor", new Color(1f, 1f, 1f, 0f));
    }


    public void UpdateParticles(Weather newWeather, Season season)
    {
        StartCoroutine(ChangeWeather(newWeather, season));
    }


    private IEnumerator ChangeWeather(Weather newWeather, Season season)
    {
        if (newWeather == _currentWeather && season == _currentSeason)
            yield break;

        Color startColor = _rainMat.GetColor("_BaseColor");

        ParticleSystem oldParticles = _currParticles;
        Material oldMaterial = _curMaterial;
        _curMaterial = null;

        switch (newWeather)
        {
            case Weather.LLUVIA:
                _currParticles = _rainSystem;
                _curMaterial = _rainMat;
                break;
            case Weather.NEUTRAL:
                if (season == Season.Fall)
                {
                    _currParticles = _fallSystem;
                    _curMaterial = _fallMat;
                    break;
                }
                break;
            case Weather.NIEVE:
            case Weather.SECO:
            case Weather.SOLEADO:
            case Weather.NUBOSO:
            default:
                break;
        }

        _currentWeather = newWeather;
        _currentSeason = season;

        if (_curMaterial == null && oldMaterial == null)
            yield break;

        Sequence seq = DOTween.Sequence();

        if (_curMaterial != null)
            seq.Join(DOTween.ToAlpha(
                () => _curMaterial.GetColor("_BaseColor"),
                c => _curMaterial.SetColor("_BaseColor", c),
                1f, _fadeTime));


        if (oldMaterial != null)
            seq.Join(DOTween.ToAlpha(
                () => oldMaterial.GetColor("_BaseColor"),
                c => oldMaterial.SetColor("_BaseColor", c),
                0f, _fadeTime));

        if (_currParticles != null)
            _currParticles.Play();

        yield return seq.WaitForCompletion();

        if (oldParticles != null)
            oldParticles.Stop();
    }
}
