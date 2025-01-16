using UnityEngine;

public class Rotate : MonoBehaviour
{
    public float xRotationsPerMinute = 0f;
    public float yRotationsPerMinute = 0f;
    public float zRotationsPerMinute = 0f;

    void Update()
    {
        var xRotations = 6.0f * xRotationsPerMinute * Time.deltaTime;
        var yRotations = 6.0f * yRotationsPerMinute * Time.deltaTime;
        var zRotations = 6.0f * zRotationsPerMinute * Time.deltaTime;
        transform.Rotate(xRotationsPerMinute, yRotationsPerMinute, zRotationsPerMinute, Space.Self);
    }
}
