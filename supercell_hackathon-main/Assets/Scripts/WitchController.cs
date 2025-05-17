using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public enum State
{
    Standing,
    Walking,
    Petting,
    OpeningDoor,
    PuttingFood,
}

public class WitchController : MonoBehaviour
{
    public PathNetwork network;
    public string currentNode;
    public State state = State.Standing;

    public float timeLeft;
    public float moveSpeed = 2f;

    public List<string> futurePath = new List<string>();

    void Start()
    {
        Edge edge = network.edges[0];
        currentNode = edge.start;
    }

    void Update()
    {
        timeLeft -= Time.deltaTime;
        var node = network.children[currentNode];

        if (state == State.Standing)
        {
            if (timeLeft < 0)
            {
                var newNode = network.GetRandomAdjacentNode(currentNode);
                Debug.Log(newNode);
                network.FindEdge(currentNode, newNode).events.Invoke();
                currentNode = newNode;

                state = State.Walking;
            }
        }
        else if (state == State.Walking)
        {
            timeLeft = 2f;
            transform.position = Vector3.MoveTowards(
                transform.position,
                node.transform.position,
                moveSpeed * Time.deltaTime
            );

            Vector3 direction = (node.transform.position - transform.position).normalized;
            if (direction != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(direction);

            if (Vector3.Distance(transform.position, node.transform.position) < 0.2)
            {
                if (futurePath.Count > 0)
                {
                    currentNode = futurePath[0];
                    futurePath.RemoveAt(0);
                }
                else
                {
                    transform.position = node.transform.position;
                    currentNode = node.name;
                    state = State.Standing;
                    timeLeft = 3f;
                }

            }
        }
    }

    public void ForceMoveTo(string node)
    {
        state = State.Walking;
        currentNode = network.children[node].name;
    }

    public void ForceMoveSequence(params string[] sequence)
    {
        ForceMoveTo(sequence[0]);
        foreach (var item in sequence)
            futurePath.Add(item);
    }

    public void DebugPrintSomeRandomThing()
    {
        print("AAAAAAAAAAAA this is for debug :3");
    }
}
