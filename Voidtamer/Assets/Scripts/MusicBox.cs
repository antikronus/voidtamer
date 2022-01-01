using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicBox : MonoBehaviour
{
    public static MusicBox Main;
    public List<AudioClip> Music;
    public float CooldownRemain = 200;
    int LastPlayed = 0;

    void Start()
    {
        if (Main != null)
            Destroy(gameObject);
        else
        {
            Main = this;
            DontDestroyOnLoad(gameObject);
            GetComponent<AudioSource>().PlayOneShot(Music[0]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Worldmap.Main.player.position;
        if(CooldownRemain > 0)
        {
            CooldownRemain -= Time.deltaTime;
        }
        else
        {
            int PlayNext = Random.Range(1, 4) + LastPlayed;
            PlayNext %= 4;
            GetComponent<AudioSource>().PlayOneShot(Music[PlayNext]);
            LastPlayed = PlayNext;
            CooldownRemain = 150;
        }
    }
}
