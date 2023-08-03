using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSpawner : MonoBehaviour
{
    public GameObject Character;
    public DungeonRenderer DR;

    private void Start()
    {
        SpawnPlayerCharacter();
    }

    public void SpawnPlayerCharacter()
    {
        if(DR.SpawnTiles.Count <= 0)
        {
            return;
        }
        else
        {
            GameObject startingSpawn = DR.SpawnTiles[Random.Range(0, DR.SpawnTiles.Count)];
            GameObject newCharacter = Instantiate(Character);
            newCharacter.transform.position = startingSpawn.transform.position + Vector3.up;
        }
    }
}
