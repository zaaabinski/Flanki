using UnityEngine;

public class LockHead : MonoBehaviour
{
    public bool lockX = true;
    public bool lockY = true;
    public bool lockZ = true;

    private Quaternion initialParentRotation;

    void Start()
    {
        // Pobierz najwyższego rodzica
        Transform root = transform;
        while (root.parent != null)
            root = root.parent;

        // Zapisz początkową rotację rodzica
        initialParentRotation = root.rotation;
    }

    void LateUpdate()
    {
        // Pobierz najwyższego rodzica
        Transform root = transform;
        while (root.parent != null)
            root = root.parent;

        // Aktualna rotacja obiektu
        Vector3 euler = transform.rotation.eulerAngles;
        Vector3 parentEuler = root.rotation.eulerAngles;

        if (lockX) euler.x = parentEuler.x;
        if (lockY) euler.y = parentEuler.y;
        if (lockZ) euler.z = parentEuler.z;

        transform.rotation = Quaternion.Euler(euler);
    }
}