using UnityEngine;

public class DemoHelper : MonoBehaviour
{
    public WitchController witch;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            witch.ForceMoveTo("A");
    }
}
