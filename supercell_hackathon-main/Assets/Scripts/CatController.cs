using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;


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
    public float catNearRadius = 0;

    public Vector2 knockOverCenter;
    public float knockOverRadius;
    public float knockOverForce;

    void Start()
    {
        character = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnMeow()
    {
        print("Meow :3");
        EventBuffer.PushEvent(new Event("meow"));
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

        if (Input.GetButtonDown("Knock"))
        {
            var havorCenter = character.transform.position + character.transform.forward * knockOverCenter.x + character.transform.up * knockOverCenter.y;
            Collider[] colliders = Physics.OverlapSphere(havorCenter, knockOverRadius, ~eyeCastIgnoreLayers);

            foreach (var collider in colliders)
            {
                var rb = collider.GetComponent<Rigidbody>();
                if (rb == null)
                    continue;
                print(rb);

                Vector3 randomDirection = Random.onUnitSphere;

                var dot = Vector3.Dot(character.transform.forward, randomDirection);
                if (dot < 0)
                {
                    randomDirection *= -1;
                }

                randomDirection.y = Mathf.Abs(randomDirection.y);

                rb.AddForce(knockOverForce * randomDirection, ForceMode.Impulse);
            }
        }
    }
    void FixedUpdate()
    {
        RaycastHit hit;
        var end = character.transform.position;

        HashSet<VisibleThing> unique = new HashSet<VisibleThing>();
        if (Physics.Raycast(character.transform.position, character.transform.forward, out hit, maxEyeCastLimit, ~eyeCastIgnoreLayers))
        {
            end = character.transform.position;
        }

        var begin = character.transform.position + character.transform.forward * catEyePivot.x + character.transform.up * catEyePivot.y;

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

        var looksAt = new List<string>();
        foreach (var item in unique)
            looksAt.Add(item.name);
        unique.Clear();
        EventBuffer.SetState("looks-at", looksAt);

        for (int i = 0; i < 45; i++)
        {
            float t = (i / 45f) * 360f;
            Ray ray = new Ray(character.transform.position, Quaternion.Euler(0, t, 0) * character.transform.forward);
            if (Physics.Raycast(ray, out hit, catNearRadius, ~eyeCastIgnoreLayers))
            {
                for (var component = hit.transform.gameObject.GetComponent<VisibleThing>(); component; component = null)
                    unique.Add(component);
            }
            Debug.DrawRay(character.transform.position, Quaternion.Euler(0, t, 0) * character.transform.forward * catNearRadius);
        }

        var near = new List<string>();
        foreach (var item in unique)
            near.Add(item.name);
        EventBuffer.SetState("near", near);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.gray;
        var havorCenter = character.transform.position + character.transform.forward * knockOverCenter.x + character.transform.up * knockOverCenter.y;
        Gizmos.DrawWireSphere(havorCenter, knockOverRadius);

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
