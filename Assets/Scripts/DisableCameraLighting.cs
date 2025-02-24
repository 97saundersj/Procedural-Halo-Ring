using UnityEngine;

public class CameraLightingControl : MonoBehaviour
{
    public Light mainLight;
    public bool enableCheck = true; // Set this to true for the camera that should have the light enabled

    public float disableRotationStart = 10f; // Start angle for disabling light
    public float disableRotationEnd = 350f; // End angle for disabling light

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

            bool isBelowStartThreshold = lightRotationX < disableRotationStart;
            bool isBelowEndThreshold = lightRotationX > disableRotationEnd;

            mainLight.enabled = !isBelowStartThreshold && !isBelowEndThreshold;
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