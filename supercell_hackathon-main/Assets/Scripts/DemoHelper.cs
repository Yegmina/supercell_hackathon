using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoHelper : MonoBehaviour
{
    public CatController controller;
    public WitchController witch;
    public PathNetwork network;
    public string firstDoorTrigger;

    void Update()
    {
        if (Input.GetKey(KeyCode.Escape) && Input.GetKeyDown(KeyCode.H))
            witch.ForceMoveTo("A");
        if (Input.GetKey(KeyCode.Escape) && Input.GetKeyDown(KeyCode.J))
        {
            network.FindEdge(firstDoorTrigger).enabled = true;
            witch.ForceMoveSequence("A", "E");
        }
        if (Input.GetKey(KeyCode.Escape) && Input.GetKeyDown(KeyCode.K))
        {
            var closest = GetClosestChild(network.gameObject, controller.transform.position);
            witch.ForceMoveTo(closest.name);
        }
        if (Input.GetKey(KeyCode.Escape) && Input.GetKeyDown(KeyCode.L))
        {
            SpeechBubbleRequester.singleton.OutOfOrderRequest();
        }

        if (Input.GetButtonDown("Debug"))
        {
            var o = "";
            foreach (var ev in EventBuffer.PullEvents())
                o += ev + ",";

            foreach (var s in EventBuffer.state)
            {
                o += s.Key + ":";
                foreach (var t in s.Value)
                    o += t + "/";
                o += ",";
            }

            Debug.Log(o);
        }
    }

    GameObject GetClosestChild(GameObject root, Vector3 position)
    {
        GameObject closest = null;
        float minDistance = Mathf.Infinity;

        foreach (Transform child in root.transform)
        {
            float dist = Vector3.Distance(position, child.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = child.gameObject;
            }
        }

        return closest;
    }
}