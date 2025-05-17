using System.Diagnostics;
using UnityEngine;

public class CatWalkInCircles : MonoBehaviour
{
    public float radius = 5f;
    public float speed = 1f;

    private float angle = 0f;

    void Update()
    {
        angle += speed * Time.deltaTime;

        if (angle > 360f) angle -= 360f;

        float x = Mathf.Cos(angle) * radius;
        float z = Mathf.Sin(angle) * radius;

        transform.position = new Vector3(x, transform.position.y, z);
        float nextAngle = angle - 0.01f;
        float targetX = Mathf.Cos(nextAngle) * radius;
        float targetZ = Mathf.Sin(nextAngle) * radius;
        Vector3 targetPosition = new Vector3(targetX, transform.position.y, targetZ);
        transform.LookAt(targetPosition);
    }
}
