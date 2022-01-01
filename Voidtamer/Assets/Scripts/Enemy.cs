using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyType
{
    VoidOrb
}

public class Enemy : MonoBehaviour
{
    public EnemyType Type;
    public float Health;
    public int MapListI = -1;
    bool Paused;
    Vector2 OldV;

    private void Awake()
    {
        if(Type == EnemyType.VoidOrb)
        {
            GetComponent<Rigidbody2D>().AddForce(new Vector2(Random.Range(-1f, 1f), Random.Range(-1, 1f)).normalized * 18);
        }
    }

    void Update()
    {
        if(Health <= 0)
        {
            Worldmap.Main.RemoveEnemy(this);
        }

        if(Type == EnemyType.VoidOrb)
        {
            Health -= Time.deltaTime;
        }
    }

    public void TogglePause()
    {
        if (Paused)
        {
            Paused = false;
            GetComponent<Rigidbody2D>().velocity = OldV;
        }
        else
        {
            Paused = true;
            OldV = GetComponent<Rigidbody2D>().velocity;
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        }
    }
}
