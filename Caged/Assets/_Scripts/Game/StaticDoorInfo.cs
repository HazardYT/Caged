using UnityEngine;

public class StaticDoorInfo : MonoBehaviour
{
    public bool isOpen;
    [HideInInspector] public Quaternion OgRot;
    [HideInInspector] public Quaternion OpenRot;
    public bool isLocked;
    [SerializeField] private int rotation;
    public void Awake()
    {
        Vector3 euler = transform.localRotation.eulerAngles;
        OpenRot = Quaternion.Euler(new Vector3(euler.x, rotation, euler.z));
        OgRot = transform.localRotation;
    }
}
