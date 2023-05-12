using UnityEngine;
using System.Collections;
using TMPro;
public class CodeCanvas : MonoBehaviour
{
    [SerializeField] private Transform[] _Spawns;
    [SerializeField] private Material noteMat;
    public TextMeshProUGUI codetext;
    private Color[] colors = { Color.red, Color.blue, Color.cyan, Color.green, Color.yellow, Color.magenta };

    private IEnumerator Start(){
        Safe _saferef = GameObject.FindObjectOfType<Safe>();
        yield return new WaitUntil(() => _saferef != null);
            int i = Random.Range(0, _Spawns.Length);
                noteMat.color = colors[Random.Range(0, colors.Length)];
                transform.position = _Spawns[i].position;
                transform.rotation = _Spawns[i].rotation;
                codetext.text = _saferef.SafeCode;
    }


}
