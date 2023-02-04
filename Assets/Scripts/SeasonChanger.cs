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
    public List<SpriteRenderer> spritesVerano = new List<SpriteRenderer>();
    ///public GameObject verano;
    public List<SpriteRenderer> spriteInvierno = new List<SpriteRenderer>();
  ///  public GameObject invierno;
    public List<SpriteRenderer> spriteOtono = new List<SpriteRenderer>();
  ///  public GameObject otono;
    public List<SpriteRenderer> spritePrimavera = new List<SpriteRenderer>();
  ///  public GameObject primavera;


    public float FadeSpeed;


    [SerializeField]
    private int contador = 0;
    [SerializeField]
    private int topeSeason = 12;// para asegurar 12 turnos


    //Variables de temporadas
  /*  private void TemporadaVerano()
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
  */

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

    public void prueba()
    {
        Debug.Log("boton presionado");
        temporada = temporada++;
        Cambiador();
    }

    //Funcion que cambia la tempo
    private void Cambiador()
    {
        contador = 0;
        switch (temporada)
        {
            case Season.Spring:
                StartCoroutine(Fadetotemp(spriteInvierno,spritePrimavera));
                return;
            case Season.Summer:
                StartCoroutine(Fadetotemp(spritePrimavera, spritesVerano));
                return;
            case Season.Fall:
                StartCoroutine(Fadetotemp(spritesVerano, spriteOtono));
                return;
            case Season.Winter:
                StartCoroutine(Fadetotemp(spriteOtono, spriteInvierno));
                return;
        }
    }

    

    private IEnumerator Fadetotemp(List<SpriteRenderer>pepito,List<SpriteRenderer>pepito2)
    {
        
        for (int f = 0; f < pepito.Count; f++)
        {
            while (pepito[f].color.a > 0f)
            {
                float fadeAmount = Mathf.Clamp(pepito[f].color.a - (FadeSpeed * Time.deltaTime), 0, 1);
                Color objectColor = new Color(pepito[f].color.r, pepito[f].color.g, pepito[f].color.b, fadeAmount);
                pepito[f].color = objectColor;

            }
     
            for (int k = 0; k < pepito2.Count; k++)
            {
                while (pepito2[k].color.a < 1)
                {
                  
                    float fadeAmountV = Mathf.Clamp(pepito2[k].color.a + (FadeSpeed * Time.deltaTime), 0, 1);


                    Color objectVerano = new Color(pepito2[k].color.r, pepito2[k].color.g, pepito2[k].color.b, fadeAmountV);
                    pepito2[k].color = objectVerano;

                }
                yield return null;
            }   
        }
    }
}
    


