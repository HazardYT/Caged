using TMPro;
using UnityEngine;
using System.Collections;

public class HudText : MonoBehaviour
{
    public TextMeshProUGUI hudtext;

    private Coroutine hudCoroutine;

    public IEnumerator SetHud(string text, Color color)
    {
        hudtext.color = color;
        if (hudCoroutine != null)
        {
            StopCoroutine(hudCoroutine);
        }

        hudtext.text = text;
        hudCoroutine = StartCoroutine(WaitAndClear());

        yield return hudCoroutine;
    }

    private IEnumerator WaitAndClear()
    {
        yield return new WaitForSeconds(1f);
        hudtext.text = null;
        hudCoroutine = null;
    }


}
