using UnityEngine;
using TMPro;
public class CodeCanvas : MonoBehaviour
{
    [SerializeField] private Transform[] _Spawns;
    public TextMeshProUGUI codetext;
    private Safe _saferef;
    int spawn;
    public void Start(){
        _saferef = GameObject.Find("Safe").GetComponent<Safe>();
        spawn = Random.Range(0, _Spawns.Length);
        Invoke("SetCode", 1f);
        SetPosition();
    }
    void SetPosition()
    {
        transform.position = _Spawns[spawn].position;
        transform.rotation = _Spawns[spawn].rotation;
    }
    void SetCode()
    {
        codetext.text = _saferef.SafeCode;
    }
}
