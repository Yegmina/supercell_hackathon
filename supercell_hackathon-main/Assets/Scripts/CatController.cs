using UnityEngine;

public class CatController : MonoBehaviour
{
    public float cameraLag;
    public float orbitSpeed;
    public float speed;

    public float turnSpeed;

    public float minPitch;
    public float maxPitch;
    float pitch;

    public Transform cameraPivot;
    public new Camera camera;

    public CharacterController character;

    void Start()
    {
        character = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        var mousePitch = Input.GetAxis("Mouse Y");
        pitch -= mousePitch;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        cameraPivot.transform.localRotation = Quaternion.Euler(pitch, cameraPivot.transform.localRotation.eulerAngles.y, camera.transform.localRotation.eulerAngles.z);

        var orbit = Input.GetAxis("Mouse X");
        cameraPivot.Rotate(Vector3.up, orbit * orbitSpeed * Time.deltaTime);

        var forward = Vector3.ProjectOnPlane(camera.transform.forward, Vector3.up).normalized;
        var right = Vector3.ProjectOnPlane(camera.transform.right, Vector3.up).normalized;

        var ws = Input.GetAxis("Vertical");
        var ad = Input.GetAxis("Horizontal");

        var wish = (forward * ws + right * ad).normalized;
        var motion = wish * speed;
        character.Move(motion * Time.deltaTime);

        cameraPivot.transform.position = Vector3.Lerp(cameraPivot.transform.position, character.transform.position, 1 - cameraLag);

        if (wish.magnitude > 0.1)
        {
            var targetLook = Quaternion.LookRotation(motion, Vector3.up);
            character.transform.rotation = Quaternion.RotateTowards(character.transform.rotation, targetLook, turnSpeed * Time.deltaTime);
        }
    }
}
