using UnityEngine;

public class Rotate : MonoBehaviour
{
    public float rotationsPerMinute = 10.0f;

    void Update()
    {
        // Rotate around the local Y-axis
        transform.Rotate(0, 6.0f * rotationsPerMinute * Time.deltaTime, 0, Space.Self);
    }
}
