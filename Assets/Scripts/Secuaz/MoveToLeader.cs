using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToLeader : IState
{
    FSM _fsm;
    Secuaz _secuaz;

    public MoveToLeader(FSM fsm, Secuaz secuaz) 
    {
        _fsm = fsm;
        _secuaz = secuaz;
    }

    public void OnEnter()
    {

    }

    public void OnUpdate()
    {
        _secuaz.AddForce(Arrive(_secuaz.myLeaderTarget.transform.position));
        _secuaz.AddForce(_secuaz.ObstacleAvoidance() * _secuaz.avoidWeight);

        _secuaz.Flocking();

        _secuaz.transform.position += _secuaz.velocity * Time.deltaTime;
        _secuaz.transform.forward = _secuaz.velocity; //Que mire para donde se esta moviendo
    }

    public void OnExit()
    {

    }

    Vector3 Arrive(Vector3 target)
    {
        var dist = Vector3.Distance(_secuaz.transform.position, target);

        if (dist > _secuaz.arriveRadius)
            return _secuaz.Seek(target);

        var desired = target - _secuaz.transform.position;
        desired.Normalize();
        desired *= _secuaz.maxSpeed * ((dist - _secuaz.viewRadius) / _secuaz.arriveRadius); //Si la dist la divido por el radio, me va achicando la velocidad

        return _secuaz.CalculateSteering(desired);
    }
}
