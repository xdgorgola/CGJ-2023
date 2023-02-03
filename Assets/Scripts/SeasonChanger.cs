using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Season
{
    Spring,
    Summer,
    Fall,
    Winter
}


public class SeasonChanger : MonoBehaviour
{
    private const int SEASON_COUNT = 4;

    private Season temporada;
    public GameObject verano;
    public GameObject invierno;
    public GameObject otono;
    public GameObject primavera;  


    [SerializeField]
    private int contador = 0;
    [SerializeField]
    private int topeSeason = 12;// para asegurar 12 turnos


    //Variables de temporadas
    private void TemporadaVerano()
    {
        verano.SetActive(true);
        invierno.SetActive(false);
        otono.SetActive(false);
        primavera.SetActive(false);
    }


    private void TemporadaInvierno()
    {
        verano.SetActive(false);
        invierno.SetActive(true);
        otono.SetActive(false);
        primavera.SetActive(false);
    }


    private void TemporadaOtono()
    {
        verano.SetActive(false);
        invierno.SetActive(false);
        otono.SetActive(true);
        primavera.SetActive(false);
    }


    private void TemporadaPrimavera()
    {
        verano.SetActive(false);
        invierno.SetActive(false);
        otono.SetActive(false);
        primavera.SetActive(true);
    }


    // Cambiador de temporada con contador incluido y retorna temporada como enum
    public Season TickSeason()
    {
        contador++;
        if (contador == topeSeason)
        {
            temporada = (Season)(((int)temporada + 1) % SEASON_COUNT);
            Cambiador();
        }
        return temporada;
    }
  

    //ESTO ES PARA EL DROPDOWN
    public void ChangeSeason(Season val)
    {
        temporada = val;
        Cambiador();
    }


    //Funcion que cambia la tempo
    private void Cambiador()
    {
        contador = 0;
        switch (temporada)
        {
            case Season.Spring:
                TemporadaPrimavera();
                return;
            case Season.Summer:
                TemporadaVerano();
                return;
            case Season.Fall:
                TemporadaOtono();
                return;
            case Season.Winter:
                TemporadaInvierno();
                return;
        }
    }
}

