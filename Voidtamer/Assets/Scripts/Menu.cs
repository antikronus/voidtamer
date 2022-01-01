using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{
    List<Gametile> MenuTiles = new List<Gametile>();
    List<GameObject> MenuTileObjects = new List<GameObject>();
    public bool InMenu = true;
    public Canvas MenuDisplay;
    private float tickProgress = 0;


    // Start is called before the first frame update
    void Start()
    {                                          //x  y          t  s
        MenuTiles.Add(new Gametile(new Vector2(-17, 22), 35, 0, 4, 6, 1));
        MenuTiles.Add(new Gametile(new Vector2(-17, 21), 20, 0, 4, 5, 1));
        MenuTiles.Add(new Gametile(new Vector2(-17, 20), 40, 0, 4, 6, 1));
        MenuTiles.Add(new Gametile(new Vector2(-16, 19), 50, 0, 4, 0, 1));
        MenuTiles.Add(new Gametile(new Vector2(-16, 18), 10, 0, 4, 0, 1));
        MenuTiles.Add(new Gametile(new Vector2(-16, 17), 80, 0, 4, 0, 1));
        MenuTiles.Add(new Gametile(new Vector2(-15, 16), 20, 0, 4, 4, 1));
        MenuTiles.Add(new Gametile(new Vector2(-14, 19), 40, 0, 4, 0, 1));
        MenuTiles.Add(new Gametile(new Vector2(-14, 18), 35, 0, 4, 0, 1));
        MenuTiles.Add(new Gametile(new Vector2(-14, 17), 40, 0, 4, 4, 1));
        MenuTiles.Add(new Gametile(new Vector2(-13, 22), 10, 0, 4, 6, 1));
        MenuTiles.Add(new Gametile(new Vector2(-13, 21), 30, 0, 4, 0, 1));
        MenuTiles.Add(new Gametile(new Vector2(-13, 20), 20, 0, 4, 0, 1));

        MenuTiles.Add(new Gametile(new Vector2(-12, 19), 30, 0, 2, 0, 1));
        MenuTiles.Add(new Gametile(new Vector2(-12, 18), 40, 0, 2, 4, 1));
        MenuTiles.Add(new Gametile(new Vector2(-12, 17), 50, 0, 2, 4, 1));
        MenuTiles.Add(new Gametile(new Vector2(-11, 20), 20, 0, 2, 0, 1));
        MenuTiles.Add(new Gametile(new Vector2(-11, 19), 10, 0, 2, 1, 1));
        MenuTiles.Add(new Gametile(new Vector2(-11, 18), 50, 0, 2, 1, 1));
        MenuTiles.Add(new Gametile(new Vector2(-11, 17), 50, 0, 2, 1, 1));
        MenuTiles.Add(new Gametile(new Vector2(-11, 16), 70, 0, 2, 3, 1));
        MenuTiles.Add(new Gametile(new Vector2(-10, 19), 50, 0, 2, 4, 1));
        MenuTiles.Add(new Gametile(new Vector2(-10, 18), 50, 0, 2, 4, 1));
        MenuTiles.Add(new Gametile(new Vector2(-10, 17), 50, 0, 2, 3, 1));

        MenuTiles.Add(new Gametile(new Vector2(-08, 21), 30, 0, 1, 0, 1));
        MenuTiles.Add(new Gametile(new Vector2(-08, 19), 45, 0, 1, 4, 1));
        MenuTiles.Add(new Gametile(new Vector2(-08, 18), 30, 0, 1, 4, 1));
        MenuTiles.Add(new Gametile(new Vector2(-08, 17), 20, 0, 1, 4, 1));
        MenuTiles.Add(new Gametile(new Vector2(-08, 16), 40, 0, 1, 3, 1));

        MenuTiles.Add(new Gametile(new Vector2(-06, 18), 100, 0, 4, 4, 1));
        MenuTiles.Add(new Gametile(new Vector2(-06, 17), 50, 0, 4, 4, 1));
        MenuTiles.Add(new Gametile(new Vector2(-05, 19), 50, 0, 4, 4, 1));
        MenuTiles.Add(new Gametile(new Vector2(-05, 18), 70, 0, 4, 1, 1));
        MenuTiles.Add(new Gametile(new Vector2(-05, 17), 70, 0, 4, 1, 1));
        MenuTiles.Add(new Gametile(new Vector2(-05, 16), 50, 0, 4, 3, 1));
        MenuTiles.Add(new Gametile(new Vector2(-04, 22), 50, 0, 4, 6, 1));
        MenuTiles.Add(new Gametile(new Vector2(-04, 21), 50, 0, 4, 0, 1));
        MenuTiles.Add(new Gametile(new Vector2(-04, 20), 80, 0, 4, 0, 1));
        MenuTiles.Add(new Gametile(new Vector2(-04, 19), 50, 0, 4, 4, 1));
        MenuTiles.Add(new Gametile(new Vector2(-04, 18), 50, 0, 4, 4, 1));
        MenuTiles.Add(new Gametile(new Vector2(-04, 17), 45, 0, 4, 4, 1));
        MenuTiles.Add(new Gametile(new Vector2(-04, 16), 30, 0, 4, 4, 1));

        MenuTiles.Add(new Gametile(new Vector2(-02, 22), 40, 0, 3, 5, 1));
        MenuTiles.Add(new Gametile(new Vector2(-02, 21), 50, 0, 3, 6, 1));
        MenuTiles.Add(new Gametile(new Vector2(-02, 20), 50, 0, 3, 0, 1));
        MenuTiles.Add(new Gametile(new Vector2(-02, 19), 50, 0, 3, 0, 1));
        MenuTiles.Add(new Gametile(new Vector2(-02, 18), 55, 0, 3, 0, 1));
        MenuTiles.Add(new Gametile(new Vector2(-02, 17), 60, 0, 3, 4, 1));
        MenuTiles.Add(new Gametile(new Vector2(-01, 19), 70, 0, 3, 0, 1));
        MenuTiles.Add(new Gametile(new Vector2(-01, 16), 50, 0, 3, 4, 1));
        MenuTiles.Add(new Gametile(new Vector2( 00, 17), 60, 0, 3, 4, 1));

        MenuTiles.Add(new Gametile(new Vector2( 02, 20), 50, 0, 0, 6, 1));
        MenuTiles.Add(new Gametile(new Vector2( 02, 17), 50, 0, 0, 0, 1));
        MenuTiles.Add(new Gametile(new Vector2( 02, 16), 50, 0, 0, 4, 1));
        MenuTiles.Add(new Gametile(new Vector2( 03, 20), 70, 0, 0, 6, 1));
        MenuTiles.Add(new Gametile(new Vector2( 03, 18), 50, 0, 0, 0, 1));
        MenuTiles.Add(new Gametile(new Vector2( 03, 17), 40, 0, 0, 1, 1));
        MenuTiles.Add(new Gametile(new Vector2( 03, 16), 50, 0, 0, 0, 1));
        MenuTiles.Add(new Gametile(new Vector2( 04, 19), 80, 0, 0, 6, 1));
        MenuTiles.Add(new Gametile(new Vector2( 04, 18), 60, 0, 0, 0, 1));
        MenuTiles.Add(new Gametile(new Vector2( 04, 17), 80, 0, 0, 0, 1));
        MenuTiles.Add(new Gametile(new Vector2( 04, 16), 60, 0, 0, 0, 1));

        MenuTiles.Add(new Gametile(new Vector2( 06, 19), 50, 0, 2, 6, 1));
        MenuTiles.Add(new Gametile(new Vector2( 06, 18), 80, 0, 2, 0, 1));
        MenuTiles.Add(new Gametile(new Vector2( 06, 17), 50, 0, 2, 0, 1));
        MenuTiles.Add(new Gametile(new Vector2( 06, 16), 80, 0, 2, 0, 1));
        MenuTiles.Add(new Gametile(new Vector2( 07, 19), 50, 0, 2, 5, 1));
        MenuTiles.Add(new Gametile(new Vector2( 08, 18), 50, 0, 2, 6, 1));
        MenuTiles.Add(new Gametile(new Vector2( 08, 17), 50, 0, 2, 0, 1));
        MenuTiles.Add(new Gametile(new Vector2( 08, 18), 55, 0, 2, 0, 1));
        MenuTiles.Add(new Gametile(new Vector2( 09, 19), 70, 0, 2, 6, 1));
        MenuTiles.Add(new Gametile(new Vector2( 10, 18), 50, 0, 2, 6, 1));
        MenuTiles.Add(new Gametile(new Vector2( 10, 17), 50, 0, 2, 0, 1));
        MenuTiles.Add(new Gametile(new Vector2( 10, 16), 100, 0, 2, 0, 1));

        MenuTiles.Add(new Gametile(new Vector2( 12, 19), 80, 0, 1, 6, 1));
        MenuTiles.Add(new Gametile(new Vector2( 12, 18), 50, 0, 1, 0, 1));
        MenuTiles.Add(new Gametile(new Vector2( 12, 17), 70, 0, 1, 0, 1));
        MenuTiles.Add(new Gametile(new Vector2( 13, 20), 50, 0, 1, 6, 1));
        MenuTiles.Add(new Gametile(new Vector2( 13, 19), 50, 0, 1, 1, 1));
        MenuTiles.Add(new Gametile(new Vector2( 13, 18), 80, 0, 1, 0, 1));
        MenuTiles.Add(new Gametile(new Vector2( 13, 16), 70, 0, 1, 4, 1));
        MenuTiles.Add(new Gametile(new Vector2( 14, 19), 50, 0, 1, 0, 1));
        MenuTiles.Add(new Gametile(new Vector2( 14, 18), 80, 0, 1, 0, 1));
        MenuTiles.Add(new Gametile(new Vector2( 14, 16), 50, 0, 1, 4, 1));

        MenuTiles.Add(new Gametile(new Vector2( 16, 19), 50, 0, 3, 0, 1));
        MenuTiles.Add(new Gametile(new Vector2( 16, 18), 100, 0, 3, 0, 1));
        MenuTiles.Add(new Gametile(new Vector2( 16, 17), 60, 0, 3, 4, 1));
        MenuTiles.Add(new Gametile(new Vector2( 16, 16), 70, 0, 3, 4, 1));
        MenuTiles.Add(new Gametile(new Vector2( 17, 19), 55, 0, 3, 0, 1));

        foreach(Gametile tile in MenuTiles)
        {
            GameObject newTile = Instantiate(Worldmap.Main.tilePrefab, transform);
            newTile.GetComponent<UnityEngine.Rendering.SortingGroup>().sortingLayerName = "UI";
            tile.Real = false;
            tile.GiveObject(newTile);
            MenuTileObjects.Add(newTile);
        }
    }

    // Update is called once per frame
    void Update()
    {
        tickProgress += Time.deltaTime;
        while (tickProgress >= Worldmap.Main.TICK_LENGTH)
        {
            if (InMenu)
            {
                transform.position = new Vector3(Mathf.Lerp(transform.position.x, Worldmap.Main.player.transform.position.x, 0.8f), Mathf.Lerp(transform.position.y, Worldmap.Main.player.transform.position.y, 0.8f));
                transform.position += Mathf.Sin(Time.fixedTime) * 0.25f * Vector3.up;
                foreach (Gametile tile in MenuTiles)
                {
                    tile.AddStab(Random.Range(0, 100f));
                    tile.SoftUpdate();
                    tile.HardUpdate();
                }
            }
            else
            {
                foreach (Gametile tile in MenuTiles)
                {
                    tile.AddStab(Random.Range(-100f, 0));
                    tile.SoftUpdate();
                    tile.HardUpdate();
                }
            }
            tickProgress -= Worldmap.Main.TICK_LENGTH;
        }
    }

    public void ToggleMenu()
    {
        Worldmap.Main.ToggleEnemyPause();
        tickProgress = 0;
        if (InMenu)
        {
            InMenu = false;
            MenuDisplay.enabled = false;
        }
        else
        {
            InMenu = true;
            MenuDisplay.enabled = true;
        }
    }

    public void ReloadWorld()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("WorldScene");
    }
}
