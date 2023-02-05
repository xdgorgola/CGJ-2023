using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatBar : MonoBehaviour
{

    [SerializeField]
    private int _maxValue = 100;

    [SerializeField]
    private Image _fillImage;
    [SerializeField]
    private Text text;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(_fillImage != null, "Missing fill image component in StatBar");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateValue(int newValue)
    {
        float targetVal = (float)newValue / (float)_maxValue;
        text.text = newValue.ToString();
        _fillImage.fillAmount = Mathf.Clamp(targetVal, 0, 1);
    }
}
