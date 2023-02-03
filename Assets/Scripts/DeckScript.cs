using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class DeckScript : MonoBehaviour
{
    public List<GameCards> cartasIniciales = new List<GameCards>();
    //sistema del mazo jugable
    [SerializeField]
     private List<GameCards> deckJugable = new List<GameCards>();
     public Text deckJugableText;
    //mazo completo
    [SerializeField]
    private List<GameCards> deckCompleto = new List<GameCards>();
    public Text deckCompletoText;
    //mazo descarte
    [SerializeField]
    private List<GameCards> deckDescarte = new List<GameCards>();
    public Text deckDescarteText;

    
    //intento de funcion para sacar carta
    public GameCards DrawCard()
    {
        if(deckJugable.Count >= 1)
        {
            GameCards randCard = deckJugable[Random.Range(0, deckJugable.Count)];
            deckJugable.Remove(randCard);
            return randCard;                
        }
        else
        {
            shuffle();
            return DrawCard();
        }
    }


    public void CardUsed(GameCards cartaUsada)
    {
        deckDescarte.Add(cartaUsada);     
    }


    public void addCard(GameCards cartaNueva)
    {
        deckJugable.Add(cartaNueva);
        deckCompleto.Add(cartaNueva);
    }


    private void shuffle()
    {
        deckJugable = deckDescarte;
        deckDescarte = new List<GameCards>();
    }
    
   
    //Funcion de prueba. No es final
    public void intento()
    {
      CardUsed(DrawCard());
    }

    
    void Update()
    {
        deckCompletoText.text = deckCompleto.Count.ToString();
        deckDescarteText.text = deckDescarte.Count.ToString();
        deckJugableText.text = deckJugable.Count.ToString();

    }
}
