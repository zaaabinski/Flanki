using UnityEngine;

public class LockHead : MonoBehaviour
{
    public bool lockX = true;
    public bool lockY = true;
    public bool lockZ = true;

    private Quaternion initialRotation;

    void Start()
    {
        // Store the starting rotation
        initialRotation = transform.rotation;
    }

    void LateUpdate()
    {
        Vector3 euler = transform.rotation.eulerAngles;

        if (lockX) euler.x = initialRotation.eulerAngles.x;
        if (lockY) euler.y = initialRotation.eulerAngles.y;
        if (lockZ) euler.z = initialRotation.eulerAngles.z;

        transform.rotation = Quaternion.Euler(euler);
    }
}
