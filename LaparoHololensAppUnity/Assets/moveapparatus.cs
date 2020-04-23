using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moveapparatus : MonoBehaviour
{
    float speed=0.01f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("w")){
            this.gameObject.transform.position=this.gameObject.transform.position+new Vector3(0,speed,0);
        }
        else if (Input.GetKeyDown("d")){
            this.gameObject.transform.position=this.gameObject.transform.position-new Vector3(0,speed,0);
        }
    }
}
