using System;
using System.Collections.Generic;
using UnityEngine;

public class PlacedObject
{
    public readonly int ID;
    public readonly List<Func<Recipe>> RecipeBook;
    public readonly bool natural;
    public readonly bool resource;
    public readonly bool collisions;
    public GameObject ObjSprite { get; private set; }
    private int ActiveRecipeI = -1;
    public Recipe ActiveRecipe { get; private set; } = null;
    public Item Drops { get; private set; }
    public Item Produces = Item.Nothing;
    public Item Contains = Item.Nothing;
    public Stack<string> ContainedBookText = new Stack<string>();
    public float Quantity { get; private set; } = 0;
    private Vector2 Position;
    private List<MinionController> LinkedMinions = new List<MinionController>();
    public string BookText = null;
    private bool[] Neighbors = { false, false, false, false };

    public static readonly List<Func<Recipe>> Recipes = new List<Func<Recipe>>()
    {
        ()=>{ return new Recipe(Item.Pylon,         new List<Item>(){ Item.CrystalPlain, Item.Silversteel },                                        new List<int>(){ 1, 2 }); },        //0
        ()=>{ return new Recipe(Item.Minion,        new List<Item>(){ Item.CrystalPlain, Item.Clay },                                               new List<int>(){ 1, 2 }); },        //1
        ()=>{ return new Recipe(Item.Workbench,     new List<Item>(){ Item.Stone },                                                                 new List<int>(){ 2 }); },           //2
        ()=>{ return new Recipe(Item.Inscriber,     new List<Item>(){ Item.CrystalEarth, Item.Silversteel },                                        new List<int>(){ 1, 2 }); },        //3
        ()=>{ return new Recipe(Item.Kiln,          new List<Item>(){ Item.CrystalFire, Item.Stone },                                               new List<int>(){ 1, 2}); },         //4
        ()=>{ return new Recipe(Item.Alchemy,       new List<Item>(){ Item.CrystalWater, Item.Obsidian, Item.Silversteel },                         new List<int>(){ 1, 2, 1 }); },     //5
        ()=>{ return new Recipe(Item.Crafts,        new List<Item>(){ Item.CrystalAir, Item.Clay, Item.Stone },                                     new List<int>(){ 1, 2, 1 }); },     //6
        ()=>{ return new Recipe(Item.Container,     new List<Item>(){ Item.Stone },                                                                 new List<int>(){ 2 }); },           //7
        ()=>{ return new Recipe(Item.PotionWhite,   new List<Item>(){ Item.FlowerWhite, Item.Obsidian },                                            new List<int>(){ 2, 1 }); },        //8
        ()=>{ return new Recipe(Item.PotionBrown,   new List<Item>(){ Item.FlowerYellow, Item.Obsidian },                                           new List<int>(){ 2, 1 }); },        //9
        ()=>{ return new Recipe(Item.PotionRed,     new List<Item>(){ Item.FlowerRed, Item.Obsidian },                                              new List<int>(){ 2, 1 }); },        //10
        ()=>{ return new Recipe(Item.PotionBlue,    new List<Item>(){ Item.FlowerPurple, Item.Obsidian },                                           new List<int>(){ 2, 1 }); },        //11
        ()=>{ return new Recipe(Item.PylonAir,      new List<Item>(){ Item.CrystalAir, Item.Silversteel },                                          new List<int>(){ 1, 3 }); },        //12
        ()=>{ return new Recipe(Item.PylonEarth,    new List<Item>(){ Item.CrystalEarth, Item.Silversteel, Item.Stone },                            new List<int>(){ 1, 2, 1 }); },     //13
        ()=>{ return new Recipe(Item.PylonFire,     new List<Item>(){ Item.CrystalFire, Item.Silversteel, Item.Obsidian },                          new List<int>(){ 1, 2, 1 }); },     //14
        ()=>{ return new Recipe(Item.PylonWater,    new List<Item>(){ Item.CrystalWater, Item.Silversteel, Item.Clay },                             new List<int>(){ 1, 2, 1 }); },     //15
        ()=>{ return new Recipe(Item.Distillate,    new List<Item>(){ Item.CrystalAir, Item.CrystalEarth, Item.CrystalFire, Item.CrystalWater },    new List<int>(){ 1, 1, 1, 1 }); },  //16
        ()=>{ return new Recipe(Item.Mechanism,     new List<Item>(){ Item.Silversteel, Item.Stone },                                               new List<int>(){ 1, 1 }); },        //17
        ()=>{ return new Recipe(Item.Soil,          new List<Item>(){ Item.Clay, Item.Obsidian },                                                   new List<int>(){ 1, 1 }); },        //18
        ()=>{ return new Recipe(Item.Condenser,     new List<Item>(){ Item.Distillate, Item.Silversteel, Item.Obsidian },                           new List<int>(){ 1, 2, 2 }); },     //19
        ()=>{ return new Recipe(Item.Drill,         new List<Item>(){ Item.Mechanism, Item.Stone },                                                 new List<int>(){ 2, 1 }); },        //20
        ()=>{ return new Recipe(Item.FlowerPot,     new List<Item>(){ Item.Soil, Item.Clay },                                                       new List<int>(){ 1, 1 }); },        //21
        ()=>{ return new Recipe(Item.LumberMinion,  new List<Item>(){ Item.CrystalPlain, Item.Soil, Item.Mechanism },                               new List<int>(){ 1, 2, 2 }); },     //22
        ()=>{ return new Recipe(Item.Paper,         new List<Item>(){ Item.Lumber },                                                                new List<int>(){ 1 }); },           //23
        ()=>{ return new Recipe(Item.Book,          new List<Item>(){ Item.Paper },                                                                 new List<int>(){ 1 }); },           //24
        ()=>{ return new Recipe(Item.Wall,          new List<Item>(){ Item.Stone },                                                                 new List<int>(){ 2 }); },           //25
        ()=>{ return new Recipe(Item.Woodsaw,       new List<Item>(){ Item.Lumber, Item.Mechanism },                                                new List<int>(){ 1, 2 }); },        //26
        ()=>{ return new Recipe(Item.Bridge,        new List<Item>(){ Item.Lumber },                                                                new List<int>(){ 1 }); },           //27
    };

    public static readonly List<List<Func<Recipe>>> RGroups = new List<List<Func<Recipe>>>()
    {
        new List<Func<Recipe>>(){ Recipes[2], Recipes[3], Recipes[4], Recipes[25], Recipes[24], Recipes[20], Recipes[26] },//Workbench
        new List<Func<Recipe>>(){ Recipes[0], Recipes[5], Recipes[12], Recipes[13], Recipes[14], Recipes[15], Recipes[19] },//Inscriber
        new List<Func<Recipe>>(){ Recipes[1], Recipes[6], Recipes[22], Recipes[21] },//Kiln
        new List<Func<Recipe>>(){ Recipes[8], Recipes[9], Recipes[10], Recipes[11], Recipes[16], Recipes[18], Recipes[23] },//Alchemy
        new List<Func<Recipe>>(){ Recipes[7], Recipes[17] },//Crafts
        new List<Func<Recipe>>(){ Recipes[27]}, //Woodsaw
    };

    public static readonly Dictionary<string, Func<PlacedObject>> POs = new Dictionary<string, Func<PlacedObject>>()
    {    //Name                                            ID  Recipes      Dropped Item        Natural     Resource    Collisions
        { "Pylon",          () => {return new PlacedObject(00, null,        Item.Pylon,         false,      false,      true); } },
        { "Nat Workbench",  () => {return new PlacedObject(01, RGroups[0],  Item.Nothing,       true,       false,      true); } },
        { "Nat Inscriber",  () => {return new PlacedObject(02, RGroups[1],  Item.Nothing,       true,       false,      true); } },
        { "Nat Kiln",       () => {return new PlacedObject(03, RGroups[2],  Item.Nothing,       true,       false,      true); } },
        { "Nat Alchemy",    () => {return new PlacedObject(04, RGroups[3],  Item.Nothing,       true,       false,      true); } },
        { "Nat Crafts",     () => {return new PlacedObject(05, RGroups[4],  Item.Nothing,       true,       false,      true); } },
        { "Silversteel",    () => {return new PlacedObject(06, null,        Item.Silversteel,   true,       true,       false); } },
        { "Stone",          () => {return new PlacedObject(07, null,        Item.Stone,         true,       true,       false); } },
        { "Clay",           () => {return new PlacedObject(08, null,        Item.Clay,          true,       true,       false); } },
        { "Obsidian",       () => {return new PlacedObject(09, null,        Item.Obsidian,      true,       true,       false); } },
        { "Flower White",   () => {return new PlacedObject(10, null,        Item.FlowerWhite,   true,       true,       false); } },
        { "Flower Yellow",  () => {return new PlacedObject(11, null,        Item.FlowerYellow,  true,       true,       false); } },
        { "Flower Red",     () => {return new PlacedObject(12, null,        Item.FlowerRed,     true,       true,       false); } },
        { "Flower Purple",  () => {return new PlacedObject(13, null,        Item.FlowerPurple,  true,       true,       false); } },
        { "Crystal Plain",  () => {return new PlacedObject(14, null,        Item.CrystalPlain,  true,       true,       false); } },
        { "Crystal Air",    () => {return new PlacedObject(15, null,        Item.CrystalAir,    true,       true,       false); } },
        { "Crystal Earth",  () => {return new PlacedObject(16, null,        Item.CrystalEarth,  true,       true,       false); } },
        { "Crystal Fire",   () => {return new PlacedObject(17, null,        Item.CrystalFire,   true,       true,       false); } },
        { "Crystal Water",  () => {return new PlacedObject(18, null,        Item.CrystalWater,  true,       true,       false); } },
        { "Workbench",      () => {return new PlacedObject(19, RGroups[0],  Item.Workbench,     false,      false,      true); } },
        { "Inscriber",      () => {return new PlacedObject(20, RGroups[1],  Item.Inscriber,     false,      false,      true); } },
        { "Kiln",           () => {return new PlacedObject(21, RGroups[2],  Item.Kiln,          false,      false,      true); } },
        { "Alchemy",        () => {return new PlacedObject(22, RGroups[3],  Item.Alchemy,       false,      false,      true); } },
        { "Crafts",         () => {return new PlacedObject(23, RGroups[4],  Item.Crafts,        false,      false,      true); } },
        { "Container",      () => {return new PlacedObject(24, null,        Item.Container,     false,      false,      true); } },
        { "PylonAir",       () => {return new PlacedObject(25, null,        Item.PylonAir,      false,      false,      true); } },
        { "PylonEarth",     () => {return new PlacedObject(26, null,        Item.PylonEarth,    false,      false,      true); } },
        { "PylonFire",      () => {return new PlacedObject(27, null,        Item.PylonFire,     false,      false,      true); } },
        { "PylonWater",     () => {return new PlacedObject(28, null,        Item.PylonWater,    false,      false,      true); } },
        { "Condenser",      () => {return new PlacedObject(29, null,        Item.Condenser,     false,      false,      true); } },
        { "Drill",          () => {return new PlacedObject(30, null,        Item.Drill,         false,      false,      true); } },
        { "FlowerPot",      () => {return new PlacedObject(31, null,        Item.FlowerPot,     false,      false,      true); } },
        { "Broken Pylon",   () => {return new PlacedObject(32, null,        Item.CrystalPlain,  false,      false,      true); } },
        { "Book",           () => {return new PlacedObject(33, null,        Item.Book,          false,      false,      true); } },
        { "Tree Plain",     () => {return new PlacedObject(34, null,        Item.Lumber,        true,       true,       true); } },
        { "Tree Air",       () => {return new PlacedObject(35, null,        Item.Lumber,        true,       true,       true); } },
        { "Tree Earth",     () => {return new PlacedObject(36, null,        Item.Lumber,        true,       true,       true); } },
        { "Tree Fire",      () => {return new PlacedObject(37, null,        Item.Lumber,        true,       true,       true); } },
        { "Tree Water",     () => {return new PlacedObject(38, null,        Item.Lumber,        true,       true,       true); } },
        { "Wall",           () => {return new PlacedObject(39, null,        Item.Wall,          false,      false,      true); } },
        { "Woodsaw",        () => {return new PlacedObject(40, RGroups[5],  Item.Woodsaw,       false,      false,      true); } },
        { "Bridge",         () => {return new PlacedObject(41, null,        Item.Bridge,        false,      false,      false); } },
    };

    public Vector2 Pos
    {
        get
        {
            return Position;
        }

        set
        {
            Position = value;
        }
    }

    public bool Crafting
    {
        get
        {
            return RecipeBook != null;
        }
    }

    public bool AddsStability
    {
        get
        {
            return ID == 0 || ID == 25 || ID == 26 || ID == 27 || ID == 28 || ID == 32;
        }
    }

    public bool ConnectsToNeighbors
    {
        get
        {
            return ID == 39;
        }
    }

    public bool Visible
    {
        get
        {
            if(ObjSprite != null)
            {
                return ObjSprite.GetComponent<SpriteRenderer>().enabled;
            }
            return false;
        }
    }


    private PlacedObject(int OID, List<Func<Recipe>> recipes, Item drops, bool nat, bool res, bool col)
    {
        ID = OID;
        RecipeBook = recipes;
        Drops = drops;
        natural = nat;
        resource = res;
        collisions = col;
        if (Crafting)
        {
            ActiveRecipeI = 0;
            ActiveRecipe = RecipeBook[0].Invoke();
            ActiveRecipe.AssignStation(this);
        }
    }

    public void HardUpdate()
    {
        if (ConnectsToNeighbors)
        {
            PollNeighbors();
        }
    }

    public static PlacedObject MakeObject(string name)
    {
        return POs[name].Invoke();
    }

    public static PlacedObject MakeBook(string text)
    {
        PlacedObject newBook = POs["Book"].Invoke();
        newBook.BookText = text;
        return newBook;
    }

    public void GiveObject(GameObject newObject)
    {
        ObjSprite = newObject;
        ObjSprite.GetComponent<FeatureAssistor>().Link(this);
    }

    public void FreeObject()
    {
        Hide();
        if(ObjSprite != null)
            ObjSprite.GetComponent<FeatureAssistor>().Unlink();
        ObjSprite = null;
    }

    public void Hide()
    {
        if(ObjSprite != null)
        {
            ObjSprite.GetComponent<SpriteRenderer>().enabled = false;
            ObjSprite.GetComponent<Collider2D>().enabled = false;
            ObjSprite.layer = LayerMask.NameToLayer("PlacedObjects");
        }
    }

    public void Show()
    {
        if (ObjSprite != null)
        {
            ObjSprite.GetComponent<SpriteRenderer>().sprite = ObjSprite.GetComponent<FeatureAssistor>().FeatureSprites[ID];
            if(ConnectsToNeighbors)
            {
                ObjSprite.GetComponent<SpriteRenderer>().sprite = FindTilesetSprite();
            }
            ObjSprite.GetComponent<SpriteRenderer>().enabled = true;
            ObjSprite.GetComponent<Collider2D>().enabled = resource || collisions;
            if (resource)
            {
                if (collisions)
                    ObjSprite.layer = LayerMask.NameToLayer("ResourceObstacle");
                else
                    ObjSprite.layer = LayerMask.NameToLayer("Resources");
            }
        }
    }

    public void EjectContents()
    {
        if (Crafting)
        {
            ActiveRecipe.EjectContents();
        }
    }

    public ItemObject ProduceItem(Item item)
    {
        ItemObject newItem = null;
        if(item != Item.Nothing)
        {
            GameObject newObj = UnityEngine.Object.Instantiate(Worldmap.Main.CraftMenu.ItemPrefab.gameObject);
            newObj.transform.position = Pos;
            newItem = newObj.GetComponent<ItemObject>();
            newItem.AssignType(item, Worldmap.Main.CraftMenu.ItemSprites[(int)item - 1]);
            newItem.Throw(new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)).normalized * 0.2f);
        }
        return newItem;
    }

    public bool HarvestItem(out ItemObject produced, Item specialGather = Item.Nothing)
    {
        produced = null;
        if(Quantity > 0 && Contains != Item.Nothing)
        {
            produced = ProduceItem(Contains);
            if (Contains == Item.Book && ContainedBookText.Count > 0)
                produced.BookText = ContainedBookText.Pop();
            Quantity--;
            if(Quantity == 0)
            {
                RemoveSubscribers();
                Contains = Item.Nothing;
            }
            return false;
        }
        else if(Drops != Item.Nothing && (!Drops.IsAdvancedHarvestable() || specialGather == Drops))
        {
            produced = ProduceItem(Drops);
            if (BookText != null)
                produced.BookText = BookText;
            RemoveFromNeighborLists();
            RemoveSubscribers();
            return true;
        }

        return false;
    }

    public bool TryAddItem(ItemObject toAdd)
    {
        return TryAddItem(toAdd.Type, toAdd.BookText);
    }

    public bool TryAddItem(Item toAdd, string bookText = "")
    {
        if (ID == 24 && (Contains == Item.Nothing || Contains == toAdd))
        {
            Contains = toAdd;
            if (toAdd == Item.Book)
                ContainedBookText.Push(bookText);
            Quantity++;
            return true;
        }
        if (Crafting && ActiveRecipe != null)
        {
            return ActiveRecipe.TryAdd(toAdd);
        }

        return false;
    }

    public bool[] PollNeighbors(bool valueToSet = true)
    {
        bool[] toReturn = { false, false, false, false };

        PlacedObject westNeighbor = Worldmap.Main.FindTile(Pos + Vector2.left).Contents;
        PlacedObject southNeighbor = Worldmap.Main.FindTile(Pos + Vector2.down).Contents;
        PlacedObject eastNeighbor = Worldmap.Main.FindTile(Pos + Vector2.right).Contents;
        PlacedObject northNeighbor = Worldmap.Main.FindTile(Pos + Vector2.up).Contents;

        if(westNeighbor != null && westNeighbor.ID == ID)
        {
            westNeighbor.SetNeighbor(2, valueToSet);
            toReturn[0] = valueToSet;
        }
        if(southNeighbor != null && southNeighbor.ID == ID)
        {
            southNeighbor.SetNeighbor(3, valueToSet);
            toReturn[1] = valueToSet;
        }
        if(eastNeighbor != null && eastNeighbor.ID == ID)
        {
            eastNeighbor.SetNeighbor(0, valueToSet);
            toReturn[2] = valueToSet;
        }
        if(northNeighbor != null && northNeighbor.ID == ID)
        {
            northNeighbor.SetNeighbor(1, valueToSet);
            toReturn[3] = valueToSet;
        }

        return toReturn;
    }

    public void RemoveFromNeighborLists()
    {
        if(ConnectsToNeighbors)
            PollNeighbors(false);
    }

    public void SetNeighbor(int neighbor, bool value)
    {
        if (Neighbors[neighbor] != value)
        {
            Neighbors[neighbor] = value;
            if (ObjSprite != null && ConnectsToNeighbors)
            {
                ObjSprite.GetComponent<SpriteRenderer>().sprite = FindTilesetSprite();
            }
        }
    }

    public Sprite FindTilesetSprite()
    {
        Sprite toReturn;
        int offset = 0;
        int neighborValue = 0;

        for(int i = 0; i < 4; i++)
        {
            if (Neighbors[i])
                neighborValue += (int)Mathf.Pow(2, i);
        }

        toReturn = ObjSprite.GetComponent<FeatureAssistor>().TilesetFeatureSprites[offset + neighborValue];
        return toReturn;
    }

    public void RemoveSubscribers()
    {
        foreach (MinionController subscriber in LinkedMinions)
            subscriber.POSubscriberCallback(this);

        LinkedMinions = new List<MinionController>();
    }

    public void Subscribe(MinionController subscriber)
    {
        if (!LinkedMinions.Contains(subscriber))
            LinkedMinions.Add(subscriber);
    }

    public void Unsubscribe(MinionController subscriber)
    {
        LinkedMinions.Remove(subscriber);
    }

    public void CycleRecipe(bool reverse = false)
    {
        if(RecipeBook.Count > 1)
        {
            ActiveRecipe.EjectContents();
            ActiveRecipeI = reverse ? ActiveRecipeI + RecipeBook.Count - 1 : ActiveRecipeI + 1;
            ActiveRecipeI %= RecipeBook.Count;
            ActiveRecipe = RecipeBook[ActiveRecipeI].Invoke();
            ActiveRecipe.AssignStation(this);
            RemoveSubscribers();
        }
    }

    public string GetTooltip()
    {
        string tooltipText = "";

        switch (ID)
        {
            case 0:
                tooltipText += "Pylon" + "\n";
                tooltipText += "Left Click: Pick Up" + "\n";
                tooltipText += "A powerful pillar of binding, capable of stabilizing large sections of void.";
                break;

            case 1:
                tooltipText += "Work Bench (natural)" + "\n";
                tooltipText += "Right Click: Set Recipe" + "\n";
                tooltipText += "This crafting station is used to make simple objects.";
                break;

            case 2:
                tooltipText += "Inscriber (natural)" + "\n";
                tooltipText += "Right Click: Set Recipe" + "\n";
                tooltipText += "This crafting station is used to shape and enchant silversteel. Pylons are crafted here.";
                break;

            case 3:
                tooltipText += "Kiln (natural)" + "\n";
                tooltipText += "Right Click: Set Recipe" + "\n";
                tooltipText += "A crafting station used to fire clay. Minions are crafted here.";
                break;

            case 4:
                tooltipText += "Alchemy Plinth (natural)" + "\n";
                tooltipText += "Right Click: Set Recipe" + "\n";
                tooltipText += "A crafting station used to brew potions from flowers.";
                break;

            case 5:
                tooltipText += "Crafts Workshop (natural)" + "\n";
                tooltipText += "Right Click: Set Recipe" + "\n";
                tooltipText += "A crafting station used to make special crafts.";
                break;

            case 6:
                tooltipText += "Silversteel" + "\n";
                tooltipText += "Left Click: Harvest" + "\n";
                tooltipText += "A wellspring of silvery liquid metal that is often used at an inscriber.";
                break;

            case 7:
                tooltipText += "Stone" + "\n";
                tooltipText += "Left Click: Harvest" + "\n";
                tooltipText += "A pile of neatly shaped rocks that would be very useful at a crafts workshop.";
                break;

            case 8:
                tooltipText += "Clay" + "\n";
                tooltipText += "Left Click: Harvest" + "\n";
                tooltipText += "A small heap of malleable clay, which can be fired in a kiln.";
                break;

            case 9:
                tooltipText += "Obsidian" + "\n";
                tooltipText += "Left Click: Harvest" + "\n";
                tooltipText += "A sheet of volcanic glass, with properties useful for alchemy.";
                break;

            case 10:
                tooltipText += "White Flower" + "\n";
                tooltipText += "Left Click: Harvest" + "\n";
                tooltipText += "A white flower that can be used to craft potions at an alchemy plinth.";
                break;

            case 11:
                tooltipText += "Yellow Flower" + "\n";
                tooltipText += "Left Click: Harvest" + "\n";
                tooltipText += "A yellow flower that can be used to craft potions at an alchemy plinth.";
                break;

            case 12:
                tooltipText += "Red Flower" + "\n";
                tooltipText += "Left Click: Harvest" + "\n";
                tooltipText += "A red flower that can be used to craft potions at an alchemy plinth.";
                break;

            case 13:
                tooltipText += "Purple Flower" + "\n";
                tooltipText += "Left Click: Harvest" + "\n";
                tooltipText += "A purple flower that can be used to craft potions at an alchemy plinth.";
                break;

            case 14:
                tooltipText += "Binding Crystal" + "\n";
                tooltipText += "Left Click: Harvest" + "\n";
                tooltipText += "An outcropping of rare and powerful crystal used for crafting at an inscriber or kiln.";
                break;

            case 15:
                tooltipText += "Air Crystal" + "\n";
                tooltipText += "Left Click: Harvest" + "\n";
                tooltipText += "An outcropping of rare and powerful crystal imbued with air magic, which is used to build crafts workshops and advanced pylons.";
                break;

            case 16:
                tooltipText += "Earth Crystal" + "\n";
                tooltipText += "Left Click: Harvest" + "\n";
                tooltipText += "An outcropping of rare and powerful crystal imbued with earth magic, which is used to build inscribers and advanced pylons.";
                break;

            case 17:
                tooltipText += "Fire Crystal" + "\n";
                tooltipText += "Left Click: Harvest" + "\n";
                tooltipText += "An outcropping of rare and powerful crystal imbued with fire magic, which is used to build kilns and advanced pylons.";
                break;

            case 18:
                tooltipText += "Water Crystal" + "\n";
                tooltipText += "Left Click: Harvest" + "\n";
                tooltipText += "An outcropping of rare and powerful crystal imbued with water magic, which is used to build alchemy plinths and advanced pylons.";
                break;

            case 19:
                tooltipText += "Work Bench" + "\n";
                tooltipText += "Left Click: Pick Up" + "\n";
                tooltipText += "Right Click: Set Recipe" + "\n";
                tooltipText += "This crafting station is used to make simple objects.";
                break;

            case 20:
                tooltipText += "Inscriber" + "\n";
                tooltipText += "Left Click: Pick Up" + "\n";
                tooltipText += "Right Click: Set Recipe" + "\n";
                tooltipText += "This crafting station is used to shape and enchant silversteel. Pylons are crafted here.";
                break;

            case 21:
                tooltipText += "Kiln" + "\n";
                tooltipText += "Left Click: Pick Up" + "\n";
                tooltipText += "Right Click: Set Recipe" + "\n";
                tooltipText += "A crafting station used to fire clay. Minions are crafted here.";
                break;

            case 22:
                tooltipText += "Alchemy Plinth" + "\n";
                tooltipText += "Left Click: Pick Up" + "\n";
                tooltipText += "Right Click: Set Recipe" + "\n";
                tooltipText += "A crafting station used to brew potions from flowers.";
                break;

            case 23:
                tooltipText += "Crafts Workshop" + "\n";
                tooltipText += "Left Click: Pick Up" + "\n";
                tooltipText += "Right Click: Set Recipe" + "\n";
                tooltipText += "A crafting station used to make special crafts.";
                break;

            case 24:
                tooltipText += "Container (";
                if (Quantity > 0)
                {
                    tooltipText += Quantity + "x";
                }
                tooltipText += Contains.GetName();
                tooltipText += ")\n";
                tooltipText += "Left Click: ";
                if (Quantity > 0)
                {
                    tooltipText += "Take Item";
                }
                else
                {
                    tooltipText += "Pick Up";
                }
                tooltipText += "\n";
                tooltipText += "A place to store a large number of the same type of item.";
                break;

            case 25:
                tooltipText += "Air Pylon" + "\n";
                tooltipText += "Left Click: Pick Up" + "\n";
                tooltipText += "An advanced pylon that increases the speed of those near it.";
                break;

            case 26:
                tooltipText += "Earth Pylon" + "\n";
                tooltipText += "Left Click: Pick Up" + "\n";
                tooltipText += "An advanced pylon with a greater stabilizing range.";
                break;

            case 27:
                tooltipText += "Fire Pylon" + "\n";
                tooltipText += "Left Click: Pick Up" + "\n";
                tooltipText += "An advanced pylon that damages nearby enemies.";
                break;

            case 28:
                tooltipText += "Water Pylon" + "\n";
                tooltipText += "Left Click: Pick Up" + "\n";
                tooltipText += "An advanced pylon that pulls nearby items toward itself.";
                break;

            case 29:
                tooltipText += "Crystal Condenser" + "\n";
                tooltipText += "Left Click: Pick Up" + "\n";
                tooltipText += "A structure that condenses ambient elemental energy into usuable crystals.";
                break;

            case 30:
                tooltipText += "Arcane Drill" + "\n";
                tooltipText += "Left Click: Pick Up" + "\n";
                tooltipText += "A structure which retrieves mineral resources from within the earth.";
                break;

            case 31:
                tooltipText += "Flower Pot" + "\n";
                tooltipText += "Left Click: Pick Up" + "\n";
                tooltipText += "A structure which grows flowers.";
                break;

            case 32:
                tooltipText += "Ancient Pylon" + "\n";
                tooltipText += "Left Click: Salvage Binding Crystal" + "\n";
                tooltipText += "An incredibly old pylon. Its power to stabilize the void has all but faded.";
                break;

            case 33:
                tooltipText += "Book" + "\n";
                tooltipText += "Left Click: Pick Up" + "\n";
                tooltipText += "Right Click: Edit Text" + "\n";
                if (BookText != null && BookText != "")
                    tooltipText += "This book has something written in it:\n\"" + BookText + "\"";
                else
                    tooltipText += "The pages of this book are empty.";
                break;

            case 34:
            case 35:
            case 36:
            case 37:
            case 38:
                tooltipText += "Tree" + "\n";
                tooltipText += "A large plant with a wooden trunk. Chopping this down will require specialized minions.";
                break;

            case 39:
                tooltipText += "Wall" + "\n";
                tooltipText += "Left Click: Pick Up" + "\n";
                tooltipText += "A defensive structure that inhibits movement and protects its space from falling into the void.";
                break;

            case 40:
                tooltipText += "Woodsaw" + "\n";
                tooltipText += "Left Click: Pick Up" + "\n";
                tooltipText += "Right Click: Set Recipe" + "\n";
                tooltipText += "This crafting station is used for advanced wood crafting.";
                break;

            case 41:
                tooltipText += "Bridge" + "\n";
                tooltipText += "Left Click: Pick Up" + "\n";
                tooltipText += "A structure which allows passage over water tiles.";
                break;
        }

        return tooltipText;
    }
}
