using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Secuaz : MonoBehaviour, IDamageable
{
    public FSM fsm;

    [Header("Params. Team")]
    public bool blueTeam;
    public Transform targetEnemy;
    [HideInInspector]public Transform safeZoneTarget;
    public List<Transform> targetsSecuaz;
    [HideInInspector]public Lider myLeaderTarget;

    [HideInInspector]public NodePathfinding initialNode;
    [HideInInspector]public NodePathfinding goalNode;
    [HideInInspector]public List<NodePathfinding> path;

    [Header("Stats")]
    public float life = 100;
    public GameObject punch;
    [HideInInspector] public bool punchAviable;
    private bool _getDamage;
    public Color baseColor;
    public Color damageColor;
    private MeshRenderer _viewDamage;
    public float maxSpeed;
    public float maxForce; //La fuerza con la cual va a girar (El margen de giro)
    public LayerMask obstacleLayer;
    public float avoidWeight; //El peso con el que esquiva las cosas, q tanto se va a mover 
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
        _viewDamage = GetComponent<MeshRenderer>();
        punchAviable = true;

        if(blueTeam)
        {
            GameManager.Instance.secuazBlue.Add(this); //Me agrego a su lista de Secuaces azules
            myLeaderTarget = GameManager.Instance.liderAzul;
            safeZoneTarget = GameManager.Instance.blueSafeZone;
        }
        else
        {
            GameManager.Instance.secuazRed.Add(this);
            myLeaderTarget = GameManager.Instance.liderRojo;
            safeZoneTarget = GameManager.Instance.redSafeZone;
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

    #region Punch & Damage

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
        if (!_getDamage && life > 0)
        {
            life -= dmg;
            StartCoroutine(viewDamage());
        }
    }

    public IEnumerator viewDamage()
    {
        _getDamage = true;
        _viewDamage.material.color = damageColor;
        yield return new WaitForSeconds(0.15f);
        _viewDamage.material.color = baseColor;
        yield return new WaitForSeconds(0.15f);
        _viewDamage.material.color = damageColor;
        yield return new WaitForSeconds(0.15f);
        _viewDamage.material.color = baseColor;
        _getDamage = false;
    }
    #endregion

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

    public bool InFOVList(List<Transform> obj) //Si lo estoy viendo
    {
        foreach (Transform targetEnemy in obj)
        {
            var dir = targetEnemy.position - transform.position;

            if (dir.magnitude < viewRadius)
            {
                //Calculo un angulo de mi vision hacia adelante y la direccion de mi target 
                if (Vector3.Angle(transform.forward, dir) <= viewAngle * 0.5f)
                {
                    if (this.targetEnemy == null)
                        this.targetEnemy = targetEnemy.transform;

                    return GameManager.Instance.InLineOfSight(transform.position, targetEnemy.position); //Si no hay nada entre medio me devuelve True
                }
                else
                    this.targetEnemy = null;
            }
        }

        return false;
    }

    #region Movimiento

    #region FLOCKING
    public void Flocking()
    {
        if (blueTeam)
            AddForce(Separation(GameManager.Instance.secuazBlue, separationRadius) * GameManager.Instance.weightSeparation);
        else
            AddForce(Separation(GameManager.Instance.secuazRed, separationRadius) * GameManager.Instance.weightSeparation);

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

    public Vector3 Seek(Vector3 targetSeek)
    {
        var desired = targetSeek - transform.position; //Me va a dar una direccion
        desired.Normalize(); //Lo normalizo para que sea mas comodo
        desired *= maxSpeed; //Lo multiplico por la velocidad

        return CalculateSteering(desired);
    }

    //Calculo la fuerza con la que va a girar su direccion
    public Vector3 CalculateSteering(Vector3 desired)
    {
        var steering = desired - velocity; //direccion = la dir. deseada - hacia donde me estoy moviendo
        steering = Vector3.ClampMagnitude(steering, maxForce / 10);

        return steering;

    }

    public void AddForce(Vector3 dir)
    {
        velocity += dir;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
    }

    public Vector3 ObstacleAvoidance()
    {
        Vector3 pos = transform.position;
        Vector3 dir = transform.forward;
        float dist = velocity.magnitude; //Que tan rapido estoy yendo

        Debug.DrawLine(pos, pos + (dir * dist));

        if(Physics.SphereCast(pos, 1, dir, out RaycastHit hit, dist, obstacleLayer))
        {
            var obstacle = hit.transform; //Obtengo el transform del obstaculo q acaba de tocar
            Vector3 dirToObject =  obstacle.position - transform.position; //La direccion del obstaculo

            float anguloEntre = Vector3.SignedAngle(transform.forward, dirToObject, Vector3.up); //(Dir. hacia donde voy, Dir. objeto, Dir. mis costados)

            Vector3 desired = anguloEntre >= 0 ? -transform.right : transform.right; //Me meuvo para derecha o izquierda dependiendo donde esta el obstaculo
            desired.Normalize();
            desired *= maxSpeed;

            return CalculateSteering(desired);
        }

        return Vector3.zero;
    }

    #region AStar
    public List<NodePathfinding> CalculateAStar(NodePathfinding startingNode, NodePathfinding goalNode)
    {
        PriorityQueue<NodePathfinding> frontier = new PriorityQueue<NodePathfinding>();
        frontier.Enqueue(startingNode, 0);

        Dictionary<NodePathfinding, NodePathfinding> cameFrom = new Dictionary<NodePathfinding, NodePathfinding>();
        cameFrom.Add(startingNode, null);

        Dictionary<NodePathfinding, int> costSoFar = new Dictionary<NodePathfinding, int>();
        costSoFar.Add(startingNode, 0);

        while (frontier.Count > 0)
        {
            NodePathfinding current = frontier.Dequeue();

            if (current == goalNode)
            {
                List<NodePathfinding> path = new List<NodePathfinding>();

                while (current != startingNode)
                {
                    path.Add(current);
                    current = cameFrom[current];
                }

                path.Reverse();
                return path;
            }

            foreach (var item in current.neighbors)
            {

                int newCost = costSoFar[current] + item.cost; //Calculo el costo como en Dijkstra
                float priority = newCost + Vector3.Distance(item.transform.position, goalNode.transform.position); //Calculo la distancia del nodo actual hasta la meta

                if (!costSoFar.ContainsKey(item))
                {
                    if (!frontier.ContainsKey(item))
                        frontier.Enqueue(item, priority);
                    cameFrom.Add(item, current);
                    costSoFar.Add(item, newCost);
                }
                else if (costSoFar[item] > newCost)
                {
                    if (!frontier.ContainsKey(item))
                        frontier.Enqueue(item, priority);
                    cameFrom[item] = current;
                    costSoFar[item] = newCost;
                }
            }
        }
        return new List<NodePathfinding>();
    }
    #endregion

    #region Theta AStar
    public List<NodePathfinding> CalculateThetaStar(NodePathfinding startingNode, NodePathfinding goalNode) //Me borra los nodos q estan de más en el recorrido
    {
        var listNode = CalculateAStar(startingNode, goalNode); //Llamo a AStar

        int current = 0;

        while (current + 2 < listNode.Count)
        {
            if (GameManager.Instance.InLineOfSight(listNode[current].transform.position, listNode[current + 2].transform.position)) //Si puedo llegar a un nodo siguiente
            {
                listNode.RemoveAt(current + 1); //Borro el anterior nodo
            }
            else
                current++; //Sino me lo sumo
        }

        return listNode;
    }
    #endregion

    #endregion


    public void HelpProxNodeLeader()
    {
        initialNode = ManagerNodes.Instance.GetNodeProx(transform.position);

        goalNode = ManagerNodes.Instance.GetNodeProx(myLeaderTarget.transform.position);
    }

    public void HelpProxNodeSafeZone()
    {
        initialNode = ManagerNodes.Instance.GetNodeProx(transform.position);

        goalNode = ManagerNodes.Instance.GetNodeProx(safeZoneTarget.transform.position);
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
