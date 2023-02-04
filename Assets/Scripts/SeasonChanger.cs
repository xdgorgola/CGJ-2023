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
    private List<SpriteRenderer> currentSeason;
    
    [SerializeField] private Season temporada;
    [SerializeField] private List<SpriteRenderer> spritesVerano = new List<SpriteRenderer>();
    ///public GameObject verano;
    [SerializeField] private List<SpriteRenderer> spriteInvierno = new List<SpriteRenderer>();
    ///  public GameObject invierno;
    [SerializeField] private List<SpriteRenderer> spriteOtono = new List<SpriteRenderer>();
    ///  public GameObject otono;
    [SerializeField] private List<SpriteRenderer> spritePrimavera = new List<SpriteRenderer>();
  ///  public GameObject primavera;

    public float FadeSpeed;

    [SerializeField]
    private int contador = 0;
    [SerializeField]
    private int topeSeason = 12;// para asegurar 12 turnos

    public void Start()
    {
        currentSeason = spritePrimavera;
    }

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


    
    public void ChangeSeason(Season val)
    {
        temporada = val;
        Cambiador();
    }

    //ESTO ES PARA EL DROPDOWN
    public void ChangeSeasonDropdown(int val)
    {
        temporada = (Season)val;
        Cambiador();
    }



    //Funcion que cambia la tempo
    private void Cambiador()
    {
        contador = 0;
        switch (temporada)
        {
            case Season.Spring:
                
                StartCoroutine(Fadetotemp(currentSeason, spritePrimavera));
                return;
            case Season.Summer:
                
                StartCoroutine(Fadetotemp(currentSeason, spritesVerano));
                return;
            case Season.Fall:
                
                StartCoroutine(Fadetotemp(currentSeason, spriteOtono));
                return;
            case Season.Winter:
                
                StartCoroutine(Fadetotemp(currentSeason, spriteInvierno));
                return;
            
        }
    }

    

    private IEnumerator Fadetotemp(List<SpriteRenderer>actualSeason,List<SpriteRenderer>nextSeason)
    {
        if (nextSeason == actualSeason)
        {
            yield return null;
        }
        else
        {
            for (int f = 0; f < actualSeason.Count; f++)
            {
                Debug.Log("Entre");
                while (actualSeason[f].color.a > 0f)
                {
                    Debug.Log(actualSeason[f].color.a);
                    float fadeAmount = Mathf.Clamp(actualSeason[f].color.a - FadeSpeed * Time.deltaTime, 0, 1);
                    Color objectColor = new Color(actualSeason[f].color.r, actualSeason[f].color.g, actualSeason[f].color.b, fadeAmount);
                    actualSeason[f].color = objectColor;
                    yield return null;
                }
                Debug.Log("Sali");

                
                for (int k = 0; k < nextSeason.Count; k++)
                {
                    Debug.Log("Entre1");
                    while (nextSeason[k].color.a < 1)
                    {
                        Debug.Log(nextSeason[k].color.a);
                        float fadeAmountV = Mathf.Clamp(nextSeason[k].color.a + FadeSpeed * Time.deltaTime, 0, 1);
                        Color objectVerano = new Color(nextSeason[k].color.r, nextSeason[k].color.g, nextSeason[k].color.b, fadeAmountV);
                        nextSeason[k].color = objectVerano;
                        yield return null;

                    }
                    Debug.Log("Sali1");
                }
                

            }
            currentSeason = nextSeason;
            yield return null;
        }
    }
}
    


