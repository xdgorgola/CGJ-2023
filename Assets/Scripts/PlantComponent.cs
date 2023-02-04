using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlantComponent : MonoBehaviour
{

    //VALORES BASE
    [SerializeField] private int baseAgua; //75
    [SerializeField] private int baseNutrientes; //0
    [SerializeField] private int baseMaxAgua;//100
    [SerializeField] private int baseMaxNutrientes;//50
    [SerializeField] private int baseMaxNumHojas;//4
    [SerializeField] private int baseTurnosHojas; //4
    [SerializeField] private float baseRatioAbsorcionAgua;//1
    [SerializeField] private float baseRatioAbsorcionNutrientes;//1
    [SerializeField] private Leaf[] hojas;

    //VALORES MAXIMOS
    [SerializeField] private int maxAgua;
    [SerializeField] private int maxNutrientes;
    [SerializeField] private float ratioAbsorcionAgua;
    [SerializeField] private float ratioAbsorcionNutrientes;
    //VALORES ACTUALES      
    [SerializeField] private int agua;
    [SerializeField] private int nutrientes;
    [SerializeField] private int turnosHojas;

    //Events
    [SerializeField] private UnityEvent OnFlowerDeath = new UnityEvent();
    [SerializeField] private UnityEvent OnNutrientsCap = new UnityEvent();

    // Start is called before the first frame update
    void Start()
    {
        resetPlant();
    }

    public void ChangeWaterCount(float cost)
    {
        float newWater = Mathf.Clamp(agua + cost, 0, maxAgua);
        if (newWater < maxAgua && newWater > 0)
        {
            agua = (int)newWater;
        }
        else if (newWater == 0)
        {
            OnFlowerDeath.Invoke();
        }

    }

    public void UpdateNutrients(float cost)
    {
        float newNutrients = Mathf.Clamp(nutrientes + cost, 0, maxNutrientes);
        if (newNutrients < maxNutrientes && newNutrients > 0)
        {
            nutrientes = (int)newNutrients;
        }
        else if (newNutrients == maxNutrientes)
        {
            //TRIGGER PARA CONSEGUIR MEJORA
            OnNutrientsCap.Invoke();
            nutrientes = 0;
        }

    }

    public void ReceiveCard(GameCards card)
    {
        CardEffects effect = card.CardEffect;
        switch (effect)
        {
            case CardEffects.GainWater:
                ChangeWaterCount(card.GetParams()["quant"]);
                ChangeWaterCount(card.WaterCost);
                return;

            case CardEffects.GainNutrient:
                UpdateNutrients(card.GetParams()["quant"]);
                ChangeWaterCount(card.WaterCost);
                return;
            case CardEffects.BetterWaterAbs:
                ChangeWaterCount(card.WaterCost);
                return;
            case CardEffects.GainLeaf:
                ChangeWaterCount(card.WaterCost);
                addLeaf();
                return;
        }
    }

    //Restart All the plant
    public void resetPlant()
    {
        resetStats();
        resetLeafs();
    }

    //Restart only the stats
    private void resetStats()
    {
        agua = baseAgua;
        nutrientes = baseNutrientes;
        maxAgua = baseMaxAgua;
        maxNutrientes = baseMaxNutrientes;
        ratioAbsorcionAgua = baseRatioAbsorcionAgua;
        turnosHojas = baseTurnosHojas;
    }

    private void resetLeafs()
    {
        hojas = new Leaf[baseMaxNumHojas];

        for(int i = 0; i < baseMaxNumHojas; i++)
        {
            hojas[i] = new Leaf();
        }
    }

    private void addLeaf()
    {
        int maxTurns = 0;
        int pos = 0;

        for(int i = 0; i <baseMaxNumHojas; i++)
        {
            if (!hojas[i].IsActive)
            {
                hojas[i].IsActive = true;
                hojas[i].Turnos = 0;
                return;
            }
            else
            {
                if (hojas[i].Turnos>=maxTurns)
                {
                    maxTurns = hojas[i].Turnos;
                    pos = i;
                }
            }
        }

        hojas[pos].Turnos = 0;
    }

    public void Tick()
    {   

        //Le Hacemos TICK a las hojas
        for (int i = 0; i < baseMaxNumHojas; i++)
        {
            if (hojas[i].IsActive)
            {
                if (hojas[i].Turnos == turnosHojas)
                {
                    hojas[i].IsActive = false;
                }
                hojas[i].addCount();
                return;
            }

        }
    }
}

public class Leaf
{

    private int turnos = 0;
    private bool isActive = false;

    public int Turnos
    {
        get { return turnos; }
        set{ turnos = value; }
    }

    public bool IsActive
    {
        get { return isActive; }
        set { isActive = value; }
    }

    public void addCount()
    {
        turnos = turnos + 1;
    }
}