using System.Collections.Generic;
using UnityEngine;


public class DeckScript : MonoBehaviour
{
    public List<GameCards> cartasIniciales = new List<GameCards>();
    //sistema del mazo jugable
    [SerializeField]
     private List<GameCards> deckJugable = new List<GameCards>();
    //mazo completo
    [SerializeField]
    private List<GameCards> deckCompleto = new List<GameCards>();
    //mazo descarte
    private List<GameCards> deckDescarte = new List<GameCards>();

    
    //intento de funcion para sacar carta
    public GameCards DrawCard()
    {
        if(deckJugable.Count >= 1)
        {
            GameCards randCard = deckJugable[Random.Range(0, deckJugable.Count)];
            CardUsed(randCard);
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
        deckJugable.Remove(cartaUsada);
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
}
