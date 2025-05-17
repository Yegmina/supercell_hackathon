using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class Edge
{
    public string start;
    public string end;
    public float weight;
    public string trigger;
    public bool enabled;
}

[ExecuteInEditMode]
public class PathNetwork : MonoBehaviour
{
    public static PathNetwork singleton;
    [SerializeField]
    public List<Edge> edges;
    public Dictionary<string, GameObject> children = new Dictionary<string, GameObject>();
    public Dictionary<string, string> associatedNodeNames = new Dictionary<string, string>();

    void Start()
    {
        singleton = this;
        foreach (Transform child in transform)
            children[child.name] = child.gameObject;
    }

    void Update()
    {

    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        foreach (Transform child in transform)
        {
            children[child.name] = child.gameObject;
            Gizmos.DrawWireSphere(child.position, 0.4f);
        }

        foreach (var edge in edges)
        {
            var start = children[edge.start];
            var end = children[edge.end];

            if (edge.enabled)
                Gizmos.color = Color.blue;
            else
                Gizmos.color = Color.red;

            Gizmos.DrawLine(start.transform.position, end.transform.position);
        }
    }

    public string GetRandomAdjacentNode(string nodeName)
    {
        List<string> adjacentNodeNames = new List<string>();

        foreach (Edge edge in edges)
        {
            if (!edge.enabled) continue;

            if (edge.start == nodeName)
            {
                adjacentNodeNames.Add(edge.end);
            }
            else if (edge.end == nodeName)
            {
                adjacentNodeNames.Add(edge.start);
            }
        }

        if (adjacentNodeNames.Count == 0)
        {
            return null; // No adjacent nodes found
        }

        string randomAdjacentName = adjacentNodeNames[UnityEngine.Random.Range(0, adjacentNodeNames.Count)];

        children.TryGetValue(randomAdjacentName, out GameObject adjacentNode);
        return adjacentNode.name;
    }
}
