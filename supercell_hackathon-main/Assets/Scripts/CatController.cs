using System.Collections.Generic;
using UnityEngine;


public class CatController : MonoBehaviour
{
    public int maximumPhysicsSteps = 1000;
    public bool showGizmos;
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

    public float jumpHeight = 2f;
    public float upGravity = 9.8f;
    public float downGravity = 14.8f;

    float verticalVelocity = 0;

    public LayerMask eyeCastIgnoreLayers;
    public Vector2 catEyePivot;
    public float maxEyeCastLimit = 15;
    public float eyeCastStep = 0;
    public float eyeCastStartRadius = 0;
    public float eyeCastEndRadius = 0;

    void Start()
    {
        character = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnMeow()
    {
        print("Meow :3");
    }

    void Update()
    {
        var mousePitch = Input.GetAxis("Mouse Y");
        pitch -= mousePitch;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        cameraPivot.transform.localRotation = Quaternion.Euler(pitch, cameraPivot.transform.localRotation.eulerAngles.y, camera.transform.localRotation.eulerAngles.z);

        var orbit = Input.GetAxis("Mouse X");
        cameraPivot.Rotate(Vector3.up, orbit * orbitSpeed * Time.deltaTime);

        var forward = Vector3.ProjectOnPlane(camera.transform.forward, Vector3.up);
        var right = Vector3.ProjectOnPlane(camera.transform.right, Vector3.up);

        var ws = Input.GetAxis("Vertical");
        var ad = Input.GetAxis("Horizontal");

        var wish = (forward * ws + right * ad).normalized * Mathf.Clamp01(Mathf.Sqrt(ws * ws + ad * ad));
        var motion = wish * speed;
        character.Move(motion * Time.deltaTime);

        cameraPivot.transform.position = Vector3.Lerp(cameraPivot.transform.position, character.transform.position, 1 - cameraLag);

        if (Input.GetButtonDown("Jump"))
        {
            verticalVelocity = Mathf.Sqrt(2 * upGravity * jumpHeight);
        }

        if (Input.GetButtonDown("Meow"))
        {
            OnMeow();
        }

        if (!character.isGrounded)
        {
            if (verticalVelocity > 0)
                verticalVelocity -= Time.deltaTime * upGravity;
            else
                verticalVelocity -= Time.deltaTime * downGravity;
        }

        character.Move(Vector3.up * verticalVelocity * Time.deltaTime);

        if (wish.magnitude > 0.1)
        {
            var targetLook = Quaternion.LookRotation(motion, Vector3.up);
            character.transform.rotation = Quaternion.RotateTowards(character.transform.rotation, targetLook, turnSpeed * Time.deltaTime);
        }
    }

    void FixedUpdate()
    {
        RaycastHit hit;
        var end = character.transform.position;

        if (Physics.Raycast(character.transform.position, character.transform.forward, out hit, maxEyeCastLimit, ~eyeCastIgnoreLayers))
        {
            end = character.transform.position;
        }

        var begin = character.transform.position + character.transform.forward * catEyePivot.x + character.transform.up * catEyePivot.y;

        HashSet<VisibleThing> unique = new HashSet<VisibleThing>();
        for (float i = 0; i < maxEyeCastLimit; i++)
        {
            if (i > maximumPhysicsSteps)
                break;

            var t = i / maxEyeCastLimit;
            var radius = Mathf.Lerp(eyeCastStartRadius, eyeCastEndRadius, t);
            var at = Vector3.Lerp(Vector3.zero, character.transform.forward * maxEyeCastLimit, t);
            var start = character.transform.position + at;

            Ray sphere = new Ray(start, character.transform.forward);
            Debug.DrawLine(character.transform.position, start);
            Debug.DrawLine(start, start + Vector3.up * radius);
            RaycastHit[] hits = Physics.SphereCastAll(sphere, radius, radius, ~eyeCastIgnoreLayers);

            foreach (var item in hits)
                for (var component = item.transform.gameObject.GetComponent<VisibleThing>(); component; component = null)
                    unique.Add(component);
        }

        foreach (var item in unique)
            print(item);
    }

    void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        var begin = character.transform.position + character.transform.forward * catEyePivot.x + character.transform.up * catEyePivot.y;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(begin, 0.1f);

        var steps = 0;
        for (float i = 0; i < maxEyeCastLimit; i += eyeCastStep)
        {
            if (steps > maximumPhysicsSteps)
                break;
            var t = i / maxEyeCastLimit;
            var radius = Mathf.Lerp(eyeCastStartRadius, eyeCastEndRadius, t);
            Gizmos.DrawSphere(begin + Vector3.Lerp(Vector3.zero, character.transform.forward * maxEyeCastLimit, t), radius);
            steps++;
        }
    }
}
