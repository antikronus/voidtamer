using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recipe
{
    PlacedObject Station;
    public Item Output;
    public List<Item> Materials;
    public List<int> MsNeeded;
    public List<int> MsReceived;

    public Recipe(Item output, List<Item> materials, List<int> msNeeded)
    {
        Output = output;
        Materials = materials;
        MsNeeded = msNeeded;
        if (msNeeded.Count < materials.Count)
        {
            Materials = new List<Item>(Materials);
            Materials.RemoveRange(msNeeded.Count, msNeeded.Count - materials.Count);
        }
        MsReceived = new List<int>();
        foreach(Item element in Materials)
            MsReceived.Add(0);
    }

    public void AssignStation(PlacedObject station)
    {
        Station = station;
    }

    public bool TryAdd(Item item)
    {
        bool complete = true;
        bool inserted = false;

        for(int i = 0; i < Materials.Count; i++)
            if(MsNeeded[i] > MsReceived[i])
            {
                if(Materials[i] == item)
                {
                    MsReceived[i]++;
                    inserted = true;

                    if (MsNeeded[i] == MsReceived[i])
                    {
                        if (complete)
                            continue;
                        else
                            break;
                    }
                    else
                    {
                        complete = false;
                        break;
                    }
                }
                complete = false;
                if (inserted)
                    break;
            }

        if (complete)
            ProduceOutput();
        return inserted;
    }

    public void EjectContents()
    {
        for(int i = 0; i < MsReceived.Count; i++)
            while(MsReceived[i] > 0)
            {
                Station.ProduceItem(Materials[i]);
                MsReceived[i]--;
            }
    }

    private void ProduceOutput()
    {
        Station.ProduceItem(Output);
        for (int i = 0; i < MsReceived.Count; i++)
            MsReceived[i] = 0;
    }

    public string GetRecipeString()
    {
        string result = Output.ToString() + "\nMaterials:";
        for (int i = 0; i < MsReceived.Count; i++)
        {
            result += "\n";
            result += Materials[i].ToString();
            result += " ";
            result += MsReceived[i];
            result += "/";
            result += MsNeeded[i];
        }
        return result;
    }

    public Item NextRequiredItem(out int index)
    {
        Item nextRequired = Item.Nothing;
        index = -1;
        for (int i = 0; i < Materials.Count; i++)
            if (MsReceived[i] < MsNeeded[i])
            {
                nextRequired = Materials[i];
                index = i;
                break;
            }
        return nextRequired;
    }

    public int FindRequirementIndexByType(Item toFind)
    {
        int output = -1;
        for (int i = 0; i < Materials.Count; i++)
            if(Materials[i] == toFind)
            {
                output = i;
                break;
            }

        return output;
    }
}
