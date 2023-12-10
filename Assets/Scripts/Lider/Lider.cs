using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lider : MonoBehaviour
{
    FSM _fsm;

    public bool blueTeam;
    public bool redTeam;

    [HideInInspector] public NodePathfinding initialNode;
    [HideInInspector] public NodePathfinding goalNode;
    public List<NodePathfinding> path;

    [Header("Stats")]
    public float maxSpeed;
    public float maxForce;
    public Node firstNode;
    [HideInInspector] public Vector3 velocity;

    [Header("Radios")]
    public float separationRadius;
    public float arriveRadius;
    public float viewRadius; //Area de vision
    public float viewAngle;  //Angulo de vision



    public LayerMask mascaraPiso;
    [HideInInspector] public Ray miRayo;
    [HideInInspector] public RaycastHit informacionDelRayo;

    private void Start()
    {

        _fsm = new FSM();

        _fsm.CreateState("Moving", new MovingLider(_fsm, this));
        _fsm.CreateState("Pathfinding", new PathfindingLider(_fsm, this));
        _fsm.CreateState("Idle", new IdleLider());

        _fsm.ChangeState("Idle");
    }

    private void Update()
    {

        _fsm.Execute();

        if (Input.GetMouseButtonDown(0) && blueTeam)
        {
            //Lanzo un rayo desde la posicion del mouse respecto a la camara
            miRayo = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(miRayo, out informacionDelRayo, Mathf.Infinity, mascaraPiso))
            {
                velocity = informacionDelRayo.point;

                _fsm.ChangeState("Moving");
            }
        }

        if(Input.GetMouseButtonDown(1) && redTeam)
        {
            //Lanzo un rayo desde la posicion del mouse respecto a la camara
            miRayo = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(miRayo, out informacionDelRayo, Mathf.Infinity, mascaraPiso))
            {
                velocity = informacionDelRayo.point;

                _fsm.ChangeState("Moving");
            }
        }



    }

    public bool InFOV(Transform obj) //Si lo estoy viendo
    {
        var dir = obj.position - transform.position;

        if (dir.magnitude < viewRadius)
        {
            //Calculo un angulo de mi vision hacia adelante y la direccion de mi target 
            if (Vector3.Angle(transform.forward, dir) <= viewAngle * 0.5f)
            {
                return GameManager.Instance.InLineOfSight(transform.position, obj.position); //Si no hay nada entre medio me devuelve True
            }
        }

        return false;
    }

    public void HelpProxNode()
    {
        initialNode = ManagerNodes.Instance.GetNodeProx(transform.position);
        goalNode = ManagerNodes.Instance.GetNodeProx(velocity);
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
