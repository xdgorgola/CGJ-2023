using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardUIElements : MonoBehaviour
{
    // UI ELEMENTS
    [SerializeField]
    private Text _costText = null;
    [SerializeField]
    private Text _nameText = null;
    [SerializeField]
    private Text _descriptionText = null;

    [SerializeField]
    private Image _cardImage = null;


    public void SetCardInfo(GameCards card)
    {
        _nameText.text = card.Name;
        _costText.text = card.WaterCost.ToString();
        _descriptionText.text = card.Description;
        _cardImage.sprite = card.Picture;
    }
}
