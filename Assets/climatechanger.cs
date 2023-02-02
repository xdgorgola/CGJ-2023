using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class climatechanger : MonoBehaviour
{

    public enum Season
    {
        spring,
        summer,
        fall,
        winter
    }

    public Season temporada;
    public GameObject verano;
    public GameObject invierno;
    public GameObject otono;
    public GameObject primavera;


    void climaVerano()
    {
        verano.SetActive(true);
        invierno.SetActive(false);
        otono.SetActive(false);
        primavera.SetActive(false);


    }
    void climaInvierno()
    {
        verano.SetActive(false);
        invierno.SetActive(true);
        otono.SetActive(false);
        primavera.SetActive(false);
    }
    void climaOtono()
    {
        verano.SetActive(false);
        invierno.SetActive(false);
        otono.SetActive(true);
        primavera.SetActive(false);
    }
    void climaPrimavera()
    {
        verano.SetActive(false);
        invierno.SetActive(false);
        otono.SetActive(false);
        primavera.SetActive(true);

    }

    public void Cambiador()
    {
        if (temporada == Season.spring)
        {

            Debug.Log("caso1");
            climaPrimavera();

        }
        else if (temporada == Season.summer)
        {

            climaVerano();
            Debug.Log("caso2");

        }
        else if (temporada == Season.fall)
        {
            climaOtono();
            Debug.Log("caso3");

        }
        else if (temporada == Season.winter)
        {
            climaInvierno();
            Debug.Log("caso4");


        }
    }

    public void HandleInputData(int val)
    {
        if (val == 0)
        {
            temporada = Season.spring;
            Cambiador();
        }
        else if (val == 1)
        {
            temporada = Season.summer;
            Cambiador();
        }
        else if (val == 2)
        {
            temporada = Season.fall;
            Cambiador();
        }
        else if (val == 3)
        {
            temporada = Season.winter;
            Cambiador();
        }
    }
}



