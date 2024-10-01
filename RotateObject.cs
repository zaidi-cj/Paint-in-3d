using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObject : MonoBehaviour
{
    private float horizontalInput; 
    private float verticalInput;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        if (horizontalInput != 0 || verticalInput!= 0)
        {
            transform.Rotate(5 * verticalInput, 5 * horizontalInput, 0);
        }
    }
}
