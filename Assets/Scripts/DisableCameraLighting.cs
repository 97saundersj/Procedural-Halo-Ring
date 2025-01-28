using UnityEngine;

public class CameraLightingControl : MonoBehaviour
{
    public Light mainLight;
    public bool enableCheck = true; // Set this to true for the camera that should have the light enabled

public float disableRotationThreshold = 10f; // Disable light if rotation is less than this value

    void OnPreRender()
    {
        if (mainLight != null)
        {
            if (!enableCheck)
            {
                mainLight.enabled = true;
                return;
            }
            float lightRotationX = mainLight.transform.eulerAngles.x;
            Debug.Log("lightxrotation" + lightRotationX);
            bool isBelowThreshold = lightRotationX < disableRotationThreshold;

            mainLight.enabled = !isBelowThreshold;
            //Debug.Log(mainLight.enabled ? "Enabling light for this camera" : "Disabling light for this camera");
        }
    }

    void OnPostRender()
    {
        if (mainLight != null)
        {
            // Optionally reset the light state if needed
            //mainLight.enabled = !enableLightForThisCamera;
        }
    }
}