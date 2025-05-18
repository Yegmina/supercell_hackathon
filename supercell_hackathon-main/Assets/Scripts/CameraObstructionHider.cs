using System.Collections.Generic;
using UnityEngine;

public class CameraObstructionHider : MonoBehaviour
{
    public Transform player;
    public List<Transform> playerPoints; // Assign head, torso, feet, etc.
    private List<MeshRenderer> hiddenRenderers = new List<MeshRenderer>();

    void Update()
    {
        // Restore previously hidden renderers
        foreach (var rend in hiddenRenderers)
        {
            if (rend != null)
                rend.enabled = true;
        }
        hiddenRenderers.Clear();

        bool playerVisible = false;

        foreach (Transform point in playerPoints)
        {
            Vector3 direction = point.position - transform.position;
            float distance = Vector3.Distance(point.position, transform.position);

            if (Physics.Raycast(transform.position, direction, out RaycastHit hit, distance))
            {
                if (hit.transform == player)
                {
                    playerVisible = true;
                    break;
                }
            }
        }

        if (!playerVisible)
        {
            // Hide all objects between camera and player center
            Vector3 direction = player.position - transform.position;
            float distance = Vector3.Distance(player.position, transform.position);
            RaycastHit[] hits = Physics.RaycastAll(transform.position, direction, distance);

            foreach (RaycastHit hit in hits)
            {
                if (hit.transform == player) continue;

                MeshRenderer rend = hit.transform.GetComponent<MeshRenderer>();
                if (rend != null)
                {
                    rend.enabled = false;
                    hiddenRenderers.Add(rend);
                }
            }
        }
    }
}
