using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyMovement : MonoBehaviour
{

    // Update is called once per frame
    
    void FixedUpdate()
    {
        if (transform.localPosition.z < -0.01f)
        {
            transform.localPosition = new Vector3(0.0f, 0.0f);
        }
    }
    
}
