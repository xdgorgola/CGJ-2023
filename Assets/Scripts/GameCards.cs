using System.Linq;
using System.Collections.Generic;
using UnityEngine;


public enum CardEffects
{
    MoveHor,        // mov: nro casillas
    MoveVert,       // mov: nro casillas
    MoveDiag,       // mov: nro casillas
    DivideRoot,     // no params
    DiscoverMap,    // num: nro casillas desbloqueadas
    BreakRock,      // num: nro rocas descubiertas
    QueueRain,      // wait: dias de cola - dur: duracion clima
    QueueCloudy,    // wait: dias de cola - dur: duracion clima
    QueueNeutral,   // wait: dias de cola
    GainWater,      // quant: nro ganado
    GainNutrient,   // quant: nro ganado
    BetterWaterAbs, // amount: porcentaje de aumento - dur: duracion en turnos
    GainLeaf        // no params
}  


[CreateAssetMenu(fileName = "CARD_", menuName = "Card")]
public class GameCards : ScriptableObject
{
    [System.Serializable]
    private struct ParamPair<T,P>
    {
        [SerializeField]
        private T _paramName;

        [SerializeField]
        private P _paramValue;

        public T GetName() =>
            _paramName;

        public P GetValue() =>
            _paramValue;
    }


    [SerializeField]
    private string _name = "CARD";
    [TextArea]
    [SerializeField]
    private string _description = "DEFAULT";
    [SerializeField]
    private Sprite _picture = null;
    [Min(0)]
    [SerializeField]
    private int _waterCost = 0;

    [SerializeField]
    private CardEffects _effect;
    [SerializeField]
    private List<ParamPair<string, float>> _params;


    public string Name
    {
        get { return _name; }
    }

    public string Description
    {
        get { return _description; }
    }

    public Sprite Picture
    {
        get { return _picture; }
    }

    public CardEffects CardEffect
    {
        get { return _effect; }
    }

    public int WaterCost
    {
        get { return _waterCost; }
    }


    public Dictionary<string, float> GetParams() =>
        new Dictionary<string, float>(_params.
            Select((p) => new KeyValuePair<string, float>(p.GetName(), p.GetValue())));
}
