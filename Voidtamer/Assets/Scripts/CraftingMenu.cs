using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CraftingMenu : MonoBehaviour
{
    public Image Backdrop;
    public Text Station;
    public Text Recipe;
    public Text RecipeDesc;
    public Text Ingredient1;
    public Text Ingredient1Desc;
    public Text Ingredient1Q;
    public Image Ingredient1S;
    public Text Ingredient2;
    public Text Ingredient2Desc;
    public Text Ingredient2Q;
    public Image Ingredient2S;
    public Text Ingredient3;
    public Text Ingredient3Desc;
    public Text Ingredient3Q;
    public Image Ingredient3S;
    public Text Ingredient4;
    public Text Ingredient4Desc;
    public Text Ingredient4Q;
    public Image Ingredient4S;
    public Button Next;
    public Button Prev;
    public Button Close;
    public ItemObject ItemPrefab;
    public List<Sprite> ItemSprites;

    public InputField BookInput;
    public Button BookConfirm;
    public Button BookCancel;

    Recipe ActiveRec = null;
    string StationType = "Work Bench";
    PlacedObject BookToWrite = null;

    void Start()
    {
        Hide();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Display(Recipe recipe, string station)
    {
        StationType = station;
        ActiveRec = recipe;
        UpdateDisplay();
        Backdrop.enabled = true;
        Station.enabled = true;
        Next.gameObject.SetActive(true);
        Prev.gameObject.SetActive(true);
        Close.gameObject.SetActive(true);
    }

    public void Display(PlacedObject book)
    {
        BookToWrite = book;
        BookInput.gameObject.SetActive(true);
        if (book.BookText != null && book.BookText != "")
            BookInput.text = BookToWrite.BookText;
        else
            BookInput.text = "";
        BookConfirm.gameObject.SetActive(true);
        BookCancel.gameObject.SetActive(true);
    }

    public void Hide()
    {
        StationType = "";
        ActiveRec = null;
        BookToWrite = null;
        Backdrop.enabled = false;
        Station.enabled = false;
        Recipe.enabled = false;
        RecipeDesc.enabled = false;
        Ingredient1.enabled = false;
        Ingredient1Desc.enabled = false;
        Ingredient1Q.enabled = false;
        Ingredient1S.enabled = false;
        Ingredient2.enabled = false;
        Ingredient2Desc.enabled = false;
        Ingredient2Q.enabled = false;
        Ingredient2S.enabled = false;
        Ingredient3.enabled = false;
        Ingredient3Desc.enabled = false;
        Ingredient3Q.enabled = false;
        Ingredient3S.enabled = false;
        Ingredient4.enabled = false;
        Ingredient4Desc.enabled = false;
        Ingredient4Q.enabled = false;
        Ingredient4S.enabled = false;
        Next.gameObject.SetActive(false);
        Prev.gameObject.SetActive(false);
        Close.gameObject.SetActive(false);
        BookInput.gameObject.SetActive(false);
        BookConfirm.gameObject.SetActive(false);
        BookCancel.gameObject.SetActive(false);
    }

    public void UpdateRecipe(Recipe recipe)
    {
        ActiveRec = recipe;
        UpdateDisplay();
    }

    public void ConfirmBookText()
    {
        BookToWrite.BookText = BookInput.text;
        Worldmap.Main.EndCrafting();
    }

    public void UpdateDisplay()
    {
        Station.text = StationType;
        if(ActiveRec != null)
        {
            Recipe.enabled = true;
            Recipe.text = ActiveRec.Output.GetName();
            RecipeDesc.enabled = true;
            RecipeDesc.text = GetItemDescription(ActiveRec.Output);
        }
        else
        {
            Recipe.enabled = false;
            RecipeDesc.enabled = false;
        }
        if(ActiveRec != null && ActiveRec.Materials.Count > 0)
        {
            Ingredient1.text = ActiveRec.Materials[0].GetName();
            Ingredient1.enabled = true;
            Ingredient1Desc.text = GetItemDescription(ActiveRec.Materials[0]);
            Ingredient1Desc.enabled = true;
            Ingredient1Q.text = ActiveRec.MsReceived[0] + "/" + ActiveRec.MsNeeded[0];
            Ingredient1Q.enabled = true;
            Ingredient1S.sprite = ItemSprites[(int)ActiveRec.Materials[0] - 1];
            Ingredient1S.enabled = true;
        }
        else
        {
            Ingredient1.enabled = false;
            Ingredient1Desc.enabled = false;
            Ingredient1Q.enabled = false;
            Ingredient1S.enabled = false;
        }
        if (ActiveRec != null && ActiveRec.Materials.Count > 1)
        {
            Ingredient2.text = ActiveRec.Materials[1].GetName();
            Ingredient2.enabled = true;
            Ingredient2Desc.text = GetItemDescription(ActiveRec.Materials[1]);
            Ingredient2Desc.enabled = true;
            Ingredient2Q.text = ActiveRec.MsReceived[1] + "/" + ActiveRec.MsNeeded[1];
            Ingredient2Q.enabled = true;
            Ingredient2S.sprite = ItemSprites[(int)ActiveRec.Materials[1] - 1];
            Ingredient2S.enabled = true;
        }
        else
        {
            Ingredient2.enabled = false;
            Ingredient2Desc.enabled = false;
            Ingredient2Q.enabled = false;
            Ingredient2S.enabled = false;
        }
        if (ActiveRec != null && ActiveRec.Materials.Count > 2)
        {
            Ingredient3.text = ActiveRec.Materials[2].GetName();
            Ingredient3.enabled = true;
            Ingredient3Desc.text = GetItemDescription(ActiveRec.Materials[2]);
            Ingredient3Desc.enabled = true;
            Ingredient3Q.text = ActiveRec.MsReceived[2] + "/" + ActiveRec.MsNeeded[2];
            Ingredient3Q.enabled = true;
            Ingredient3S.sprite = ItemSprites[(int)ActiveRec.Materials[2] - 1];
            Ingredient3S.enabled = true;
        }
        else
        {
            Ingredient3.enabled = false;
            Ingredient3Desc.enabled = false;
            Ingredient3Q.enabled = false;
            Ingredient3S.enabled = false;
        }
        if (ActiveRec != null && ActiveRec.Materials.Count > 3)
        {
            Ingredient4.text = ActiveRec.Materials[3].GetName();
            Ingredient4.enabled = true;
            Ingredient4Desc.text = GetItemDescription(ActiveRec.Materials[3]);
            Ingredient4Desc.enabled = true;
            Ingredient4Q.text = ActiveRec.MsReceived[3] + "/" + ActiveRec.MsNeeded[3];
            Ingredient4Q.enabled = true;
            Ingredient4S.sprite = ItemSprites[(int)ActiveRec.Materials[3] - 1];
            Ingredient4S.enabled = true;
        }
        else
        {
            Ingredient4.enabled = false;
            Ingredient4Desc.enabled = false;
            Ingredient4Q.enabled = false;
            Ingredient4S.enabled = false;
        }
    }

    public string GetItemDescription(Item item)
    {
        string description = "";
        switch (item)
        {
            case Item.Pylon:
                description = "A powerful pillar of binding, capable of stabilizing large sections of void.";
                break;

            case Item.Silversteel:
                description = "A silvery liquid found in sandy areas, used for inscribing.";
                break;

            case Item.Stone:
                description = "A boulder, perfectly shaped for crafting, found in rocky areas.";
                break;

            case Item.Clay:
                description = "Soft clay that can be shaped easily, found in the dirt.";
                break;

            case Item.Obsidian:
                description = "Volcanic glass with magical properties, found amidst smooth stone.";
                break;

            case Item.FlowerWhite:
                description = "A small flower imbued with latent air magic, found in grassy areas.";
                break;

            case Item.FlowerYellow:
                description = "A small flower imbued with latent earth magic, found in grassy areas.";
                break;

            case Item.FlowerRed:
                description = "A small flower imbued with latent fire magic, found in grassy areas.";
                break;

            case Item.FlowerPurple:
                description = "A small flower imbued with latent water magic, found in grassy areas.";
                break;

            case Item.CrystalPlain:
                description = "A rare and powerful crystal that focuses magic in mysterious ways.";
                break;

            case Item.CrystalAir:
                description = "A rare and powerful crystal that focuses air magic in mysterious ways.";
                break;

            case Item.CrystalEarth:
                description = "A rare and powerful crystal that focuses earth magic in mysterious ways.";
                break;

            case Item.CrystalFire:
                description = "A rare and powerful crystal that focuses fire magic in mysterious ways.";
                break;

            case Item.CrystalWater:
                description = "A rare and powerful crystal that focuses water magic in mysterious ways.";
                break;

            case Item.Workbench:
                description = "A crafting station used to make simple objects.";
                break;

            case Item.Inscriber:
                description = "A crafting station used to shape and enchant silversteel.";
                break;

            case Item.Kiln:
                description = "A crafting station used to fire clay.";
                break;

            case Item.Alchemy:
                description = "A crafting station used to brew potions from flowers.";
                break;

            case Item.Crafts:
                description = "A crafting station used to make special crafts.";
                break;

            case Item.Minion:
                description = "A helpful minion that can collect, carry, and craft items.";
                break;

            case Item.Container:
                description = "A container which can hold a large amount of a single item type.";
                break;

            case Item.PotionWhite:
                description = "A potion that lets the drinker run at incredible speeds.";
                break;

            case Item.PotionBrown:
                description = "A potion that increases the drinker's ability to stabilize the void.";
                break;

            case Item.PotionRed:
                description = "A potion that increases the damage of the items thrown by the drinker.";
                break;

            case Item.PotionBlue:
                description = "A potion that causes items to float around, following the drinker.";
                break;

            case Item.PylonAir:
                description = "An advanced pylon that increases the speed of those near it.";
                break;

            case Item.PylonEarth:
                description = "An advanced pylon with a greater stabilizing range.";
                break;

            case Item.PylonFire:
                description = "An advanced pylon that damages nearby enemies.";
                break;

            case Item.PylonWater:
                description = "An advanced pylon that pulls nearby items toward itself.";
                break;

            case Item.Distillate:
                description = "Residue of elemental crystals, created with alchemy.";
                break;

            case Item.Mechanism:
                description = "An intricate piece of machinery, requiring delicate crating tools to create.";
                break;

            case Item.Soil:
                description = "An alchemically enhanced mixture of clays useful for growing plants and advanced golem crafting.";
                break;

            case Item.Condenser:
                description = "A structure that condenses ambient elemental energy into usuable crystals.";
                break;

            case Item.Drill:
                description = "A structure that retrieves mineral resources from within the earth.";
                break;

            case Item.FlowerPot:
                description = "A structure that grows flowers.";
                break;

            case Item.Book:
                description = "A collection of paper sheets that can store written information.";
                break;

            case Item.LumberMinion:
                description = "A more advanced helper minion capable of harvesting wood from trees.";
                break;

            case Item.Lumber:
                description = "A useful crating material which requires specialized tools to acquire from trees.";
                break;

            case Item.Paper:
                description = "Sheets of lightweight material used to craft books.";
                break;

            case Item.Wall:
                description = "A structure that stabilizes its tile and prevents movement.";
                break;

            case Item.Woodsaw:
                description = "A crafting station used for making advanced wooden objects.";
                break;

            case Item.Bridge:
                description = "A structure that allows walking over water tiles.";
                break;
        }
        return description;
    }
}
