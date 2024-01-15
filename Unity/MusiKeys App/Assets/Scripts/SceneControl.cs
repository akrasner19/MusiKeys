using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneControl : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        ManageEvents.manager.onKeyTriggered += onKeyTriggered;
    }

    public void onKeyTriggered(string keyStr)
    {
        if (keyStr == "moveback")
        {
            transform.Translate(new Vector3(0.0f, 0.0f, -0.1f));
        }
        else if (keyStr == "moveforward")
        {
            transform.Translate(new Vector3(0.0f, 0.0f, 0.1f));
        }
        else if (keyStr == "moveleft")
        {
            transform.Translate(new Vector3(-0.1f, 0.0f, 0.0f));
        }
        else if (keyStr == "moveright")
        {
            transform.Translate(new Vector3(0.1f, 0.0f, 0.0f));
        }
        else if (keyStr == "moveup")
        {
            transform.Translate(new Vector3(0.0f, 0.02f, 0.0f));
        }
        else if (keyStr == "movedown")
        {
            transform.Translate(new Vector3(0.0f, -0.02f, 0.0f));
        }
        else if (keyStr == "rotate")
        {
            transform.Rotate(new Vector3(0.0f,90.0f,0.0f));
        }
    }
}
