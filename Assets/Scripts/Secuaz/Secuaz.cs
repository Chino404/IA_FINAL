using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Secuaz : MonoBehaviour, IDamageable
{
    public FSM fsm;

    [Header("Team")]
    public bool blueTeam;
    public Transform target;
    public List<Transform> targetsSeacuaz;
    [HideInInspector]public Lider myLeaderTarget;

    [HideInInspector]public NodePathfinding initialNode;
    [HideInInspector]public NodePathfinding goalNode;
    [HideInInspector]public List<NodePathfinding> path;

    [Header("Stats")]
    public float life = 100;
    public GameObject punch;
    [HideInInspector] public bool punchAviable;
    private bool _getDamage;
    private MeshRenderer _viewDamage;
    public float maxSpeed;
    public float maxForce; //La fuerza con la cual va a girar (El margen de giro)
    public Node firstNode;
    [HideInInspector]public Vector3 velocity; //Para donde miro

    [Header("Radios")]
    public float separationRadius;
    public float arriveRadius;
    public float viewRadius; //Area de vision
    public float viewAngle;  //Angulo de vision


    private void Start()
    {
        life = 100;
        punchAviable = true;

        if(blueTeam)
        {
            GameManager.Instance.secuazAzul.Add(this); //Me agrego a su lista de Secuaces azules
            myLeaderTarget = GameManager.Instance.liderAzul;
        }
        else
        {
            GameManager.Instance.secuazRojo.Add(this);
            myLeaderTarget = GameManager.Instance.liderRojo;
        }

        fsm = new FSM();

        fsm.CreateState("MoveToLeader", new MoveToLeader(fsm, this));
        fsm.CreateState("Pathfinding", new PathfindingSecuaz(fsm, this));
        fsm.CreateState("Attack", new AttackSecuaz(fsm, this));
        fsm.CreateState("Flight", new Flight(fsm, this));
    }

    private void Update()
    {
        firstNode.Execute(this);
        fsm.Execute();


    }

    public void Punch()
    {
        StartCoroutine(InvokePunch());
    }

    public IEnumerator InvokePunch()
    {
        punchAviable = false;
        punch.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        punch.SetActive(false);
        yield return new WaitForSeconds(1f);
        punchAviable = true;

    }

    public void GetDamage(int dmg)
    {
        life -= dmg;

        //if (!getDamage)
        //{
        //    life -= dmg;
        //    StartCoroutine(Invincibility());
        //}
    }

    public IEnumerator Invincibility()
    {
        _getDamage = true;
        yield return new WaitForSeconds(0.5f);
        _getDamage = true;

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

        goalNode = ManagerNodes.Instance.GetNodeProx(myLeaderTarget.transform.position);
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
