using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Animator))]
public class PlantComponent : MonoBehaviour
{
    //Componentes
    private Animator _animator = null;

    //VALORES BASE
    [SerializeField] private int baseAgua = 75; //75
    [SerializeField] private int baseNutrientes = 0; //0
    [SerializeField] private int baseMaxAgua = 100;//100
    [SerializeField] private int baseMaxNutrientes = 50;//50
    [SerializeField] private int baseMaxNumHojas = 4;//4
    [SerializeField] private int baseTurnosHojas = 4; //4
    [SerializeField] private float baseRatioAbsorcionAgua = 1f;//1
    [SerializeField] private float baseRatioAbsorcionNutrientes = 1f;//1
    [SerializeField] private int baseConsumoAguaXTurno = 1; //1
    [SerializeField] private int baseConsumoNutrienteXTurno = 1; //1
    [SerializeField] private LeafComponent[] hojas;
    [SerializeField] private List<WaterGainUpgrade> absorcionAgua; //ESTAS SON MEJORAS TEMPORALES

    //VALORES MAXIMOS
    [SerializeField] private int maxAgua;
    [SerializeField] private int maxNutrientes;
    [SerializeField] private float ratioAbsorcionAgua;
    [SerializeField] private float ratioAbsorcionNutrientes;
    //VALORES ACTUALES      
    [SerializeField] private int agua;
    [SerializeField] private int nutrientes;
    [SerializeField] private int turnosHojas;
    [SerializeField] private int consumoAguaXTurno;
    [SerializeField] private int consumoNutrienteXTurno;

    //Events
    [SerializeField] private UnityEvent OnFlowerDeath = new UnityEvent();
    [SerializeField] private UnityEvent OnNutrientsCap = new UnityEvent();
    public UnityEvent<GameCards> OnUsedCard = new UnityEvent<GameCards>();


    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }


    public void ChangeWaterCount(float cost)
    {
        agua = (int)Mathf.Clamp(agua + cost, 0, maxAgua);
        if (agua <= 0)
        {
            if (OnFlowerDeath != null)
                OnFlowerDeath.Invoke();

            _animator.SetTrigger("Die");
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
            if (OnNutrientsCap != null)
                OnNutrientsCap.Invoke();
            nutrientes = 0;
        }

    }

    public void ReceiveCard(GameCards card)
    {
        CardEffects effect = card.CardEffect;

        switch (effect)
        {
            case CardEffects.GainWater: //GANANCIA DE AGUA FIJA POR CARTA
                ChangeWaterCount(card.GetParams()["quant"]);
                break;
            case CardEffects.GainNutrient://GANANCIA DE NUTRIENTES FIJA POR CARTA
                UpdateNutrients(card.GetParams()["quant"]);
                break;
            case CardEffects.BetterWaterAbs://MEJORA DE ABSORCIÓN DE AGUA POR CARTA
                //WaterGainUpgrade nuevaMejora = new WaterGainUpgrade();
                //nuevaMejora.MaxTurnos = (int)card.GetParams()["quant"];
                //absorcionAgua.Add(nuevaMejora);
                break;
            case CardEffects.GainLeaf://GENERACIÓN DE UNA HOJA POR CARTA
                addLeaf();
                break;
        }
        ChangeWaterCount(-card.WaterCost);
        if (OnUsedCard != null)
            OnUsedCard.Invoke(card);
    }


    //Restart All the plant
    public void resetPlant()
    {
        _animator.SetTrigger("Revive");
        resetStats();
        resetLeafs();
        //resetWaterUpgrades();
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
        consumoNutrienteXTurno = baseConsumoNutrienteXTurno;
        consumoAguaXTurno = baseConsumoAguaXTurno;
    }


    private void resetLeafs()
    {
        for(int i = 0; i < hojas.Length; i++)
        {
            if (hojas[i].IsActive)
                hojas[i].KillLeaf();
        }
    }


    private void resetWaterUpgrades()
    {
       absorcionAgua.Clear();
       // SI es necesario agregar otra función para eliminar mejoras de agua
    }


    private void addLeaf()
    {
        for(int i = 0; i < hojas.Length; i++)
        {
            if (!hojas[i].IsActive)
            {
                hojas[i].StartLeaf(turnosHojas);
                return;
            }
        }
    }


    public void Tick()
    {   
        //Le Hacemos TICK a las hojas
        for (int i = 0; i < hojas.Length; i++)
        {
            if (hojas[i].IsActive)
                hojas[i].Tick();
        }

        //int j = 0;
        ////Le hacemos tic a la mejoras temporales de absorción de agua
        //while (j<absorcionAgua.Count)
        //{
        //    if(absorcionAgua[j].MaxTurnos <= absorcionAgua[j].Turnos)
        //    {
        //        absorcionAgua.RemoveAt(j);
        //    }
        //    else
        //    {
        //        absorcionAgua[j].addCount();
        //        j++;
        //    }
        //}

        //Le haceos Tic al agua y Nutrientes
        ChangeWaterCount(-consumoAguaXTurno);
        UpdateNutrients(-consumoNutrienteXTurno);
    }
}

//Clase Auxiliar para definir mejoras temporales a la ganancia de agua
public class WaterGainUpgrade
{
    [SerializeField] private int turnos = 0;
    [SerializeField] private int maxTurnos = 4;

    public int Turnos
    {
        get { return turnos; }
        set { turnos = value; }
    }

    public int MaxTurnos
    {
        get { return maxTurnos; }
        set { maxTurnos = value; }
    }

    public void addCount()
    {
        turnos = turnos + 1;
    }
}