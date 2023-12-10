using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class ManagerNodes : MonoBehaviour
{
    public static ManagerNodes Instance;
    public List<NodePathfinding> nodes;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            var currentNode = nodes[i];

            for (int j = 0; j < nodes.Count; j++)
            {
                if (nodes[j] == currentNode || currentNode.neighbors.Contains(nodes[j]))
                    continue;

                if (GameManager.Instance.InLineOfSight(currentNode.transform.position, nodes[j].transform.position))
                    currentNode.neighbors.Add(nodes[j]);
            }
        }
    }

    /// <summary>
    /// El nodo mas cercano al objetivo
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public NodePathfinding GetNodeProx(Vector3 pos)
    {
        var disProx = Mathf.Infinity;
        NodePathfinding nodeMasCercano = default;

        for (int i = 0; i < nodes.Count; i++)
        {
            if (GameManager.Instance.InLineOfSight(nodes[i].transform.position, pos)) //Pregunto si hay algo que interfiera entre el nodo y la pos del objetivo
            {
                var dis = pos - nodes[i].transform.position;

                if (dis.magnitude < disProx)
                {
                    disProx = dis.magnitude;
                    nodeMasCercano = nodes[i];
                }
            }
        }

        return nodeMasCercano;
    }
}
