using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lider : MonoBehaviour
{
    FSM _fsm;

    public Node initialNode;
    public Node goalNode;
    public List<Node> path;

    [Header("Radios")]
    public float separationRadius;
    public float arriveRadius;
    public float viewRadius; //Area de vision
    public float viewAngle;  //Angulo de vision

    [Header("Velocidades")]
    public float maxSpeed;
    public float maxForce;
    [HideInInspector] public Vector3 velocity;


    public LayerMask mascaraPiso;
    [HideInInspector] public Ray miRayo;
    [HideInInspector] public RaycastHit informacionDelRayo;

    private void Start()
    {

        _fsm = new FSM();

        _fsm.CreateState("Moving", new MovingLider(_fsm, this));
        _fsm.CreateState("Pathfinding", new Pathfinding(_fsm, this));
        _fsm.CreateState("Idle", new IdleLider());

        _fsm.ChangeState("Idle");
    }

    private void Update()
    {

        _fsm.Execute();

        if (Input.GetMouseButtonDown(0))
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
