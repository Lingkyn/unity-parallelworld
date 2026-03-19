using UnityEngine;

public class MainCarMove : MonoBehaviour
{
    public WheelCollider[] wheelcolliders;

    public Transform[] wheelObj;

    public float tprqueSpeed = 120f; // Torque

    public float steerAngle = 35f; // Max steer

    private float Horizontal; // X
    private float Vertical; // Z

    private void Update()
    {
        Horizontal = Input.GetAxis("Horizontal");
        Vertical = Input.GetAxis("Vertical");

        wheelcolliders[2].motorTorque = Vertical * tprqueSpeed;
        wheelcolliders[3].motorTorque = Vertical * tprqueSpeed;

        wheelcolliders[0].steerAngle = Horizontal * steerAngle;
        wheelcolliders[1].steerAngle = Horizontal * steerAngle;

        // Brake
        if (Input.GetKey(KeyCode.Space))
        {
            wheelcolliders[2].brakeTorque = tprqueSpeed;
            wheelcolliders[3].brakeTorque = tprqueSpeed;
        }
        else
        {
            wheelcolliders[2].brakeTorque = 0;
            wheelcolliders[3].brakeTorque = 0;

            wheelcolliders[2].motorTorque = Vertical * tprqueSpeed;
            wheelcolliders[3].motorTorque = Vertical * tprqueSpeed;
        }

        WheelConvers(wheelObj, wheelcolliders);
    }

    private Vector3 wheelposturepos;
    private Quaternion wheelpreurerotate;
    private void WheelConvers(Transform[] _wheelObj, WheelCollider[] _wheelColliders)
    {
        for (int i = 0; i < 4; i++)
        {
            wheelposturepos = _wheelObj[i].position;
            wheelpreurerotate = _wheelObj[i].rotation;
            _wheelColliders[i].GetWorldPose(out wheelposturepos, out wheelpreurerotate);

            // Apply to visual
            _wheelObj[i].position = wheelposturepos;
            _wheelObj[i].rotation = wheelpreurerotate;
        } 
    }
}
