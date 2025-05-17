using UnityEngine;

public class VisibleThing : MonoBehaviour
{
    private Rigidbody rb;
    private bool wasFalling = false;

    public string name;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (rb == null) return;

        if (!wasFalling && !rb.IsSleeping() && rb.linearVelocity.magnitude > 0.1f)
            wasFalling = true;

        if (wasFalling && rb.IsSleeping())
        {
            EventBuffer.PushEvent(new Event("knocked over", name));
            wasFalling = false;
        }
    }
}
