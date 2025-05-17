using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Edge
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

    void Start()
    {
        singleton = this;
    }

    void Update()
    {

    }

    void OnDrawGizmos()
    {
        var children = new Dictionary<string, GameObject>();
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
}
