using UnityEngine;
using System.Collections;

public class CarControl : MonoBehaviour {
    
    public WheelCollider wheelFR;
    public WheelCollider wheelFL;
    public WheelCollider wheelRL;
    public WheelCollider wheelRR;


    //AI switch
    public bool enableAI = true;
    //engine
   

    public float maxEngineRPM = 7000f;
    public float currentRPM;
    public float minRPM = 700f;

    public float currentTorque; //torque of 
    public float maxTorque = 45f;  //45kg*m
    public float calculatedRPM;

    //transmitter
    public float transFactor = 0.95f; //efficiency
    public float[] gears;
    public int currentGear = 0;
    public int gearCount = 6;

    public float velocity = 0f;

    private float throttle; //-1 to 1
    private float steer; //-1 to 1
    private bool handBraked = false;
    //
    private AudioSource engineSound;
    private Transform carBody;
    public Transform carWheelFR;
    public Transform carWheelFL;
    public Transform carWheelRL;
    public Transform carWheelRR;

    //tires 
    public GameObject skimarkL;
    public GameObject skimarkR;
    public ParticleEmitter smokeL;
    public ParticleEmitter smokeR;

    private float suspensionTravelFL = 0f;
    private float suspensionTravelFR = 0f;
    private float suspensionTravelRL = 0f;
    private float suspensionTravelRR = 0f;


    void gearChange()
    {
        calculatedRPM = wheelFL.rpm * gears[currentGear];
        engineSound.pitch = 0.5f + (calculatedRPM / 7000f) * 1f;
        if (calculatedRPM >= 6500 && currentGear < 5)
        {
            currentGear++;
        }
        else if (calculatedRPM <= 5000 && currentGear > 0)
        {
            currentGear--;
        }
    }

    void applyThrottle()
    {

        currentRPM = minRPM + (throttle * (maxEngineRPM - minRPM));

        if (currentRPM >= 2000f && currentRPM <= 5000f)
        {
            currentTorque = maxTorque;
        }
        else if (currentRPM < 2000f)
        {
            currentTorque = ((currentRPM - minRPM) / (2000f - minRPM)) * maxTorque;
        }
        else
        {
            //currentTorque = maxTorque - ((currentRPM - 5000f) * 0.05f);
            currentTorque = maxTorque;
        }

    }
    void updateSuspension()
    {
        WheelHit groundHit;
        //distance of collider and wheel 3d model
        Vector3 diffWheelColli = new Vector3(0f, -wheelFL.suspensionDistance, 0f);  
        float antiRollFactor = 62000f;
        float frontDiff;
        float rearDiff;
        bool groundedFL;
        bool groundedFR;
        bool groundedRL;
        bool groundedRR;
        if (groundedFL = wheelFL.GetGroundHit(out groundHit))
        {
            suspensionTravelFL = (-wheelFL.transform.InverseTransformPoint(groundHit.point).y - wheelFL.radius);
            carWheelFL.position = wheelFL.transform.position 
                                + diffWheelColli 
                                + new Vector3 (0, -suspensionTravelFL, 0);   //move the wheel 3d model
        }
        else
        {
            suspensionTravelFL = wheelFL.suspensionDistance;
            carWheelFL.position = wheelFL.transform.position + diffWheelColli;
        }

        if (groundedFR = wheelFR.GetGroundHit(out groundHit))
        {
            suspensionTravelFR = (-wheelFR.transform.InverseTransformPoint(groundHit.point).y - wheelFR.radius);
            carWheelFR.position = wheelFR.transform.position
                                + diffWheelColli
                                + new Vector3(0, -suspensionTravelFR, 0);
        }
        else
        {
            suspensionTravelFR = wheelFR.suspensionDistance ;
            carWheelFR.position = wheelFR.transform.position + diffWheelColli;
        }

        if (groundedRL = wheelRL.GetGroundHit(out groundHit))
        {
            suspensionTravelRL = (-wheelRL.transform.InverseTransformPoint(groundHit.point).y - wheelRL.radius);
            carWheelRL.position = wheelRL.transform.position
                                + diffWheelColli
                                + new Vector3(0, -suspensionTravelRL, 0);
            UpdateSkimark(groundHit, skimarkL, smokeL);

        }
        else
        {
            suspensionTravelRL = wheelRL.suspensionDistance;
            carWheelRL.position = wheelRL.transform.position + diffWheelColli;
            skimarkL.SetActive(false);  //make the skimark not active
        }

        if (groundedRR = wheelRR.GetGroundHit(out groundHit))
        {
            suspensionTravelRR = (-wheelRR.transform.InverseTransformPoint(groundHit.point).y - wheelRR.radius);
            carWheelRR.position = wheelRR.transform.position
                                + diffWheelColli
                                + new Vector3(0, -suspensionTravelRR, 0);
            UpdateSkimark(groundHit, skimarkR, smokeR);
        }
        else
        {
            suspensionTravelRR = wheelRR.suspensionDistance;
            carWheelRR.position = wheelRR.transform.position + diffWheelColli;
            skimarkR.SetActive(false);  //hide the skimark
        }

        frontDiff = suspensionTravelFL - suspensionTravelFR;
        rearDiff = suspensionTravelRL - suspensionTravelRR;

        if (groundedFL)
        {
            transform.rigidbody.AddForceAtPosition(wheelFL.transform.up * frontDiff * -antiRollFactor,
                                                    wheelFL.transform.position);
        }

        if (groundedFR)
        {
            transform.rigidbody.AddForceAtPosition(wheelFR.transform.up * frontDiff * antiRollFactor,
                                                    wheelFR.transform.position);
        }

        if (groundedRL)
        {
            transform.rigidbody.AddForceAtPosition(wheelRL.transform.up * rearDiff * -antiRollFactor,
                                                    wheelRL.transform.position);
        }

        if (groundedRR)
        {
            transform.rigidbody.AddForceAtPosition(wheelRR.transform.up * rearDiff * antiRollFactor,
                                                    wheelRR.transform.position);
        }

      //  Debug.Log("front" + frontDiff.ToString());
       // Debug.Log("rear" + rearDiff.ToString());
    }

    void UpdateSkimark(WheelHit hit, GameObject skimark, ParticleEmitter smoke)
    {
        if (Mathf.Abs(hit.sidewaysSlip) > 3 || Mathf.Abs(hit.forwardSlip) > 0.3)
        {
            skimark.SetActive(true);
            smoke.emit = true;
        }
        else if (Mathf.Abs(hit.sidewaysSlip) < 2.8 && Mathf.Abs(hit.forwardSlip) < 0.28)
        {
            skimark.SetActive(false);
            smoke.emit = false;
        }
        
    }
    void FixedUpdate()
    {
        float x  = transform.rigidbody.velocity.x;
        float y = transform.rigidbody.velocity.y;
        float z = transform.rigidbody.velocity.z;
        velocity = Mathf.Sqrt(x * x + y * y + z * z);

        if (enableAI)
        {
            //auto throttle
            throttle = 1f;
        }
        else
        {   //otherwise AI control the wheel steerangle directly
            wheelFL.steerAngle = steer * 7;
            wheelFR.steerAngle = steer * 7;
        }
      

        if (throttle >= 0)
        {
            applyThrottle();
            
            wheelFR.motorTorque = currentTorque * gears[currentGear] * transFactor;    //motor Torque is torque on wheel
            wheelFL.motorTorque = currentTorque * gears[currentGear] * transFactor;
            wheelRR.motorTorque = currentTorque * gears[currentGear] * transFactor;    //motor Torque is torque on wheel
            wheelRL.motorTorque = currentTorque * gears[currentGear] * transFactor;
            if (enableAI == false)
            {
                wheelFL.brakeTorque = 0f;
                wheelFR.brakeTorque = 0f;
                wheelRL.brakeTorque = 0f;
                wheelRR.brakeTorque = 0f;
            }
            
        }
        else if (velocity > 0.1f )  //brake
        {
            wheelFL.motorTorque = -1000f;
            wheelFR.motorTorque = -1000f;
            wheelFL.brakeTorque = 300f;
            wheelFR.brakeTorque = 300f;
            wheelRL.brakeTorque = 300f;
            wheelRR.brakeTorque = 300f;
        }
        else
        {
            currentTorque = 0;
            wheelFR.motorTorque = -1000f;    //motor Torque is torque on wheel
            wheelFL.motorTorque = -1000f;
            wheelRR.motorTorque = -1000f;    //motor Torque is torque on wheel
            wheelRL.motorTorque = -1000f;
    
        }

        if (handBraked)
        {
            wheelFL.brakeTorque = 700f;
            wheelFR.brakeTorque = 700f;
            wheelRL.brakeTorque = 0f;
            wheelRR.brakeTorque = 0f;
        }
    
      //  carWheelFL.rotation = Quaternion.EulerAngles(0f, steer * 7 * (3.1416f / 180f), 0f);
      //  carWheelFR.rotation = Quaternion.EulerAngles(0f, steer * 7 * (3.1416f / 180f), 0f);
        updateSuspension();
        gearChange();
    }

    void updateDebugInfo()
    {
        GameObject debugText = GameObject.Find("DebugInfo");
        debugText.transform.guiText.text = "Velocity: " + (velocity * 3.6f).ToString()+ "kph\n";
        debugText.transform.guiText.text += "Engine RPM:" + calculatedRPM.ToString() + "\n";
        debugText.transform.guiText.text += "FL Suspension: " + wheelFL.suspensionDistance.ToString() + "m \n";
        debugText.transform.guiText.text += "FR Suspension: " + wheelFR.suspensionDistance.ToString() + "m \n";
        debugText.transform.guiText.text += "RL Suspension: " + wheelRL.suspensionDistance.ToString() + "m \n";
        debugText.transform.guiText.text += "RR Suspension: " + wheelRR.suspensionDistance.ToString() + "m \n";
        debugText.transform.guiText.text += " CoM x:" + transform.rigidbody.centerOfMass.x.ToString();
        debugText.transform.guiText.text += "CoM y:" + transform.rigidbody.centerOfMass.y.ToString();
        debugText.transform.guiText.text += "CoM z:" + transform.rigidbody.centerOfMass.z.ToString() + "\n";
        debugText.transform.guiText.text += "FL Torque:" + wheelFL.motorTorque.ToString() + "\n";
        debugText.transform.guiText.text += "FR Torque:" + wheelFR.motorTorque.ToString() + "\n";
        debugText.transform.guiText.text += "FL angle:" + wheelFL.steerAngle.ToString() + "\n";
        debugText.transform.guiText.text += "FR angle:" + wheelFR.steerAngle.ToString() + "\n";
    }
	// Use this for initialization
	void Start () {
        //transform.rigidbody.centerOfMass = new Vector3(0f, 0.3f, 0f);
        currentRPM = minRPM;
        currentTorque = 0f;
        carBody = transform.GetChild(0);
        engineSound = transform.audio;

        skimarkL.SetActive(false);
        skimarkR.SetActive(false);

        //set up gears

        gears = new float[6];
        gears[0] = 10.5f;
        gears[1] = 7.2f;
        gears[2] = 6.7f;
        gears[3] = 5.8f;
        gears[4] = 5.3f;
        gears[5] = 4.5f;

        //smokeL.SetActive(false);
        //smokeR.SetActive(false);
        smokeL.emit = false;
        smokeR.emit = false;
      
        /*wheelFR = (WheelCollider) carWheelFR.GetComponent("Wheel Collider");
        wheelFL = (WheelCollider)carWheelFL.GetComponent("Wheel Collider");
        wheelRL = (WheelCollider)carWheelRL.GetComponent("Wheel Collider");
        wheelRR = (WheelCollider)carWheelRR.GetComponent("Wheel Collider");*/

        //wheelFR = (WheelCollider)transform.GetComponent("Wheel Collider");
        
	}
	
    void updateWheelAnimations(float deltaTime){
        carWheelFL.Rotate(new Vector3(1f, 0f, 0f), deltaTime * wheelFL.rpm / 60 * 360);
        carWheelFR.Rotate(new Vector3(1f, 0f, 0f), deltaTime * wheelFR.rpm / 60 * 360);
        carWheelRL.Rotate(new Vector3(1f, 0f, 0f), deltaTime * wheelRL.rpm / 60 * 360);
        carWheelRR.Rotate(new Vector3(1f, 0f, 0f), deltaTime * wheelRR.rpm / 60 * 360);

    }
	// Update is called once per frame
	void Update () {
        //Debug.Log(velocity.ToString());
        float deltaTime = Time.deltaTime;
        updateDebugInfo();
        updateWheelAnimations(deltaTime);

        
        throttle = Input.GetAxis("Vertical");
        steer = Input.GetAxis("Horizontal");
        
       
        if (Input.GetKeyDown("space"))
        {
            handBraked = true;
        }
        if (Input.GetKeyUp("space"))
        {
            handBraked = false;
        }
    }
}
