using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class changeCamera : MonoBehaviour
{
    // Start is called before the first frame update
    private GameObject changeARCamera;
    void Start()
    {
        //changeARCamera = this.GetComponent<VuforiaBehaviour>().enabled = false;
        GetComponent<VuforiaBehaviour>().enabled = true;
        GetComponent<Camera>().enabled = false;
    }
        // Update is called once per frame
        void Update()
    {
        
    }

 
}
