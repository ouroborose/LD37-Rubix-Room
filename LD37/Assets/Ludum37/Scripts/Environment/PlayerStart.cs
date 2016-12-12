using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStart : LevelCell {

    protected void Start()
    {
        Player player = FindObjectOfType<Player>();
        if(player != null)
        {
            player.transform.position = transform.position;
            player.transform.rotation = transform.rotation;
            player.UpdateGroundCell();
        }
    }
}
