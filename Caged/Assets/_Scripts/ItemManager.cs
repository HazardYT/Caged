using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;

public class ItemManager : MonoBehaviourPun
{
    public static ItemManager instance;

    public List<Sprite> ItemSprites = new List<Sprite>();
    public List<Sprite> ValueablesSprites = new List<Sprite>();
    [HideInInspector] public List<string> ItemNames = new List<string>();
    [HideInInspector] public List<string> ValueablesNames = new List<string>();

    public void Awake(){
        if (instance == null) { instance = this; }
        for (int i = 0; i < ItemSprites.Count; i++)
        {
            ItemNames.Add(ItemSprites[i].name);
        }
        for (int v = 0; v < ValueablesSprites.Count; v++)
        {
            ValueablesNames.Add(ValueablesSprites[v].name);
        }
    }
}
