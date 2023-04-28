using TMPro;
using UnityEngine;
using System.Collections;

public class HudText : MonoBehaviour
{
    public TextMeshProUGUI hudtmpro;

    private Coroutine hudCoroutine;

    public IEnumerator SetHud(string text)
    {
        if (hudCoroutine != null)
        {
            StopCoroutine(hudCoroutine);
        }

        hudtmpro.text = text;
        hudCoroutine = StartCoroutine(WaitAndClear());

        yield return hudCoroutine;
    }

    private IEnumerator WaitAndClear()
    {
        yield return new WaitForSeconds(1f);
        hudtmpro.text = null;
        hudCoroutine = null;
    }


}
