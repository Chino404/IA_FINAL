using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Secuaz : MonoBehaviour
{
    [Header("Radios")]
    public float separationRadius;
    public float arriveRadius;
    public float viewRadius; //Area de vision
    public float viewAngle;  //Angulo de vision

    [Header("Velocidades")]
    public float maxSpeed;
    public float maxForce; //La fuerza con la cual va a girar (El margen de giro)
    Vector3 _velocity; //Para donde miro
    public Vector3 Velocity { get { return _velocity; } }

    private void Start()
    {
        GameManager.Instance.secuaz.Add(this); //Me agrego a su lista de Boids
    }

    private void Update()
    {
        AddForce(Arrive(GameManager.Instance.lider.transform.position));
        Flocking();

        transform.position += _velocity * Time.deltaTime;
        transform.forward = _velocity; //Que mire para donde se esta moviendo
    }

    #region FLOCKING
    void Flocking()
    {
        AddForce(Separation(GameManager.Instance.secuaz, separationRadius) * GameManager.Instance.weightSeparation);
    }

    Vector3 Separation(List<Secuaz> boids, float radius)
    {
        Vector3 desired = Vector3.zero; //Dir deseada
        foreach (var item in boids)
        {
            var dir = item.transform.position - transform.position; //Saco la direccion de la posicion del boid menos la mia

            if (dir.magnitude > radius || item == this) //Si la magnitud de la direccion es mayor al radio o soy yo...
                continue;

            desired -= dir; //Voy restando a mi direccion deseado para eventualmente separarme (ir al lado opuesto)
        }

        if (desired == Vector3.zero) //Si no hay nadie en mi radio...
            return desired;

        desired.Normalize(); 
        desired *= maxSpeed; 

        return CalculateSteering(desired);
    }

    #endregion

    Vector3 Arrive(Vector3 target)
    {
        var dist = Vector3.Distance(transform.position, target);

        if (dist > arriveRadius)
            return Seek(target);

        var desired = target - transform.position; 
        desired.Normalize();
        desired *= maxSpeed * ((dist - viewRadius) / arriveRadius); //Si la dist la divido por el radio, me va achicando la velocidad

        return CalculateSteering(desired);
    }

    Vector3 Seek(Vector3 targetSeek)
    {
        var desired = targetSeek - transform.position; //Me va a dar una direccion
        desired.Normalize(); //Lo normalizo para que sea mas comodo
        desired *= maxSpeed; //Lo multiplico por la velocidad

        return CalculateSteering(desired);
    }

    //Calculo la fuerza con la que va a girar su direccion
    Vector3 CalculateSteering(Vector3 desired)
    {
        var steering = desired - _velocity; //direccion = la dir. deseada - hacia donde me estoy moviendo
        steering = Vector3.ClampMagnitude(steering, maxForce);

        return steering;

    }

    Vector3 Flee(Vector3 targetFlee)
    {
        return -Seek(targetFlee); //Es negativo para que huya
    }

    public void AddForce(Vector3 dir)
    {
        _velocity += dir;
        _velocity = Vector3.ClampMagnitude(_velocity, maxSpeed);
    }

    #region Gizmos
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, separationRadius);

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        Vector3 lineA = GetVectorFromAngle(viewAngle * 0.5f + transform.eulerAngles.y);
        Vector3 lineB = GetVectorFromAngle(-viewAngle * 0.5f + transform.eulerAngles.y);

        Gizmos.DrawLine(transform.position, transform.position + lineA * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + lineB * viewRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, arriveRadius);
    }

    Vector3 GetVectorFromAngle(float angle)
    {
        return new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0, Mathf.Cos(angle * Mathf.Deg2Rad));
    }

#endregion
}
