using UnityEngine;
using TMPro;
public class CodeCanvas : MonoBehaviour
{
    public TextMeshProUGUI text;
    public void Start()
    {
        int i = Random.Range(1000, 9999);
        text.text = "" + i;
    }
}
