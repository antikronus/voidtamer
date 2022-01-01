using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Worldmap : MonoBehaviour
{
    public static Worldmap Main;

    [Header("Components")]
    public Transform player;
    public CraftingMenu CraftMenu;
    public Menu MainMenu;
    public RectTransform TTBackdrop;
    public UnityEngine.UI.Text Tooltip;
    public Camera cam;
    public GameObject tilePrefab;
    public GameObject minionPrefab;
    public GameObject ItemPrefab;
    public List<GameObject> EnemyPrefabs;

    [Space]

    [Header("Parameters")]
    public List<Item> CheatItems;
    public bool RANDOM_SEED = false;
    public int SEED_VALUE = 0;
    public int CHUNK_ACTIVE_DIST = 1;
    public int CHUNK_SIZE = 16;
    public float CAM_LERP = 0.8f;
    public float CAM_MIN_ZOOM = 1f;
    public float CAM_MAX_ZOOM = 5f;
    public float CAM_MENU_Y_OFFSET = 5f;
    public float TICK_LENGTH = 0.1f;
    public float WALL_STAB_ADD = 20f;

    //Private Values

    //Seeded Values
    private Vector2 NOISE_ORIGIN_A; //Combined Stability & Decay
    private Vector2 NOISE_ORIGIN_B; //Only Tile Stability
    private Vector2 NOISE_ORIGIN_C; //Only Tile Decay
    private Vector2 NOISE_ORIGIN_D; //Proportion Combined vs Individual Stability & Decay

    private Vector2 NOISE_ORIGIN_E; //Basic Tile Map
    private Vector2 NOISE_ORIGIN_F; //Air Tile Map
    private Vector2 NOISE_ORIGIN_G; //Earth Tile Map
    private Vector2 NOISE_ORIGIN_H; //Fire Tile Map
    private Vector2 NOISE_ORIGIN_I; //Water Tile Map

    private Vector2 NOISE_ORIGIN_J; //Workstation Map
    private Vector2 NOISE_ORIGIN_K; //Flower Map
    private Vector2 NOISE_ORIGIN_L; //Silversteel Map
    private Vector2 NOISE_ORIGIN_M; //Stone Map
    private Vector2 NOISE_ORIGIN_N; //Clay Map
    private Vector2 NOISE_ORIGIN_O; //Obsidian Map
    private Vector2 NOISE_ORIGIN_P; //Crystal Map
    private Vector2 NOISE_ORIGIN_Q; //Resource Abundance Map

    private Vector2 NOISE_ORIGIN_R; //Grass Map
    private Vector2 NOISE_ORIGIN_S; //Pit Map
    private Vector2 NOISE_ORIGIN_T; //Cliff Map
    private Vector2 NOISE_ORIGIN_U; //Sand Map
    private Vector2 NOISE_ORIGIN_V; //Dirt Map
    private Vector2 NOISE_ORIGIN_W; //Gravel Map
    private Vector2 NOISE_ORIGIN_X; //Smooth Map

    private Vector2 NOISE_ORIGIN_Y; //Civilization Map
    private Vector2 NOISE_ORIGIN_Z; //Tree Map

    private Vector2Int CHUNKTYPE_COORD_OFFSET;

    //Working Values
    private List<List<Chunk>> GenChunks;
    private List<Queue<GameObject>> TileObjects;
    private List<MinionController> Minions = new List<MinionController>();
    private List<Enemy> Enemies = new List<Enemy>();
    private Dictionary<int, LinkedList<int>> PylonCoords = new Dictionary<int, LinkedList<int>>();
    private Dictionary<int, LinkedList<int>> AirPylonCoords = new Dictionary<int, LinkedList<int>>();
    private Dictionary<int, LinkedList<int>> EarthPylonCoords = new Dictionary<int, LinkedList<int>>();
    private Dictionary<int, LinkedList<int>> FirePylonCoords = new Dictionary<int, LinkedList<int>>();
    private Dictionary<int, LinkedList<int>> WaterPylonCoords = new Dictionary<int, LinkedList<int>>();
    private Dictionary<int, LinkedList<int>> BrokenPylonCoords = new Dictionary<int, LinkedList<int>>();
    private int TOListFirstAvail = 0;
    private Vector2 ChunkListStart;
    private Vector2 PlayChunk;
    private float zoom = 25;
    private float cheatItemDelay = 2;
    private float tickProgress = 0;
    public bool ShowTooltips = true;
    private Queue<Vector2> MapGenPylons = new Queue<Vector2>();

    //Claim Maps
    private List<List<float>> PlayerClaimMap;
    private List<List<float>> EarthClaimMap;
    private List<List<float>> PylonClaimMap;
    private List<List<float>> BigPylonClaimMap;
    private List<List<float>> MinionClaimMap;
    private List<List<float>> VoidOrbClaimMap;
    private List<List<float>> BrokenPylonClaimMap;

    void Awake()
    {
        if(Main == null)
        {
            Main = this;
        }

        if (RANDOM_SEED)
        {
            SEED_VALUE = Random.Range(int.MinValue, int.MaxValue);
        }

        PlayerClaimMap = BuildClaimMap(3, 8, 150);
        EarthClaimMap = BuildClaimMap(4, 12, 150);
        PylonClaimMap = BuildClaimMap(3, 10, 8);
        BigPylonClaimMap = BuildClaimMap(3, 12, 12);
        MinionClaimMap = BuildClaimMap(2, 3, 150);
        VoidOrbClaimMap = BuildClaimMap(1, 3, -25);
        BrokenPylonClaimMap = BuildClaimMap(1, 5, 6);

        Random.State randomized = Random.state;
        Random.InitState(SEED_VALUE);

        NOISE_ORIGIN_A = new Vector2(Random.value, Random.value) * 1000.0f;
        NOISE_ORIGIN_B = new Vector2(Random.value, Random.value) * 1000.0f;
        NOISE_ORIGIN_C = new Vector2(Random.value, Random.value) * 1000.0f;
        NOISE_ORIGIN_D = new Vector2(Random.value, Random.value) * 1000.0f;

        NOISE_ORIGIN_E = new Vector2(Random.value, Random.value) * 1000.0f;
        NOISE_ORIGIN_F = new Vector2(Random.value, Random.value) * 1000.0f;
        NOISE_ORIGIN_G = new Vector2(Random.value, Random.value) * 1000.0f;
        NOISE_ORIGIN_H = new Vector2(Random.value, Random.value) * 1000.0f;
        NOISE_ORIGIN_I = new Vector2(Random.value, Random.value) * 1000.0f;

        NOISE_ORIGIN_J = new Vector2(Random.value, Random.value) * 1000.0f;
        NOISE_ORIGIN_K = new Vector2(Random.value, Random.value) * 1000.0f;
        NOISE_ORIGIN_L = new Vector2(Random.value, Random.value) * 1000.0f;
        NOISE_ORIGIN_M = new Vector2(Random.value, Random.value) * 1000.0f;
        NOISE_ORIGIN_N = new Vector2(Random.value, Random.value) * 1000.0f;
        NOISE_ORIGIN_O = new Vector2(Random.value, Random.value) * 1000.0f;
        NOISE_ORIGIN_P = new Vector2(Random.value, Random.value) * 1000.0f;
        NOISE_ORIGIN_Q = new Vector2(Random.value, Random.value) * 1000.0f;

        NOISE_ORIGIN_R = new Vector2(Random.value, Random.value) * 1000.0f;
        NOISE_ORIGIN_S = new Vector2(Random.value, Random.value) * 1000.0f;
        NOISE_ORIGIN_T = new Vector2(Random.value, Random.value) * 1000.0f;
        NOISE_ORIGIN_U = new Vector2(Random.value, Random.value) * 1000.0f;
        NOISE_ORIGIN_V = new Vector2(Random.value, Random.value) * 1000.0f;
        NOISE_ORIGIN_W = new Vector2(Random.value, Random.value) * 1000.0f;
        NOISE_ORIGIN_X = new Vector2(Random.value, Random.value) * 1000.0f;

        NOISE_ORIGIN_Y = new Vector2(Random.value, Random.value) * 1000.0f;
        NOISE_ORIGIN_Z = new Vector2(Random.value, Random.value) * 1000.0f;

        CHUNKTYPE_COORD_OFFSET = new Vector2Int(Random.Range(int.MinValue, int.MaxValue), Random.Range(int.MinValue, int.MaxValue));

        Random.state = randomized;

        List<float[]> ChunkTileTypes = new List<float[]>();
        List<float[]> ChunkTileSubtypes = new List<float[]>();

        DetermineTotalChunkStats(new Vector2(-1, -1), out float[] types, out float[] subtypes);
        ChunkTileTypes.Add(types);
        ChunkTileSubtypes.Add(subtypes);
        DetermineTotalChunkStats(new Vector2(-1, 0), out types, out subtypes);
        ChunkTileTypes.Add(types);
        ChunkTileSubtypes.Add(subtypes);
        DetermineTotalChunkStats(new Vector2(-1, 1), out types, out subtypes);
        ChunkTileTypes.Add(types);
        ChunkTileSubtypes.Add(subtypes);
        DetermineTotalChunkStats(new Vector2(0, -1), out types, out subtypes);
        ChunkTileTypes.Add(types);
        ChunkTileSubtypes.Add(subtypes);
        ChunkTileTypes.Add(null);
        ChunkTileSubtypes.Add(null);
        DetermineTotalChunkStats(new Vector2(0, 1), out types, out subtypes);
        ChunkTileTypes.Add(types);
        ChunkTileSubtypes.Add(subtypes);
        DetermineTotalChunkStats(new Vector2(1, -1), out types, out subtypes);
        ChunkTileTypes.Add(types);
        ChunkTileSubtypes.Add(subtypes);
        DetermineTotalChunkStats(new Vector2(1, 0), out types, out subtypes);
        ChunkTileTypes.Add(types);
        ChunkTileSubtypes.Add(subtypes);
        DetermineTotalChunkStats(new Vector2(1, 1), out types, out subtypes);
        ChunkTileTypes.Add(types);
        ChunkTileSubtypes.Add(subtypes);

        List<ChunkType> ChunkTypes = new List<ChunkType>() { ChunkType.Standard, ChunkType.Standard, ChunkType.Standard, ChunkType.Standard, ChunkType.Origin, ChunkType.Standard, ChunkType.Standard, ChunkType.Standard, ChunkType.Standard };

        //find an earth crystal source
        int prevI = -1;
        float prevIValue = 0;

        for(int i = 0; i < 9; i++)
        {
            if(ChunkTypes[i] == ChunkType.Standard)
            {
                float value = ChunkTileTypes[i][2];
                if(prevI == -1 || prevIValue < value)
                {
                    prevI = i;
                    prevIValue = value;
                }
            }
        }

        ChunkTypes[prevI] = ChunkType.EarthCrystal;
        float earthValue = prevIValue;

        //find a fire crystal source
        prevI = -1;
        prevIValue = 0;

        for(int i = 0; i < 9; i++)
        {
            if (ChunkTypes[i] == ChunkType.Standard)
            {
                float value = ChunkTileTypes[i][3];
                if (prevI == -1 || prevIValue < value)
                {
                    prevI = i;
                    prevIValue = value;
                }
            }
        }

        ChunkTypes[prevI] = ChunkType.FireCrystal;
        float fireValue = prevIValue;

        //find a silversteel source
        prevI = -1;
        prevIValue = 0;

        for (int i = 0; i < 9; i++)
        {
            if (ChunkTypes[i] == ChunkType.Standard)
            {
                float value = 0.5f * (Mathf.Max(ChunkTileTypes[i][0], ChunkTileTypes[i][1], ChunkTileTypes[i][4]) + ChunkTileSubtypes[i][3]);
                if (prevI == -1 || prevIValue < value)
                {
                    prevI = i;
                    prevIValue = value;
                }
            }
        }

        ChunkTypes[prevI] = ChunkType.Silversteel;
        float silversteelValue = prevIValue;

        //find a stone source

        prevI = -1;
        prevIValue = 0;

        for (int i = 0; i < 9; i++)
        {
            if (ChunkTypes[i] == ChunkType.Standard)
            {
                float value = 0.5f * (Mathf.Max(ChunkTileTypes[i][0], ChunkTileTypes[i][2], ChunkTileTypes[i][3]) + ChunkTileSubtypes[i][5]);
                if (prevI == -1 || prevIValue < value)
                {
                    prevI = i;
                    prevIValue = value;
                }
            }
        }

        ChunkTypes[prevI] = ChunkType.Stone;
        float stoneValue = prevIValue;

        //find a clay source

        prevI = -1;
        prevIValue = 0;

        for (int i = 0; i < 9; i++)
        {
            if(ChunkTypes[i] == ChunkType.Standard)
            {
                float value = 0.5f * (Mathf.Max(ChunkTileTypes[i][0], ChunkTileTypes[i][2], ChunkTileTypes[i][4]) + ChunkTileSubtypes[i][4]);
                if(prevI == -1 || prevIValue < value)
                {
                    prevI = i;
                    prevIValue = value;
                }
            }
        }

        ChunkTypes[prevI] = ChunkType.Clay;
        float clayValue = prevIValue;

        GenChunks = new List<List<Chunk>>();
        ChunkListStart = new Vector2(0 - CHUNK_ACTIVE_DIST, 0 - CHUNK_ACTIVE_DIST);
        PlayChunk = new Vector2(0, 0);
        TileObjects = new List<Queue<GameObject>>();

        for(int i = 0; i < (1 + CHUNK_ACTIVE_DIST * 2) * (1 + CHUNK_ACTIVE_DIST * 2); i++)
        {
            TileObjects.Add(new Queue<GameObject>());
            for(int j = 0; j < CHUNK_SIZE * CHUNK_SIZE; j++)
            {
                TileObjects[i].Enqueue(Instantiate(tilePrefab));
            }
        }

        for(int x = - CHUNK_ACTIVE_DIST; x <= CHUNK_ACTIVE_DIST; x++)
        {
            GenChunks.Add(new List<Chunk>());
            
            for(int y = -CHUNK_ACTIVE_DIST; y <= CHUNK_ACTIVE_DIST; y++)
            {
                int xPos = x - Mathf.FloorToInt(ChunkListStart.x);
                int yPos = y - Mathf.FloorToInt(ChunkListStart.y);
                if (x >= -1 && x <= 1 && y >= -1 && y <= 1)
                    GenChunks[xPos].Add(BuildChunk(new Vector2(x, y), ChunkTypes[3 * (x + 1) + y + 1]));
                else
                    GenChunks[xPos].Add(BuildChunk(new Vector2(x, y)));
                GenChunks[xPos][yPos].AssignObjects(TileObjects[TOListFirstAvail], TOListFirstAvail);
                TOListFirstAvail++;
            }
        }

        while (MapGenPylons.Count > 0)
            AddPylon(MapGenPylons.Dequeue(), 5);

        int claimX = 0;
        int claimY = 0;

        //foreach (List<float> column in PlayerClaimMap)
        //{
        //    claimY = 0;
        //    foreach (float element in column)
        //    {
        //        if (element == 150)
        //        {
        //            Gametile target = FindTile(new Vector2(Mathf.Floor(player.position.x + 0.5f + claimX - (column.Count / 2)), Mathf.Floor(player.position.y + 0.5f + claimY - (column.Count / 2))));

        //            target.SetStab(100);
        //            target.SoftUpdate();
        //        }
        //        claimY++;
        //    }
        //    claimX++;
        //}

        foreach (KeyValuePair<int, LinkedList<int>> column in BrokenPylonCoords)
        {
            int x = column.Key;
            foreach (int y in column.Value)
            {
                claimX = 0;
                claimY = 0;

                foreach (List<float> claimCol in BrokenPylonClaimMap)
                {
                    claimY = 0;
                    foreach (float element in claimCol)
                    {
                        Gametile target = FindTile(new Vector2(Mathf.Floor(x + claimX - (claimCol.Count / 2)), Mathf.Floor(y + claimY - (claimCol.Count / 2))));

                        if (target.STAB_LOSS < element)
                            target.SetStab(100);
                        claimY++;
                    }
                    claimX++;
                }
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!MainMenu.InMenu && player.GetComponent<characterControl>().IsInspecting)
            {
                player.GetComponent<characterControl>().StopInspecting();
                CraftMenu.Hide();
            }
            else
                MainMenu.ToggleMenu();
        }
        if (!player.GetComponent<characterControl>().IsInspecting)
        {
            if (CheatItems.Count > 0 && cheatItemDelay < 1000 && cheatItemDelay > 0)
                cheatItemDelay -= Time.deltaTime;
            else if (cheatItemDelay < 0)
            {
                cheatItemDelay = 1000;
                FindTile(new Vector2(CHUNK_SIZE / 2, CHUNK_SIZE / 2 - 1)).AddPlacedObject("Container");
                foreach (Item item in CheatItems)
                    FindTile(new Vector2(CHUNK_SIZE / 2, CHUNK_SIZE / 2 - 1)).Contents.ProduceItem(item);
            }

            Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
            Gametile targetTile = FindTile(new Vector2(Mathf.Floor(mousePos.x + 0.5f), Mathf.Floor(mousePos.y + 0.5f)));
            PlacedObject toToolTip = targetTile.Contents;
            LayerMask mask = LayerMask.GetMask("Items", "Minions");
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, 100.0f, mask);
            if (hit.collider != null && hit.collider.TryGetComponent(out MinionController ttminion))
            {
                ShowTooltip(ttminion.GetMinionTooltip());
            }
            else if(targetTile.Stable && toToolTip != null)
            {
                string warning = "";
                if (toToolTip.AddsStability && !player.GetComponent<characterControl>().HasItem)
                {
                    if (DisplayStabRemoval(toToolTip))
                    {
                        warning = "\n<color=#b30000>WARNING: This pylon is currently supporting one or more artificial structures. If you remove the pylon, those structures may be lost to the void forever!</color>";
                    }
                }
                else if (!targetTile.Contents.natural && targetTile.Stab_Inc - targetTile.STAB_LOSS < 0)
                {
                    warning = "\n<color=#b30000>WARNING: This artificial structure is currently on an unstable tile. If you leave without placing a pylon to keep the area stable, it will fall into the void and be lost!</color>";
                }

                ShowTooltip(toToolTip, warning);
            }
            else
            {
                TTBackdrop.gameObject.SetActive(false);
            }

            if(player.GetComponent<characterControl>().Holding.AddsStability() && targetTile.Stable && targetTile.CanPlaceObject())
            {
                DisplayStabAddition(targetTile, player.GetComponent<characterControl>().Holding);
            }

            if (Input.GetMouseButtonDown(0))
            {
                //FindTile(new Vector2(Mathf.Floor(mousePos.x + 0.5f), Mathf.Floor(mousePos.y + 0.5f))).PrintTileDebugInfo();
                if (player.GetComponent<characterControl>().HasItem)
                {
                    if (player.GetComponent<characterControl>().Holding.IsPlaceable() && targetTile.Stable)
                    {
                        string toPlace = player.GetComponent<characterControl>().Holding.ToString();
                        if (targetTile.AddPlacedObject(toPlace))
                        {
                            if (toPlace == "Pylon")
                            {
                                AddPylon(new Vector2(Mathf.Floor(mousePos.x + 0.5f), Mathf.Floor(mousePos.y + 0.5f)));
                            }
                            else if(toPlace == "PylonAir")
                            {
                                AddPylon(new Vector2(Mathf.Floor(mousePos.x + 0.5f), Mathf.Floor(mousePos.y + 0.5f)), 1);
                            }
                            else if(toPlace == "PylonEarth")
                            {
                                AddPylon(new Vector2(Mathf.Floor(mousePos.x + 0.5f), Mathf.Floor(mousePos.y + 0.5f)), 2);
                            }
                            else if(toPlace == "PylonFire")
                            {
                                AddPylon(new Vector2(Mathf.Floor(mousePos.x + 0.5f), Mathf.Floor(mousePos.y + 0.5f)), 3);
                            }
                            else if(toPlace == "PylonWater")
                            {
                                AddPylon(new Vector2(Mathf.Floor(mousePos.x + 0.5f), Mathf.Floor(mousePos.y + 0.5f)), 4);
                            }
                            else if(toPlace == "Book")
                            {
                                targetTile.Contents.BookText = player.GetComponent<characterControl>().HeldItem.BookText;
                            }
                            else if(toPlace == "Wall")
                            {
                                targetTile.ChangeStabIncome(WALL_STAB_ADD);
                            }
                            player.GetComponent<characterControl>().DropItem(true);
                        }
                    }
                    if(player.GetComponent<characterControl>().HasItem)
                    {
                        player.GetComponent<characterControl>().ThrowItem();
                    }
                }
                else
                {
                    if (hit.collider != null)
                    {
                        ItemObject item = null;
                        if (hit.collider.TryGetComponent(out MinionController minion))
                            item = minion.DropItem();
                        else
                            hit.collider.TryGetComponent(out item);

                        if(item != null)
                            player.GetComponent<characterControl>().GiveItem(item);
                    }
                    else if(targetTile.SUBTYPE != 1 || targetTile.WorldPos != new Vector2(Mathf.Floor(player.position.x + 0.5f), Mathf.Floor(player.position.y + 0.5f)))
                    {
                        targetTile.HarvestPlacedObject(out ItemObject harvested);
                        if(harvested != null)
                        {
                            if(harvested.Type == Item.Wall && targetTile.Contents == null)
                            {
                                targetTile.ChangeStabIncome(-WALL_STAB_ADD);
                            }
                            player.GetComponent<characterControl>().GiveItem(harvested);
                        }
                        RemovePylon(new Vector2(Mathf.Floor(mousePos.x + 0.5f), Mathf.Floor(mousePos.y + 0.5f)));
                    }
                }
            }
            if (Input.GetMouseButtonDown(1))
            {
                mask = LayerMask.GetMask("PlacedObjects", "Minions");
                hit = Physics2D.Raycast(cam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 100.0f, mask);
                if (hit.collider != null)
                {
                    if (hit.collider.gameObject.layer == 10)
                    {
                        PlacedObject item = FindTile(new Vector2(Mathf.Floor(mousePos.x + 0.5f), Mathf.Floor(mousePos.y + 0.5f))).Contents;
                        if (item != null && item.Crafting)
                        {
                            player.GetComponent<characterControl>().Inspect(item);
                            string stationType = "";
                            switch (item.ID)
                            {
                                case 1:
                                    stationType = "Work Bench";
                                    break;

                                case 2:
                                    stationType = "Inscriber";
                                    break;

                                case 3:
                                    stationType = "Kiln";
                                    break;

                                case 4:
                                    stationType = "Alchemy Plinth";
                                    break;

                                case 5:
                                    stationType = "Crafts Workshop";
                                    break;

                                case 19:
                                    stationType = "Work Bench";
                                    break;

                                case 20:
                                    stationType = "Inscriber";
                                    break;

                                case 21:
                                    stationType = "Kiln";
                                    break;

                                case 22:
                                    stationType = "Alchemy Plinth";
                                    break;

                                case 23:
                                    stationType = "Crafts Workshop";
                                    break;
                            }
                            CraftMenu.Display(item.ActiveRecipe, stationType);
                        }
                        else if(item != null && item.ID == 33)
                        {
                            player.GetComponent<characterControl>().Inspect(item);
                            CraftMenu.Display(item);
                        }
                    }
                    else
                    {
                        MinionController minion = hit.collider.GetComponent<MinionController>();
                        GameObject newObj = Instantiate(ItemPrefab, minion.transform);
                        newObj.transform.parent = null;
                        newObj.transform.position = new Vector3(newObj.transform.position.x, newObj.transform.position.y, 0);
                        ItemObject newItem = newObj.GetComponent<ItemObject>();
                        newItem.AssignType(minion.Drops, CraftMenu.ItemSprites[(int)minion.Drops - 1]);
                        minion.DropItem();
                        RemoveMinion(minion);
                        if(!player.GetComponent<characterControl>().HasItem)
                            player.GetComponent<characterControl>().GiveItem(newItem);
                    }
                }
                else if (player.GetComponent<characterControl>().HasItem)
                {
                    characterControl playerScript = player.GetComponent<characterControl>();
                    bool potion = false;
                    switch (playerScript.Holding)
                    {
                        case (Item.PotionWhite):
                            playerScript.ApplyPotion(0);
                            potion = true;
                            break;

                        case (Item.PotionBrown):
                            playerScript.ApplyPotion(1);
                            potion = true;
                            break;

                        case (Item.PotionRed):
                            playerScript.ApplyPotion(2);
                            potion = true;
                            break;

                        case (Item.PotionBlue):
                            playerScript.ApplyPotion(3);
                            potion = true;
                            break;
                    }

                    playerScript.DropItem(potion);
                }
            }
        }
        else
        {
            TTBackdrop.gameObject.SetActive(false);
        }
        if (MainMenu.InMenu)
        {
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, 14, 0.5f);
            cam.transform.position = new Vector3(Mathf.Lerp(cam.transform.position.x, player.position.x, CAM_LERP), Mathf.Lerp(cam.transform.position.y, player.position.y + 0.5f + CAM_MENU_Y_OFFSET, CAM_LERP), -10);
        }
        else
        {
            zoom = Mathf.Clamp(zoom - Input.mouseScrollDelta.y * 2, 0, 50);
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, Mathf.Lerp(CAM_MIN_ZOOM, CAM_MAX_ZOOM, zoom / 50), 0.5f);
            cam.transform.position = new Vector3(Mathf.Lerp(cam.transform.position.x, player.position.x, CAM_LERP), Mathf.Lerp(cam.transform.position.y, player.position.y + 0.5f, CAM_LERP), -10);
        }
        

        if (!player.GetComponent<characterControl>().IsInspecting)
        {
            tickProgress += Time.deltaTime;
            while (tickProgress >= TICK_LENGTH)
            {
                UpdateActChunks();
                UpdateGenChunks();
                tickProgress -= TICK_LENGTH;
            }
        }
    }

    void UpdateGenChunks()
    {
        while (MapGenPylons.Count > 0)
            AddPylon(MapGenPylons.Dequeue(), 5);
        
        foreach (KeyValuePair<int, LinkedList<int>> column in AirPylonCoords)
        {
            int x = column.Key;
            foreach (int y in column.Value)
            {
                if (new Vector2(x - player.position.x, y - player.position.y).sqrMagnitude <= Mathf.Pow(10.5f, 2))
                {
                    player.GetComponent<characterControl>().ApplyPotion(4);
                }
            }
        }
        foreach (KeyValuePair<int, LinkedList<int>> column in FirePylonCoords)
        {
            int x = column.Key;
            foreach (int y in column.Value)
            {
                ContactFilter2D mask = new ContactFilter2D();
                mask.SetLayerMask(LayerMask.GetMask("Enemies"));
                Collider2D[] hits = new Collider2D[100];
                int numHits = Physics2D.OverlapCircle(new Vector2(x, y), 10.5f, mask, hits);
                for (int i = 0; i < numHits; i++)
                {
                    Collider2D hit = hits[i];
                    if (hit.TryGetComponent(out Enemy enemy))
                    {
                        enemy.Health -= 2 * TICK_LENGTH + (player.GetComponent<characterControl>().FireEffect?3:0);
                    }
                }
            }
        }
        foreach (KeyValuePair<int, LinkedList<int>> column in WaterPylonCoords)
        {
            int x = column.Key;
            foreach (int y in column.Value)
            {
                ContactFilter2D mask = new ContactFilter2D();
                mask.SetLayerMask(LayerMask.GetMask("Items"));
                Collider2D[] hits = new Collider2D[100];
                int numHits = Physics2D.OverlapCircle(new Vector2(x, y), 10.5f, mask, hits);
                for (int i = 0; i < numHits; i++)
                {
                    Collider2D hit = hits[i];
                    if (hit.TryGetComponent(out ItemObject item))
                    {
                        item.PullToward(new Vector2(x, y), 100 * TICK_LENGTH);
                    }
                }
            }
        }
        foreach (MinionController minion in Minions)
        {
            int claimX = 0;
            int claimY = 0;

            foreach(List<float> claimCol in MinionClaimMap)
            {
                claimY = 0;
                foreach(float element in claimCol)
                {
                    Gametile target = FindTile(new Vector2(Mathf.Floor(minion.transform.position.x + claimX - (claimCol.Count / 2)), Mathf.Floor(minion.transform.position.y + claimY - (claimCol.Count / 2))));

                    target.AddStab(element);
                    claimY++;
                }
                claimX++;
            }
        }
        foreach (Enemy enemy in Enemies)
        {
            if(enemy.Type == EnemyType.VoidOrb)
            {
                int claimX = 0;
                int claimY = 0;

                foreach(List<float> claimCol in VoidOrbClaimMap)
                {
                    claimY = 0;
                    foreach(float element in claimCol)
                    {
                        Gametile target = FindTile(new Vector2(Mathf.Floor(enemy.transform.position.x + claimX - (claimCol.Count / 2)), Mathf.Floor(enemy.transform.position.y + claimY - (claimCol.Count / 2))));

                        target.AddStab(element);
                        claimY++;
                    }
                    claimX++;
                }
            }
        }
        foreach(List<Chunk> column in GenChunks)
        {
            foreach(Chunk element in column)
            {
                element.SoftUpdate();
            }
        }
    }

    void UpdateActChunks()
    {
        Vector2 OldPlayChunk = PlayChunk;
        PlayChunk = new Vector2(Mathf.Floor((player.position.x + 0.5f) / CHUNK_SIZE), Mathf.Floor((player.position.y + 0.5f) / CHUNK_SIZE));
        int claimX = 0;
        int claimY = 0;
        List<List<float>> toClaim = player.GetComponent<characterControl>().EarthEffect ? EarthClaimMap : PlayerClaimMap;
        //Debug.Log("I think the player is in chunk ( " + PlayChunk.x + ", " + PlayChunk.y + ") and tile ( " + Mathf.Floor(player.position.x + 0.5f) + ", " + Mathf.Floor(player.position.y + 0.5f) + ")");
        foreach(List<float> column in toClaim)
        {
            claimY = 0;
            foreach(float element in column)
            {
                Gametile target = FindTile(new Vector2(Mathf.Floor(player.position.x + 0.5f +claimX - (column.Count / 2)), Mathf.Floor(player.position.y + 0.5f + claimY - (column.Count/2))));

                target.AddStab(element);
                claimY++;
            }
            claimX++;
        }
        for(int x = -CHUNK_ACTIVE_DIST; x <= CHUNK_ACTIVE_DIST; x++)
        {
            for(int y = -CHUNK_ACTIVE_DIST; y <= CHUNK_ACTIVE_DIST; y++)
            {
                FindChunk(new Vector2(PlayChunk.x + x, PlayChunk.y + y)).HardUpdate();
            }
        }
        Vector2 DeltaChunk = PlayChunk - OldPlayChunk;
        if(DeltaChunk != Vector2.zero)
        {
            while(DeltaChunk.x > 0)
            {
                DeltaChunk.x--;
                for (int yOffset = 0; yOffset < 1 + 2 * CHUNK_ACTIVE_DIST; yOffset++)
                {
                    int objIndex = FindChunk(new Vector2(PlayChunk.x - CHUNK_ACTIVE_DIST - 1 - DeltaChunk.x, PlayChunk.y - CHUNK_ACTIVE_DIST + yOffset - DeltaChunk.y)).FreeObjects();
                    FindChunk(new Vector2(PlayChunk.x + CHUNK_ACTIVE_DIST - DeltaChunk.x, PlayChunk.y - CHUNK_ACTIVE_DIST + yOffset - DeltaChunk.y)).AssignObjects(TileObjects[objIndex], objIndex);
                }
            }
            while(DeltaChunk.x < 0)
            {
                DeltaChunk.x++;
                for (int yOffset = 0; yOffset < 1 + 2 * CHUNK_ACTIVE_DIST; yOffset++)
                {
                    int objIndex = FindChunk(new Vector2(PlayChunk.x + CHUNK_ACTIVE_DIST + 1 - DeltaChunk.x, PlayChunk.y - CHUNK_ACTIVE_DIST + yOffset - DeltaChunk.y)).FreeObjects();
                    FindChunk(new Vector2(PlayChunk.x - CHUNK_ACTIVE_DIST - DeltaChunk.x, PlayChunk.y - CHUNK_ACTIVE_DIST + yOffset - DeltaChunk.y)).AssignObjects(TileObjects[objIndex], objIndex);
                }
            }
            while(DeltaChunk.y > 0)
            {
                DeltaChunk.y--;
                for (int xOffset = 0; xOffset < 1 + 2 * CHUNK_ACTIVE_DIST; xOffset++)
                {
                    int objIndex = FindChunk(new Vector2(PlayChunk.x - CHUNK_ACTIVE_DIST + xOffset, PlayChunk.y - CHUNK_ACTIVE_DIST - 1 - DeltaChunk.y)).FreeObjects();
                    FindChunk(new Vector2(PlayChunk.x - CHUNK_ACTIVE_DIST + xOffset, PlayChunk.y + CHUNK_ACTIVE_DIST - DeltaChunk.y)).AssignObjects(TileObjects[objIndex], objIndex);
                }
            }
            while(DeltaChunk.y < 0)
            {
                DeltaChunk.y++;
                for (int xOffset = 0; xOffset < 1 + 2 * CHUNK_ACTIVE_DIST; xOffset++)
                {
                    int objIndex = FindChunk(new Vector2(PlayChunk.x - CHUNK_ACTIVE_DIST + xOffset, PlayChunk.y + CHUNK_ACTIVE_DIST + 1 - DeltaChunk.y)).FreeObjects();
                    FindChunk(new Vector2(PlayChunk.x - CHUNK_ACTIVE_DIST + xOffset, PlayChunk.y - CHUNK_ACTIVE_DIST - DeltaChunk.y)).AssignObjects(TileObjects[objIndex], objIndex);
                }
            }
        }
    }

    void ResizeGenChunks(Vector2 Dir)
    {
        //Debug.Log("Resize Vector: (" + Dir.x + ", " + Dir.y + ")");
        while (Mathf.FloorToInt(Dir.y) > 0)
        {
            Dir.y -= 1;
            for (int i = 0; i < GenChunks.Count; i++)
            {
                GenChunks[i].Add(BuildChunk(new Vector2(ChunkListStart.x + i, ChunkListStart.y + GenChunks[i].Count)));
            }
        }

        while (Mathf.FloorToInt(Dir.y) < 0)
        {
            Dir.y += 1;
            ChunkListStart.y -= 1;
            for (int i = 0; i < GenChunks.Count; i++)
            {
                GenChunks[i].Insert(0, BuildChunk(new Vector2(ChunkListStart.x + i, ChunkListStart.y)));
            }
        }

        while (Mathf.FloorToInt(Dir.x) > 0)
        {
            Dir.x -= 1;
            GenChunks.Add(new List<Chunk>());
            for(int i = 0; i < GenChunks[0].Count; i++)
            {
                GenChunks[GenChunks.Count - 1].Add(BuildChunk(new Vector2(ChunkListStart.x + GenChunks.Count - 1, ChunkListStart.y + i)));
            }
        }

        while(Mathf.FloorToInt(Dir.x) < 0)
        {
            Dir.x += 1;
            ChunkListStart.x -= 1;
            GenChunks.Insert(0, new List<Chunk>());
            for(int i = 0; i < GenChunks[1].Count; i++)
            {
                GenChunks[0].Add(BuildChunk(new Vector2(ChunkListStart.x, ChunkListStart.y + i)));
            }
        }
    }

    Chunk BuildChunk(Vector2 Pos)
    {
        ChunkType type = ChunkType.Standard;

        int PosValue = Mod((int)Pos.x * 16 + (int)Pos.y, 256);
        Vector2Int MetaChunkPos = new Vector2Int((int)Pos.x - Mod((int)Pos.x, 16) + CHUNKTYPE_COORD_OFFSET.x, (int)Pos.y - Mod((int)Pos.y, 16) + CHUNKTYPE_COORD_OFFSET.y);
        int PRandStart = MetaChunkPos.x + MetaChunkPos.y / 16;
        int PRandIncr = 4 * MetaChunkPos.y + 2 * MetaChunkPos.x + 257;

        for(int i = 0; i < 24; i++)
        {
            int PRandVal = Mod(PRandStart + PRandIncr * i, 256);
            if(PRandVal == PosValue)
            {
                switch (i)
                {
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                        type = ChunkType.Silversteel;
                        break;

                    case 4:
                    case 5:
                    case 6:
                    case 7:
                        type = ChunkType.Clay;
                        break;

                    case 8:
                    case 9:
                    case 10:
                    case 11:
                        type = ChunkType.Stone;
                        break;

                    case 12:
                    case 13:
                    case 14:
                    case 15:
                        type = ChunkType.Obsidian;
                        break;

                    case 16:
                    case 17:
                        type = ChunkType.PlainCrystal;
                        break;

                    case 18:
                    case 19:
                        type = ChunkType.Remnant;
                        break;

                    case 20:
                        type = ChunkType.AirCrystal;
                        break;

                    case 21:
                        type = ChunkType.EarthCrystal;
                        break;

                    case 22:
                        type = ChunkType.FireCrystal;
                        break;

                    case 23:
                        type = ChunkType.WaterCrystal;
                        break;
                }
                break;
            }
        }

        if (type == ChunkType.Standard && Mathf.PerlinNoise(Pos.x * 0.6125f + NOISE_ORIGIN_Y.x, Pos.y * 0.6125f + NOISE_ORIGIN_Y.y) > 0.75f)
            type = ChunkType.Remnant;

        return BuildChunk(Pos, type);
    }

    Chunk BuildChunk(Vector2 Pos, ChunkType type)
    {
        Queue<Vector2> brokenPylons = new Queue<Vector2>();
        float chunkCivility = Mathf.PerlinNoise(Pos.x * 0.6125f + NOISE_ORIGIN_Y.x, Pos.y * 0.6125f + NOISE_ORIGIN_Y.y);

        Item containerItem = Item.CrystalPlain;

        float plainBiomeBias = 1;
        float airBiomeBias = 1;
        float earthBiomeBias = 1;
        float fireBiomeBias = 1;
        float waterBiomeBias = 1;

        float grassTerrainBias = 1;
        float pitTerrainBias = 1;
        float cliffTerrainBias = 1;
        float sandTerrainBias = 1;
        float dirtTerrainBias = 1;
        float gravelTerrainBias = 1;
        float smoothTerrainBias = 1;

        float workstationBias = 1;
        float flowerBias = 1;
        float silversteelBias = 1;
        float stoneBias = 1;
        float clayBias = 1;
        float obsidianBias = 1;
        float crystalBias = 1;
        float nonNativeBias = 1;
        float treeBias = 1;

        switch (type)
        {
            case ChunkType.Origin:
                containerItem = Item.Minion;
                pitTerrainBias = -1;
                cliffTerrainBias = -1;
                workstationBias =-1;
                nonNativeBias = 1.2f;
                treeBias = -1;
                break;

            case ChunkType.Remnant:
                grassTerrainBias = 0.6f;
                pitTerrainBias = 0;
                cliffTerrainBias = 0;
                workstationBias = 0.2f;
                flowerBias = 1.5f;
                silversteelBias = 0.7f;
                stoneBias = 0.7f;
                clayBias = 0.7f;
                obsidianBias = 0.7f;
                crystalBias = 0.4f;
                nonNativeBias = 1.3f;
                treeBias = 0f;
                break;

            case ChunkType.PlainCrystal:
                plainBiomeBias = 1.4f;
                airBiomeBias = 0.8f;
                earthBiomeBias = 0.8f;
                fireBiomeBias = 0.8f;
                waterBiomeBias = 0.8f;
                grassTerrainBias = 0.9f;
                pitTerrainBias = 0.5f;
                cliffTerrainBias = 0.5f;
                workstationBias = -1;
                flowerBias = 0.8f;
                silversteelBias = 0.8f;
                stoneBias = 0.8f;
                clayBias = 0.8f;
                obsidianBias = 0.8f;
                crystalBias = 1.2f;
                nonNativeBias = 0.8f;
                treeBias = 0.5f;
                break;

            case ChunkType.AirCrystal:
                plainBiomeBias = 0.8f;
                airBiomeBias = 1.4f;
                earthBiomeBias = 0.8f;
                fireBiomeBias = 0.8f;
                waterBiomeBias = 0.8f;
                grassTerrainBias = 0.9f;
                pitTerrainBias = 0.5f;
                cliffTerrainBias = 0.5f;
                sandTerrainBias = 1.1f;
                workstationBias = -1;
                flowerBias = 0.8f;
                silversteelBias = 0.8f;
                stoneBias = 0.8f;
                clayBias = 0.8f;
                obsidianBias = 0.8f;
                crystalBias = 1.2f;
                treeBias = 0.5f;
                break;

            case ChunkType.EarthCrystal:
                plainBiomeBias = 0.8f;
                airBiomeBias = 0.8f;
                earthBiomeBias = 1.4f;
                fireBiomeBias = 0.8f;
                waterBiomeBias = 0.8f;
                grassTerrainBias = 0.9f;
                pitTerrainBias = 0.5f;
                cliffTerrainBias = 0.5f;
                gravelTerrainBias = 1.1f;
                workstationBias = -1;
                flowerBias = 0.8f;
                silversteelBias = 0.8f;
                stoneBias = 0.8f;
                clayBias = 0.8f;
                obsidianBias = 0.8f;
                crystalBias = 1.2f;
                treeBias = 0.5f;
                break;

            case ChunkType.FireCrystal:
                plainBiomeBias = 0.8f;
                airBiomeBias = 0.8f;
                earthBiomeBias = 0.8f;
                fireBiomeBias = 1.4f;
                waterBiomeBias = 0.8f;
                grassTerrainBias = 0.9f;
                pitTerrainBias = 0.5f;
                cliffTerrainBias = 0.5f;
                smoothTerrainBias = 1.1f;
                workstationBias = -1;
                flowerBias = 0.8f;
                silversteelBias = 0.8f;
                stoneBias = 0.8f;
                clayBias = 0.8f;
                obsidianBias = 0.8f;
                crystalBias = 1.2f;
                treeBias = 0.5f;
                break;

            case ChunkType.WaterCrystal:
                plainBiomeBias = 0.8f;
                airBiomeBias = 0.8f;
                earthBiomeBias = 0.8f;
                fireBiomeBias = 0.8f;
                waterBiomeBias = 1.4f;
                grassTerrainBias = 0.9f;
                pitTerrainBias = 0.5f;
                cliffTerrainBias = 0.5f;
                dirtTerrainBias = 1.1f;
                workstationBias = -1;
                flowerBias = 0.8f;
                silversteelBias = 0.8f;
                stoneBias = 0.8f;
                clayBias = 0.8f;
                obsidianBias = 0.8f;
                crystalBias = 1.2f;
                treeBias = 0.5f;
                break;

            case ChunkType.Silversteel:
                plainBiomeBias = 1.1f;
                airBiomeBias = 1.1f;
                earthBiomeBias = 0.9f;
                fireBiomeBias = 0.9f;
                waterBiomeBias = 1.1f;
                grassTerrainBias = 0.9f;
                pitTerrainBias = 0.5f;
                cliffTerrainBias = 0.5f;
                sandTerrainBias = 1.2f;
                dirtTerrainBias = 0.8f;
                gravelTerrainBias = 0.8f;
                smoothTerrainBias = 0.8f;
                workstationBias = 0;
                flowerBias = 0.8f;
                silversteelBias = 1.2f;
                stoneBias = 0.8f;
                clayBias = 0.8f;
                obsidianBias = 0.8f;
                crystalBias = 0.8f;
                nonNativeBias = 1.2f;
                treeBias = 0.5f;
                break;

            case ChunkType.Clay:
                plainBiomeBias = 1.1f;
                airBiomeBias = 0.9f;
                earthBiomeBias = 1.1f;
                fireBiomeBias = 0.9f;
                waterBiomeBias = 1.1f;
                grassTerrainBias = 0.9f;
                pitTerrainBias = 0.5f;
                cliffTerrainBias = 0.5f;
                sandTerrainBias = 0.8f;
                dirtTerrainBias = 1.2f;
                gravelTerrainBias = 0.8f;
                smoothTerrainBias = 0.8f;
                workstationBias = 0;
                flowerBias = 0.8f;
                silversteelBias = 0.8f;
                stoneBias = 0.8f;
                clayBias = 1.2f;
                obsidianBias = 0.8f;
                crystalBias = 0.8f;
                nonNativeBias = 1.2f;
                treeBias = 0.5f;
                break;

            case ChunkType.Stone:
                plainBiomeBias = 1.1f;
                airBiomeBias = 0.9f;
                earthBiomeBias = 1.1f;
                fireBiomeBias = 1.1f;
                waterBiomeBias = 0.9f;
                grassTerrainBias = 0.9f;
                pitTerrainBias = 0.5f;
                cliffTerrainBias = 0.5f;
                sandTerrainBias = 0.8f;
                dirtTerrainBias = 0.8f;
                gravelTerrainBias = 1.2f;
                smoothTerrainBias = 0.8f;
                workstationBias = 0;
                flowerBias = 0.8f;
                silversteelBias = 0.8f;
                stoneBias = 1.2f;
                clayBias = 0.8f;
                obsidianBias = 0.8f;
                crystalBias = 0.8f;
                nonNativeBias = 1.2f;
                treeBias = 0.5f;
                break;

            case ChunkType.Obsidian:
                plainBiomeBias = 1.1f;
                airBiomeBias = 1.1f;
                earthBiomeBias = 0.9f;
                fireBiomeBias = 1.1f;
                waterBiomeBias = 0.9f;
                grassTerrainBias = 0.9f;
                pitTerrainBias = 0.5f;
                cliffTerrainBias = 0.5f;
                sandTerrainBias = 0.8f;
                dirtTerrainBias = 0.8f;
                gravelTerrainBias = 0.8f;
                smoothTerrainBias = 1.2f;
                workstationBias = 0;
                flowerBias = 0.8f;
                silversteelBias = 0.8f;
                stoneBias = 0.8f;
                clayBias = 0.8f;
                obsidianBias = 1.2f;
                crystalBias = 0.8f;
                nonNativeBias = 1.2f;
                treeBias = 0.5f;
                break;
        }

        List<List<Gametile>> Tiles = new List<List<Gametile>>();
        for (int x = 0; x < CHUNK_SIZE; x++)
        {
            Tiles.Add(new List<Gametile>());
            for (int y = 0; y < CHUNK_SIZE; y++)
            {
                float sqrDist = (new Vector2(x, y) - new Vector2(CHUNK_SIZE / 2, CHUNK_SIZE / 2)).sqrMagnitude;
                float w = 0;

                if (sqrDist <= Mathf.Pow(CHUNK_SIZE / 4, 2))
                    w = 1;
                else if (sqrDist <= Mathf.Pow(CHUNK_SIZE / 2, 2))
                    w = 1 - ((sqrDist - Mathf.Pow(CHUNK_SIZE / 4, 2)) / (Mathf.Pow(CHUNK_SIZE / 2, 2) - Mathf.Pow(CHUNK_SIZE / 4, 2)));

                Vector2 noisePos = new Vector2(Pos.x * CHUNK_SIZE + x, Pos.y * CHUNK_SIZE + y);
                float combinedRatio = Mathf.PerlinNoise((noisePos.x + NOISE_ORIGIN_D.x) * 0.8001f, (noisePos.y + NOISE_ORIGIN_D.y) * 0.8002f);
                Vector2 tilePos = noisePos;
                float tileThresh = 75 - 50 * (combinedRatio * Mathf.PerlinNoise((noisePos.x + NOISE_ORIGIN_A.x) * 0.8003f, (noisePos.y + NOISE_ORIGIN_A.y) * 0.8004f) + (1 - combinedRatio) * Mathf.PerlinNoise((noisePos.x + NOISE_ORIGIN_B.x) * 0.8005f, (noisePos.y + NOISE_ORIGIN_B.y) * 0.8006f));
                float tileLoss = 2 * (0.5f + combinedRatio * 1.5f * Mathf.PerlinNoise((noisePos.x + NOISE_ORIGIN_A.x) * 0.8007f, (noisePos.y + NOISE_ORIGIN_A.y) * 0.8008f) + (1 - combinedRatio) * Mathf.PerlinNoise((noisePos.x + NOISE_ORIGIN_C.x) * 0.8009f, (noisePos.y + NOISE_ORIGIN_C.y) * 0.801f));

                //determine tile element
                float[] elems = {
                    Mathf.PerlinNoise((noisePos.x + NOISE_ORIGIN_E.x) * .018011f, (noisePos.y + NOISE_ORIGIN_E.y) * .018012f),
                    0.8f * Mathf.PerlinNoise((noisePos.x + NOISE_ORIGIN_F.x) * .008013f, (noisePos.y + NOISE_ORIGIN_F.y) * .010014f) + 0.2f * Mathf.PerlinNoise((noisePos.x + NOISE_ORIGIN_F.x) * .100013f, (noisePos.y + NOISE_ORIGIN_F.y) * .100014f),
                    0.9f * Mathf.PerlinNoise((noisePos.x + NOISE_ORIGIN_G.x) * .008015f, (noisePos.y + NOISE_ORIGIN_G.y) * .010016f) + 0.1f * Mathf.PerlinNoise((noisePos.x + NOISE_ORIGIN_G.x) * .100015f, (noisePos.y + NOISE_ORIGIN_G.y) * .100016f),
                    0.8f * Mathf.PerlinNoise((noisePos.x + NOISE_ORIGIN_H.x) * .010017f, (noisePos.y + NOISE_ORIGIN_H.y) * .008018f) + 0.2f * Mathf.PerlinNoise((noisePos.x + NOISE_ORIGIN_H.x) * .100017f, (noisePos.y + NOISE_ORIGIN_H.y) * .100018f),
                    0.9f * Mathf.PerlinNoise((noisePos.x + NOISE_ORIGIN_I.x) * .010019f, (noisePos.y + NOISE_ORIGIN_I.y) * .008020f) + 0.1f * Mathf.PerlinNoise((noisePos.x + NOISE_ORIGIN_I.x) * .100019f, (noisePos.y + NOISE_ORIGIN_I.y) * .108020f)
                };

                elems[0] *= w * (plainBiomeBias - 1) + 1f;
                elems[1] *= w * (airBiomeBias - 1) + 1f;
                elems[2] *= w * (earthBiomeBias - 1) + 1f;
                elems[3] *= w * (fireBiomeBias - 1) + 1f;
                elems[4] *= w * (waterBiomeBias - 1) + 1f;

                int elem = 0;
                for(int i = 1; i < elems.Length; i++)
                {
                    elem = (elems[i] > elems[elem]) ? i : elem;
                }
                int runnerUpElem = elem % 4 + 1;
                for(int i = 1; i < 4; i++)
                {
                    int toCheck = (elem + i) % 4 + 1;
                    runnerUpElem = elems[toCheck] > elems[runnerUpElem] && toCheck != elem ? toCheck : runnerUpElem;
                }
                

                float[] subtypes =
                {
                    0.9f * Mathf.PerlinNoise((noisePos.x + NOISE_ORIGIN_R.x) * 0.07223f, (noisePos.y + NOISE_ORIGIN_R.y) * 0.07223f) + 0.2f * elems[0],
                    0.8f * Mathf.PerlinNoise((noisePos.x + NOISE_ORIGIN_S.x) * 0.04233f, (noisePos.y + NOISE_ORIGIN_S.y) * 0.04233f + 0.2f) + 0.2f * elems[1] + 0.2f * elems[4] - 0.1f * elems[2] - 0.1f * elems[3],
                    0.8f * Mathf.PerlinNoise((noisePos.x + NOISE_ORIGIN_T.x) * 0.18243f, (noisePos.y + NOISE_ORIGIN_T.y) * 0.18243f + 0.2f) + 0.2f * elems[2] + 0.2f * elems[3] - 0.1f * elems[1] - 0.1f * elems[4],
                    0.7f * Mathf.PerlinNoise((noisePos.x + NOISE_ORIGIN_U.x) * 0.16223f, (noisePos.y + NOISE_ORIGIN_U.y) * 0.16223f) + 0.2f * elems[1] + 0.1f * elems[4],
                    0.7f * Mathf.PerlinNoise((noisePos.x + NOISE_ORIGIN_V.x) * 0.08223f, (noisePos.y + NOISE_ORIGIN_V.y) * 0.08223f) + 0.2f * elems[4] + 0.1f * elems[2],
                    0.7f * Mathf.PerlinNoise((noisePos.x + NOISE_ORIGIN_W.x) * 0.16223f, (noisePos.y + NOISE_ORIGIN_W.y) * 0.16223f) + 0.2f * elems[2] + 0.1f * elems[3],
                    0.7f * Mathf.PerlinNoise((noisePos.x + NOISE_ORIGIN_X.x) * 0.08223f, (noisePos.y + NOISE_ORIGIN_X.y) * 0.08223f) + 0.2f * elems[3] + 0.1f * elems[1],
                };

                subtypes[0] *= w * (grassTerrainBias - 1) +1f;
                subtypes[1] *= w * (pitTerrainBias - 1) +1f;
                subtypes[2] *= w * (cliffTerrainBias - 1) +1f;
                subtypes[3] *= w * (sandTerrainBias - 1) +1f;
                subtypes[4] *= w * (dirtTerrainBias - 1) +1f;
                subtypes[5] *= w * (gravelTerrainBias - 1) +1f;
                subtypes[6] *= w * (smoothTerrainBias - 1) +1f;

                int subtype = 0;
                for (int i = 1; i < subtypes.Length; i++)
                {
                    subtype = (subtypes[i] > subtypes[subtype]) ? i : subtype;
                }

                //determine tile features
                string feat = null;

                if (subtype != 1 && subtype != 2)
                {
                    float[] feats =
                    {
                    Mathf.PerlinNoise(noisePos.x * .254321f + NOISE_ORIGIN_J.x, noisePos.y * .254321f + NOISE_ORIGIN_J.y),  //workstations    0
                    Mathf.PerlinNoise(noisePos.x * .222223f + NOISE_ORIGIN_K.x, noisePos.y * .222224f + NOISE_ORIGIN_K.y),  //flowers         1
                    Mathf.PerlinNoise(noisePos.x * .522225f + NOISE_ORIGIN_L.x, noisePos.y * .522226f + NOISE_ORIGIN_L.y),  //silversteel     2
                    Mathf.PerlinNoise(noisePos.x * .522227f + NOISE_ORIGIN_M.x, noisePos.y * .522228f + NOISE_ORIGIN_M.y),  //stone           3
                    Mathf.PerlinNoise(noisePos.x * .522228f + NOISE_ORIGIN_N.x, noisePos.y * .522232f + NOISE_ORIGIN_N.y),  //clay            4
                    Mathf.PerlinNoise(noisePos.x * .522231f + NOISE_ORIGIN_O.x, noisePos.y * .522232f + NOISE_ORIGIN_O.y),  //obsidian        5
                    Mathf.PerlinNoise(noisePos.x * .081033f + NOISE_ORIGIN_L.x, noisePos.y * .081034f + NOISE_ORIGIN_L.y),  //crystals        6
                    Mathf.PerlinNoise(noisePos.x * .720277f + NOISE_ORIGIN_Q.x, noisePos.y * .720277f + NOISE_ORIGIN_Q.y),  //non-native      7
                    Mathf.PerlinNoise(noisePos.x * .254321f + NOISE_ORIGIN_Z.x, noisePos.y * .254321f + NOISE_ORIGIN_Z.y)   //trees           8
                    };

                    feats[0] *= w * (workstationBias - 1) + 1f;
                    feats[1] *= w * (flowerBias - 1) + 1f;
                    feats[2] *= w * (silversteelBias - 1) + 1f;
                    feats[3] *= w * (stoneBias - 1) + 1f;
                    feats[4] *= w * (clayBias - 1) + 1f;
                    feats[5] *= w * (obsidianBias - 1) + 1f;
                    feats[6] *= w * (crystalBias - 1) + 1f;
                    feats[7] *= w * (nonNativeBias - 1) + 1f;
                    feats[8] *= w * (treeBias - 1) + 1f;

                    switch (subtype)
                    {
                        case 0: //Grass
                            int flowerType = elem == 0 ? runnerUpElem : elem;
                            if((elem != 0 && feats[1] > 0.6f) || feats[7] > 0.75f)
                            {
                                switch (flowerType)
                                {
                                    case 1:
                                        feat = "Flower White";
                                        break;

                                    case 2:
                                        feat = "Flower Yellow";
                                        break;

                                    case 3:
                                        feat = "Flower Red";
                                        break;

                                    case 4:
                                        feat = "Flower Purple";
                                        break;
                                }
                            }
                            else if(feats[8] > 0.8f)
                            {
                                subtype = 4;
                                switch (elem)
                                {
                                    case 0:
                                        feat = "Tree Plain";
                                        break;

                                    case 1:
                                        feat = "Tree Air";
                                        break;

                                    case 2:
                                        feat = "Tree Earth";
                                        break;

                                    case 3:
                                        feat = "Tree Fire";
                                        break;

                                    case 4:
                                        feat = "Tree Water";
                                        break;
                                }
                            }
                            break;

                        case 1: //Pit
                            break;

                        case 2: //Cliff
                            break;

                        case 3: //Sand
                            if (((elem == 0 || elem == 1 || elem == 4) && feats[2] > 0.7f - 0.05f * elems[1]) || feats[7] > 0.85f - 0.05f * elems[1])
                                feat = "Silversteel";
                            break;

                        case 4: //Dirt
                            if (((elem == 0 || elem == 2 || elem == 4) && feats[4] > 0.7f - 0.05f * elems[4]) || feats[7] > 0.85f - 0.05f * elems[4])
                                feat = "Clay";
                            else if (feats[8] > 0.75f)
                            {
                                switch (elem)
                                {
                                    case 0:
                                        feat = "Tree Plain";
                                        break;

                                    case 1:
                                        feat = "Tree Air";
                                        break;

                                    case 2:
                                        feat = "Tree Earth";
                                        break;

                                    case 3:
                                        feat = "Tree Fire";
                                        break;

                                    case 4:
                                        feat = "Tree Water";
                                        break;
                                }
                            }
                            break;

                        case 5: //Gravel
                            if (((elem == 0 || elem == 2 || elem == 3) && feats[3] > 0.7f - 0.05f * elems[2]) || feats[7] > 0.85f - 0.05f * elems[2])
                                feat = "Stone";
                            break;

                        case 6: //Smooth
                            if (((elem == 0 || elem == 1 || elem == 3) && feats[5] > 0.7f - 0.05f * elems[3]) || feats[7] > 0.85f - 0.05f * elems[3])
                                feat = "Obsidian";
                            break;
                    }

                    if (feat == null)
                    {
                        switch (elem)
                        {
                            case 1: //Air
                                if (feats[0] - Mathf.Abs(noisePos.x) % 5 / 20 >= 0.47f && feats[0] + Mathf.Abs(noisePos.y) % 5 / 20 <= 0.53f)
                                {
                                    if (type == ChunkType.Remnant && x > BrokenPylonClaimMap.Count / 2 && x < CHUNK_SIZE - (BrokenPylonClaimMap.Count / 2) && y > BrokenPylonClaimMap.Count / 2 && y < CHUNK_SIZE - (BrokenPylonClaimMap.Count / 2))
                                        feat = "Broken Pylon";
                                    else
                                        feat = (feats[0] >= 0.5f) ? "Nat Crafts" : "Nat Workbench";
                                }
                                else if (feats[6] > 1 || (feats[6] - Mathf.Abs(noisePos.x) % 5 / 40 >= 0.48f && feats[6] + Mathf.Abs(noisePos.y) % 5 / 40 <= 0.52f))
                                {
                                    feat = (feats[6] >= 0.5f || type == ChunkType.AirCrystal) ? "Crystal Air" : "Crystal Plain";
                                }
                                break;

                            case 2: //Earth
                                if (feats[0] - Mathf.Abs(noisePos.x) % 5 / 20 >= 0.47f && feats[0] + Mathf.Abs(noisePos.y) % 5 / 20 <= 0.53f)
                                {
                                    if (type == ChunkType.Remnant && x > BrokenPylonClaimMap.Count / 2 && x < CHUNK_SIZE - (BrokenPylonClaimMap.Count / 2) && y > BrokenPylonClaimMap.Count / 2 && y < CHUNK_SIZE - (BrokenPylonClaimMap.Count / 2))
                                        feat = "Broken Pylon";
                                    else
                                        feat = (feats[0] >= 0.5f) ? "Nat Inscriber" : "Nat Workbench";
                                }
                                else if (feats[6] > 1 || (feats[6] - Mathf.Abs(noisePos.x) % 5 / 40 >= 0.48f && feats[6] + Mathf.Abs(noisePos.y) % 5 / 40 <= 0.52f))
                                {
                                    feat = (feats[6] >= 0.5f || type == ChunkType.EarthCrystal) ? "Crystal Earth" : "Crystal Plain";
                                }
                                break;

                            case 3: //Fire
                                if (feats[0] - Mathf.Abs(noisePos.x) % 5 / 20 >= 0.47f && feats[0] + Mathf.Abs(noisePos.y) % 5 / 20 <= 0.53f)
                                {
                                    if (type == ChunkType.Remnant && x > BrokenPylonClaimMap.Count / 2 && x < CHUNK_SIZE - (BrokenPylonClaimMap.Count / 2) && y > BrokenPylonClaimMap.Count / 2 && y < CHUNK_SIZE - (BrokenPylonClaimMap.Count / 2))
                                        feat = "Broken Pylon";
                                    else
                                        feat = (feats[0] >= 0.5f) ? "Nat Kiln" : "Nat Workbench";
                                }
                                else if (feats[6] > 1 || (feats[6] - Mathf.Abs(noisePos.x) % 5 / 40 >= 0.48f && feats[6] + Mathf.Abs(noisePos.y) % 5 / 40 <= 0.52f))
                                {
                                    feat = (feats[6] >= 0.5f || type == ChunkType.FireCrystal) ? "Crystal Fire" : "Crystal Plain";
                                }
                                break;

                            case 4: //Water
                                if (feats[0] - Mathf.Abs(noisePos.x) % 5 / 20 >= 0.47f && feats[0] + Mathf.Abs(noisePos.y) % 5 / 20 <= 0.53f)
                                {
                                    if (type == ChunkType.Remnant && x > BrokenPylonClaimMap.Count / 2 && x < CHUNK_SIZE - (BrokenPylonClaimMap.Count / 2) && y > BrokenPylonClaimMap.Count / 2 && y < CHUNK_SIZE - (BrokenPylonClaimMap.Count / 2))
                                        feat = "Broken Pylon";
                                    else
                                        feat = (feats[0] >= 0.5f) ? "Nat Alchemy" : "Nat Workbench";
                                }
                                else if (feats[6] > 1 || (feats[6] - Mathf.Abs(noisePos.x) % 5 / 40 >= 0.48f && feats[6] + Mathf.Abs(noisePos.y) % 5 / 40 <= 0.52f))
                                {
                                    feat = (feats[6] >= 0.5f || type == ChunkType.WaterCrystal) ? "Crystal Water" : "Crystal Plain";
                                }
                                break;

                            default: //Plain
                                if (feats[0] - Mathf.Abs(noisePos.x) % 5 / 20 >= 0.47f && feats[0] + Mathf.Abs(noisePos.y) % 5 / 20 <= 0.53f)
                                {
                                    if (type == ChunkType.Remnant && x > BrokenPylonClaimMap.Count / 2 && x < CHUNK_SIZE - (BrokenPylonClaimMap.Count / 2) && y > BrokenPylonClaimMap.Count / 2 && y < CHUNK_SIZE - (BrokenPylonClaimMap.Count / 2))
                                        feat = "Broken Pylon";
                                    else
                                        feat = "Nat Workbench";
                                }
                                if (feat == null && feats[6] > 1 || (feats[6] - Mathf.Abs(noisePos.x) % 5 / 40 >= 0.48f && feats[6] + Mathf.Abs(noisePos.y) % 5 / 40 <= 0.52f))
                                {
                                    feat = "Crystal Plain";
                                }
                                break;
                        }
                    }
                }

                string bookText = null;

                if (type == ChunkType.Origin)
                {
                    if (x >= CHUNK_SIZE / 2 - 2 && x < CHUNK_SIZE / 2 + 2 && y >= CHUNK_SIZE / 2 - 2 && y < CHUNK_SIZE / 2 + 2)
                        feat = null;

                    if (x == CHUNK_SIZE / 2 && y == CHUNK_SIZE / 2)
                    {
                        feat = "Broken Pylon";
                    }
                    else if (x == CHUNK_SIZE / 2 + 1 && y == CHUNK_SIZE / 2)
                    {
                        feat = "Nat Workbench";
                    }
                    else if (x == CHUNK_SIZE / 2 + 2 && y == CHUNK_SIZE / 2)
                    {
                        feat = "Container";
                    }
                    else if (x == CHUNK_SIZE / 2 - 1 && y == CHUNK_SIZE / 2)
                    {
                        feat = "Book";
                        bookText = "The Void continues to push against our borders. If our pylons fail, everything we've built will crumble into nothing.";
                    }
                }
                else if (type == ChunkType.Remnant)
                {
                    Vector2Int wallCalc = new Vector2Int(Mod(x + 3 - CHUNK_SIZE / 2, 6), Mod(y + 3 - CHUNK_SIZE / 2, 6));
                    if (x < 3 || y < 3 || x + 3 >= CHUNK_SIZE || y + 3 >= CHUNK_SIZE)
                        wallCalc = new Vector2Int(-1, -1);
                    float tileCivility = Mathf.PerlinNoise((Pos.x * CHUNK_SIZE + x) * 0.6125f + NOISE_ORIGIN_Y.x, (Pos.y * CHUNK_SIZE + y) * 0.6125f + NOISE_ORIGIN_Y.y);
                    if (x == CHUNK_SIZE / 2 && y == CHUNK_SIZE / 2)
                    {
                        feat = "Broken Pylon";
                    }
                    else if (x == CHUNK_SIZE / 2 - 1 && y == CHUNK_SIZE / 2 && chunkCivility >= 0.875f)
                    {
                        feat = "Book";
                        if (chunkCivility >= 0.975f)
                            bookText = "I can see another group, stranded like us, across a massive gulf of Void. If only we had pylons or a golem, we could stabilize the area and reach each other.";
                        else if (chunkCivility >= 0.9625)
                            bookText = "I can hear some of them shouting. They think they're going to be saved. I know better. There's no rescue coming. This is the end.";
                        else if (chunkCivility >= 0.95f)
                            bookText = "It's oddly peaceful out here. The Void Orbs have gone for now, and everything is so still and serene.";
                        else if (chunkCivility >= 0.9375)
                            bookText = "I've seen natural formations out here that look similar to our artifical crafting stations. If we're going to rebuild, perhaps they could be useful to us.";
                        else if (chunkCivility >= 0.925f)
                            bookText = "There's a huge flaw in the project. I've been going over the calculations, and there can be no doubt. They think it will save us, and I'm stuck here, unable to tell them it won't!";
                        else if (chunkCivility >= 0.9125)
                            bookText = "Sometimes I can't help but stare deep into the Void. It's so enchanting... almost like it's calling to me.";
                        else if (chunkCivility >= 0.9f)
                            bookText = "These Void creatures are unlike anything I've ever seen before. Everything they touch vanishes. In large groups they even manage to destroy our pylons.";
                        else if (chunkCivility >= 0.8875)
                            bookText = "My project to turn fire crystals into defensive pylons is the perfect solution to this crisis. If I can get to an inscriber, I can save everyone.";
                        else
                            bookText = "It's been days since I last saw another person. I'm running out of supplies. Hopefully they've found a solution to this mess, and rescue will come soon.";
                    }
                    else if (x == CHUNK_SIZE / 2 + 1 && y == CHUNK_SIZE / 2 && tileCivility >= 0.66f)
                    {
                        feat = "Container";
                    }
                    else if (feat == null && ((wallCalc.x == 0 && wallCalc.y != 3) || (wallCalc.y == 0 && wallCalc.x != 3)) && !(subtype == 1 || subtype == 2) && tileCivility >= 0.33f)
                    {
                        feat = "Wall";
                    }
                    if (feat != null && (subtype == 1 || subtype == 2))
                        subtype = 0;
                }
                else if (type == ChunkType.FireCrystal)
                {
                    if (Pos.x <= 1 && Pos.x >= -1 && Pos.y <= 1 && Pos.y >= -1)
                    {
                        if (x == CHUNK_SIZE / 2 && y == CHUNK_SIZE / 2)
                        {
                            feat = "Broken Pylon";
                        }
                        else if (x == CHUNK_SIZE / 2 - 1 && y == CHUNK_SIZE / 2)
                        {
                            feat = "Book";
                            bookText = "These crystal fields remain secure for now, and will be vital for building kilns to replenish our depleted golem workforce.";
                        }
                    }

                    if (feat != null && (subtype == 1 || subtype == 2))
                        subtype = 0;
                }
                else if (type == ChunkType.EarthCrystal)
                {
                    if (Pos.x <= 1 && Pos.x >= -1 && Pos.y <= 1 && Pos.y >= -1)
                    {
                        if (x == CHUNK_SIZE / 2 && y == CHUNK_SIZE / 2)
                        {
                            feat = "Broken Pylon";
                        }
                        else if (x == CHUNK_SIZE / 2 - 1 && y == CHUNK_SIZE / 2)
                        {
                            feat = "Book";
                            bookText = "We've managed to keep the earth crystal fields stable. Relief should come soon - without these crystals, they won't be able to build incribers for replacing the destroyed pylons.";
                        }
                    }

                    if (feat != null && (subtype == 1 || subtype == 2))
                        subtype = 0;
                }

                Tiles[x].Add(new Gametile(tilePos, tileThresh, tileLoss, elem, subtype, CHUNK_SIZE));
                if(feat != null)
                {
                    if (feat != "Book")
                        Tiles[x][y].AddPlacedObject(feat);
                    else
                        Tiles[x][y].AddBook(bookText);
                    if (!Tiles[x][y].Contents.natural)
                    {
                        if(feat == "Broken Pylon")
                            brokenPylons.Enqueue(new Vector2(Pos.x * CHUNK_SIZE + x, Pos.y * CHUNK_SIZE + y));
                        if (feat == "Container" && containerItem != Item.Nothing)
                            Tiles[x][y].Contents.TryAddItem(containerItem);
                        if (feat == "Wall")
                            Tiles[x][y].ChangeStabIncome(WALL_STAB_ADD);
                        Tiles[x][y].SetStab(100);
                        Tiles[x][y].SoftUpdate();
                    }
                }
            }
        }
        //Debug.Log("Built Chunk ( " + Pos.x + ", " + Pos.y + ")");
        while (brokenPylons.Count > 0)
            MapGenPylons.Enqueue(brokenPylons.Dequeue());
        return new Chunk(Pos, Tiles);
    }

    void DetermineTotalChunkStats(Vector2 Pos, out float[] Types, out float[] Subtypes)
    {
        Types = new float[] { 0, 0, 0, 0, 0 };
        Subtypes = new float[] { 0, 0, 0, 0, 0, 0, 0 };

        for (int x = 0; x < CHUNK_SIZE; x++)
        {
            for (int y = 0; y < CHUNK_SIZE; y++)
            {
                Vector2 noisePos = new Vector2(Pos.x * CHUNK_SIZE + x, Pos.y * CHUNK_SIZE + y);

                //determine tile element
                float[] elems = {
                    Mathf.PerlinNoise((noisePos.x + NOISE_ORIGIN_E.x) * .018011f, (noisePos.y + NOISE_ORIGIN_E.y) * .018012f),
                    0.8f * Mathf.PerlinNoise((noisePos.x + NOISE_ORIGIN_F.x) * .008013f, (noisePos.y + NOISE_ORIGIN_F.y) * .010014f) + 0.2f * Mathf.PerlinNoise((noisePos.x + NOISE_ORIGIN_F.x) * .100013f, (noisePos.y + NOISE_ORIGIN_F.y) * .100014f),
                    0.9f * Mathf.PerlinNoise((noisePos.x + NOISE_ORIGIN_G.x) * .008015f, (noisePos.y + NOISE_ORIGIN_G.y) * .010016f) + 0.1f * Mathf.PerlinNoise((noisePos.x + NOISE_ORIGIN_G.x) * .100015f, (noisePos.y + NOISE_ORIGIN_G.y) * .100016f),
                    0.8f * Mathf.PerlinNoise((noisePos.x + NOISE_ORIGIN_H.x) * .010017f, (noisePos.y + NOISE_ORIGIN_H.y) * .008018f) + 0.2f * Mathf.PerlinNoise((noisePos.x + NOISE_ORIGIN_H.x) * .100017f, (noisePos.y + NOISE_ORIGIN_H.y) * .100018f),
                    0.9f * Mathf.PerlinNoise((noisePos.x + NOISE_ORIGIN_I.x) * .010019f, (noisePos.y + NOISE_ORIGIN_I.y) * .008020f) + 0.1f * Mathf.PerlinNoise((noisePos.x + NOISE_ORIGIN_I.x) * .100019f, (noisePos.y + NOISE_ORIGIN_I.y) * .108020f)
                };

                for (int i = 0; i < elems.Length; i++)
                {
                    Types[i] += elems[i];
                }

                float obsValue = (noisePos.magnitude < CHUNK_SIZE) ? Mathf.Clamp01(noisePos.magnitude / CHUNK_SIZE) : 1;

                float[] subtypes =
                {
                    0.9f * Mathf.PerlinNoise((noisePos.x + NOISE_ORIGIN_R.x) * 0.07223f, (noisePos.y + NOISE_ORIGIN_R.y) * 0.07223f) + 0.2f * elems[0],
                    0.9f * obsValue * (0.8f * Mathf.PerlinNoise((noisePos.x + NOISE_ORIGIN_S.x) * 0.04233f, (noisePos.y + NOISE_ORIGIN_S.y) * 0.04233f + 0.2f) + 0.2f * elems[1] + 0.2f * elems[4] - 0.1f * elems[2] - 0.1f * elems[3]),
                    0.9f * obsValue * (0.8f * Mathf.PerlinNoise((noisePos.x + NOISE_ORIGIN_T.x) * 0.18243f, (noisePos.y + NOISE_ORIGIN_T.y) * 0.18243f + 0.2f) + 0.2f * elems[2] + 0.2f * elems[3] - 0.1f * elems[1] - 0.1f * elems[4]),
                    0.7f * Mathf.PerlinNoise((noisePos.x + NOISE_ORIGIN_U.x) * 0.16223f, (noisePos.y + NOISE_ORIGIN_U.y) * 0.16223f) + 0.2f * elems[1] + 0.1f * elems[4],
                    0.7f * Mathf.PerlinNoise((noisePos.x + NOISE_ORIGIN_V.x) * 0.08223f, (noisePos.y + NOISE_ORIGIN_V.y) * 0.08223f) + 0.2f * elems[4] + 0.1f * elems[2],
                    0.7f * Mathf.PerlinNoise((noisePos.x + NOISE_ORIGIN_W.x) * 0.16223f, (noisePos.y + NOISE_ORIGIN_W.y) * 0.16223f) + 0.2f * elems[2] + 0.1f * elems[3],
                    0.7f * Mathf.PerlinNoise((noisePos.x + NOISE_ORIGIN_X.x) * 0.08223f, (noisePos.y + NOISE_ORIGIN_X.y) * 0.08223f) + 0.2f * elems[3] + 0.1f * elems[1],
                };
                
                for (int i = 0; i < subtypes.Length; i++)
                {
                    Subtypes[i] += subtypes[i];
                }
            }
        }
    }

    List<List<float>> BuildClaimMap(int innerR, int outerR, float max)
    {
        List<List<float>> ClaimMap = new List<List<float>>();

        for(int x = 0; x < 2 * outerR + 1; x++)
        {
            ClaimMap.Add(new List<float>());
            for(int y = 0; y < 2 * outerR + 1; y++)
            {
                float value = 0;
                float sqrDistFromCenter = (x - outerR) * (x - outerR) + (y - outerR) * (y - outerR);
                if (sqrDistFromCenter <= outerR * outerR)
                {
                    if(sqrDistFromCenter <= innerR * innerR)
                    {
                        value = max;
                    }
                    else
                    {
                        value = Mathf.Lerp(max, 0, (Mathf.Sqrt(sqrDistFromCenter) - innerR) / (outerR - innerR));
                    }
                }

                ClaimMap[x].Add(value);
            }
        }

        return ClaimMap;
    }

    Chunk FindChunk(Vector2 Pos)
    {
        Vector2 listOffset = Pos - ChunkListStart;


        if(listOffset.x < 0 || listOffset.y < 0 || listOffset.x >= GenChunks.Count || listOffset.y >= GenChunks[0].Count)
        {
            Vector2 expandVector = listOffset;

            if(expandVector.x > 0)
            {
                expandVector.x -= (GenChunks.Count - 1);
            }
            if(expandVector.y > 0)
            {
                expandVector.y -= (GenChunks[0].Count - 1);
            }
            //Debug.Log("POS: (" + Pos.x + ", " + Pos.y + ") CLS: (" + ChunkListStart.x + ", " + ChunkListStart.y + ") LO: (" + listOffset.x + ", " + listOffset.y + ")");
            //Debug.Log("XSize: " + GenChunks.Count + "YSize: " + GenChunks[0].Count);
            ResizeGenChunks(expandVector);

            listOffset = Pos - ChunkListStart;
        }

        return GenChunks[Mathf.FloorToInt(listOffset.x)][Mathf.FloorToInt(listOffset.y)];
    }

    public Gametile FindTile(Vector2 Pos)
    {
        //Debug.Log("POS: (" + Pos.x + ", " + Pos.y + ") FCV: (" + Mathf.Floor(Pos.x / CHUNK_SIZE) + ", " + Mathf.Floor(Pos.y / CHUNK_SIZE) + ")");
        return FindChunk(new Vector2(Mathf.Floor(Pos.x/CHUNK_SIZE), Mathf.Floor(Pos.y/CHUNK_SIZE))).FindTile(new Vector2(Mod(Mathf.FloorToInt(Pos.x), CHUNK_SIZE), Mod(Mathf.FloorToInt(Pos.y), CHUNK_SIZE)));
    }

    public Gametile FindTile(Vector2Int Pos)
    {
        return FindTile(new Vector2(Pos.x, Pos.y));
    }

    void DisplayStabAddition(Gametile targetTile, Item pylon)
    {
        Vector2 pos = targetTile.WorldPos;
        float x = pos.x;
        float y = pos.y;
        int claimX = 0;
        int claimY = 0;
        if (pylon == Item.Pylon || pylon == Item.PylonAir || pylon == Item.PylonFire || pylon==Item.PylonWater)
        {
            foreach (List<float> claimCol in PylonClaimMap)
            {
                claimY = 0;
                foreach (float element in claimCol)
                {
                    Gametile target = FindTile(new Vector2(Mathf.Floor(x + claimX - (claimCol.Count / 2)), Mathf.Floor(y + claimY - (claimCol.Count / 2))));

                    target.DisplayStabChangeResult(element);
                    claimY++;
                }
                claimX++;
            }
        }
        else if (pylon == Item.PylonEarth)
        {
            foreach (List<float> claimCol in BigPylonClaimMap)
            {
                claimY = 0;
                foreach (float element in claimCol)
                {
                    Gametile target = FindTile(new Vector2(Mathf.Floor(x + claimX - (claimCol.Count / 2)), Mathf.Floor(y + claimY - (claimCol.Count / 2))));

                    target.DisplayStabChangeResult(element);
                    claimY++;
                }
                claimX++;
            }
        }
    }

    bool DisplayStabRemoval(PlacedObject pylon)
    {
        bool vulnerable = false;
        Vector2 pos = pylon.Pos;
        int POID = pylon.ID;
        float x = pos.x;
        float y = pos.y;
        int claimX = 0;
        int claimY = 0;
        if (POID == 0 || POID == 25 || POID == 27 || POID == 28)
        {
            foreach (List<float> claimCol in PylonClaimMap)
            {
                claimY = 0;
                foreach (float element in claimCol)
                {
                    Gametile target = FindTile(new Vector2(Mathf.Floor(x + claimX - (claimCol.Count / 2)), Mathf.Floor(y + claimY - (claimCol.Count / 2))));

                    if (target.DisplayStabChangeResult(-element) && (claimX != claimCol.Count/2 || claimY != claimCol.Count/2))
                        vulnerable = true;
                    claimY++;
                }
                claimX++;
            }
        }
        else if (POID == 26)
        {
            foreach (List<float> claimCol in BigPylonClaimMap)
            {
                claimY = 0;
                foreach (float element in claimCol)
                {
                    Gametile target = FindTile(new Vector2(Mathf.Floor(x + claimX - (claimCol.Count / 2)), Mathf.Floor(y + claimY - (claimCol.Count / 2))));

                    if (target.DisplayStabChangeResult(-element) && (claimX != claimCol.Count / 2 || claimY != claimCol.Count / 2))
                        vulnerable = true;
                    claimY++;
                }
                claimX++;
            }
        }
        else if (POID == 32)
        {
            foreach (List<float> claimCol in BrokenPylonClaimMap)
            {
                claimY = 0;
                foreach (float element in claimCol)
                {
                    Gametile target = FindTile(new Vector2(Mathf.Floor(x + claimX - (claimCol.Count / 2)), Mathf.Floor(y + claimY - (claimCol.Count / 2))));

                    if (target.DisplayStabChangeResult(-element) && (claimX != claimCol.Count / 2 || claimY != claimCol.Count / 2))
                        vulnerable = true;
                    claimY++;
                }
                claimX++;
            }
        }
        return vulnerable;
    }

    void AddPylon(Vector2 pos, int type = 0)
    {
        Dictionary<int, LinkedList<int>> TargetDict = PylonCoords;
        List<List<float>> TargetClaimMap = PylonClaimMap;

        switch (type)
        {
            case 1:
                TargetDict = AirPylonCoords;
                break;

            case 2:
                TargetDict = EarthPylonCoords;
                TargetClaimMap = BigPylonClaimMap;
                break;

            case 3:
                TargetDict = FirePylonCoords;
                break;

            case 4:
                TargetDict = WaterPylonCoords;
                break;

            case 5:
                TargetDict = BrokenPylonCoords;
                TargetClaimMap = BrokenPylonClaimMap;
                break;
        }

        if (!TargetDict.ContainsKey(Mathf.FloorToInt(pos.x)))
        {
            TargetDict.Add(Mathf.FloorToInt(pos.x), new LinkedList<int>());
        }
        TargetDict[Mathf.FloorToInt(pos.x)].AddLast(Mathf.FloorToInt(pos.y));
        float x = pos.x;
        float y = pos.y;
        int claimX = 0;
        int claimY = 0;
        foreach (List<float> claimCol in TargetClaimMap)
        {
            claimY = 0;
            foreach (float element in claimCol)
            {
                Gametile target = FindTile(new Vector2(Mathf.Floor(x + claimX - (claimCol.Count / 2)), Mathf.Floor(y + claimY - (claimCol.Count / 2))));

                target.ChangeStabIncome(element);
                claimY++;
            }
            claimX++;
        }
    }

    void RemovePylon(Vector2 Pos)
    {
        if (PylonCoords.ContainsKey(Mathf.FloorToInt(Pos.x)))
        {
            if (PylonCoords[Mathf.FloorToInt(Pos.x)].Contains(Mathf.FloorToInt(Pos.y)))
            {
                PylonCoords[Mathf.FloorToInt(Pos.x)].Remove(Mathf.FloorToInt(Pos.y));
                if (PylonCoords[Mathf.FloorToInt(Pos.x)].Count == 0)
                {
                    PylonCoords.Remove(Mathf.FloorToInt(Pos.x));
                }
                float x = Pos.x;
                float y = Pos.y;
                int claimX = 0;
                int claimY = 0;
                foreach (List<float> claimCol in PylonClaimMap)
                {
                    claimY = 0;
                    foreach (float element in claimCol)
                    {
                        Gametile target = FindTile(new Vector2(Mathf.Floor(x + claimX - (claimCol.Count / 2)), Mathf.Floor(y + claimY - (claimCol.Count / 2))));

                        target.ChangeStabIncome(-element);
                        claimY++;
                    }
                    claimX++;
                }
                return;
            }
        }

        if (AirPylonCoords.ContainsKey(Mathf.FloorToInt(Pos.x)))
        {
            if (AirPylonCoords[Mathf.FloorToInt(Pos.x)].Contains(Mathf.FloorToInt(Pos.y)))
            {
                AirPylonCoords[Mathf.FloorToInt(Pos.x)].Remove(Mathf.FloorToInt(Pos.y));
                if (AirPylonCoords[Mathf.FloorToInt(Pos.x)].Count == 0)
                {
                    AirPylonCoords.Remove(Mathf.FloorToInt(Pos.x));
                }
                float x = Pos.x;
                float y = Pos.y;
                int claimX = 0;
                int claimY = 0;
                foreach (List<float> claimCol in PylonClaimMap)
                {
                    claimY = 0;
                    foreach (float element in claimCol)
                    {
                        Gametile target = FindTile(new Vector2(Mathf.Floor(x + claimX - (claimCol.Count / 2)), Mathf.Floor(y + claimY - (claimCol.Count / 2))));

                        target.ChangeStabIncome(-element);
                        claimY++;
                    }
                    claimX++;
                }
                return;
            }
        }

        if (EarthPylonCoords.ContainsKey(Mathf.FloorToInt(Pos.x)))
        {
            if (EarthPylonCoords[Mathf.FloorToInt(Pos.x)].Contains(Mathf.FloorToInt(Pos.y)))
            {
                EarthPylonCoords[Mathf.FloorToInt(Pos.x)].Remove(Mathf.FloorToInt(Pos.y));
                if (EarthPylonCoords[Mathf.FloorToInt(Pos.x)].Count == 0)
                {
                    EarthPylonCoords.Remove(Mathf.FloorToInt(Pos.x));
                }
                float x = Pos.x;
                float y = Pos.y;
                int claimX = 0;
                int claimY = 0;
                foreach (List<float> claimCol in BigPylonClaimMap)
                {
                    claimY = 0;
                    foreach (float element in claimCol)
                    {
                        Gametile target = FindTile(new Vector2(Mathf.Floor(x + claimX - (claimCol.Count / 2)), Mathf.Floor(y + claimY - (claimCol.Count / 2))));

                        target.ChangeStabIncome(-element);
                        claimY++;
                    }
                    claimX++;
                }
                return;
            }
        }

        if (FirePylonCoords.ContainsKey(Mathf.FloorToInt(Pos.x)))
        {
            if (FirePylonCoords[Mathf.FloorToInt(Pos.x)].Contains(Mathf.FloorToInt(Pos.y)))
            {
                FirePylonCoords[Mathf.FloorToInt(Pos.x)].Remove(Mathf.FloorToInt(Pos.y));
                if (FirePylonCoords[Mathf.FloorToInt(Pos.x)].Count == 0)
                {
                    FirePylonCoords.Remove(Mathf.FloorToInt(Pos.x));
                }
                float x = Pos.x;
                float y = Pos.y;
                int claimX = 0;
                int claimY = 0;
                foreach (List<float> claimCol in PylonClaimMap)
                {
                    claimY = 0;
                    foreach (float element in claimCol)
                    {
                        Gametile target = FindTile(new Vector2(Mathf.Floor(x + claimX - (claimCol.Count / 2)), Mathf.Floor(y + claimY - (claimCol.Count / 2))));

                        target.ChangeStabIncome(-element);
                        claimY++;
                    }
                    claimX++;
                }
                return;
            }
        }

        if (WaterPylonCoords.ContainsKey(Mathf.FloorToInt(Pos.x)))
        {
            if (WaterPylonCoords[Mathf.FloorToInt(Pos.x)].Contains(Mathf.FloorToInt(Pos.y)))
            {
                WaterPylonCoords[Mathf.FloorToInt(Pos.x)].Remove(Mathf.FloorToInt(Pos.y));
                if (WaterPylonCoords[Mathf.FloorToInt(Pos.x)].Count == 0)
                {
                    WaterPylonCoords.Remove(Mathf.FloorToInt(Pos.x));
                }
                float x = Pos.x;
                float y = Pos.y;
                int claimX = 0;
                int claimY = 0;
                foreach (List<float> claimCol in PylonClaimMap)
                {
                    claimY = 0;
                    foreach (float element in claimCol)
                    {
                        Gametile target = FindTile(new Vector2(Mathf.Floor(x + claimX - (claimCol.Count / 2)), Mathf.Floor(y + claimY - (claimCol.Count / 2))));

                        target.ChangeStabIncome(-element);
                        claimY++;
                    }
                    claimX++;
                }
                return;
            }
        }

        if (BrokenPylonCoords.ContainsKey(Mathf.FloorToInt(Pos.x)))
        {
            if (BrokenPylonCoords[Mathf.FloorToInt(Pos.x)].Contains(Mathf.FloorToInt(Pos.y)))
            {
                BrokenPylonCoords[Mathf.FloorToInt(Pos.x)].Remove(Mathf.FloorToInt(Pos.y));
                if(BrokenPylonCoords[Mathf.FloorToInt(Pos.x)].Count == 0)
                {
                    BrokenPylonCoords.Remove(Mathf.FloorToInt(Pos.x));
                }
                float x = Pos.x;
                float y = Pos.y;
                int claimX = 0;
                int claimY = 0;
                foreach (List<float> claimCol in BrokenPylonClaimMap)
                {
                    claimY = 0;
                    foreach (float element in claimCol)
                    {
                        Gametile target = FindTile(new Vector2(Mathf.Floor(x + claimX - (claimCol.Count / 2)), Mathf.Floor(y + claimY - (claimCol.Count / 2))));

                        target.ChangeStabIncome(-element);
                        claimY++;
                    }
                    claimX++;
                }
                return;
            }
        }
    }

    public void AddMinion(Vector2 pos, MinionType type, PlacedObject hit = null)
    {
        MinionController newMinion = Instantiate(minionPrefab, pos, Quaternion.identity).GetComponent<MinionController>();
        newMinion.Type = type;
        newMinion.MapListI = Minions.Count;
        Minions.Add(newMinion);
        newMinion.GiveTask(hit);
    }

    void RemoveMinion(MinionController minion)
    {
        if (minion.MapListI != -1)
        {
            Minions[Minions.Count - 1].MapListI = minion.MapListI;
            Minions[minion.MapListI] = Minions[Minions.Count - 1];
            Minions.RemoveAt(Minions.Count - 1);
        }
        Destroy(minion.gameObject);
    }

    public void AddEnemy(Vector2 pos, EnemyType type)
    {
        Enemy newEnemy = Instantiate(EnemyPrefabs[(int)type], pos, Quaternion.identity).GetComponent<Enemy>();
        newEnemy.MapListI = Enemies.Count;
        Enemies.Add(newEnemy);
    }

    public void RemoveEnemy(Enemy enemy)
    {
        if(enemy.MapListI != -1)
        {
            Enemies[Enemies.Count - 1].MapListI = enemy.MapListI;
            Enemies[enemy.MapListI] = Enemies[Enemies.Count - 1];
            Enemies.RemoveAt(Enemies.Count - 1);
        }
        Destroy(enemy.gameObject);
    }

    public bool TryHarvest(MinionController minion, Vector2 target)
    {
        if(((Vector2)minion.transform.position - target).magnitude > 1)
        {
            return false;
        }

        return FindTile(new Vector2(Mathf.Floor(target.x + 0.5f), Mathf.Floor(target.y + 0.5f))).HarvestPlacedObject(out _, minion.specialGathering);
    }

    public void ReportMissingPylon(int ID)
    {
        Vector2 RemoveCoords = Vector2.zero;
        bool found = false;
        switch (ID)
        {
            case 0:
                foreach (KeyValuePair<int, LinkedList<int>> column in PylonCoords)
                {
                    if (found)
                        break;
                    int x = column.Key;
                    foreach (int y in column.Value)
                        if (!FindTile(new Vector2(x, y)).Stable)
                        {
                            RemoveCoords = new Vector2(x, y);
                            found = true;
                            break;
                        }
                }
                RemovePylon(RemoveCoords);
                break;

            case 25:
                foreach (KeyValuePair<int, LinkedList<int>> column in AirPylonCoords)
                {
                    if (found)
                        break;
                    int x = column.Key;
                    foreach (int y in column.Value)
                        if (!FindTile(new Vector2(x, y)).Stable)
                        {
                            RemoveCoords = new Vector2(x, y);
                            found = true;
                            break;
                        }
                }
                RemovePylon(RemoveCoords);
                break;

            case 26:
                foreach (KeyValuePair<int, LinkedList<int>> column in EarthPylonCoords)
                {
                    if (found)
                        break;
                    int x = column.Key;
                    foreach (int y in column.Value)
                        if (!FindTile(new Vector2(x, y)).Stable)
                        {
                            RemoveCoords = new Vector2(x, y);
                            found = true;
                            break;
                        }
                }
                RemovePylon(RemoveCoords);
                break;

            case 27:
                foreach (KeyValuePair<int, LinkedList<int>> column in FirePylonCoords)
                {
                    if (found)
                        break;
                    int x = column.Key;
                    foreach (int y in column.Value)
                        if (!FindTile(new Vector2(x, y)).Stable)
                        {
                            RemoveCoords = new Vector2(x, y);
                            found = true;
                            break;
                        }
                }
                RemovePylon(RemoveCoords);
                break;

            case 28:
                foreach (KeyValuePair<int, LinkedList<int>> column in WaterPylonCoords)
                {
                    if (found)
                        break;
                    int x = column.Key;
                    foreach (int y in column.Value)
                        if (!FindTile(new Vector2(x, y)).Stable)
                        {
                            RemoveCoords = new Vector2(x, y);
                            found = true;
                            break;
                        }
                }
                RemovePylon(RemoveCoords);
                break;

            case 32:
                foreach(KeyValuePair<int, LinkedList<int>> column in BrokenPylonCoords)
                {
                    if (found)
                        break;
                    int x = column.Key;
                    foreach(int y in column.Value)
                        if(!FindTile(new Vector2(x, y)).Stable)
                        {
                            RemoveCoords = new Vector2(x, y);
                            found = true;
                            break;
                        }
                }
                RemovePylon(RemoveCoords);
                break;
        }
    }

    public void ToggleEnemyPause()
    {
        foreach (Enemy enemy in Enemies)
            enemy.TogglePause();
    }

    int Mod(int A, int B)
    {
        A %= B;
        if(A < 0)
        {
            A += B;
        }
        return A;
    }

    public void EndCrafting()
    {
        player.GetComponent<characterControl>().StopInspecting();
        CraftMenu.Hide();
    }

    public void PreviousRecipe()
    {
        player.GetComponent<characterControl>().Inspecting.CycleRecipe(true);
        CraftMenu.UpdateRecipe(player.GetComponent<characterControl>().Inspecting.ActiveRecipe);
    }

    public void NextRecipe()
    {
        player.GetComponent<characterControl>().Inspecting.CycleRecipe();
        CraftMenu.UpdateRecipe(player.GetComponent<characterControl>().Inspecting.ActiveRecipe);
    }

    public void ShowTooltip(PlacedObject toShow, string append = "")
    {
        ShowTooltip(toShow.GetTooltip() + append);
    }

    public void ShowTooltip(string toShow)
    {
        if (ShowTooltips)
        {
            TTBackdrop.gameObject.SetActive(true);
            Tooltip.text = toShow;
            TTBackdrop.offsetMax = new Vector2(Input.mousePosition.x + 352, Input.mousePosition.y - 16);
            TTBackdrop.offsetMin = new Vector2(Input.mousePosition.x + 16, Input.mousePosition.y - Tooltip.preferredHeight - 32);
        }
    }

    public void ToggleTooltipEnabled()
    {
        ShowTooltips = !ShowTooltips;
    }
}
