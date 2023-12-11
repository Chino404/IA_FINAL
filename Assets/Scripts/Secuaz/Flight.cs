using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flight : IState
{
    FSM _fsm;
    Secuaz _secuaz;

    public Flight(FSM fsm, Secuaz secuaz)
    {
        _fsm = fsm;
        _secuaz = secuaz;
    }
    
    public void OnEnter()
    {
        _secuaz.HelpProxNodeSafeZone();
        _secuaz.path = _secuaz.CalculateThetaStar(_secuaz.initialNode, _secuaz.goalNode);
    }

    public void OnUpdate()
    {
        _secuaz.AddForce(_secuaz.ObstacleAvoidance() * _secuaz.avoidWeight);
        //_secuaz.Flocking();

        if (_secuaz.path.Count > 0)
        {

            _secuaz.AddForce(_secuaz.Seek(_secuaz.path[0].transform.position));

            if (Vector3.Distance(_secuaz.gameObject.transform.position, _secuaz.path[0].transform.position) <= 0.3f) _secuaz.path.RemoveAt(0);

            _secuaz.transform.position += _secuaz.velocity * Time.deltaTime;
            _secuaz.transform.forward = _secuaz.velocity;
        }
        else
        {
            if (_secuaz.life <100)
                _secuaz.life += Time.deltaTime;
        }
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
