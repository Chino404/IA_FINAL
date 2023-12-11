using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingSecuaz : IState
{
    FSM _fsm;
    Secuaz _secuaz;

    public PathfindingSecuaz(FSM fsm, Secuaz secuaz)
    {
        _fsm = fsm;
        _secuaz = secuaz;
    }

    public void OnEnter()
    {
        _secuaz.HelpProxNodeLeader();
        _secuaz.path = _secuaz.CalculateThetaStar(_secuaz.initialNode, _secuaz.goalNode);
    }

    public void OnUpdate()
    {
        _secuaz.AddForce(_secuaz.ObstacleAvoidance() * _secuaz.avoidWeight);
        _secuaz.Flocking();

        if (_secuaz.path.Count > 0)
         {
         
           _secuaz.AddForce(_secuaz.Seek(_secuaz.path[0].transform.position));
         
           if (Vector3.Distance(_secuaz.gameObject.transform.position, _secuaz.path[0].transform.position) <= 0.3f) _secuaz.path.RemoveAt(0);
         
           _secuaz.transform.position += _secuaz.velocity * Time.deltaTime;
           _secuaz.transform.forward = _secuaz.velocity;
         
         }

    }

    public void OnExit()
    {

    }
}
