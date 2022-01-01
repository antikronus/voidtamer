using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Ext
{
    public static bool IsPlaceable(this Item item)
    {
        return
            item == Item.Pylon ||
            item == Item.Workbench ||
            item == Item.Inscriber ||
            item == Item.Kiln ||
            item == Item.Alchemy ||
            item == Item.Crafts ||
            item == Item.Container ||
            item == Item.PylonAir ||
            item == Item.PylonEarth ||
            item == Item.PylonFire ||
            item == Item.PylonWater ||
            item == Item.Condenser ||
            item == Item.Drill ||
            item == Item.FlowerPot ||
            item == Item.Book ||
            item == Item.Wall ||
            item == Item.Woodsaw ||
            item == Item.Bridge;
    }

    public static bool IsMinion(this Item item)
    {
        return
            item == Item.Minion ||
            item == Item.LumberMinion;
    }

    public static MinionType GetMinionType(this Item item)
    {
        MinionType type = MinionType.Standard;

        switch (item)
        {
            case Item.LumberMinion:
                type = MinionType.Lumber;
                break;
        }

        return type;
    }

    public static bool IsAdvancedHarvestable(this Item item)
    {
        return
            item == Item.Lumber;
    }

    public static bool AddsStability(this Item item)
    {
        return
            item == Item.Pylon ||
            item == Item.PylonAir ||
            item == Item.PylonEarth ||
            item == Item.PylonFire ||
            item == Item.PylonWater;
    }

    public static string GetName(this Item item)
    {
        string name = "";
        switch (item)
        {
            case Item.Nothing:
                name = "Nothing";
                break;

            case Item.Pylon:
                name = "Pylon";
                break;

            case Item.Silversteel:
                name = "Silversteel";
                break;

            case Item.Stone:
                name = "Stone";
                break;

            case Item.Clay:
                name = "Clay";
                break;

            case Item.Obsidian:
                name = "Obsidian";
                break;

            case Item.FlowerWhite:
                name = "White Flower";
                break;

            case Item.FlowerYellow:
                name = "Yellow Flower";
                break;

            case Item.FlowerRed:
                name = "Red Flower";
                break;

            case Item.FlowerPurple:
                name = "Purple Flower";
                break;

            case Item.CrystalPlain:
                name = "Binding Crystal";
                break;

            case Item.CrystalAir:
                name = "Air Crystal";
                break;

            case Item.CrystalEarth:
                name = "Earth Crystal";
                break;

            case Item.CrystalFire:
                name = "Fire Crystal";
                break;

            case Item.CrystalWater:
                name = "Water Crystal";
                break;

            case Item.Workbench:
                name = "Work Bench";
                break;

            case Item.Inscriber:
                name = "Inscriber";
                break;

            case Item.Kiln:
                name = "Kiln";
                break;

            case Item.Alchemy:
                name = "Alchemy Plinth";
                break;

            case Item.Crafts:
                name = "Crafts Workshop";
                break;

            case Item.Minion:
                name = "Helper Golem";
                break;

            case Item.Container:
                name = "Container";
                break;

            case Item.PotionWhite:
                name = "Air Potion";
                break;

            case Item.PotionBrown:
                name = "Earth Potion";
                break;

            case Item.PotionRed:
                name = "Fire Potion";
                break;

            case Item.PotionBlue:
                name = "Water Potion";
                break;

            case Item.PylonAir:
                name = "Air Pylon";
                break;

            case Item.PylonEarth:
                name = "Earth Pylon";
                break;

            case Item.PylonFire:
                name = "Fire Pylon";
                break;

            case Item.PylonWater:
                name = "Water Pylon";
                break;

            case Item.Distillate:
                name = "Elemental Distillate";
                break;

            case Item.Mechanism:
                name = "Enchanted Mechanism";
                break;

            case Item.Soil:
                name = "Alchemical Clay";
                break;

            case Item.Condenser:
                name = "Crystal Condenser";
                break;

            case Item.Drill:
                name = "Arcane Drill";
                break;

            case Item.FlowerPot:
                name = "Flower Pot";
                break;

            case Item.Book:
                name = "Book";
                break;

            case Item.LumberMinion:
                name = "Woodcutter Golem";
                break;

            case Item.Lumber:
                name = "Wood";
                break;

            case Item.Paper:
                name = "Paper";
                break;

            case Item.Wall:
                name = "Wall Segment";
                break;

            case Item.Woodsaw:
                name = "Woodsaw";
                break;

            case Item.Bridge:
                name = "Bridge";
                break;
        }
        return name;
    }
}

public enum Item
{                   //Image Index
    Nothing,        //-1
    Pylon,          //0
    Silversteel,    //1
    Stone,          //2
    Clay,           //3
    Obsidian,       //4
    FlowerWhite,    //5
    FlowerYellow,   //6
    FlowerRed,      //7
    FlowerPurple,   //8
    CrystalPlain,   //9
    CrystalAir,     //10
    CrystalEarth,   //11
    CrystalFire,    //12
    CrystalWater,   //13
    Workbench,      //14
    Inscriber,      //15
    Kiln,           //16
    Alchemy,        //17
    Crafts,         //18
    Minion,         //19
    Container,      //20
    PotionWhite,    //21
    PotionBrown,    //22
    PotionRed,      //23
    PotionBlue,     //24
    PylonAir,       //25
    PylonEarth,     //26
    PylonFire,      //27
    PylonWater,     //28
    Distillate,     //29
    Mechanism,      //30
    Soil,           //31
    Condenser,      //32
    Drill,          //33
    FlowerPot,      //34
    Book,           //35
    LumberMinion,   //36
    Lumber,         //37
    Paper,          //38
    Wall,           //39
    Woodsaw,        //40
    Bridge,         //41
}

public class ItemObject : MonoBehaviour
{
    public Item Type;
    public Rigidbody2D RB;
    private bool ThrownMinion;
    public Transform Owner { get; private set; }
    private float initialTime = 0;
    public float ThrowCD;
    private float Damage;
    public string BookText = null;

    void Update()
    {
        if(Owner != null)
        {
            transform.position = new Vector3(Mathf.Lerp(transform.position.x, Owner.position.x, 0.8f), Mathf.Lerp(transform.position.y, Owner.position.y + 1, 0.8f));
        }

        if(ThrownMinion && RB.velocity.magnitude == 0f && ThrowCD <= 0f)
        {
            Worldmap.Main.AddMinion(transform.position, Type.GetMinionType());
            Destroy(gameObject);
        }

        if(ThrowCD > 0)
        {
            ThrowCD -= Time.deltaTime;
        }

        gameObject.transform.GetChild(0).localPosition = Mathf.Sin(Time.fixedTime - initialTime) * 0.25f * Vector3.up;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.GetComponent<TileAssistor>() != null)
            if(collision.gameObject.GetComponent<TileAssistor>().tile != null && collision.gameObject.GetComponent<TileAssistor>().tile.Destroys)
                Destroy(gameObject);

        if (collision.gameObject.GetComponent<FeatureAssistor>() != null)
        {
            PlacedObject addTo = collision.gameObject.GetComponent<FeatureAssistor>().LinkedObject;
            if (addTo != null)
            {
                if (Type.IsMinion() && ThrownMinion && !(addTo.ID == 24 && (addTo.Contains == Item.Nothing || addTo.Contains == Type)))
                {
                    Worldmap.Main.AddMinion(transform.position, Type.GetMinionType(), addTo);
                    Destroy(gameObject);
                }
                if (ThrowCD > 0)
                {
                    if (addTo.TryAddItem(this))
                    {
                        Destroy(gameObject);
                    }
                }
            }
        }

        if(ThrowCD > 0 && collision.gameObject.GetComponent<Enemy>() != null)
        {
            collision.gameObject.GetComponent<Enemy>().Health -= Damage;
        }
    }

    public void AssignType(Item type, Sprite sprite)
    {
        Type = type;
        gameObject.GetComponentInChildren<SpriteRenderer>().sprite = sprite;
        initialTime = Time.fixedTime;
    }

    public void Bind(Transform owner)
    {
        if(Owner == null)
        {
            Owner = owner;
            GetComponent<Collider2D>().enabled = false;
            ThrownMinion = false;
        }
    }

    public void Throw(Vector2 dir, float throwDamage = 0)
    {
        Owner = null;
        GetComponent<Collider2D>().enabled = true;
        GetComponent<Rigidbody2D>().AddForce(750 * dir);
        if (throwDamage > 0)
        {
            ThrownMinion = Type.IsMinion();
            if (ThrownMinion)
                gameObject.layer = LayerMask.NameToLayer("ThrownMinions");
            ThrowCD = 1;
            Damage = throwDamage;
        }
    }

    public void Drop(bool delete)
    {
        Owner = null;
        if (delete)
        {
            Destroy(gameObject);
        }
        else
        {
            GetComponent<Collider2D>().enabled = true;
        }
    }

    public void PullToward(Vector3 loc, float force)
    {
        GetComponent<Rigidbody2D>().AddForce(force * (loc - transform.position).normalized);
    }
}
