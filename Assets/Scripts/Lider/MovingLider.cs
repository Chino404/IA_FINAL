using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingLider : IState
{
    FSM _fsm;
    Lider _lider;

    public MovingLider(FSM fsm, Lider lider)
    {
        _fsm = fsm;
        _lider = lider;
    }

    public void OnEnter()
    {
        if(!GameManager.Instance.InLineOfSight(_lider.transform.position, _lider.informacionDelRayo.point))
        {
            _lider.HelpProxNode();
            _fsm.ChangeState("Pathfinding");
        }
    }

    public void OnUpdate()
    {
        _lider.AddForce(ArriveLeader(_lider.informacionDelRayo.point));
        _lider.AddForce(_lider.ObstacleAvoidance() * _lider.avoidWeight);

        _lider.transform.position += _lider.velocity * Time.deltaTime;
        _lider.transform.forward = _lider.velocity; //Que mire para donde se esta moviendo
    }

    public void OnExit()
    {

    }

    Vector3 ArriveLeader(Vector3 target)
    {
        var dist = Vector3.Distance(_lider.transform.position, target);

        if (dist > _lider.arriveRadius)
            return _lider.Seek(target);

        var desired = target - _lider.transform.position;
        desired.Normalize();
        desired *= _lider.maxSpeed * (dist / _lider.arriveRadius); //Si la dist la divido por el radio, me va achicando la velocidad

        return _lider.CalculateSteering(desired);
    }
}
