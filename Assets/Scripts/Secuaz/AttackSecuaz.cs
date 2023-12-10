using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackSecuaz : IState
{
    FSM _fsm;
    Secuaz _secuaz;

    public AttackSecuaz(FSM fsm, Secuaz secuaz) 
    {
        _fsm = fsm;
        _secuaz = secuaz;
    }

    public void OnEnter()
    {

    }

    public void OnUpdate()
    {
        AddForce(Arrive(_secuaz.target.transform.position));
        Flocking();
        _secuaz.transform.position += _secuaz.velocity * Time.deltaTime;
        _secuaz.transform.forward = _secuaz.velocity;

        if (_secuaz.punchAviable)
            _secuaz.Punch();
    }

    public void OnExit()
    {

    }

    #region FLOCKING
    void Flocking()
    {
         AddForce(Separation(GameManager.Instance.secuazAzul, _secuaz.separationRadius) * GameManager.Instance.weightSeparation);
         AddForce(Separation(GameManager.Instance.secuazRojo, _secuaz.separationRadius) * GameManager.Instance.weightSeparation);

    }

    Vector3 Separation(List<Secuaz> boids, float radius)
    {
        Vector3 desired = Vector3.zero; //Dir deseada
        foreach (var item in boids)
        {
            var dir = item.transform.position - _secuaz.transform.position; //Saco la direccion de la posicion del boid menos la mia

            if (dir.magnitude > radius || item == _secuaz) //Si la magnitud de la direccion es mayor al radio o soy yo...
                continue;

            desired -= dir; //Voy restando a mi direccion deseado para eventualmente separarme (ir al lado opuesto)
        }

        if (desired == Vector3.zero) //Si no hay nadie en mi radio...
            return desired;

        desired.Normalize();
        desired *= _secuaz.maxSpeed;

        return CalculateSteering(desired);
    }
    #endregion


    Vector3 Arrive(Vector3 target)
    {
        var dist = Vector3.Distance(_secuaz.transform.position, target);

        if (dist > _secuaz.arriveRadius)
            return Seek(target);

        var desired = target - _secuaz.transform.position;
        desired.Normalize();
        desired *= _secuaz.maxSpeed * ((dist - _secuaz.separationRadius) / _secuaz.arriveRadius); //Si la dist la divido por el radio, me va achicando la velocidad

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

    public void AddForce(Vector3 dir)
    {
        _secuaz.velocity += dir;
        _secuaz.velocity = Vector3.ClampMagnitude(_secuaz.velocity, _secuaz.maxSpeed);
    }

}
