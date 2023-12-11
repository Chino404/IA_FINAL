using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingLider : IState
{
    FSM _fsm;
    Lider _lider;

    public PathfindingLider( FSM fsm, Lider lider )
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
        _lider.AddForce(_lider.ObstacleAvoidance() * _lider.avoidWeight);

        if (GameManager.Instance.InLineOfSight(_lider.transform.position, _lider.informacionDelRayo.point))
            _fsm.ChangeState("Moving");

        else if (_lider.path.Count > 0)
        {

            _lider.AddForce(_lider.Seek(_lider.path[0].transform.position));

            if (Vector3.Distance(_lider.gameObject.transform.position, _lider.path[0].transform.position) <= 0.3f) _lider.path.RemoveAt(0);

            _lider.transform.position += _lider.velocity * Time.deltaTime;
            _lider.transform.forward = _lider.velocity;

        }

        else if (_lider.path.Count <= 0) _fsm.ChangeState("Moving");
    }

    public void OnExit()
    {

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
}
