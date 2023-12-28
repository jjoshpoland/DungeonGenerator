using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSpawner : MonoBehaviour
{
    public GameObject Character;
    public DungeonRenderer DR;

    GameObject startingSpawn;

    private void Start()
    {
        //DR.Generator.LoadDungeon();
        SpawnPlayerCharacter();
    }

    public void SetSpawnPoint()
    {
        startingSpawn = DR.SpawnTiles[Random.Range(0, DR.SpawnTiles.Count)];
    }

    public void SpawnPlayerCharacter()
    {
        if (DR.SpawnTiles.Count <= 0)
        {
            return;
        }
        else
        {
            SetSpawnPoint();
            GameObject newCharacter = Instantiate(Character);
            newCharacter.transform.position = startingSpawn.transform.position + Vector3.up;
        }
    }

    private void OnDrawGizmos()
    {
        if (startingSpawn != null)
        {
            Gizmos.color = Color.green;

            Gizmos.DrawWireSphere(startingSpawn.transform.position, 3f);
        }

    }
}
