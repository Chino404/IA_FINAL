using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleLider : IState
{
    public void OnEnter()
    {
        Debug.Log("Idle");
    }

    public void OnUpdate()
    {

    }

    public void OnExit()
    {

    }
}
