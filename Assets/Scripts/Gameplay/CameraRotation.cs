using UnityEngine;

public class FlyCamera : MonoBehaviour
{
    float mainSpeed = 100.0f; // regular speed
    float shiftAdd = 250.0f; // multiplied by how long shift is held. Basically running
    float maxShift = 1000.0f; // Maximum speed when holding shift
    private float totalRun = 1.0f;
    private float initialHeight; // Store the initial height

    void Start()
    {
        // Record the initial height of the camera
        initialHeight = transform.localPosition.y;
    }

    void Update()
    {
        // Keyboard commands
        Vector3 p = GetBaseInput();
        if (Input.GetKey(KeyCode.LeftShift))
        {
            totalRun += Time.deltaTime;
            p = p * totalRun * shiftAdd;
            p.x = Mathf.Clamp(p.x, -maxShift, maxShift);
            p.y = Mathf.Clamp(p.y, -maxShift, maxShift);
            p.z = Mathf.Clamp(p.z, -maxShift, maxShift);
        }
        else
        {
            totalRun = Mathf.Clamp(totalRun * 0.5f, 1f, 1000f);
            p = p * mainSpeed;
        }

        p = p * Time.deltaTime;
        Vector3 newPosition = transform.localPosition + p;

        // Maintain the initial height
        //newPosition.y = initialHeight;

        transform.localPosition = newPosition;

    }

    private Vector3 GetBaseInput()
    { // returns the basic values, if it's 0 then it's not active.
        Vector3 p_Velocity = new Vector3();
        if (Input.GetKey(KeyCode.W))
        {
            p_Velocity += new Vector3(0, 0, 1);
        }
        if (Input.GetKey(KeyCode.S))
        {
            p_Velocity += new Vector3(0, 0, -1);
        }
        if (Input.GetKey(KeyCode.A))
        {
            p_Velocity += new Vector3(0, 1, 0);
        }
        if (Input.GetKey(KeyCode.D))
        {
            p_Velocity += new Vector3(0, -1, 0);
        }
        if (Input.GetKey(KeyCode.LeftControl))
        {
            p_Velocity += new Vector3(-1, 0, 0);
        }
        if (Input.GetKey(KeyCode.Space))
        {
            p_Velocity += new Vector3(1, 0, 0);
        }
        return p_Velocity;
    }
}