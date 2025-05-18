using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public enum State
{
    Standing,
    Petting,
    OpeningDoor,
    PuttingFood,
}


public class WitchController : MonoBehaviour
{
    public ParticleSystem coverup;
    public Animator animator;
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
                coverup.Play();
                timeLeft = Random.Range(10, 15);
                transform.position = node.transform.position;
            }
        }
    }

    public void ForceMoveTo(string node)
    {
        coverup.Play();
        currentNode = network.children[node].name;
        transform.position = network.children[node].transform.position;
    }

    void DoNextFutureMove()
    {
        if (futurePath.Count > 0)
        {
            var next = futurePath[0];
            futurePath.RemoveAt(0);
            ForceMoveTo(next);
            RunAfterSeconds(1f, () =>
            {
                DoNextFutureMove();
            });
        }
    }

    public void ForceMoveSequence(params string[] sequence)
    {
        foreach (var item in sequence)
            futurePath.Add(item);
        DoNextFutureMove();
    }

    public void DebugPrintSomeRandomThing()
    {
        print("AAAAAAAAAAAA this is for debug :3");
    }

    IEnumerator WaitAndRun(float t, System.Action action)
    {
        yield return new WaitForSeconds(t);
        action();
    }

    void RunAfterSeconds(float t, System.Action action)
    {
        StartCoroutine(WaitAndRun(t, action));
    }
}
