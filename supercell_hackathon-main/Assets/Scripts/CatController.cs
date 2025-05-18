using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class CatController : MonoBehaviour
{
    public Animator animator;
    [Range(1f, 20f)]
    public float blendSmoothSpeed = 8f;
    float currentBlend = 0f;
    [Range(1f, 20f)] public float jumpBlendSmooth = 7f;
    float currentJumpBlend = 0f;
    float targetJumpBlend = 0f;
    public float jumpSpeed = 4.5f;
    public float gravity = 9.81f;
    bool wasGroundedLastFrame;
    [SerializeField] float landingHold = 0.15f;
    Coroutine landingRoutine;
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
        animator = GetComponent<Animator>();
        wasGroundedLastFrame = character.isGrounded;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnMeow()
    {
        print("Meow :3");
        animator.SetTrigger("Meow");
        EventBuffer.PushEvent(new Event("meow"));
    }
    void CheckForwardObstacle()
    {
        RaycastHit hit;
        Vector3 origin = character.transform.position + Vector3.up * 0.5f;
        Vector3 direction = character.transform.forward;

        if (Physics.Raycast(origin, direction, out hit, 1f, ~eyeCastIgnoreLayers))
        {
            Debug.Log("Blocked by: " + hit.collider.gameObject.name);
        }
    }


    void Update()
    {
        CheckForwardObstacle();
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
        var horizontalMotion = wish * speed;

        bool isGrounded = character.isGrounded;

        if (isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            verticalVelocity = Mathf.Sqrt(2f * upGravity * jumpHeight);
            currentJumpBlend = targetJumpBlend = -1f;
            animator.SetBool("IsJumping", true);
            wasGroundedLastFrame = false;
        }

        if (!isGrounded)
        {
            if (verticalVelocity > 0f)
                verticalVelocity -= upGravity * Time.deltaTime;
            else
                verticalVelocity -= downGravity * Time.deltaTime;
        }

        Vector3 motion = horizontalMotion;
        motion.y = verticalVelocity;
        character.Move(motion * Time.deltaTime);

        cameraPivot.transform.position = Vector3.Lerp(cameraPivot.transform.position, character.transform.position, 1 - cameraLag);

        bool isWalking = horizontalMotion.sqrMagnitude > 0.001f;
        animator.SetBool("IsWalking", isWalking);

        float targetBlend = 0f;
        if (isWalking)
        {
            float signed = Vector3.SignedAngle(
                               character.transform.forward,
                               horizontalMotion,
                               Vector3.up);

            targetBlend = Mathf.Clamp(signed / 90f, -1f, 1f);
        }

        currentBlend = Mathf.MoveTowards(
                           currentBlend,
                           targetBlend,
                           blendSmoothSpeed * Time.deltaTime);

        animator.SetFloat("Blend", currentBlend);

        if (!isGrounded)
            targetJumpBlend = 0f;
        else
        {
            if (!wasGroundedLastFrame && animator.GetBool("IsJumping"))
            {
                targetJumpBlend = 1f;
                if (landingRoutine != null) StopCoroutine(landingRoutine);
                landingRoutine = StartCoroutine(LandingHold());
            }
        }

        currentJumpBlend = Mathf.MoveTowards(currentJumpBlend,
                                     targetJumpBlend,
                                     jumpBlendSmooth * Time.deltaTime);

        animator.SetFloat("JumpBlend", currentJumpBlend);
        wasGroundedLastFrame = isGrounded;

        if (wish.magnitude > 0.1f)
        {
            var flat = new Vector3(horizontalMotion.x, 0f, horizontalMotion.z);
            var targetLook = Quaternion.LookRotation(flat, Vector3.up);
            character.transform.rotation = Quaternion.RotateTowards(character.transform.rotation, targetLook, turnSpeed * Time.deltaTime);
        }

        if (Input.GetButtonDown("Meow"))
        {
            OnMeow();
        }
        if (Input.GetButtonDown("Yawn"))
        {
            animator.SetTrigger("Yawn");
        }
        if (Input.GetButtonDown("Strech"))
        {
            animator.SetTrigger("Strech");
        }
        if (Input.GetButtonDown("Eat"))
        {
            animator.SetTrigger("Eat");
        }
        if (Input.GetButtonDown("Sleep"))
        {
            animator.SetTrigger("SleepStart");
        }
        if (Input.GetButtonDown("Scratch"))
        {
            animator.SetTrigger("Scratch");
        }

        if (Input.GetButtonDown("Knock"))
        {
            var havorCenter = character.transform.position + character.transform.forward * knockOverCenter.x + character.transform.up * knockOverCenter.y;
            Collider[] colliders = Physics.OverlapSphere(havorCenter, knockOverRadius, ~eyeCastIgnoreLayers);

            animator.SetTrigger("Knock");
            foreach (var collider in colliders)
            {
                var named = collider.GetComponent<VisibleThing>();
                if (named != null)
                    EventBuffer.PushEvent(new Event("whacks", named.name));

                var rb = collider.GetComponent<Rigidbody>();
                if (rb == null)
                    continue;

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

    IEnumerator LandingHold()
    {
        yield return new WaitForSeconds(landingHold);
        targetJumpBlend = 0f;
        animator.SetBool("IsJumping", false);
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
