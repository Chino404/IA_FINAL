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

    }

    public void OnUpdate()
    {
        AddForce(Flee(_secuaz.target.transform.position));
        _secuaz.transform.position += _secuaz.velocity * Time.deltaTime;
        _secuaz.transform.forward = _secuaz.velocity;
    }

    public void OnExit()
    {

    }

    Vector3 Arrive(Vector3 target)
    {
        var dist = Vector3.Distance(_secuaz.transform.position, target);

        if (dist > _secuaz.arriveRadius)
            return Seek(target);

        var desired = target - _secuaz.transform.position;
        desired.Normalize();
        desired *= _secuaz.maxSpeed * ((dist - _secuaz.viewRadius) / _secuaz.arriveRadius); //Si la dist la divido por el radio, me va achicando la velocidad

        return CalculateSteering(desired);
    }

    Vector3 Seek(Vector3 targetSeek)
    {
        var desired = targetSeek - _secuaz.transform.position; //Me va a dar una direccion
        desired.Normalize(); //Lo normalizo para que sea mas comodo
        desired *= _secuaz.maxSpeed; //Lo multiplico por la velocidad

        return CalculateSteering(desired);
    }

    //Calculo la fuerza con la que va a girar su direccion
    Vector3 CalculateSteering(Vector3 desired)
    {
        var steering = desired - _secuaz.velocity; //direccion = la dir. deseada - hacia donde me estoy moviendo
        steering = Vector3.ClampMagnitude(steering, _secuaz.maxForce);

        return steering;

    }

    Vector3 Flee(Vector3 targetFlee)
    {
        return -Seek(targetFlee); //Es negativo para que huya
    }

    public void AddForce(Vector3 dir)
    {
        _secuaz.velocity += dir;
        _secuaz.velocity = Vector3.ClampMagnitude(_secuaz.velocity, _secuaz.maxSpeed);
    }

}
