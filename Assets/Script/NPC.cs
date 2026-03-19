using UnityEngine;

public class NPC : MonoBehaviour
{
    public Transform[] waypointList;
    public float moveSpeed = 8f;

    public float detectDistance = 10f;
    public float safeDistance = 5f;
    public LayerMask carLayer;

    int currentIndex = 0;
    float currentSpeed = 0f;
    bool reachedEnd = false;

    void Start()
    {
        currentSpeed = moveSpeed;
    }

    void Update()
    {
        if (reachedEnd == true)
        {
            currentSpeed = 0f;
            return;
        }

        CheckFrontCar();
        DriveToWaypoint();
    }

    void DriveToWaypoint()
    {
        if (waypointList.Length == 0 || reachedEnd)
        {
            return;
        }

        Transform targetPoint = waypointList[currentIndex];

        Vector3 moveDirection = (targetPoint.position - transform.position).normalized;
        moveDirection.y = 0;

        transform.position += moveDirection * currentSpeed * Time.deltaTime;

        if (Vector3.Distance(transform.position, targetPoint.position) < 1f)
        {
            currentIndex++;
            if (currentIndex >= waypointList.Length)
            {
                reachedEnd = true;
            }
        }
    }

    void CheckFrontCar()
    {
        RaycastHit hitInformation;

        if (Physics.Raycast(transform.position + Vector3.up,
                            transform.forward,
                            out hitInformation,
                            detectDistance,
                            carLayer))
        {
            if (hitInformation.distance < safeDistance)
            {
                currentSpeed = 0f;
            }
            else
            {
                float percent = hitInformation.distance / detectDistance;
                currentSpeed = Mathf.Lerp(2f, moveSpeed, percent);
            }
        }
        else
        {
            currentSpeed = moveSpeed;
        }
    }
}