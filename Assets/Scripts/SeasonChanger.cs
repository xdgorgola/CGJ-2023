using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeasonChanger : MonoBehaviour
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
    [SerializeField]
    private int contador;
    [SerializeField]
    private int topeSeason;// para asegurar 12 turnos

    //Variables de temporadas
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

    // Cambiador de temporada con contador incluido y retorna temporada como enum
    public Season cambioTempo()
    {
        contador++;
        if (contador == topeSeason)
        {
            temporada++;
            Cambiador();
          
            

        }
        return temporada;
    }
  
    /// prueba de que funcione todo
    public void EndTurn()
    {
        contador++;
        if (contador == topeSeason)
        {
            temporada++;
            Cambiador();
        }
    }
    //ESTO ES PARA EL DROPDOWN
    public void HandleInputData(int val)
    {
        temporada =(Season)val;
        Cambiador();
    }

    //Funcion que cambia la tempo
    private void Cambiador()
    {
        contador = 0;
        if (temporada == Season.spring)
        {

            climaPrimavera();


        }
        else if (temporada == Season.summer)
        {

            climaVerano();
          

        }
        else if (temporada == Season.fall)
        {
            climaOtono();
            

        }
        else if (temporada == Season.winter)
        {
            climaInvierno();
           


        }
    }

   

   
}

