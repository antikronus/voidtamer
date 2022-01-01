using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMap : MonoBehaviour
{
    public static readonly List<List<Color>> BaseTileColors = new List<List<Color>>
    {                       //Access: BaseTileColors[Subtype][Type]
                            //PLAIN                                         //AIR                                           //EARTH                                         //FIRE                                          //WATER
        new List<Color>{    new Color(191f / 255, 210f / 255, 106f / 255),  new Color(165f / 255, 217f / 255, 144f / 255),  new Color(157f / 255, 172f / 255, 064f / 255),  new Color(203f / 255, 166f / 255, 062f / 255),  new Color(093f / 255, 176f / 255, 161f / 255)    },  //GRASS
        new List<Color>{    new Color(163f / 255, 199f / 255, 221f / 255),  new Color(171f / 255, 221f / 255, 233f / 255),  new Color(136f / 255, 160f / 255, 171f / 255),  new Color(184f / 255, 158f / 255, 214f / 255),  new Color(117f / 255, 142f / 255, 213f / 255)    },  //PIT
        new List<Color>{    new Color(181f / 255, 174f / 255, 136f / 255),  new Color(174f / 255, 189f / 255, 171f / 255),  new Color(156f / 255, 152f / 255, 132f / 255),  new Color(183f / 255, 151f / 255, 128f / 255),  new Color(142f / 255, 142f / 255, 163f / 255)    },  //CLIFF
        new List<Color>{    new Color(219f / 255, 213f / 255, 158f / 255),  new Color(210f / 255, 223f / 255, 192f / 255),  new Color(198f / 255, 194f / 255, 153f / 255),  new Color(234f / 255, 195f / 255, 153f / 255),  new Color(184f / 255, 185f / 255, 204f / 255)    },  //SAND
        new List<Color>{    new Color(172f / 255, 154f / 255, 105f / 255),  new Color(171f / 255, 169f / 255, 113f / 255),  new Color(162f / 255, 142f / 255, 084f / 255),  new Color(178f / 255, 133f / 255, 082f / 255),  new Color(163f / 255, 138f / 255, 132f / 255)    },  //DIRT
        new List<Color>{    new Color(183f / 255, 177f / 255, 136f / 255),  new Color(184f / 255, 197f / 255, 171f / 255),  new Color(169f / 255, 162f / 255, 132f / 255),  new Color(198f / 255, 165f / 255, 139f / 255),  new Color(162f / 255, 158f / 255, 175f / 255)    },  //GRAVEL
        new List<Color>{    new Color(190f / 255, 186f / 255, 146f / 255),  new Color(190f / 255, 207f / 255, 185f / 255),  new Color(176f / 255, 173f / 255, 145f / 255),  new Color(209f / 255, 178f / 255, 154f / 255),  new Color(169f / 255, 170f / 255, 190f / 255)    }   //SMOOTH
    };

    public int pxWidth;
    public int pxHeight;

    private Image Img;

    private readonly int Crispness = 4;

    private Texture2D MapTexture;

    // Start is called before the first frame update
    void Start()
    {
        MapTexture = new Texture2D(Crispness * pxWidth, Crispness * pxHeight, TextureFormat.ARGB32, false);
        Img = GetComponent<Image>();
        Img.material.mainTexture = MapTexture;
    }

    // Update is called once per frame
    void Update()
    {
        if (!Worldmap.Main.player.GetComponent<characterControl>().IsInspecting)
        {
            Img.enabled = true;
            List<Color> colors;
            bool changeMade = false;
            for (int x = 0; x < pxWidth; x++)
                for (int y = 0; y < pxHeight; y++)
                {
                    int translX = Mathf.FloorToInt(Worldmap.Main.player.transform.position.x + 0.5f) - (pxWidth / 2) + x;
                    int translY = Mathf.FloorToInt(Worldmap.Main.player.transform.position.y + 0.5f) - (pxHeight / 2) + y;
                    Gametile atXY = Worldmap.Main.FindTile(new Vector2(translX, translY));

                    int type = atXY.TYPE;
                    int subtype = atXY.SUBTYPE;
                    bool explored = atXY.Explored;
                    bool stable = atXY.Stable;

                    Color tileColor = Color.black;

                    if (x == pxWidth / 2 && y == pxHeight / 2)
                    {
                        tileColor = Color.white;
                    }
                    else if (explored)
                    {
                        tileColor = BaseTileColors[subtype][type];

                        if (!stable)
                        {
                            tileColor.r *= 0.5f;
                            tileColor.g *= 0.5f;
                            tileColor.b *= 0.5f;
                        }
                    }
                    if (tileColor != MapTexture.GetPixel(Crispness * x, Crispness * y))
                    {
                        changeMade = true;
                        colors = new List<Color>(System.Linq.Enumerable.Repeat(tileColor, Crispness * Crispness));
                        MapTexture.SetPixels(Crispness * x, Crispness * y, Crispness, Crispness, colors.ToArray());
                    }
                }
            if (changeMade)
                MapTexture.Apply();
        }
        else
            Img.enabled = false;
    }
}
