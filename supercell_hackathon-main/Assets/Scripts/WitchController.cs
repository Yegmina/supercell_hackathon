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

            if (Vector3.Distance(transform.position, node.transform.position) < 0.2)
            {
                transform.position = node.transform.position;

                currentNode = node.name;
                state = State.Standing;
                timeLeft = Random.Range(2f, 5f);
            }
        }
    }
}
