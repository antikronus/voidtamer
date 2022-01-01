using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileAssistor : MonoBehaviour
{
    public Gametile tile;
    public List<Sprite> TILE_SPRITES;
    public SpriteRenderer StabIndicatorSprite;
    public Animator StabIndicator;
    bool FlashFrame = false;
    bool FlashOff = false;

    public void Update()
    {
        if (FlashFrame)
        {
            FlashFrame = false;
            FlashOff = true;
        }
        else if(FlashOff)
        {
            StabIndicatorSprite.enabled = false;
            StabIndicator.Play("FlashRed", -1, 0);
            StabIndicator.enabled = false;
            FlashOff = false;
        }
    }

    public void FlashRed()
    {
        StabIndicatorSprite.enabled = true;
        StabIndicator.enabled = true;
        StabIndicator.SetBool("Red", true);
        StabIndicator.Play(0, -1, Time.fixedTime/1.5f);
        FlashFrame = true;
    }

    public void FlashGreen()
    {
        StabIndicatorSprite.enabled = true;
        StabIndicator.enabled = true;
        StabIndicator.SetBool("Red", false);
        StabIndicator.Play(0, -1, Time.fixedTime/1.5f);
        FlashFrame = true;
    }
}
