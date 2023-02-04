using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class LeafComponent : MonoBehaviour
{
    private Animator _animator = null;

    private int _count = 0;
    private int _maxTurns = 0;
    private bool _isActive = false;

    public bool IsActive
    {
        get { return _isActive; }
    }

    public int Turns
    {
        get { return _count; }
    }


    private void Awake()
    {
        _animator = GetComponent<Animator>();    
    }


    public void KillLeaf()
    {
        _count = 0;
        _isActive = false;
        _animator.SetTrigger("Shrink");
    }


    public void StartLeaf(int duration)
    {
        _count = 0;
        _maxTurns = duration;
        _isActive = true;
        _animator.SetTrigger("Grow");
    }
    

    public void Tick()
    {
        _count = _count + 1;
        if (_count >= _maxTurns)
        {
            KillLeaf();
            return;
        }
    }
}
