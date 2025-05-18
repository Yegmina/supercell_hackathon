using System.Collections.Generic;
using UnityEngine;

public class CameraObstructionHandler : MonoBehaviour
{
    public Transform player;
    public Material transparentMaterial;

    private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();

    void Update()
    {
        Vector3 direction = player.position - transform.position;
        float distance = Vector3.Distance(player.position, transform.position);

        Ray ray = new Ray(transform.position, direction);
        RaycastHit[] hits = Physics.RaycastAll(ray, distance);

        HashSet<Renderer> currentHits = new HashSet<Renderer>();

        foreach (RaycastHit hit in hits)
        {
            if (hit.transform == player) continue;

            Renderer rend = hit.transform.GetComponent<Renderer>();
            if (rend != null)
            {
                currentHits.Add(rend);

                if (!originalMaterials.ContainsKey(rend))
                {
                    originalMaterials[rend] = rend.materials;
                    Material[] newMats = new Material[rend.materials.Length];
                    for (int i = 0; i < newMats.Length; i++)
                        newMats[i] = transparentMaterial;
                    rend.materials = newMats;
                }
            }
        }

        List<Renderer> toRestore = new List<Renderer>();
        foreach (var kvp in originalMaterials)
        {
            if (!currentHits.Contains(kvp.Key))
            {
                kvp.Key.materials = kvp.Value;
                toRestore.Add(kvp.Key);
            }
        }

        foreach (var rend in toRestore)
        {
            originalMaterials.Remove(rend);
        }
    }
}
