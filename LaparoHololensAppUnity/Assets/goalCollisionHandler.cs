using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class goalCollisionHandler : MonoBehaviour
{
    mouseInput rig;
    public Vector3 initialTipPosition;
    float[] randomRange=new float[]{0.1f,2.0f};
    // Start is called before the first frame update
    void Start()
    {
        rig=(mouseInput)Object.FindObjectOfType(typeof(mouseInput));
        //initialTipPosition=new Vector3(rig.rightManipulatorTip.gameObject.transform.position.x,rig.rightManipulatorTip.gameObject.transform.position.y,rig.rightManipulatorTip.gameObject.transform.position.z);
        //handled in rig
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTriggerEnter(Collider other){
        Debug.Log("goal hit "+other.gameObject.name);
        //spawn in different place at least 1/2 m away
        this.gameObject.transform.position=new Vector3(initialTipPosition.x+(Random.Range(randomRange[0],randomRange[1])*(Random.Range(0,2)<1?-1:1)),this.gameObject.transform.position.y,initialTipPosition.z+(Random.Range(randomRange[0],randomRange[1])*(Random.Range(0,2)<1?-1:1)));
        Debug.Log(rig);
        rig.spawnPath();
    }
}
