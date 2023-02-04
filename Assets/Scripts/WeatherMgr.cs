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

    public GameObject seco;
    public GameObject soleado;
    public GameObject neutral;
    public GameObject lluvia;
    public GameObject nuboso;
    public GameObject nieve;

    // Start is called before the first frame update
    void Start()
    {
        clima_actual = Weather.NEUTRAL;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Weather Tick(Season season)
    {
        switch(season)
        {
            case Season.Spring:
               break;
            case Season.Summer:
                break;
            case Season.Fall:
                break;
            case Season.Winter:
                break;
        }
        return 0;
    }
}
