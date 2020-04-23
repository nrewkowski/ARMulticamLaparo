using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SplineMesh;
public class mouseInput : MonoBehaviour
{
    public GameObject rightManipulatorPivot;
    public GameObject rightManipulatorShaft;
    public GameObject rightManipulatorTip;
    public GameObject goalForRightManipulator;
    public GameObject idealPathTemplate;
    //unity can't deal with negative angles for some reason internally like Unreal can, so this is a hack. Unity DISPLAYS negatives but if you try to assign a negative angle 
    //in code, it will teleport to 0 degrees. IDIOTIC. not sure how recent this teleportation thing is b/c others don't seem to run into it with the same code
    public float upperThresholdOffset=60;
    public float lowerThresholdOffset=60;
    float upperThreshold,lowerThreshold;
    public float rotationalSpeedOfManipulator=50;
    public float translationalSpeedOfManipulator=1.0f;
    //Vector3 toAdd=new Vector3(0.1f,0.1f,0.1f);
    //Quaternion rotate=Quaternion.Euler(0, 90, 0);
    public GameObject currentPath;

    int updateFrequency=30;
    int numFramesTilUpdate=0;
    bool useAutoUpdate=false;

    float maxDistanceFromPivot;
    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
        numFramesTilUpdate=updateFrequency;
        upperThreshold=rightManipulatorPivot.transform.localEulerAngles.y+upperThresholdOffset;
        lowerThreshold=rightManipulatorPivot.transform.localEulerAngles.y-lowerThresholdOffset;
        maxDistanceFromPivot=(rightManipulatorPivot.gameObject.transform.position-rightManipulatorTip.gameObject.transform.position).magnitude;
        rightManipulatorShaft.gameObject.transform.position=rightManipulatorShaft.gameObject.transform.position+(rightManipulatorPivot.gameObject.transform.position-rightManipulatorShaft.gameObject.transform.position)*1.0f;
        ((goalCollisionHandler)Object.FindObjectOfType(typeof(goalCollisionHandler))).initialTipPosition=rightManipulatorTip.gameObject.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        numFramesTilUpdate--;
        //X is for rotation, Y is for moving tool in and out
        float horizontalAxis = Input.GetAxis ("Mouse X");       
        float possiblyNegativeYaw=Mathf.Clamp(rightManipulatorPivot.transform.localEulerAngles.y+rotationalSpeedOfManipulator*Time.deltaTime*horizontalAxis, lowerThreshold, upperThreshold);
        if (possiblyNegativeYaw<0){
            possiblyNegativeYaw+=360.0f;
        }
        rightManipulatorPivot.transform.localEulerAngles = new Vector3(rightManipulatorPivot.transform.localEulerAngles.x,
        possiblyNegativeYaw,
        rightManipulatorPivot.transform.localEulerAngles.z);

        //clamping it
        Vector3 currentRotation = rightManipulatorPivot.transform.localRotation.eulerAngles;
        currentRotation.y = Mathf.Clamp(currentRotation.y, lowerThreshold, upperThreshold);
        rightManipulatorPivot.transform.localRotation = Quaternion.Euler (currentRotation);

        DrawArrow.ForDebug(rightManipulatorTip.transform.position, -rightManipulatorPivot.transform.forward, Color.green);


        float verticalAxis = Input.GetAxis ("Mouse Y"); 
        //if (verticalAxis>0.01f || verticalAxis<-0.01f) Debug.Log("vert axis="+verticalAxis);


        if ((rightManipulatorPivot.gameObject.transform.position-rightManipulatorTip.gameObject.transform.position).magnitude<maxDistanceFromPivot && verticalAxis>0.01f){
            rightManipulatorShaft.gameObject.transform.position=rightManipulatorShaft.gameObject.transform.position-(rightManipulatorPivot.transform.forward*Time.deltaTime*translationalSpeedOfManipulator*verticalAxis);
        } else if ((rightManipulatorPivot.gameObject.transform.position-rightManipulatorTip.gameObject.transform.position).magnitude>0 && verticalAxis<-0.01f){
            rightManipulatorShaft.gameObject.transform.position=rightManipulatorShaft.gameObject.transform.position-(rightManipulatorPivot.transform.forward*Time.deltaTime*translationalSpeedOfManipulator*verticalAxis);
        }

        /*******************************************NEED TO USE SCROLL WHEEL FOR UP DOWN MOTION FOR 3d)*/

        //direction is position of the tangent handle going OUT
        if (Input.GetKeyDown("k") || (numFramesTilUpdate<0 && useAutoUpdate)){
            numFramesTilUpdate=updateFrequency;
            //Debug.Log("do it");
            spawnPath();
        }

        if (currentPath !=null){
            DrawArrow.ForDebug(currentPath.GetComponent<Spline>().nodes[0].Position, currentPath.GetComponent<Spline>().nodes[0].Direction, 10000.0f);
            DrawArrow.ForDebug(currentPath.GetComponent<Spline>().nodes[1].Position, currentPath.GetComponent<Spline>().nodes[1].Direction, 10000.0f);
            //DrawArrow.ForDebug(currentPath.GetComponent<Spline>().nodes[2].Position, currentPath.GetComponent<Spline>().nodes[2].Direction, 10000.0f);

            DrawArrow.ForDebug(new Vector3(0,0,0), currentPath.GetComponent<Spline>().nodes[0].Position, Color.red, 10000.0f);
            DrawArrow.ForDebug(new Vector3(0,0,0), currentPath.GetComponent<Spline>().nodes[1].Position, Color.red, 10000.0f);
            //DrawArrow.ForDebug(new Vector3(0,0,0), currentPath.GetComponent<Spline>().nodes[2].Position, Color.red, 10000.0f);
        }
        
    }

    public void spawnPath(){
        //spawn ideal path template
            //set points and direction
            //p1 will be at tip. direction = forward
            //p2 will be in front of tip towards goal by 10% of the distance between tip and goal. do this to force curvature
            //p4 will be at goal. direction = goal-tip
            //p3 halfway between. direction is direction of 1 and 2 added (between the 2)
            //^^^^^^^^^^^may not need this if I do the edge nodes. middle handled automatically
            //normalize directions

            if (currentPath!=null){
                Object.Destroy(currentPath);
            }

            currentPath=(GameObject) Object.Instantiate(idealPathTemplate, new Vector3(0,0,0),new Quaternion(0,0,0,1));

            Debug.Log("sxaff");
            //Vector3 node1vec=new Vector3(3,5.5f,-7);
            //Vector3 node2vec=new Vector3(3.5f,6,-8f);
            
            float distanceToGoal=(rightManipulatorTip.transform.position-goalForRightManipulator.transform.position).magnitude;
            //NEED 3D DISTANCE FOR TANGENT IN EACH AXIS
            //contribution of each axis in final forward direction=axis distance*.1 in direction of goal. above/below (y) is trivial


            float angleBetweenRightAndGoal=Mathf.Acos(Vector3.Dot((goalForRightManipulator.transform.position-rightManipulatorTip.transform.position),-rightManipulatorPivot.transform.right)/
            ((goalForRightManipulator.transform.position-rightManipulatorTip.transform.position).magnitude*(-rightManipulatorPivot.transform.right).magnitude))* Mathf.Rad2Deg;

            DrawArrow.ForDebug(rightManipulatorTip.transform.position, goalForRightManipulator.transform.position-rightManipulatorTip.transform.position, Color.magenta, 10000.0f);

            DrawArrow.ForDebug(rightManipulatorTip.transform.position, -rightManipulatorPivot.transform.right, Color.magenta, 10000.0f);
            Debug.Log("GOAL IS TO RIGHT?"+(angleBetweenRightAndGoal<90));
            //this.gameObject.GetComponent<Spline>().InsertNode(1,node1);
            //this.gameObject.GetComponent<Spline>().InsertNode(1,node2);
            currentPath.GetComponent<Spline>().nodes[0].Position=rightManipulatorTip.transform.position;
            //currentPath.GetComponent<Spline>().nodes[0].Direction=-rightManipulatorPivot.transform.forward;
            currentPath.GetComponent<Spline>().nodes[0].Direction=rightManipulatorTip.transform.position-rightManipulatorPivot.transform.right*(angleBetweenRightAndGoal<90?1:-1)*(distanceToGoal<1?distanceToGoal:1);

            //currentPath.GetComponent<Spline>().nodes[3].Position=goalForRightManipulator.transform.position;
            //currentPath.GetComponent<Spline>().nodes[0].Direction=-rightManipulatorPivot.transform.forward;
            //currentPath.GetComponent<Spline>().nodes[3].Direction=currentPath.GetComponent<Spline>().nodes[3].Position-toAdd;

            //currentPath.GetComponent<Spline>().nodes[1].Position=rightManipulatorTip.transform.position+0.1f*distanceToGoal*-rightManipulatorPivot.transform.forward+0.05f*distanceToGoal*(goalForRightManipulator.transform.position-rightManipulatorTip.transform.position).normalized;
            //currentPath.GetComponent<Spline>().nodes[1].Direction=currentPath.GetComponent<Spline>().nodes[1].Position-toAdd;
            currentPath.GetComponent<Spline>().nodes[1].Position=goalForRightManipulator.transform.position;
            currentPath.GetComponent<Spline>().nodes[1].Direction=currentPath.GetComponent<Spline>().nodes[1].Position+(goalForRightManipulator.transform.position-rightManipulatorTip.transform.position).normalized*(distanceToGoal<1?distanceToGoal:1);
            //currentPath.GetComponent<Spline>().nodes[2].Position=goalForRightManipulator.transform.position+0.1f*distanceToGoal*-rightManipulatorPivot.transform.forward+0.05f*distanceToGoal*(-goalForRightManipulator.transform.position+rightManipulatorTip.transform.position).normalized;
            //currentPath.GetComponent<Spline>().nodes[2].Direction=currentPath.GetComponent<Spline>().nodes[2].Position-toAdd;


            //currentPath.GetComponent<Spline>().nodes[2].Position=goalForRightManipulator.transform.position;
            //currentPath.GetComponent<Spline>().nodes[2].Direction=goalForRightManipulator.transform.position-rightManipulatorTip.transform.position;
            //currentPath.GetComponent<Spline>().nodes[2].Direction=rotate*(currentPath.GetComponent<Spline>().nodes[2].Position-toAdd);
            //currentPath.GetComponent<Spline>().nodes[1].Position=(rightManipulatorTip.transform.position+goalForRightManipulator.transform.position)/2.0f;
            //currentPath.GetComponent<Spline>().nodes[1].Direction=(goalForRightManipulator.transform.position-rightManipulatorTip.transform.position)-rightManipulatorPivot.transform.forward;
            //currentPath.GetComponent<Spline>().nodes[1].Direction=rotate*(currentPath.GetComponent<Spline>().nodes[1].Position-toAdd);

            //Debug.Log("vector should be "+(-rightManipulatorPivot.transform.forward)+", is = "+currentPath.GetComponent<Spline>().nodes[0].Direction);
            
            //this.gameObject.GetComponent<Spline>().nodes[0].Direction=node1vec;
            currentPath.GetComponent<SplineMeshTiling>().enabled=true;

            //Debug.Log("found"+this.gameObject.transform.Find("generated by SplineMeshTiling"));
            //Object.Destroy(currentPath.transform.Find("generated by SplineMeshTiling").gameObject,1);
            //this.gameObject.GetComponent<SplineMeshTiling>().enabled=false;
            //node1.Position=node1.Position-toAdd;
            //node2.Position=node2.Position-toAdd;
    }
    
}
