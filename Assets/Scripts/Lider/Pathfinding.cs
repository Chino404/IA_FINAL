using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : IState
{
    FSM _fsm;
    Lider _lider;

    public Pathfinding( FSM fsm, Lider lider )
    {
        _fsm = fsm;
        _lider = lider;
    }

    public void OnEnter()
    {
        _lider.path = CalculateThetaStar(_lider.initialNode, _lider.goalNode);
    }

    public void OnUpdate()
    {
        if (_lider.path.Count > 0)
        {

            AddForce(Seek(_lider.path[0].transform.position));

            if (Vector3.Distance(_lider.gameObject.transform.position, _lider.path[0].transform.position) <= 0.3f) _lider.path.RemoveAt(0);

            _lider.transform.position += _lider.velocity * Time.deltaTime;
            _lider.transform.forward = _lider.velocity;

        }

        else if (_lider.path.Count <= 0) _fsm.ChangeState("Moving");
    }

    public void OnExit()
    {

    }

    Vector3 Seek(Vector3 targetSeek)
    {
        var desired = targetSeek - _lider.transform.position; //Me va a dar una direccion
        desired.Normalize(); //Lo normalizo para que sea mas comodo
        desired *= _lider.maxSpeed; //Lo multiplico por la velocidad

        return CalculateSteering(desired);
    }

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

    #region AStar
    public List<Node> CalculateAStar(Node startingNode, Node goalNode)
    {
        PriorityQueue<Node> frontier = new PriorityQueue<Node>();
        frontier.Enqueue(startingNode, 0);

        Dictionary<Node, Node> cameFrom = new Dictionary<Node, Node>();
        cameFrom.Add(startingNode, null);

        Dictionary<Node, int> costSoFar = new Dictionary<Node, int>();
        costSoFar.Add(startingNode, 0);

        while (frontier.Count > 0)
        {
            Node current = frontier.Dequeue();

            if (current == goalNode)
            {
                List<Node> path = new List<Node>();

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
        return new List<Node>();
    }
    #endregion

    #region Theta AStar
    public List<Node> CalculateThetaStar(Node startingNode, Node goalNode) //Me borra los nodos q estan de más en el recorrido
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
}
