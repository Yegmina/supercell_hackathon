using UnityEngine;
using System.Collections;

namespace LogCabin
{
    public class Interactions : MonoBehaviour
    {

        Camera cam;
        [Range(1, 5)]
        public float rayDistance = 3f;
        public Texture2D crosshair, eButton;

        int crossHairStatus = 0;

        void Start()
        {
            cam = Camera.main;
            if (cam == null)
            {
                Debug.LogError("Main camera tag not found in scene!");
                Destroy(this.gameObject);
            }
            if (!cam.allowHDR)
                cam.allowHDR = true;
        }

        void Update()
        {
            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, rayDistance))
            {
                if (hit.transform.GetComponent<LC_Door>())
                {
                    crossHairStatus = 1;
                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        LC_Door d = hit.transform.GetComponent<LC_Door>();
                        d.InteractWithThisDoor();
                    }
                } 
                else
                {
                    crossHairStatus = 0;
                }
            }
            else
            {
                crossHairStatus = 0;
            }

        }

        void OnGUI()
        {
            switch (crossHairStatus)
            {
                case 0:
                    //Draw default crosshair if integer set to 0
                    Rect rect = new Rect(Screen.width / 2, Screen.height / 2, crosshair.width, crosshair.height);
                    GUI.DrawTexture(rect, crosshair);
                    break;
                case 1:
                    //Draw E button sprite is integer set to 1 (object recognized by raycaster)
                    Rect rect2 = new Rect((Screen.width / 2) - eButton.width / 2, (Screen.height / 2) - eButton.height / 2, eButton.width, eButton.height);
                    GUI.DrawTexture(rect2, eButton);
                    break;
            }
        }
    }
}
