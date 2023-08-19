using UnityEngine;

public class WorldMarchingCubes : MonoBehaviour
{
    private int worldSizeInChunks = 5;

    public bool _drawGizmo = false;

    void Start()
    {
        // Instantiate multiple copies of the chunk object on x and z
        for (int x = 0; x < worldSizeInChunks; x++)
        {
            for (int z = 0; z < worldSizeInChunks; z++)
            {
                Vector3Int position = new Vector3Int(x * 16, 0, z * 16);
                MarchingCubes chunk = new MarchingCubes(position);
                chunk._chunkObject.transform.SetParent(transform);
            }
        }
    }

    void OnDrawGizmos()
    {
        if (_drawGizmo && transform.childCount > 0)
        {
            foreach (Transform child in transform)
            {
                Gizmos.color = Color.green;

                Vector3 chunkDrawSize = new Vector3(Constants.CHUNK_WIDTH, Constants.CHUNK_HEIGHT, Constants.CHUNK_WIDTH);
                Vector3 chunkDrawCenterPostion = new Vector3(Constants.CHUNK_WIDTH / 2, Constants.CHUNK_HEIGHT / 2, Constants.CHUNK_WIDTH / 2) + child.position;
                Gizmos.DrawWireCube(chunkDrawCenterPostion, chunkDrawSize);
            }
        }
    }
}