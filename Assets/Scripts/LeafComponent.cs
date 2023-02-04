using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeafComponent : MonoBehaviour
{
    [SerializeField] private int turnos = 0;
    [SerializeField] private bool isActive = false;

    public int Turnos
    {
        get { return turnos; }
        set { turnos = value; }
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
