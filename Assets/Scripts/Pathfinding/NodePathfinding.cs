using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodePathfinding : MonoBehaviour
{
    public List<NodePathfinding> neighbors = new List<NodePathfinding>();

    public int cost;

    private void Start()
    {
        if(!ManagerNodes.Instance.nodes.Contains(this))
            ManagerNodes.Instance.nodes.Add(this);
    }
}
