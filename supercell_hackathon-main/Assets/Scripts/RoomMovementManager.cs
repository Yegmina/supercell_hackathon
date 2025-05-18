using UnityEngine;

public class RoomMovementManager : MonoBehaviour
{
    public string room;

    private void OnTriggerEnter(Collider other)
    {
        CatController chungus = other.GetComponent<CatController>();
        if (chungus != null)
        {
            Debug.Log("Entered room " + room);
            EventBuffer.PushEvent(new Event("enters", room));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        CatController chungus = other.GetComponent<CatController>();
        if (chungus != null)
        {
            Debug.Log("Left room " + room);
            EventBuffer.PushEvent(new Event("leaves", room));
        }
    }
}
