using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotorMoveForward : MonoBehaviour
{   
    ArticulationBody bd;
    const float RAD2DEG = 57.295779513f;
    // float smooth = 1.0f;
    // float speed = 1000f;
    float stiffness = 0f;
    float damping = 1000f;
    float forceLimit = 100f;

    void Awake()
    {
        bd = GetComponent<ArticulationBody>();


    }

    public void SetVoltage(float voltage)
    {
        var drive = bd.xDrive;
        drive.targetVelocity = voltage;
        // Debug.Log(voltage);
        bd.xDrive = drive;
    }
    
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        ArticulationDrive currentDrive = bd.xDrive;
        currentDrive.stiffness = stiffness;
        currentDrive.damping = damping;
        currentDrive.forceLimit = forceLimit;
        bd.xDrive = currentDrive;
    }
}
