using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.WSA;

public class stabilizer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        HolographicSettings.SetFocusPointForFrame(this.gameObject.transform.position,-Camera.main.transform.forward);
    }
}
