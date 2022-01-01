using System.Collections.Generic;
using UnityEngine;

public enum Task
{
    Idle,
    Collect,
    Craft
}

public enum MinionType
{
    Standard,
    Lumber
}

public enum MinionCollectionState
{
    NoItem,
    FollowPlayerNoItem,
    WaitAtStructure,
    HarvestNode,
    GrabItem,
    ItemObtained,
    FollowPlayerItemObtained,
    DepositItem
}

public enum MinionCraftingState
{
    FollowPlayerNoItem,
    WithdrawItem,
    HarvestNode,
    GrabItem,
    ItemObtained,
    AddItemToCraftingStation,
    ReturnItem,
    FollowPlayerItemObtained,
    DepositItem
}

public class MinionController : MonoBehaviour
{
    public Rigidbody2D rb;
    public Animator anim;

    float SPD_MAX = 3.75f;
    public Vector2 dir;
    public float spd;
    private MinionType mType;
    public Item Drops { get; private set; }
    public Item specialGathering { get; private set; } = Item.Nothing;
    public int MapListI = -1;
    public Task CurrentTask = Task.Idle;
    ItemObject Held = null;

    protected static MinionController LineTail = null;
    protected MinionController Leader = null;
    protected MinionController Follower = null;
    private bool InLine = false;

    private Stack<Vector2Int> Path = new Stack<Vector2Int>();
    private Vector2Int PathTargetPos;
    private float PathFindCooldown = 0;
    private Vector2Int LastLocation;
    private float TimeDelayed = 0;

    private readonly float SEEK_RANGE = 96f;

    //Collection Variables
    Item ToCollect = Item.Nothing;                                          //Also used in Crafting
    MinionCollectionState CollectionState = MinionCollectionState.NoItem;
    PlacedObject Producer = null;
    Vector2Int ProducerLocation;
    PlacedObject HarvestNode = null;                                        //Also used in Crafting
    Vector2Int HarvestNodeLocation;                                         //Also used in Crafting
    ItemObject GrabTarget = null;                                           //Also used in Crafting
    PlacedObject Container = null;                                          //Also used in Crafting
    Vector2Int ContainerLocation;                                           //Also used in Crafting

    //Crafting Variables
    MinionCraftingState CraftingState = MinionCraftingState.FollowPlayerNoItem;
    PlacedObject CraftingStation = null;
    Vector2Int CraftingStationLocation;
    Recipe CraftingRecipe = null;
    Item NextItem = Item.Nothing;
    int NextItemIndex = 0;
    List<PlacedObject> SupplyContainers = new List<PlacedObject> { null, null, null, null };
    List<Vector2Int> SupplyContainerLocations = new List<Vector2Int> { Vector2Int.zero, Vector2Int.zero, Vector2Int.zero, Vector2Int.zero };

    public MinionType Type
    {
        get
        {
            return mType;
        }
        set
        {
            mType = value;
            anim.SetInteger("Type", (int)value);
            switch (value)
            {
                case MinionType.Standard:
                    specialGathering = Item.Nothing;
                    Drops = Item.Minion;
                    break;

                case MinionType.Lumber:
                    specialGathering = Item.Lumber;
                    Drops = Item.LumberMinion;
                    break;
            }
        }
    }

    void Update()
    {
        spd = 0;
        if (PathFindCooldown > 0)
            PathFindCooldown -= Time.deltaTime;

        TaskUpdate();
        ProcessMovement();
        ProcessAnimation();
    }

    private void OnDestroy()
    {
        LeaveLine();
        if (Producer != null)
            Producer.Unsubscribe(this);
        if (HarvestNode != null)
            HarvestNode.Unsubscribe(this);
        if (Container != null)
            Container.Unsubscribe(this);
        if (CraftingStation != null)
            CraftingStation.Unsubscribe(this);

        foreach(PlacedObject container in SupplyContainers)
            if (container != null)
                container.Unsubscribe(this);
    }

    public void POSubscriberCallback(PlacedObject subscribedTo)
    {
        if (Producer == subscribedTo)
            Producer = null;
        if (HarvestNode == subscribedTo)
            HarvestNode = null;
        if (Container == subscribedTo)
        {
            Container = null;
        }
        if (CraftingStation == subscribedTo)
        {
            CraftingStation = null;
            CurrentTask = Task.Idle;
        }
        
        for(int i = 0; i < 4; i++)
            if (SupplyContainers[i] == subscribedTo)
            {
                SupplyContainers[i] = null;
            }
    }

    public string GetMinionTooltip()
    {
        string tooltip = "This minion is currently ";

        switch (CurrentTask)
        {
            case Task.Idle:
                tooltip += " idle.\n";
                tooltip += "Right-click this minion to pick it up, then throw it at something to give it a task. Until then, it will simply follow you around.";
                break;

            case Task.Collect:
                tooltip += " collecting " + ToCollect.GetName() + ".\n";
                switch (CollectionState)
                {
                    case MinionCollectionState.FollowPlayerNoItem:
                        tooltip += "It will follow you until it finds the item it's looking for or a harvesting node that produces it.";
                        break;

                    case MinionCollectionState.WaitAtStructure:
                        tooltip += "It will wait at the nearby production structure until it finds the item it's looking for.";
                        break;

                    case MinionCollectionState.HarvestNode:
                        tooltip += "It has found a harvesting node that produces the item it's looking for and is attempting to harvest it.";
                        break;

                    case MinionCollectionState.GrabItem:
                        tooltip += "It has found the item it's looking for and is attempting to grab it.";
                        break;

                    case MinionCollectionState.FollowPlayerItemObtained:
                        tooltip += "It is carrying the item it was looking for. It will follow you until you take the item from it, or it finds an appropriate container in which to place the item.";
                        break;

                    case MinionCollectionState.DepositItem:
                        tooltip += "It has found a place to deposit the item it's holding, and is attempting to do so.";
                        break;
                }
                break;

            case Task.Craft:
                tooltip += " crafting " + CraftingRecipe.Output.GetName() + "\n";
                switch (CraftingState)
                {
                    case MinionCraftingState.FollowPlayerNoItem:
                        tooltip += "It is currently looking for " + NextItem.GetName() + " and will follow you until it finds it, a harvesting node that produces it, or a container holding at least 2 copies of it.";
                        break;

                    case MinionCraftingState.WithdrawItem:
                        tooltip += "It has found a container holding " + NextItem.GetName() + " and will withdraw the item once the container has at least 2 copies of it.";
                        break;

                    case MinionCraftingState.HarvestNode:
                        tooltip += "It has found a harvesting node that produces the item it's looking for and is attempting to harvest it.";
                        break;

                    case MinionCraftingState.GrabItem:
                        tooltip += "It has found the item it's looking for and is attempting to grab it.";
                        break;

                    case MinionCraftingState.AddItemToCraftingStation:
                        tooltip += "It has found the next item in the recipe and is bringing it to the crafting station.";
                        break;

                    case MinionCraftingState.ReturnItem:
                        tooltip += "The item it's carrying is no longer needed for the recipe. The minion is putting it back where it found it.";
                        break;

                    case MinionCraftingState.FollowPlayerItemObtained:
                        tooltip += "It is carrying the item it was crafting. It will follow you until you take the item from it, or it finds an appropriate container in which to place the item.";
                        break;

                    case MinionCraftingState.DepositItem:
                        tooltip += "It has found a place to deposit the item it's holding, and is attempting to do so.";
                        break;
                }
                break;
        }

        return tooltip;
    }

    private void EnterLine()
    {
        if (!InLine)
        {
            Leader = LineTail;
            if (Leader != null)
                Leader.Follower = this;
            LineTail = this;
            InLine = true;
        }
    }

    private void LeaveLine()
    {
        if (InLine)
        {
            if (Follower == null)
                LineTail = Leader;
            else
                Follower.Leader = Leader;

            if (Leader != null)
                Leader.Follower = Follower;

            Leader = null;
            Follower = null;
            InLine = false;
        }
    }

    public void GiveTask(PlacedObject collidedWith = null)
    {
        CurrentTask = Task.Idle;
        LastLocation = new Vector2Int(Mathf.FloorToInt(transform.position.x + 0.5f), Mathf.FloorToInt(transform.position.y + 0.5f));

        if (collidedWith != null)
        {
            Vector2Int collidedWithLocation = new Vector2Int(Mathf.FloorToInt(collidedWith.Pos.x), Mathf.FloorToInt(collidedWith.Pos.y));
            if (collidedWith.resource && (!collidedWith.Drops.IsAdvancedHarvestable() || collidedWith.Drops == specialGathering))
            {
                CurrentTask = Task.Collect;
                ToCollect = collidedWith.Drops;
            }
            else if (collidedWith.ID == 24 && collidedWith.Contains != Item.Nothing)
            {
                CurrentTask = Task.Collect;
                ToCollect = collidedWith.Contains;
                Container = collidedWith;
                Container.Subscribe(this);
                ContainerLocation = collidedWithLocation;
            }
            else if (collidedWith.Produces != Item.Nothing)
            {
                CurrentTask = Task.Collect;
                ToCollect = collidedWith.Produces;
                Producer = collidedWith;
                Producer.Subscribe(this);
                ProducerLocation = collidedWithLocation;
            }
            else if (collidedWith.Crafting)
            {
                CurrentTask = Task.Craft;
                CraftingStation = collidedWith;
                CraftingStation.Subscribe(this);
                CraftingStationLocation = collidedWithLocation;
                CraftingRecipe = CraftingStation.ActiveRecipe;
                ToCollect = CraftingRecipe.Output;
            }
        }

        if (CurrentTask == Task.Idle)
            //Debug.Log("Minion is Idle");
        if (CurrentTask == Task.Collect)
        {
            CollectionState = MinionCollectionState.NoItem;
            //Debug.Log("Minion is Collecting " + ToCollect.ToString());
        }
        if (CurrentTask == Task.Craft)
        {
            CraftingState = MinionCraftingState.FollowPlayerNoItem;
            //Debug.Log("Minion is Crafting " + ToCollect.ToString());
        }
    }

    public void GiveItem(ItemObject item)
    {
        Held = item;
        Held.Bind(transform);
    }

    public ItemObject DropItem()
    {
        ItemObject Dropped = Held;
        if (Held != null)
        {
            Held.Drop(false);
            Held = null;
        }
        return Dropped;
    }

    void UpdateNextItem()
    {
        if (CraftingRecipe != null)
            NextItem = CraftingRecipe.NextRequiredItem(out NextItemIndex);
    }

    void TaskUpdate()
    {
        switch (CurrentTask)
        {
            case Task.Collect:
                CollectionTask();
                break;

            case Task.Craft:
                CraftingTask();
                break;

            default:
                SeekPlayer();
                break;
        }
    }

    void ProcessMovement()
    {
        Vector3 toMove = transform.position + (Vector3)(dir * (spd + 0.2f));
        Gametile targetTile = Worldmap.Main.FindTile(new Vector2(toMove.x + 0.5f, toMove.y + 0.5f));
        //if (targetTile.Traversable || Path.Count > 0)
            transform.position += (Vector3)(dir * spd);
    }

    void ProcessAnimation()
    {
        if (dir != Vector2.zero)
        {
            anim.SetFloat("Horiz", dir.x);
            anim.SetFloat("Vert", dir.y);
        }
        anim.SetFloat("Spd", (anim.GetFloat("Spd") + spd) / 2.0f);
    }

    void SeekPlayer()
    {
        if (InLine)
        {
            if (Leader == null)
                MoveToTarget(Worldmap.Main.player.position);
            else if ((Leader.transform.position - transform.position).sqrMagnitude <= Mathf.Pow(SEEK_RANGE, 2))
                MoveToTarget(Leader.transform.position);
            else
                LeaveLine();
        }
        else if ((LineTail == null && (Worldmap.Main.player.transform.position - transform.position).sqrMagnitude <= Mathf.Pow(SEEK_RANGE, 2))||(LineTail != null && (LineTail.transform.position - transform.position).sqrMagnitude <= Mathf.Pow(SEEK_RANGE, 2)))
            EnterLine();
    }

    bool DetectItem(Item targetItem)
    {
        bool found = false;
        ContactFilter2D mask = new ContactFilter2D();
        mask.SetLayerMask(LayerMask.GetMask("Items"));
        Collider2D[] hits = new Collider2D[25];
        int numHits = Physics2D.OverlapCircle(transform.position, 2, mask, hits);
        for (int i = 0; i < numHits; i++)
        {
            Collider2D hit = hits[i];
            if (hit.TryGetComponent(out ItemObject item) && item.Type == targetItem && item.ThrowCD <= 0)
            {
                GrabTarget = item;
                found = true;
                break;
            }
        }
        return found;
    }

    bool DetectItem(Item primaryTarget, Item secondaryTarget)
    {
        bool found = false;
        ContactFilter2D mask = new ContactFilter2D();
        mask.SetLayerMask(LayerMask.GetMask("Items"));
        Collider2D[] hits = new Collider2D[25];
        int numHits = Physics2D.OverlapCircle(transform.position, 2, mask, hits);
        for (int i = 0; i < numHits; i++)
        {
            Collider2D hit = hits[i];
            if (hit.TryGetComponent(out ItemObject item) && (item.Type == primaryTarget || item.Type == secondaryTarget) && item.ThrowCD <= 0)
            {
                GrabTarget = item;
                found = true;
                if(item.Type == primaryTarget)
                    break;
            }
        }
        return found;
    }

    bool DetectResourceNode(Item targetItem)
    {
        bool found = false;
        if (!targetItem.IsAdvancedHarvestable() || specialGathering == targetItem)
        {
            ContactFilter2D mask = new ContactFilter2D();
            mask.SetLayerMask(LayerMask.GetMask("Resources", "ResourceObstacle"));
            Collider2D[] hits = new Collider2D[25];
            int numHits = Physics2D.OverlapCircle(transform.position, 2, mask, hits);
            for (int i = 0; i < numHits; i++)
            {
                Collider2D hit = hits[i];
                if (hit.TryGetComponent(out FeatureAssistor node) && node.LinkedObject.Drops == targetItem)
                {
                    HarvestNode = node.LinkedObject;
                    HarvestNode.Subscribe(this);
                    HarvestNodeLocation = new Vector2Int(Mathf.FloorToInt(node.transform.position.x), Mathf.FloorToInt(node.transform.position.y));
                    found = true;
                    break;
                }
            }
        }
        return found;
    }

    bool DetectContainer(Item targetItem)
    {
        bool found = false;
        ContactFilter2D mask = new ContactFilter2D();
        mask.SetLayerMask(LayerMask.GetMask("PlacedObjects"));
        Collider2D[] hits = new Collider2D[25];
        int numHits = Physics2D.OverlapCircle(transform.position, 2, mask, hits);
        for (int i = 0; i < numHits; i++)
        {
            Collider2D hit = hits[i];
            if (hit.TryGetComponent(out FeatureAssistor container) && container.LinkedObject.ID == 24 && container.LinkedObject.Contains == targetItem)
            {
                Container = container.LinkedObject;
                Container.Subscribe(this);
                ContainerLocation = new Vector2Int(Mathf.FloorToInt(container.transform.position.x), Mathf.FloorToInt(container.transform.position.y));
                found = true;
                break;
            }
        }
        return found;
    }

    bool DetectContainer(Item targetItem, int recipeIndex)
    {
        bool found = false;
        ContactFilter2D mask = new ContactFilter2D();
        mask.SetLayerMask(LayerMask.GetMask("PlacedObjects"));
        Collider2D[] hits = new Collider2D[25];
        int numHits = Physics2D.OverlapCircle(transform.position, 2, mask, hits);
        for (int i = 0; i < numHits; i++)
        {
            Collider2D hit = hits[i];
            if (hit.TryGetComponent(out FeatureAssistor container) && container.LinkedObject.ID == 24 && container.LinkedObject.Contains == targetItem)
            {
                SupplyContainers[recipeIndex] = container.LinkedObject;
                SupplyContainers[recipeIndex].Subscribe(this);
                SupplyContainerLocations[recipeIndex] = new Vector2Int(Mathf.FloorToInt(container.transform.position.x), Mathf.FloorToInt(container.transform.position.y));
                found = true;
                break;
            }
        }
        return found;
    }

    void MoveToTarget(Vector2 targetPos)
    {
        Vector2Int currentPositionInt = new Vector2Int(Mathf.FloorToInt(transform.position.x + 0.5f), Mathf.FloorToInt(transform.position.y + 0.5f));
        Vector2Int targetPositionInt = new Vector2Int(Mathf.FloorToInt(targetPos.x + 0.5f), Mathf.FloorToInt(targetPos.y + 0.5f));
        if(currentPositionInt == LastLocation && (currentPositionInt - targetPositionInt).sqrMagnitude > 1)
        {
            TimeDelayed += Time.deltaTime;
        }
        else
        {
            TimeDelayed = 0;
            LastLocation = currentPositionInt;
        }

        if (PathFindCooldown <= 0 && Worldmap.Main.FindTile(currentPositionInt).Traversable && (Path.Count == 0 || PathTargetPos != targetPositionInt) && (OctalDistance(targetPositionInt, currentPositionInt) >= 5 || TimeDelayed > 2f))
        {
            Stack<Vector2Int> newPath = FindPath(currentPositionInt, targetPositionInt);
            if (newPath.Count > 0)
            {
                newPath.Pop();
                Path = newPath;
                PathTargetPos = targetPositionInt;
            }
        }
        //else if(PathFindCooldown > 0 && Worldmap.Main.FindTile(currentPositionInt).Traversable && (Path.Count == 0 || PathTargetPos != targetPositionInt) && (OctalDistance(targetPositionInt, currentPositionInt) >= 5 || TimeDelayed > 2f))
        //{
        //    Debug.Log("I want to find a path, but I'm on cooldown.");
        //    Debug.Log("Path count = " + Path.Count + ", Path Target Position = " + PathTargetPos.ToString() + ", Target Position = " + targetPositionInt.ToString());
        //}
        //else if (PathFindCooldown <= 0 && !Worldmap.Main.FindTile(currentPositionInt).Traversable && (Path.Count == 0 || PathTargetPos != targetPositionInt) && (OctalDistance(targetPositionInt, currentPositionInt) >= 5 || TimeDelayed > 2f))
        //{
        //    Debug.Log("I want to find a path, but my location " + currentPositionInt.ToString() + " is not traversable.");
        //}

        float mag = 0;
        if(Path.Count > 0)
        {
            Vector2Int nextPathNode = Path.Peek();
            dir = nextPathNode - (Vector2)transform.position;
            mag = dir.magnitude;
            while (mag < SPD_MAX * Time.deltaTime && Path.Count > 0)
            {
                Vector2Int prevPathNode = Path.Pop();
                if (Path.Count > 0)
                {
                    nextPathNode = Path.Peek();
                    float remainingMove = SPD_MAX * Time.deltaTime - mag;
                    Vector2 tempDir = nextPathNode - prevPathNode;
                    if (tempDir.magnitude >= remainingMove)
                    {
                        dir = (prevPathNode + tempDir.normalized * remainingMove) - (Vector2)transform.position;
                        mag += remainingMove;
                    }
                    else
                    {
                        mag += tempDir.magnitude;
                    }
                }
            }
            spd = Mathf.Clamp(dir.magnitude, 0, SPD_MAX * Time.deltaTime);
            dir.Normalize();
        }
        else
        {
            dir = targetPos - (Vector2)transform.position;
            mag = dir.magnitude - 0.9f;
            dir.Normalize();
            spd = Mathf.Clamp(mag, 0, SPD_MAX * Time.deltaTime);
        }
    }

    Stack<Vector2Int> FindPath(Vector2Int currentPos, Vector2Int targetPos)
    {
        Stack<Vector2Int> path = new Stack<Vector2Int>();

        float octDistance = OctalDistance(currentPos, targetPos);

        if(octDistance <= SEEK_RANGE)
        {
            Vector2Int mapMin = new Vector2Int(Mathf.Min(targetPos.x, currentPos.x) - Worldmap.Main.CHUNK_SIZE, Mathf.Min(targetPos.y, currentPos.y) - Worldmap.Main.CHUNK_SIZE);
            Vector2Int mapMax = new Vector2Int(Mathf.Max(targetPos.x, currentPos.x) + Worldmap.Main.CHUNK_SIZE, Mathf.Max(targetPos.y, currentPos.y) + Worldmap.Main.CHUNK_SIZE);

            float maxCost = (mapMax.x - mapMin.x) * (mapMax.y - mapMin.y);

            PriorityQueue<Vector2Int, float> openSet = new PriorityQueue<Vector2Int, float>();

            List<List<Vector2Int>> source = new List<List<Vector2Int>>();
            List<List<float>> gScore = new List<List<float>>();

            for (int x = mapMin.x; x <= mapMax.x; x++)
            {
                List<Vector2Int> sourceCol = new List<Vector2Int>();
                List<float> gScoreCol = new List<float>();

                for (int y = mapMin.y; y <= mapMax.y; y++)
                {
                    sourceCol.Add(currentPos);
                    gScoreCol.Add(maxCost);
                }

                source.Add(sourceCol);
                gScore.Add(gScoreCol);
            }

            gScore[currentPos.x - mapMin.x][currentPos.y - mapMin.y] = 0;
            openSet.Insert(currentPos, octDistance);

            while (!openSet.IsEmpty)
            {
                Vector2Int current = openSet.Pop();

                if(current != targetPos)
                {
                    float gPlusOne = gScore[current.x - mapMin.x][current.y - mapMin.y] + 1;
                    float gPlusRootTwo = gPlusOne + 0.414f;

                    Vector2Int eastNeighbor = current + Vector2Int.right;
                    Vector2Int southNeighbor = current + Vector2Int.down;
                    Vector2Int westNeighbor = current + Vector2Int.left;
                    Vector2Int northNeighbor = current + Vector2Int.up;

                    if(eastNeighbor.x <= mapMax.x && (Worldmap.Main.FindTile(eastNeighbor).Traversable || eastNeighbor == targetPos))
                    {
                        if(gScore[eastNeighbor.x - mapMin.x][eastNeighbor.y - mapMin.y] == maxCost)
                        {
                            source[eastNeighbor.x - mapMin.x][eastNeighbor.y - mapMin.y] = current;
                            gScore[eastNeighbor.x - mapMin.x][eastNeighbor.y - mapMin.y] = gPlusOne;
                            openSet.Insert(eastNeighbor, gPlusOne + OctalDistance(eastNeighbor, targetPos));
                        }

                        Vector2Int northEastNeighbor = eastNeighbor + Vector2Int.up;

                        if(Worldmap.Main.FindTile(northNeighbor).Traversable && current != currentPos && northEastNeighbor.y <= mapMax.y && gScore[northEastNeighbor.x - mapMin.x][northEastNeighbor.y - mapMin.y] == maxCost && (Worldmap.Main.FindTile(northEastNeighbor).Traversable || northEastNeighbor == targetPos))
                        {
                            source[northEastNeighbor.x - mapMin.x][northEastNeighbor.y - mapMin.y] = current;
                            gScore[northEastNeighbor.x - mapMin.x][northEastNeighbor.y - mapMin.y] = gPlusRootTwo;
                            openSet.Insert(northEastNeighbor, gPlusRootTwo + OctalDistance(northEastNeighbor, targetPos));
                        }
                    }

                    if (southNeighbor.y >= mapMin.y && (Worldmap.Main.FindTile(southNeighbor).Traversable || southNeighbor == targetPos))
                    {
                        if(gScore[southNeighbor.x - mapMin.x][southNeighbor.y - mapMin.y] == maxCost)
                        {
                            source[southNeighbor.x - mapMin.x][southNeighbor.y - mapMin.y] = current;
                            gScore[southNeighbor.x - mapMin.x][southNeighbor.y - mapMin.y] = gPlusOne;
                            openSet.Insert(southNeighbor, gPlusOne + OctalDistance(southNeighbor, targetPos));
                        }

                        Vector2Int southEastNeighbor = southNeighbor + Vector2Int.right;

                        if(Worldmap.Main.FindTile(eastNeighbor).Traversable && current != currentPos && southEastNeighbor.x <= mapMax.x && gScore[southEastNeighbor.x - mapMin.x][southEastNeighbor.y - mapMin.y] == maxCost && (Worldmap.Main.FindTile(southEastNeighbor).Traversable || southEastNeighbor == targetPos))
                        {
                            source[southEastNeighbor.x - mapMin.x][southEastNeighbor.y - mapMin.y] = current;
                            gScore[southEastNeighbor.x - mapMin.x][southEastNeighbor.y - mapMin.y] = gPlusRootTwo;
                            openSet.Insert(southEastNeighbor, gPlusRootTwo + OctalDistance(southEastNeighbor, targetPos));
                        }
                    }

                    if (westNeighbor.x >= mapMin.x && (Worldmap.Main.FindTile(westNeighbor).Traversable || westNeighbor == targetPos))
                    {
                        if(gScore[westNeighbor.x - mapMin.x][westNeighbor.y - mapMin.y] == maxCost)
                        {
                            source[westNeighbor.x - mapMin.x][westNeighbor.y - mapMin.y] = current;
                            gScore[westNeighbor.x - mapMin.x][westNeighbor.y - mapMin.y] = gPlusOne;
                            openSet.Insert(westNeighbor, gPlusOne + OctalDistance(westNeighbor, targetPos));
                        }

                        Vector2Int southWestNeighbor = westNeighbor + Vector2Int.down;

                        if(Worldmap.Main.FindTile(southNeighbor).Traversable && current != currentPos && southWestNeighbor.y >= mapMin.y && gScore[southWestNeighbor.x - mapMin.x][southWestNeighbor.y - mapMin.y] == maxCost && (Worldmap.Main.FindTile(southWestNeighbor).Traversable || southWestNeighbor == targetPos))
                        {
                            source[southWestNeighbor.x - mapMin.x][southWestNeighbor.y - mapMin.y] = current;
                            gScore[southWestNeighbor.x - mapMin.x][southWestNeighbor.y - mapMin.y] = gPlusRootTwo;
                            openSet.Insert(southWestNeighbor, gPlusRootTwo + OctalDistance(southWestNeighbor, targetPos));
                        }
                    }

                    if (northNeighbor.y <= mapMax.y && (Worldmap.Main.FindTile(northNeighbor).Traversable || northNeighbor == targetPos))
                    {
                        if(gScore[northNeighbor.x - mapMin.x][northNeighbor.y - mapMin.y] == maxCost)
                        {
                            source[northNeighbor.x - mapMin.x][northNeighbor.y - mapMin.y] = current;
                            gScore[northNeighbor.x - mapMin.x][northNeighbor.y - mapMin.y] = gPlusOne;
                            openSet.Insert(northNeighbor, gPlusOne + OctalDistance(northNeighbor, targetPos));
                        }

                        Vector2Int northWestNeighbor = northNeighbor + Vector2Int.left;

                        if(Worldmap.Main.FindTile(westNeighbor).Traversable && current != currentPos && northWestNeighbor.x >= mapMin.x && gScore[northWestNeighbor.x - mapMin.x][northWestNeighbor.y - mapMin.y] == maxCost && (Worldmap.Main.FindTile(northWestNeighbor).Traversable || northWestNeighbor == targetPos))
                        {
                            source[northWestNeighbor.x - mapMin.x][northWestNeighbor.y - mapMin.y] = current;
                            gScore[northWestNeighbor.x - mapMin.x][northWestNeighbor.y - mapMin.y] = gPlusRootTwo;
                            openSet.Insert(northWestNeighbor, gPlusRootTwo + OctalDistance(northWestNeighbor, targetPos));
                        }
                    }
                }
                else
                {
                    while(current != currentPos)
                    {
                        current = source[current.x - mapMin.x][current.y - mapMin.y];
                        path.Push(current);
                    }
                    break;
                }
            }
        }
        else
        {
            //Debug.Log("Outside of pathing range");
        }
        PathFindCooldown = 1;
        if(path.Count <= 0)
        {
            //Debug.Log("Pathfinding failed.");
        }
        return path;
    }

    private float OctalDistance(Vector2Int posA, Vector2Int posB)
    {
        int xDistance = Mathf.Abs(posA.x - posB.x);
        int yDistance = Mathf.Abs(posA.y - posB.y);
        return Mathf.Max(xDistance, yDistance) + 0.414f * Mathf.Min(xDistance, yDistance);
    }

    private void CollectionTask()
    {
        switch (CollectionState)
        {
            case MinionCollectionState.NoItem:
                CS_NoItem();
                break;

            case MinionCollectionState.FollowPlayerNoItem:
                CS_FollowPlayerNoItem();
                break;

            case MinionCollectionState.WaitAtStructure:
                CS_WaitAtStructure();
                break;

            case MinionCollectionState.HarvestNode:
                CS_HarvestNode();
                break;

            case MinionCollectionState.GrabItem:
                CS_GrabItem();
                break;

            case MinionCollectionState.ItemObtained:
                CS_ItemObtained();
                break;

            case MinionCollectionState.FollowPlayerItemObtained:
                CS_FollowPlayerItemObtained();
                break;

            case MinionCollectionState.DepositItem:
                CS_DepositItem();
                break;
        }
    }

    private void CraftingTask()
    {
        switch (CraftingState)
        {
            case MinionCraftingState.FollowPlayerNoItem:
                CrS_FollowPlayerNoItem();
                break;

            case MinionCraftingState.WithdrawItem:
                CrS_WithdrawItem();
                break;

            case MinionCraftingState.HarvestNode:
                CrS_HarvestNode();
                break;

            case MinionCraftingState.GrabItem:
                CrS_GrabItem();
                break;

            case MinionCraftingState.ItemObtained:
                CrS_ItemObtained();
                break;

            case MinionCraftingState.AddItemToCraftingStation:
                CrS_AddItemToCraftingStation();
                break;

            case MinionCraftingState.ReturnItem:
                CrS_ReturnItem();
                break;

            case MinionCraftingState.FollowPlayerItemObtained:
                CrS_FollowPlayerItemObtained();
                break;

            case MinionCraftingState.DepositItem:
                CrS_DepositItem();
                break;
        }
    }

    private Vector2Int PositionToTileLocation(Vector2 position)
    {
        return new Vector2Int(Mathf.FloorToInt(position.x + 0.5f), Mathf.FloorToInt(position.y + 0.5f));
    }

    //Collection Subtasks

    private void CS_NoItem()
    {
        CollectionState = MinionCollectionState.NoItem;

        if (Producer != null && OctalDistance(ProducerLocation, PositionToTileLocation(transform.position)) <= SEEK_RANGE)
            CS_WaitAtStructure();
        else
            CS_FollowPlayerNoItem();
    }

    private void CS_FollowPlayerNoItem()
    {
        CollectionState = MinionCollectionState.FollowPlayerNoItem;
        
        if (Producer != null && OctalDistance(ProducerLocation, PositionToTileLocation(transform.position)) <= SEEK_RANGE)
            CS_WaitAtStructure();
        else if(DetectItem(ToCollect))
            CS_GrabItem();
        else if (DetectResourceNode(ToCollect))
            CS_HarvestNode();
        else
            SeekPlayer();

        if (CollectionState != MinionCollectionState.FollowPlayerNoItem)
            LeaveLine();
    }

    private void CS_WaitAtStructure()
    {
        CollectionState = MinionCollectionState.WaitAtStructure;

        if (DetectItem(ToCollect))
            CS_GrabItem();
        else if (Producer == null)
            CS_FollowPlayerNoItem();
        else if ((ProducerLocation - (Vector2)transform.position).sqrMagnitude > 1f)
            MoveToTarget(ProducerLocation);
    }

    private void CS_HarvestNode()
    {
        CollectionState = MinionCollectionState.HarvestNode;

        if (HarvestNode == null)
            CS_FollowPlayerNoItem();
        else if ((HarvestNodeLocation - (Vector2)transform.position).sqrMagnitude > 1f)
            MoveToTarget(HarvestNodeLocation);
        else
            Worldmap.Main.TryHarvest(this, HarvestNodeLocation);
    }

    private void CS_GrabItem()
    {
        CollectionState = MinionCollectionState.GrabItem;

        if (GrabTarget == null || GrabTarget.Owner != null)
            CS_FollowPlayerNoItem();
        else if ((GrabTarget.transform.position - transform.position).sqrMagnitude > 1f)
            MoveToTarget(GrabTarget.transform.position);
        else
        {
            GiveItem(GrabTarget);
            CS_ItemObtained();
        }
    } 

    private void CS_ItemObtained()
    {
        CollectionState = MinionCollectionState.ItemObtained;

        if (Container != null && OctalDistance(ContainerLocation, PositionToTileLocation(transform.position)) <= SEEK_RANGE)
            CS_DepositItem();
        else
            CS_FollowPlayerItemObtained();
    }

    private void CS_FollowPlayerItemObtained()
    {
        CollectionState = MinionCollectionState.FollowPlayerItemObtained;

        if (Held == null)
            CS_NoItem();
        else if ((Container != null && OctalDistance(ContainerLocation, PositionToTileLocation(transform.position)) <= SEEK_RANGE) || (Container == null && DetectContainer(ToCollect)))
            CS_DepositItem();
        else
            SeekPlayer();

        if (CollectionState != MinionCollectionState.FollowPlayerItemObtained)
            LeaveLine();
    }

    private void CS_DepositItem()
    {
        CollectionState = MinionCollectionState.DepositItem;

        if (Held == null)
            CS_NoItem();
        else if (Container == null)
            CS_FollowPlayerItemObtained();
        else if ((ContainerLocation - (Vector2)transform.position).sqrMagnitude > 1f)
            MoveToTarget(ContainerLocation);
        else if (Container.TryAddItem(Held))
        {
            Destroy(Held.gameObject);
            Held = null;
            CS_NoItem();
        }
        else
        {
            Container.Unsubscribe(this);
            Container = null;
            CS_FollowPlayerItemObtained();
        }
    }

    //Crafting Subtasks

    private void CrS_FollowPlayerNoItem()
    {
        CraftingState = MinionCraftingState.FollowPlayerNoItem;
        UpdateNextItem();

        if (DetectItem(ToCollect, NextItem))
            CrS_GrabItem();
        else if (SupplyContainers[NextItemIndex] != null || DetectContainer(NextItem, NextItemIndex))
            CrS_WithdrawItem();
        else if (DetectResourceNode(NextItem))
            CrS_HarvestNode();
        else
            SeekPlayer();

        if (CraftingState != MinionCraftingState.FollowPlayerNoItem)
            LeaveLine();
    }

    private void CrS_WithdrawItem()
    {
        CraftingState = MinionCraftingState.WithdrawItem;
        UpdateNextItem();

        if (DetectItem(ToCollect, NextItem))
            CrS_GrabItem();
        else if (SupplyContainers[NextItemIndex] == null)
            CrS_FollowPlayerNoItem();
        else if((SupplyContainerLocations[NextItemIndex] - (Vector2)transform.position).sqrMagnitude > 1f)
        {   //Add memorized paths here
            MoveToTarget(SupplyContainerLocations[NextItemIndex]);
        }
        else if(SupplyContainers[NextItemIndex].Quantity > 1)
        {
            SupplyContainers[NextItemIndex].HarvestItem(out ItemObject item);
            GiveItem(item);
            CrS_ItemObtained();
        }
    }

    private void CrS_HarvestNode()
    {
        CraftingState = MinionCraftingState.HarvestNode;

        if (HarvestNode == null)
            CrS_FollowPlayerNoItem();
        else if ((HarvestNodeLocation - (Vector2)transform.position).sqrMagnitude > 1f)
            MoveToTarget(HarvestNodeLocation);
        else
            Worldmap.Main.TryHarvest(this, HarvestNodeLocation);
    }

    private void CrS_GrabItem()
    {
        CraftingState = MinionCraftingState.GrabItem;

        if (GrabTarget == null || GrabTarget.Owner != null)
            CrS_FollowPlayerNoItem();
        else if ((GrabTarget.transform.position - transform.position).sqrMagnitude > 1f)
            MoveToTarget(GrabTarget.transform.position);
        else
        {
            GiveItem(GrabTarget);
            CrS_ItemObtained();
        }
    }

    private void CrS_ItemObtained()
    {
        CraftingState = MinionCraftingState.ItemObtained;
        UpdateNextItem();

        if (Held.Type == ToCollect)
        {
            if (Container != null && OctalDistance(ContainerLocation, PositionToTileLocation(transform.position)) <= SEEK_RANGE)
            {
                //Debug.Log("I have obtained my crafted item and will attempt to deposit it.");
                CrS_DepositItem();
            }
            else
            {
                //Debug.Log("I have obtained my crafted item and will now attempt to follow the player.");
                CrS_FollowPlayerItemObtained();
            }
        }
        else if (Held.Type == NextItem)
            CrS_AddItemToCraftingStation();
        else
            CrS_ReturnItem();
    }

    private void CrS_AddItemToCraftingStation()
    {
        CraftingState = MinionCraftingState.AddItemToCraftingStation;
        UpdateNextItem();

        if (Held == null)
            CrS_FollowPlayerNoItem();
        else if (Held.Type != NextItem)
            CrS_ReturnItem();
        else if ((CraftingStationLocation - (Vector2)transform.position).sqrMagnitude > 1f)
            MoveToTarget(CraftingStationLocation);
        else if (CraftingStation.TryAddItem(Held))
        {
            Destroy(Held.gameObject);
            Held = null;
        }
    }

    private void CrS_ReturnItem()
    {
        CraftingState = MinionCraftingState.ReturnItem;

        if (Held == null)
            CrS_FollowPlayerNoItem();
        else
        {
            int index = CraftingRecipe.FindRequirementIndexByType(Held.Type);
            if (index == -1 || SupplyContainers[index] == null)
                DropItem();
            else if ((SupplyContainerLocations[index] - (Vector2)transform.position).sqrMagnitude > 1f)
                MoveToTarget(SupplyContainerLocations[index]);
            else if (SupplyContainers[index].TryAddItem(Held))
            {
                Destroy(Held.gameObject);
                Held = null;
            }
            else
            {
                SupplyContainers[index].Unsubscribe(this);
                SupplyContainers[index] = null;
            }
        }
    }

    private void CrS_FollowPlayerItemObtained()
    {
        CraftingState = MinionCraftingState.FollowPlayerItemObtained;

        if (Held == null)
            CrS_FollowPlayerNoItem();
        else if ((Container != null && OctalDistance(ContainerLocation, PositionToTileLocation(transform.position)) <= SEEK_RANGE) || (Container == null && DetectContainer(ToCollect)))
            CrS_DepositItem();
        else
            SeekPlayer();

        if (CraftingState != MinionCraftingState.FollowPlayerItemObtained)
            LeaveLine();
    }

    private void CrS_DepositItem()
    {
        CraftingState = MinionCraftingState.DepositItem;

        if (Held == null)
            CrS_FollowPlayerNoItem();
        else if (Container == null)
            CrS_FollowPlayerItemObtained();
        else if ((ContainerLocation - (Vector2)transform.position).sqrMagnitude > 1f)
            MoveToTarget(ContainerLocation);
        else if (Container.TryAddItem(Held))
        {
            Destroy(Held.gameObject);
            Held = null;
        }
        else
        {
            Container.Unsubscribe(this);
            Container = null;
        }
    }
}
