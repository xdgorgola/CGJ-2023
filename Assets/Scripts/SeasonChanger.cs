using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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
    [SerializeField] private List<SpriteRenderer> currentSeason;

    [SerializeField] private Season temporada;
    [SerializeField] private List<SpriteRenderer> spritesVerano = new List<SpriteRenderer>();
    [SerializeField] private List<SpriteRenderer> spriteInvierno = new List<SpriteRenderer>();
    [SerializeField] private List<SpriteRenderer> spriteOtono = new List<SpriteRenderer>();
    [SerializeField] private List<SpriteRenderer> spritePrimavera = new List<SpriteRenderer>();

    public float FadeSpeed;

    [SerializeField]
    private int contador = 0;
    [SerializeField]
    private int topeSeason = 12;// para asegurar 12 turnos

    public UnityEvent<Season> OnSeasonChanged;

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
        if (OnSeasonChanged != null)
            OnSeasonChanged.Invoke(val);
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
                if (currentSeason != spritePrimavera)
                {
                    StartCoroutine(AuxChangeSeason(spritePrimavera, true));
                    StartCoroutine(AuxChangeSeason(currentSeason, false));
                    
                    currentSeason = spritePrimavera;
                }
                //StartCoroutine(Fadetotemp(currentSeason, spritePrimavera));
                return;
            case Season.Summer:
                if (currentSeason != spritesVerano)
                {
                    StartCoroutine(AuxChangeSeason(spritesVerano, true));
                    StartCoroutine(AuxChangeSeason(currentSeason, false));
                    
                    currentSeason = spritesVerano;
                }
                //StartCoroutine(Fadetotemp(currentSeason, spritesVerano));
                return;
            case Season.Fall:
                if (currentSeason != spriteOtono)
                {
                    StartCoroutine(AuxChangeSeason(spriteOtono, true));
                    StartCoroutine(AuxChangeSeason(currentSeason, false));
                    
                    currentSeason = spriteOtono;
                }
                //StartCoroutine(Fadetotemp(currentSeason, spriteOtono));
                return;
            case Season.Winter:
                if (currentSeason != spriteInvierno)
                {
                    StartCoroutine(AuxChangeSeason(spriteInvierno, true));
                    StartCoroutine(AuxChangeSeason(currentSeason, false));
                    
                    currentSeason = spriteInvierno;
                }
                //StartCoroutine(Fadetotemp(currentSeason, spriteInvierno));
                return;

        }
    }


    //SEGUNDA CORRUTINA. Utilizada para recorrer todos los sprites y luego llamar a cada uno para su alteraci?n.
    // Si se le hace Fade In o Fade Out Dependera de la variable Incrementar
    // SI ES TRUE, procedera a hacer Fade In (Hacer niitda la imagen), SI ES FALSE, har? lo contrario.
    private IEnumerator AuxChangeSeason(List<SpriteRenderer> season, bool incremetar)
    {
        for (int i = 0; i < season.Count; i++)
        {
            StartCoroutine(Fadetotemp1(season[i], incremetar));
            yield return null;
        }

    }


    //CORUTINA QUE HACE FADE IN O FADE OUT A UN SPRITE. El comportamiento depende de la variable Incrementar
    // SI ES TRUE, procedera a hacer Fade In (Hacer niitda la imagen), SI ES FALSE, har? lo contrario.
    private IEnumerator Fadetotemp1(SpriteRenderer imagen, bool incrementar)
    {
        float newFade = FadeSpeed;
        if (!incrementar)
        {
            newFade = -FadeSpeed;
            while (imagen.color.a > 0f)
            {
                float fadeAmount = Mathf.Clamp(imagen.color.a + newFade * Time.deltaTime, 0, 1);
                Color objectColor = new Color(imagen.color.r, imagen.color.g, imagen.color.b, fadeAmount);
                imagen.color = objectColor;
                yield return null;
            }
        }
        else
        {
            while (imagen.color.a < 1f)
            {
                float fadeAmountV = Mathf.Clamp(imagen.color.a + newFade * Time.deltaTime, 0, 1);
                Color objectVerano = new Color(imagen.color.r, imagen.color.g, imagen.color.b, fadeAmountV);
                imagen.color = objectVerano;
                yield return null;

            }
        }
        yield return null;

    }
}
    


