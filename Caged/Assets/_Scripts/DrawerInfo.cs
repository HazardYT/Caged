using UnityEngine;

public class DrawerInfo : MonoBehaviour
{
    public bool isOpen;
    [HideInInspector]public Vector3 ClosePos = new Vector3(0,0,0);
    [HideInInspector]public Vector3 OpenPos = new Vector3(-0.01f, 0 ,0);

}
