using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class characterControl : MonoBehaviour
{
    [Header("Components")]
    public Rigidbody2D rb;
    public Animator anim;

    [Space]

    [Header("Debug")]
    public Vector2 dir;
    public float spd;

    [Space]

    [Header("Parameters")]
    public float SPD_MAX;
    public float POTION_DUR;

    public ItemObject HeldItem = null;
    public PlacedObject Inspecting { get; private set; } = null;

    private float airPylonTime = 0;
    private float airPotionTime = 0;
    private float earthPotionTime = 0;
    private float firePotionTime = 0;
    private float waterPotionTime = 0;

    public bool HasItem
    {
        get
        {
            return HeldItem != null;
        }
    }

    public bool IsInspecting
    {
        get
        {
            return Inspecting != null || Worldmap.Main.MainMenu.InMenu;
        }
    }

    public bool AirPylonEffect
    {
        get
        {
            return airPylonTime > 0;
        }
    }

    public bool AirEffect
    {
        get
        {
            return airPotionTime > 0;
        }
    }

    public bool EarthEffect
    {
        get
        {
            return earthPotionTime > 0;
        }
    }

    public bool FireEffect
    {
        get
        {
            return firePotionTime > 0;
        }
    }

    public bool WaterEffect
    {
        get
        {
            return waterPotionTime > 0;
        }
    }

    public Item Holding
    {
        get
        {
            if (HasItem)
            {
                return HeldItem.Type;
            }
            return Item.Nothing;
        }
    }

    void Update()
    {
        if (!IsInspecting)
        {
            ProcessInput();
            if (AirPylonEffect)
            {
                airPylonTime -= Time.deltaTime;
            }
            if (AirEffect)
            {
                airPotionTime -= Time.deltaTime;
            }
            if (EarthEffect)
            {
                earthPotionTime -= Time.deltaTime;
            }
            if (FireEffect)
            {
                firePotionTime -= Time.deltaTime;
            }
            if (WaterEffect)
            {
                waterPotionTime -= Time.deltaTime;
                ContactFilter2D mask = new ContactFilter2D();
                mask.SetLayerMask(LayerMask.GetMask("Items"));
                Collider2D[] hits = new Collider2D[25];
                int numHits = Physics2D.OverlapCircle(transform.position, 5, mask, hits);
                for (int i = 0; i < numHits; i++)
                {
                    Collider2D hit = hits[i];
                    if (hit.TryGetComponent(out ItemObject item))
                    {
                        item.PullToward(transform.position, 500 * Time.deltaTime);
                    }
                }
            }
        }
        else
        {
            dir = Vector2.zero;
        }
        ProcessMovement();
        ProcessAnimation();
    }

    void ProcessInput()
    {
        dir = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        spd = Mathf.Clamp(dir.magnitude, 0.0f, 1.0f);
        dir.Normalize();
    }

    void ProcessMovement()
    {
        rb.velocity = dir * spd * SPD_MAX * (AirEffect?1.5f:1f) * (AirPylonEffect?1.5f:1f);
    }

    void ProcessAnimation()
    {
        if (dir != Vector2.zero)
        {
            anim.SetFloat("Horiz", dir.x);
            anim.SetFloat("Vert", dir.y);
        }
        if (IsInspecting)
            anim.SetFloat("Spd", 0);
        else
            anim.SetFloat("Spd", spd);
    }

    public void GiveItem(ItemObject item)
    {
        if (!HasItem)
        {
            HeldItem = item;
            HeldItem.Bind(transform);
        }
    }

    public void ThrowItem()
    {
        if (HasItem)
        {
            Vector3 throwVector = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
            throwVector.z = 0;
            HeldItem.transform.position = transform.position;
            HeldItem.Throw(throwVector.normalized, 5 * (FireEffect?10:1));
            HeldItem = null;
        }
    }

    public void DropItem(bool delete = false)
    {
        if (HasItem)
        {
            HeldItem.Drop(delete);
            HeldItem = null;
        }
    }

    public void Inspect(PlacedObject station)
    {
        if (!IsInspecting)
        {
            Inspecting = station;
        }
    }

    public void StopInspecting()
    {
        Inspecting = null;
    }

    public void ApplyPotion(int type)
    {
        switch (type)
        {
            case 0:
                airPotionTime = POTION_DUR;
                break;

            case 1:
                earthPotionTime = POTION_DUR;
                break;

            case 2:
                firePotionTime = POTION_DUR;
                break;

            case 3:
                waterPotionTime = POTION_DUR;
                break;

            case 4:
                airPylonTime = 1;
                break;
        }
    }
}
