using UnityEngine;

public class SkyboxRotator : MonoBehaviour
{
    // Speed at which the skybox rotates (degrees per second)
    public float rotationSpeed = 1f;

    // Current rotation angle
    private float currentRotation = 0f;

    void Update()
    {
        // Increment the rotation based on time and speed
        currentRotation += rotationSpeed * Time.deltaTime;
        
        // Apply the rotation to the skybox material.
        // Note: The property name "_Rotation" is used by default in Unity's skybox shaders.
        RenderSettings.skybox.SetFloat("_Rotation", currentRotation);
    }
}
