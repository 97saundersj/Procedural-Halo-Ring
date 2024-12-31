using UnityEngine;

public class Rotate : MonoBehaviour
{
    public Transform transform;
    public float rotationsPerMinute = 10.0f;

    void Update()
    {
        transform.Rotate(0, (float)(6.0 * rotationsPerMinute * Time.deltaTime), 0);
    }
}
