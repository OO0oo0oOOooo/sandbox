using UnityEngine;

public class SpawnSystem : MonoBehaviour
{
    public GameObject[] spawnPoints;
    public GameObject Player;
    
    void Start()
    {
        // Instantiate(PlayerPrefab, spawnPoints[Random.Range(0, spawnPoints.Length)].transform.position, Quaternion.identity);
        Player.SetActive(true);
    }
}
