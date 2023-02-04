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
    private List<SpriteRenderer> currentSeason = new List<SpriteRenderer>();
    private List<SpriteRenderer> nextSeason = new List<SpriteRenderer>();
    
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
                currentSeason = spritePrimavera;
                nextSeason = spritesVerano; 
                StartCoroutine(Fadetotemp(currentSeason,nextSeason));
                return;
            case Season.Summer:
                currentSeason = spritesVerano;
                nextSeason = spriteOtono;
                StartCoroutine(Fadetotemp(currentSeason, nextSeason));
                return;
            case Season.Fall:
                currentSeason = spriteOtono;
                nextSeason = spriteInvierno;
                StartCoroutine(Fadetotemp(currentSeason, nextSeason));
                return;
            case Season.Winter:
                currentSeason = spriteInvierno;
                nextSeason = spritePrimavera;
                StartCoroutine(Fadetotemp(currentSeason, nextSeason));
                return;
            
        }
    }

    

    private IEnumerator Fadetotemp(List<SpriteRenderer>actualSeason,List<SpriteRenderer>nextSeason)
    {
        
        for (int f = 0; f < actualSeason.Count; f++)
        {
            while (actualSeason[f].color.a > 0f)
            {
                float fadeAmount = Mathf.Clamp(actualSeason[f].color.a - (FadeSpeed * Time.deltaTime), 0, 1);
                Color objectColor = new Color(actualSeason[f].color.r, actualSeason[f].color.g, actualSeason[f].color.b, fadeAmount);
                actualSeason[f].color = objectColor;

            }

            for (int k = 0; k < nextSeason.Count; k++)
            {
                while (nextSeason[k].color.a < 1)
                {

                    float fadeAmountV = Mathf.Clamp(nextSeason[k].color.a + (FadeSpeed * Time.deltaTime), 0, 1);


                    Color objectVerano = new Color(nextSeason[k].color.r, nextSeason[k].color.g, nextSeason[k].color.b, fadeAmountV);
                    nextSeason[k].color = objectVerano;

                }
            }
                currentSeason = nextSeason;
                nextSeason = null;
                if(currentSeason == actualSeason && nextSeason == null)
                {
                    yield break;
                }
                yield return null;
              
        }
    }
}
    


