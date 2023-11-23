using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotorSteering : MonoBehaviour
{
    ArticulationBody bd;
    const float RAD2DEG = 57.295779513f;
    // float smooth = 1.0f;
    float speed = 500;
    float stiffness = 1000f;
    float damping = 0f;
    float forceLimit = 100;

    float angle = 0f;

    void Awake()
    {
        bd = GetComponent<ArticulationBody>();
        

    }

    public void SetAngle(float targetRadian)
    {   
        angle = targetRadian * RAD2DEG;
    }

    // Start is called before the first frame update
    void Start()
    {
        ArticulationDrive currentDrive = bd.xDrive;
        currentDrive.stiffness = stiffness;
        currentDrive.damping = damping;
        currentDrive.forceLimit = forceLimit;
        bd.xDrive = currentDrive;
        
    }

    // Update is called once per frame
    void Update()
    {
        var drive = bd.xDrive;
        drive.targetVelocity = angle;
        drive.damping = damping;
        drive.forceLimit = forceLimit;
        bd.xDrive = drive;
        
    }
}
