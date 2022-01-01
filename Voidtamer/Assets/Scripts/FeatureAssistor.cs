using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeatureAssistor : MonoBehaviour
{
    public PlacedObject LinkedObject;
    public List<Sprite> FeatureSprites;
    public List<Sprite> TilesetFeatureSprites;
    public GameObject ItemPrefab;
    //public List<Sprite> ItemSprites;
    public Worldmap worldmap;

    public void Link(PlacedObject toLink)
    {
        LinkedObject = toLink;
    }

    public void Unlink()
    {
        LinkedObject = null;
    }
}
