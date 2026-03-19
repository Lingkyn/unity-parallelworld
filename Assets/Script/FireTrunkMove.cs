using UnityEngine;

public class FireTrunkMove : MonoBehaviour
{
    public WheelCollider[] wheelColliders;
    public Transform[] wheelObjs;

    public float torqueSpeed = 120f; // Torque
    public float steerAngle = 0f;

    private float horizontal = 0f;
    private float vertical = 1f;

    private void Update()
    {
        // Back Wheel
        wheelColliders[2].motorTorque = vertical * torqueSpeed;
        wheelColliders[3].motorTorque = vertical * torqueSpeed;

        // Straight
        wheelColliders[0].steerAngle = steerAngle * horizontal;
        wheelColliders[1].steerAngle = steerAngle * horizontal;

        wheelColliders[2].brakeTorque = 0;
        wheelColliders[3].brakeTorque = 0;

        UpdateWheelVisuals();
    }

    private Vector3 wheelPos;
    private Quaternion wheelRot;
    private void UpdateWheelVisuals()
    {
        for (int i = 0; i < 4; i++)
        {
            wheelColliders[i].GetWorldPose(out wheelPos, out wheelRot);
            wheelObjs[i].position = wheelPos;
            wheelObjs[i].rotation = wheelRot;
        }
    }
}
