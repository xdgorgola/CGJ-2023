using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlantComponent : MonoBehaviour
{   

    //VALORES MAXIMOS
    [SerializeField] private float maxAgua;
    [SerializeField] private float maxNutrientes;
    [SerializeField] private float maxVida;

    //VALORES ACTUALES
    [SerializeField] private float agua;
    [SerializeField] private float nutrientes;
    [SerializeField] private float vida;

    //Events
    [SerializeField] private UnityEvent OnFlowerDeath = new UnityEvent();
    [SerializeField] private UnityEvent OnNutrientsCap = new UnityEvent();

    // Start is called before the first frame update
    void Start()
    {
        maxAgua = 20f;
        maxNutrientes = 10f;
        maxVida = 10f;
        agua = 10f;
        nutrientes = 0f;
        vida = 10f;
    }

    public void ChangeWaterCount(float cost)
    {
        float newWater = Mathf.Clamp(vida + cost, 0, maxAgua);
        if (newWater < maxAgua && newWater > 0)
        {
            agua = newWater;
        }
        
    }

    public void UpdateLife(float cost)
    {
        float newLife = Mathf.Clamp(vida + cost, 0, maxVida);
        if (newLife < maxVida && newLife > 0)
        {
            vida = newLife;
        }
        else if (newLife == 0)
        {
            OnFlowerDeath.Invoke();
        }

    }

    public void UpdateNutrients(float cost)
    {   
        float newNutrients = Mathf.Clamp(nutrientes + cost, 0, maxNutrientes);
        if (newNutrients < maxNutrientes && newNutrients > 0)
        {
            nutrientes = newNutrients;
        }
        else if (newNutrients == maxNutrientes)
        {
            //TRIGGER PARA CONSEGUIR MEJORA
            OnNutrientsCap.Invoke();
            nutrientes = 0f;
        }

    }

    public void ReceiveCard(GameCards card)
    {
        CardEffects effect = card.CardEffect;
        switch (effect)
        {
            case CardEffects.GainWater:
                ChangeWaterCount(card.GetParams()["quant"]);
                return;

            case CardEffects.GainNutrient:
                UpdateNutrients(card.GetParams()["quant"]);
                ChangeWaterCount(-1);
                return;

        }
    }
}
