using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    public Transform transform;
    public float rotationsPerMinute  = 10.0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        transform.Rotate(0, (float)(6.0 * rotationsPerMinute * Time.deltaTime), 0);

    }
}
