using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CarAI : MonoBehaviour {
    public WheelCollider wheelFL;
    public WheelCollider wheelFR;
    public WheelCollider wheelRL;
    public WheelCollider wheelRR;

    public bool debug = false;
    private const float maxSteerAngle = 8.5f;

    private List<Transform> path = null;
    public GameObject currentComponent = null;

    private const float minPathpointScope = 15f;


    //update the road component currently on
    void updateComponent(GameObject comp)
    {
        Transform pathTransform;
        currentComponent = comp;
        pathTransform = comp.transform.Find("path");
        //update the path in new component
        path = new List<Transform>();
        path.AddRange(pathTransform.GetComponentsInChildren<Transform>());
        if (debug)
        {
            for (int i = 1; i < path.Count; ++i)
            {
                Debug.Log("path[" + i.ToString() + "]:" + path[i].position.ToString());
            }
        }
        
        //
    }

	// Use this for initialization
	void Start () {
	    
	}

    bool SensorUpdate()
    {
        bool isHit = false ;
        bool leftSideRayHit = false;
        bool rightSideRayHit = false;

        RaycastHit hitInfo;
        Vector3 frontRayPos = transform.position 
                            + transform.up * 0.5f 
                            + transform.forward * 2.2f;
        Vector3 leftRayPos = transform.position
                            + transform.up * 0.5f 
                            + transform.forward * 2f
                            + transform.right * -0.7f;
        Vector3 rightRayPos = transform.position
                            + transform.up * 0.5f 
                            + transform.forward * 2f
                            + transform.right * 0.7f;

        Vector3 leftSideRayPos = leftRayPos;
        Vector3 rightSideRayPos = rightRayPos;
        Vector3 leftSideRayAngle = Quaternion.AngleAxis(-30, transform.up) * transform.forward;
        Vector3 rightSideRayAngle = Quaternion.AngleAxis(30, transform.up) * transform.forward;

        if (Physics.Raycast(frontRayPos, transform.forward, out hitInfo, 8f))
        {
            if (hitInfo.transform.tag == "Road")
            {
                
                isHit = true;
                wheelFL.brakeTorque = 1000f;
                wheelFR.brakeTorque = 1000f;
                if (debug)
                {
                    Debug.DrawLine(frontRayPos, hitInfo.point, Color.cyan);
                    Debug.Log("Hit");
                }
               
            }
            
        }

        if (Physics.Raycast(leftSideRayPos, leftSideRayAngle, out hitInfo, 12f))
        {
            if (hitInfo.transform.tag == "Road")
            {
                leftSideRayHit = true;
                isHit = true;
                wheelFL.steerAngle = 4.0f;
                wheelFR.steerAngle = 4.0f;
                wheelFL.brakeTorque = 400f;
                wheelFR.brakeTorque = 400f;
                if (debug)
                {
                    Debug.DrawLine(leftSideRayPos, hitInfo.point, Color.cyan);
                    Debug.Log("Left Side Hit");
                }
               
            }
           
        }

        if (Physics.Raycast(rightSideRayPos, rightSideRayAngle, out hitInfo, 12f))
        {
            if (hitInfo.transform.tag == "Road")
            {
                rightSideRayHit = true;
                isHit = true;
                wheelFL.steerAngle = -4.0f;
                wheelFR.steerAngle = -4.0f;
                wheelFL.brakeTorque = 400f;
                wheelFR.brakeTorque = 400f;
                if (debug)
                {
                    Debug.DrawLine(rightSideRayPos, hitInfo.point, Color.cyan);
                    Debug.Log("Right Side Hit");
                }
                
            }
        }

        if (Physics.Raycast(leftRayPos, transform.forward, out hitInfo, 7f) && leftSideRayHit)
        {
            if (hitInfo.transform.tag == "Road")
            {
                isHit = true;
                wheelFL.steerAngle = 8;
                wheelFR.steerAngle = 8;

                if (debug)
                {
                    Debug.DrawLine(leftRayPos, hitInfo.point, Color.cyan);
                    Debug.Log("Left Hit");
                }
               
            }
        }

        if (Physics.Raycast(rightRayPos, transform.forward, out hitInfo, 7f) && rightSideRayHit)
        {
            if (hitInfo.transform.tag == "Road")
            {
                isHit = true;
                wheelFL.steerAngle = -8;
                wheelFR.steerAngle = -8;

                if (debug)
                {
                    Debug.DrawLine(rightRayPos, hitInfo.point, Color.cyan);
                    Debug.Log("Right Hit");
                }
                
            }
        }

       
        return isHit;

    }
    void FixedUpdate()
    {
       
        if (currentComponent != null)
        {
            //find the next waypoint
            bool found;
            float minDist = -1f;
            found = false;
            Vector3 nextWaypointPosition = new Vector3();   //it's global
            
            Vector3 CarPosition = transform.position;
 
            //find the next waypoint position in the path
            //omit the path[0], because the path[0] is the path itself
            for (int i = 1; i < path.Count; ++i)
            {
                Vector3 tempPosition = path[i].position;
                Vector3 diffVector = tempPosition - CarPosition;
                if (i == 1)
                {
                    minDist = diffVector.magnitude;
                }
                //only consider the path points that in the forward direction of the car
                if (Vector3.Angle(transform.forward, diffVector) < 90f
                    && diffVector.magnitude <= minDist
                    && diffVector.magnitude >= minPathpointScope)
                {
                    found = true;
                    minDist = diffVector.magnitude;
                    nextWaypointPosition = path[i].position;
                }
              
            }

            if (found)
            {

                Vector3 pathVector = nextWaypointPosition - CarPosition;
                Vector3 carDirVector = transform.forward;
                Vector3 crossProduct = Vector3.Cross(carDirVector, pathVector);
              
                if (crossProduct.y > 0f)
                {
                    wheelFL.steerAngle = maxSteerAngle * Mathf.Sin(3.1416f / 180f * Vector3.Angle(carDirVector, pathVector));
                    wheelFR.steerAngle = maxSteerAngle * Mathf.Sin(3.1416f / 180f * Vector3.Angle(carDirVector, pathVector));
                 //   wheelFL.steerAngle = Mathf.Clamp( Vector3.Angle(carDirVector, pathVector) * 0.6f, 0, 6f);
                  //  wheelFR.steerAngle = Mathf.Clamp(Vector3.Angle(carDirVector, pathVector) * 0.6f, 0, 6f);
                }
                else
                {
                    wheelFL.steerAngle = -maxSteerAngle * Mathf.Sin(3.1416f / 180f * Vector3.Angle(carDirVector, pathVector));
                    wheelFR.steerAngle = -maxSteerAngle * Mathf.Sin(3.1416f / 180f * Vector3.Angle(carDirVector, pathVector));
                   // wheelFL.steerAngle = Mathf.Clamp(-Vector3.Angle(carDirVector, pathVector) * 0.6f, -6f, 0);
                    //wheelFR.steerAngle = Mathf.Clamp(- Vector3.Angle(carDirVector, pathVector) * 0.6f, -6f, 0);
                }


                if (debug)
                {
                    Debug.Log("pathVector:" + pathVector.ToString());
                    Debug.Log("Cross Product.z" + crossProduct.y);
                    Debug.Log("Car Dir vector" + carDirVector.ToString());
                    Debug.Log("Auto Steer:" + wheelFL.steerAngle.ToString() + "\n");
                }
               
            }
            else
            {
                //not found, just reset the steer
                wheelFL.steerAngle = 0;
                wheelFR.steerAngle = 0;
            }
        }
       
        
        SensorUpdate();

    }
	// Update is called once per frame
	void Update () {
	    
	}
}
