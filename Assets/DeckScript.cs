using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class DeckScript : MonoBehaviour
{
    //sistema de clima
    public climatechanger clima;
    public int contador; // para asegurar 12 turnos
    public bool endTurn; // para asegurar que se hace el cambio de temporada al finalizar turno

    //sistema del mazo
    public List<GameCards> deck = new List<GameCards>();
    public Transform[] cardSlots;
    public bool[] espacioLibre;
    public Text deckSizeText;

    //intento de funcion para sacar carta
    public void DrawCard()
    {
        if(deck.Count >= 1)
        {
            GameCards randCard = deck[Random.Range(0, deck.Count)];

            for (int i = 0; i < espacioLibre.Length; i++)
            {
                if (espacioLibre[i] == true)
                {
                   
                    espacioLibre[i] = false;
                    deck.Remove(randCard);
                    return;
                }
                }
            }

        }
    






    //Sumando al contador y reiniciando el bool
   public  void ContadorChecker()
    {
        if (endTurn == true)
        {
            contador++;
            Debug.Log("subio un numero");
            endTurn = false;
        }
    }
   //una vez arranca el juego en primavera los demas climas son automaticos
    void CambioClima()
    {
        if (contador == 12)
        {
            clima.temporada++;
            clima.Cambiador();
            contador = 0;
            Debug.Log("Entre al cambio de clima");

        }
    }
    //lo pongo para probar el tema del boton en un solo script
   public void Endturn()
    {
        endTurn  = true;

    }





    void Update()
    {
        ///chequeando la temporada y cambiandola
        ContadorChecker();
        CambioClima();
        ///manteniendo el numero de cartas del deck visibles
    deckSizeText.text = deck.Count.ToString();
    }


}
