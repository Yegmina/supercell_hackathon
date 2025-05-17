using UnityEngine;

public class DoorRotator : MonoBehaviour
{
    public float openAngle = 90f;
    public float rotationSpeed = 2f;
    public Vector3 positionOffset = new Vector3(1f, 0f, 1f);
    private Quaternion targetRotation;
    private bool isOpen = false;
    private BoxCollider boxCollider;
    private BoxCollider parentCollider;

    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();

        if (transform.parent != null)
        {
            parentCollider = transform.parent.GetComponent<BoxCollider>();
        }

        targetRotation = transform.rotation;
        OpenDoor();
    }

    public void OpenDoor()
    {
        if (isOpen) return;

        transform.position += positionOffset;
        targetRotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0, openAngle, 0));
        isOpen = true;

        if (boxCollider != null)
            boxCollider.enabled = false;

        if (parentCollider != null)
            parentCollider.enabled = false;
    }

    void Update()
    {
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }
}
