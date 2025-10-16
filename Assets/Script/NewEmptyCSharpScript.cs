using UnityEngine;

public class CameraFollowPitchOnly : MonoBehaviour
{
    [Header("Target to Follow")]
    public Transform target; // Player head

    [Header("Camera Settings")]
    public Vector3 offset = new Vector3(0f, 0.5f, -2f);
    public float smoothSpeed = 5f;

    void LateUpdate()
    {
        if (target == null) return;

        // Move camera to follow target position
        Vector3 desiredPosition = target.position + target.TransformDirection(offset);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // Calculate direction to target
        Vector3 direction = target.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(direction);

        // Lock rotation so only X axis (pitch) is applied
        Vector3 euler = lookRotation.eulerAngles;
        transform.rotation = Quaternion.Euler(euler.x, 0f, 0f);
    }
}