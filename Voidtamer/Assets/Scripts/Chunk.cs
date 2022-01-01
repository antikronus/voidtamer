using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ChunkType
{
    Standard,
    Origin,
    Remnant,
    PlainCrystal,
    AirCrystal,
    EarthCrystal,
    FireCrystal,
    WaterCrystal,
    Silversteel,
    Clay,
    Stone,
    Obsidian
}

public class Chunk
{
    private Vector2 Pos;
    private List<List<Gametile>> Tiles;
    private int ObjIndex = -1;
    private bool Awake = false;

    public Chunk(Vector2 newPos, List<List<Gametile>> newTiles)
    {
        Pos = newPos;
        Tiles = newTiles;
    }

    public void SoftUpdate()
    {
        if (Awake)
        {
            bool Sleep = true;
            foreach (List<Gametile> column in Tiles)
            {
                foreach (Gametile tile in column)
                {
                    bool tileChange = tile.SoftUpdate();
                    Sleep = !tileChange && Sleep;
                }
            }
            if (Sleep)
            {
                Awake = false;
            }
        }
    }

    public void HardUpdate()
    {
        Awake = true;
        foreach (List<Gametile> column in Tiles)
        {
            foreach (Gametile tile in column)
            {
                tile.HardUpdate();
            }
        }
    }

    public Gametile FindTile(Vector2 Pos)
    {
        if(Pos.x < Tiles.Count && Pos.y < Tiles.Count)
        {
            return Tiles[Mathf.FloorToInt(Pos.x)][Mathf.FloorToInt(Pos.y)];
        }
        return null;
    }

    public void AssignObjects(Queue<GameObject> Objects, int Index)
    {
        ObjIndex = Index;
        Queue<GameObject> ToGive = new Queue<GameObject>(Objects);
        foreach(List<Gametile> column in Tiles)
        {
            foreach(Gametile element in column)
            {
                element.GiveObject(ToGive.Dequeue());
            }
        }
    }

    public int FreeObjects()
    {
        int oldIndex = ObjIndex;
        ObjIndex = -1;
        foreach (List<Gametile> column in Tiles)
        {
            foreach (Gametile element in column)
            {
                element.FreeObject();
            }
        }
        return oldIndex;
    }

    public void Wake()
    {
        Awake = true;
    }
}
