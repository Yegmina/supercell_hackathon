using UnityEngine;

public class DemoHelper : MonoBehaviour
{
    public WitchController witch;
    public PathNetwork network;
    public string firstDoorTrigger;

    void Update()
    {
        if (Input.GetKey(KeyCode.Escape) && Input.GetKeyDown(KeyCode.Q))
            witch.ForceMoveTo("A");
        if (Input.GetKey(KeyCode.Escape) && Input.GetKeyDown(KeyCode.E))
        {
            witch.ForceMoveTo("A");
            network.FindEdge(firstDoorTrigger).enabled = true;
        }
    }
}
