using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gametile
{

    public Vector2 WorldPos;
    private Vector2 LocalPos;
    public GameObject TileSprite;
    public PlacedObject Contents { get; private set; }
    public bool Explored { get; private set; } = false;
    public bool Real = true;

    public readonly int TYPE;
    public readonly int SUBTYPE;
    private float STAB_THRESH;
    public readonly float STAB_LOSS;
    private int CHUNK_SIZE;

    private float Stab = 0;
    public float Stab_Inc { get; private set; } = 0;
    private float ProductionCooldown = 0;
    private Queue<float> Stab_Burst = new Queue<float>();

    public bool Stable
    {
        get
        {
            return Stab >= STAB_THRESH;
        }
    }

    public bool Destroys
    {
        get
        {
            return !Stable;
        }
    }

    public bool Traversable
    {
        get
        {
            return (Contents != null && !Contents.collisions) || (Contents == null && (SUBTYPE != 1 && SUBTYPE != 2));
        }
    }
    
    public bool CanPlaceObject(string name = "")
    {
        return Contents == null && SUBTYPE != 2 && (SUBTYPE != 1 ^ name == "Bridge");
    }

    public Gametile(Vector2 newPos, float Threshold, float Loss, int type, int subtype, int ChunkSize, bool real = true)
    {
        TYPE = type;
        SUBTYPE = subtype;
        STAB_THRESH = Threshold;
        STAB_LOSS = Loss;
        WorldPos = newPos;
        LocalPos = new Vector2(Mathf.FloorToInt(WorldPos.y) % ChunkSize, Mathf.FloorToInt(WorldPos.y) % ChunkSize);
        CHUNK_SIZE = ChunkSize;
        Contents = null;
    }

    public void AddStab(float value)
    {
        Stab_Burst.Enqueue(value);
    }

    public bool SoftUpdate()
    {
        bool stableStart = Stable;
        float stabStartVal = Stab;

        Stab += (Stab_Inc - STAB_LOSS) * Worldmap.Main.TICK_LENGTH;
        while(Stab_Burst.Count > 0)
        {
            Stab += Stab_Burst.Dequeue() * Worldmap.Main.TICK_LENGTH;
        }
        Stab = Mathf.Clamp(Stab, 0, 100);

        if(stableStart != Stable)
        {
            if (Stable)
            {
                Materialize();
            }
            else
            {
                if (TileSprite != null && Random.Range(0, 250) == 0 && Real && (WorldPos.x < 0 || WorldPos.x >= CHUNK_SIZE) && (WorldPos.y < 0 || WorldPos.y >= CHUNK_SIZE))
                {
                    Worldmap.Main.AddEnemy(TileSprite.transform.position, EnemyType.VoidOrb);
                }
                Dematerialize();
            }
        }

        if(Stable && Contents != null)
        {
            switch (Contents.ID)
            {
                case 29:
                    CondenseCrystal();
                    break;

                case 30:
                    DigUpMaterial();
                    break;

                case 31:
                    GrowFlower();
                    break;
            }
        }

        return !(stabStartVal == 0 && stabStartVal == Stab);
    }

    public void HardUpdate()
    {
        if (TileSprite != null)
        {
            if (TileSprite.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Hidden") && !Stable)
            {
                TileSprite.GetComponent<Animator>().enabled = false;
                TileSprite.GetComponent<SpriteRenderer>().sprite = TileSprite.GetComponent<TileAssistor>().TILE_SPRITES[0];
                if(Real)
                    TileSprite.GetComponent<Collider2D>().enabled = true;
            }
        }
        Contents?.HardUpdate();
    }

    public void Materialize()
    {
        if (TileSprite != null)
        {
            TileSprite.GetComponent<Animator>().enabled = true;
            TileSprite.GetComponent<Animator>().SetBool("Active", true);

            if((SUBTYPE != 1 && SUBTYPE != 2) || !Real)
                TileSprite.GetComponent<Collider2D>().enabled = false;
            if (Contents != null)
            {
                if (Contents.ID == 41)
                    TileSprite.GetComponent<Collider2D>().enabled = false;
                Contents.Show();
            }
        }
        Explored = true;
    }

    public void Dematerialize()
    {
        if (TileSprite != null)
        {
            TileSprite.GetComponent<Animator>().enabled = true;
            TileSprite.GetComponent<Animator>().SetBool("Active", false);
            if(Contents != null)
            {
                Contents.Hide();
                if (!Stable)
                {
                    Contents.RemoveSubscribers();
                    if (!Contents.natural)
                    {
                        if(Contents.AddsStability)
                            Worldmap.Main.ReportMissingPylon(Contents.ID);
                        if (Contents.ConnectsToNeighbors)
                            Contents.RemoveFromNeighborLists();
                        if (Contents.ID == 39)
                            ChangeStabIncome(-Worldmap.Main.WALL_STAB_ADD);
                        Contents.FreeObject();
                        Contents = null;
                    }
                }
            }
        }
    }

    public void GiveObject(GameObject newObject)
    {
        TileSprite = newObject;
        TileSprite.GetComponent<TileAssistor>().tile = this;
        TileSprite.GetComponent<Animator>().SetInteger("Type", TYPE);
        TileSprite.GetComponent<Animator>().SetInteger("Subtype", SUBTYPE);
        TileSprite.GetComponent<Animator>().SetBool("Active", Stable);
        TileSprite.GetComponent<Animator>().Play("Base Layer.Hidden");
        TileSprite.transform.position = WorldPos;
        if (Stable)
        {
            Materialize();
        }
        else
        {
            Dematerialize();
        }
        if(Contents != null)
        {
            Contents.GiveObject(TileSprite.transform.GetChild(0).gameObject);
            if (Stable)
            {
                Contents.Show();
            }
            else
            {
                Contents.Hide();
            }
        }
        TileSprite.GetComponent<Animator>().enabled = Stable || !TileSprite.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Hidden");
        if (!Real)
        {
            TileSprite.GetComponent<Collider2D>().enabled = false;
        }
    }

    public void FreeObject()
    {
        if (Contents != null)
        {
            Contents.FreeObject();
        }
        Dematerialize();
        TileSprite.GetComponent<Animator>().SetBool("Active", false);
        TileSprite.GetComponent<TileAssistor>().tile = null;
        TileSprite = null;
    }

    public bool AddPlacedObject(string name)
    {
        if(CanPlaceObject(name))
        {
            Contents = PlacedObject.MakeObject(name);
            Contents.Pos = WorldPos;
            Contents.Produces = DetermineProducedItem(Contents.ID);
            if (TileSprite != null)
            {
                if(name == "Bridge")
                    TileSprite.GetComponent<Collider2D>().enabled = false;
                Contents.GiveObject(TileSprite.transform.GetChild(0).gameObject);
                if (Stable)
                {
                    Contents.Show();
                }
            }
            return true;
        }

        return false;
    }

    public bool AddBook(string text)
    {
        if(Contents == null && !(SUBTYPE == 1 || SUBTYPE == 2))
        {
            Contents = PlacedObject.MakeBook(text);
            Contents.Pos = WorldPos;
            Contents.Produces = DetermineProducedItem(Contents.ID);
            if (TileSprite != null)
            {
                Contents.GiveObject(TileSprite.transform.GetChild(0).gameObject);
                if (Stable)
                {
                    Contents.Show();
                }
            }
            return true;
        }

        return false;
    }

    public bool HarvestPlacedObject(out ItemObject produced, Item specialGather = Item.Nothing)
    {
        produced = null;
        if(Stable && Contents != null)
        {
            if (Contents.HarvestItem(out produced, specialGather))
            {
                RemovePlacedObject();
                return true;
            }
        }
        return false;
    }

    public void RemovePlacedObject()
    {
        if(Contents != null)
        {
            if(Contents.ID == 41)
                TileSprite.GetComponent<Collider2D>().enabled = true;
            Contents.EjectContents();
            Contents.FreeObject();
        }
        Contents = null;
        ProductionCooldown = 0;
    }

    public void PrintTileDebugInfo()
    {
        Debug.Log("X: " + LocalPos.x + ", Y: " + LocalPos.y + ", Type: " + TYPE + ", Subtype: " + SUBTYPE + ", Stability: " + Stab + ", Threshold: " + STAB_THRESH + ", Delta: " + STAB_LOSS + ", Animator Active: " + TileSprite.GetComponent<Animator>().isActiveAndEnabled);
    }

    public void SetStab(float setTo)
    {
        bool stableStart = Stable;
        float stabStartVal = Stab;

        Stab = setTo;
        Stab = Mathf.Clamp(Stab, 0, 100);

        if (stableStart != Stable)
        {
            if (Stable)
            {
                Materialize();
            }
            else
            {
                Dematerialize();
            }
        }
    }

    public void ChangeStabIncome(float change)
    {
        Stab_Inc += change;
    }

    public bool DisplayStabChangeResult(float change)
    {
        bool vulnerable = false;
        if (TileSprite != null)
        {
            float oldDeltaStab = Stab_Inc - STAB_LOSS;
            float newDeltaStab = Stab_Inc - STAB_LOSS + change;

            if (oldDeltaStab >= 0 && (newDeltaStab < 0 || (!Stable && newDeltaStab == 0)))
            {
                TileSprite.GetComponent<TileAssistor>().FlashRed();
                if (Contents != null && !Contents.natural)
                    vulnerable = true;
            }
            else if (oldDeltaStab <= 0 && (newDeltaStab > 0 || (Stable && newDeltaStab == 0)))
                TileSprite.GetComponent<TileAssistor>().FlashGreen();
        }
        return vulnerable;
    }

    private Item DetermineProducedItem(int POID)
    {
        Item produced = Item.Nothing;
        if(POID == 29)
        {   //Crystal Condenser
            switch (TYPE)
            {
                case 0:
                    produced = Item.CrystalPlain;
                    break;

                case 1:
                    produced = Item.CrystalAir;
                    break;

                case 2:
                    produced = Item.CrystalEarth;
                    break;

                case 3:
                    produced = Item.CrystalFire;
                    break;

                case 4:
                    produced = Item.CrystalWater;
                    break;
            }
        }
        else if(POID == 30)
        {   //Arcane Drill
            switch (SUBTYPE)
            {
                case 0: //Grass
                    switch (Random.Range(0, 4))
                    {
                        case 0:
                            produced = Item.Silversteel;
                            break;

                        case 1:
                            produced = Item.Clay;
                            break;

                        case 2:
                            produced = Item.Stone;
                            break;

                        case 3:
                            produced = Item.Obsidian;
                            break;
                    }
                    break;

                case 3: //Sand
                    produced = Item.Silversteel;
                    break;

                case 4: //Dirt
                    produced = Item.Clay;
                    break;

                case 5: //Gravel
                    produced = Item.Stone;
                    break;

                case 6: //Smooth
                    produced = Item.Obsidian;
                    break;
            }
        }
        else if(POID == 31)
        {   //Flower Pot
            switch (TYPE)
            {
                case 0:
                    switch (Random.Range(0, 4))
                    {
                        case 0:
                            produced = Item.FlowerWhite;
                            break;

                        case 1:
                            produced = Item.FlowerYellow;
                            break;

                        case 2:
                            produced = Item.FlowerRed;
                            break;

                        case 3:
                            produced = Item.FlowerPurple;
                            break;
                    }
                    break;

                case 1:
                    produced = Item.FlowerWhite;
                    break;

                case 2:
                    produced = Item.FlowerYellow;
                    break;

                case 3:
                    produced = Item.FlowerRed;
                    break;

                case 4:
                    produced = Item.FlowerPurple;
                    break;
            }
        }
        return produced;
    }

    private void CondenseCrystal()
    {
        if(ProductionCooldown > -300)
        {
            ProductionCooldown -= Worldmap.Main.TICK_LENGTH * Random.Range(0.9f, 1.1f);
        }
        else
        {
            switch (TYPE)
            {
                case 0:
                    if(Random.Range(0f, 1f) >= 0.9f)
                    {
                        Contents.ProduceItem(Item.CrystalPlain);
                        ProductionCooldown = 0;
                    }
                    break;

                case 1:
                    if(Random.Range(0f, 1f) >= 0.95f)
                    {
                        Contents.ProduceItem(Item.CrystalAir);
                        ProductionCooldown = 0;
                    }
                    break;

                case 2:
                    if(Random.Range(0f, 1f) >= 0.95f)
                    {
                        Contents.ProduceItem(Item.CrystalEarth);
                        ProductionCooldown = 0;
                    }
                    break;

                case 3:
                    if(Random.Range(0f, 1f) >= 0.95f)
                    {
                        Contents.ProduceItem(Item.CrystalFire);
                        ProductionCooldown = 0;
                    }
                    break;

                case 4:
                    if(Random.Range(0f, 1f) >= 0.95f)
                    {
                        Contents.ProduceItem(Item.CrystalWater);
                        ProductionCooldown = 0;
                    }
                    break;
            }
        }
    }

    private void DigUpMaterial()
    {
        if(ProductionCooldown > -180)
        {
            ProductionCooldown -= Worldmap.Main.TICK_LENGTH * Random.Range(0.9f, 1.1f);
        }
        else
        {
            switch (SUBTYPE)
            {
                case 0: //Grass
                    if(Random.Range(0f, 1f) >= 0.95f)
                    {
                        Contents.ProduceItem(Contents.Produces);
                        ProductionCooldown = 0;
                    }
                    break;

                case 3: //Sand
                    if(Random.Range(0f, 1f) >= 0.9f)
                    {
                        Contents.ProduceItem(Item.Silversteel);
                        ProductionCooldown = 0;
                    }
                    break;

                case 4: //Dirt
                    if(Random.Range(0f, 1f) >= 0.9f)
                    {
                        Contents.ProduceItem(Item.Clay);
                        ProductionCooldown = 0;
                    }
                    break;

                case 5: //Gravel
                    if(Random.Range(0f, 1f) >= 0.9f)
                    {
                        Contents.ProduceItem(Item.Stone);
                        ProductionCooldown = 0;
                    }
                    break;

                case 6: //Smooth
                    if(Random.Range(0f, 1f) >= 0.9f)
                    {
                        Contents.ProduceItem(Item.Obsidian);
                        ProductionCooldown = 0;
                    }
                    break;
            }
        }
    }

    private void GrowFlower()
    {
        if (ProductionCooldown > -120)
        {
            ProductionCooldown -= Worldmap.Main.TICK_LENGTH * Random.Range(0.9f, 1.1f);
        }
        else
        {
            switch (TYPE)
            {
                case 0:
                    if (Random.Range(0f, 1f) >= 0.95f)
                    {
                        Contents.ProduceItem(Contents.Produces);
                        //switch (Random.Range(0, 4))
                        //{
                        //    case 0:
                        //        Contents.ProduceItem(Item.FlowerWhite);
                        //        break;

                        //    case 1:
                        //        Contents.ProduceItem(Item.FlowerYellow);
                        //        break;

                        //    case 2:
                        //        Contents.ProduceItem(Item.FlowerRed);
                        //        break;

                        //    case 3:
                        //        Contents.ProduceItem(Item.FlowerPurple);
                        //        break;
                        //}
                        ProductionCooldown = 0;
                    }
                    break;

                case 1:
                    if (Random.Range(0f, 1f) >= 0.9f)
                    {
                        Contents.ProduceItem(Item.FlowerWhite);
                        ProductionCooldown = 0;
                    }
                    break;

                case 2:
                    if (Random.Range(0f, 1f) >= 0.9f)
                    {
                        Contents.ProduceItem(Item.FlowerYellow);
                        ProductionCooldown = 0;
                    }
                    break;

                case 3:
                    if (Random.Range(0f, 1f) >= 0.9f)
                    {
                        Contents.ProduceItem(Item.FlowerRed);
                        ProductionCooldown = 0;
                    }
                    break;

                case 4:
                    if (Random.Range(0f, 1f) >= 0.9f)
                    {
                        Contents.ProduceItem(Item.FlowerPurple);
                        ProductionCooldown = 0;
                    }
                    break;
            }
        }
    }
}
