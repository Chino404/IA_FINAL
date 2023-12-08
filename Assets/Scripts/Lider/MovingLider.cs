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

    }

    public void OnUpdate()
    {

        if(!GameManager.Instance.InLineOfSight(_lider.transform.position, _lider.informacionDelRayo.point))
        {
            _lider.HelpProxNode();
            _fsm.ChangeState("Pathfinding");
        }

        AddForce(ArriveLeader(_lider.informacionDelRayo.point));

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
            return Seek(target);

        var desired = target - _lider.transform.position;
        desired.Normalize();
        desired *= _lider.maxSpeed * (dist / _lider.arriveRadius); //Si la dist la divido por el radio, me va achicando la velocidad

        return CalculateSteering(desired);
    }

    Vector3 Seek(Vector3 targetSeek)
    {
        var desired = targetSeek - _lider.transform.position; //Me va a dar una direccion
        desired.Normalize(); //Lo normalizo para que sea mas comodo
        desired *= _lider.maxSpeed; //Lo multiplico por la velocidad

        return CalculateSteering(desired);
    }

    //Calculo la fuerza con la que va a girar su direccion
    Vector3 CalculateSteering(Vector3 desired)
    {
        var steering = desired - _lider.velocity; //direccion = la dir. deseada - hacia donde me estoy moviendo
        steering = Vector3.ClampMagnitude(steering, _lider.maxForce);

        return steering;

    }

    public void AddForce(Vector3 dir)
    {
        _lider.velocity += dir;
        _lider.velocity.y = _lider.transform.position.y; //Mantengo mi altura
        _lider.velocity = Vector3.ClampMagnitude(_lider.velocity, _lider.maxSpeed);
    }
}
