using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Weather
{
    SECO,
    SOLEADO,
    NEUTRAL,
    LLUVIA,
    NUBOSO,
    NIEVE
};

public class WeatherMgr : MonoBehaviour
{
    private Weather clima_actual;
    private Weather sig_clima;
    private Weather alt_clima;
    private bool alter;
    private int turnos;
    private int alt_turnos;


    // Start is called before the first frame update
    void Start()
    {
        RestartWeather();
    }


    public void RestartWeather()
    {
        clima_actual = Weather.NEUTRAL;
        sig_clima = Weather.NEUTRAL;
        alt_turnos = 0;
        alter = false;
        turnos = 0;
    }


    public void ReceiveCard(GameCards card)
    {
        Weather qWeat = Weather.NEUTRAL;
        switch (card.CardEffect)
        {
            case CardEffects.QueueRain:
                qWeat = Weather.LLUVIA;
                break;
            case CardEffects.QueueCloudy:
                qWeat = Weather.NUBOSO;
                break;
            case CardEffects.QueueSunny:
                qWeat = Weather.SOLEADO;
                break;
            case CardEffects.QueueNeutral:
                qWeat = Weather.NEUTRAL;
                break;
        }

        alter_weather((int)card.GetParams()["dur"], qWeat);
    }


    public Weather Tick(Season season)
    {
        if (alter)
        {
            if (alt_turnos == 0)
            {
                clima_actual = alt_clima;
                turnos = 2;
                alter = false;
                return clima_actual;
            }
            else
            {
                alt_turnos--;
                return clima_actual;
            }
        }
        else
        {
            if (turnos == 0)
            {
                turnos = 2;
                if (clima_actual != Weather.NEUTRAL)
                {
                    clima_actual = Weather.NEUTRAL;
                    return clima_actual;
                }
                else
                {
                    clima_actual = sig_clima;
                    sig_clima = next_clima(season);
                    return clima_actual;
                }
            }
            else
            {
                turnos--;
                return clima_actual;
            }
        }
    }

    public void alter_weather(int turnos, Weather c)
    {
        alter = true;
        alt_clima = c;
        alt_turnos = turnos;

    }


    private Weather next_clima(Season s)
    {
        int random = Random.Range(0, 100);
        switch (s)
        {
            case Season.Fall:
                if (random >= 0 && random < 30)
                {
                    return Weather.SECO;
                }
                else if (random >= 30 && random < 40)
                {
                    return Weather.SOLEADO;
                }
                else if (random >= 40 && random < 60)
                {
                    return Weather.NEUTRAL;
                }
                else if (random >= 60 && random < 80)
                {
                    return Weather.LLUVIA;
                }
                else if (random >= 80 && random < 95)
                {
                    return Weather.NUBOSO;
                }
                else if (random >= 95)
                {
                    return Weather.NIEVE;
                }
                break;
            case Season.Spring:
                if (random >= 0 && random < 10)
                {
                    return Weather.SECO;
                }
                else if (random >= 10 && random < 40)
                {
                    return Weather.SOLEADO;
                }
                else if (random >= 40 && random < 70)
                {
                    return Weather.NEUTRAL;
                }
                else if (random >= 70 && random < 85)
                {
                    return Weather.LLUVIA;
                }
                else if (random >= 85)
                {
                    return Weather.NUBOSO;
                }
                break;
            case Season.Summer:
                if (random >= 0 && random < 20)
                {
                    return Weather.SECO;
                }
                else if (random >= 20 && random < 60)
                {
                    return Weather.SOLEADO;
                }
                else if (random >= 60 && random < 90)
                {
                    return Weather.NEUTRAL;
                }
                else if (random >= 90 &&  random < 95)
                {
                    return Weather.LLUVIA;
                }
                else if (random >= 95 )
                {
                    return Weather.NUBOSO;
                }
                break;
            case Season.Winter:
                if (random >= 0 && random < 5)
                {
                    return Weather.SOLEADO;
                }
                else if (random >= 5 && random < 20)
                {
                    return Weather.NEUTRAL;
                }
                else if (random >= 20 && random < 50)
                {
                    return Weather.NUBOSO;
                }
                else if (random >= 50 )
                {
                    return Weather.NIEVE;
                }
                break;
                default:
                break;
        }
        return Weather.NEUTRAL;
    }
}
