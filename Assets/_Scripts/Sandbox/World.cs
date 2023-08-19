using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public int MaxWorldSizeInChunks = 10;
    public int MaxWorldHeightInChunks = 10;

    public NoiseSettings settings;
    
    public Texture2D[] TerrainTextures;

    [HideInInspector]
    public Texture2DArray TerrainTexArray;

    Dictionary<Vector3Int, Chunk> chunks = new Dictionary<Vector3Int, Chunk>();

    public bool _drawGizmo = false;

    void Awake()
    {
        PopulateTextureArray();
    }

    void Start()
    {
        GenerateWorld();
    }

    private void GenerateWorld()
    {
        // Instantiate multiple copies of the chunk object on x and z
        for (int x = 0; x < MaxWorldSizeInChunks; x++)
        {
            for (int y = 0; y < MaxWorldHeightInChunks; y++)
            {
                for (int z = 0; z < MaxWorldSizeInChunks; z++)
                {
                    Vector3Int position = new Vector3Int(x * Constants.CHUNK_WIDTH, y * Constants.CHUNK_HEIGHT, z * Constants.CHUNK_WIDTH); // Set this to chunksize/2
                
                    chunks.Add(position, new Chunk(position, this));
                    chunks[position]._chunkObject.transform.SetParent(transform);
                }
            }
        }
    }

    public TerrainPoint GetTerrainPoint(int x, float y, int z)
    {
        float terrainHeight = GetTerrainHeight(x, z);
        float surface = y - terrainHeight;
        
        int textureID = 0;
        
        if(y < terrainHeight - 3)
            textureID = 1;
        if(y < terrainHeight - 15)
            textureID = 2;

        return new TerrainPoint(surface, textureID);
    }

    // This should probably go in NoiseSettings Or Constants
    public float BaseTerrainHeight = 50f; // Minimum height of terrain.
	public float TerrainHeightRange = 80f; // The max height (above BaseTerrainHeight) our terrain can be.

    public float GetTerrainHeight(int x, int z)
	{
		// return (float)TerrainHeightRange * Mathf.PerlinNoise((float)x / Frequency * Amplitude + 0.001f, (float)z / Frequency * Amplitude + 0.001f) + BaseTerrainHeight;
		// return (float)TerrainHeightRange * Noise.Perlin2D((float)x, (float)z, Frequency, Amplitude) + BaseTerrainHeight;
		// return (float)TerrainHeightRange * Noise.FractalBrownianMotion(new Vector3(x, z, 0), Octaves, Frequency, Amplitude, Lacunarity, Gain) + BaseTerrainHeight;
		// return (float)TerrainHeightRange * Noise.DomainWarp(new Vector3(x, z, 0), settings.WarpStrength, settings.Octaves, settings.Frequency, settings.Amplitude, settings.Lacunarity, settings.Gain) + BaseTerrainHeight;

		float noise = Noise.DomainWarp(new Vector3(x, z, 0), settings.WarpStrength, settings.Octaves, settings.Frequency, settings.Amplitude, settings.Lacunarity, settings.Gain);
        noise = Noise.Redistribution(noise, settings);
        float surfaceHeight = Noise.RemapNoise01(noise, BaseTerrainHeight, TerrainHeightRange);

        return surfaceHeight;
	}

    public float GetTerrainDensity(int x, int y, int z)
	{
		return (float)TerrainHeightRange * Noise.Perlin3D(new Vector3(x, y, z), settings.Frequency, settings.Amplitude) + BaseTerrainHeight;
	}

    public Chunk GetChunkFromVector3(Vector3 pos)
    {
        int x = (int)pos.x;
        int y = (int)pos.y;
        int z = (int)pos.z;

        return chunks[new Vector3Int(x, y, z)];
    }

    public Chunk GetChunkFromExactVector3(Vector3 pos)
    {
        int chunkX = Mathf.FloorToInt(pos.x / Constants.CHUNK_WIDTH);
        int chunkY = Mathf.FloorToInt(pos.y / Constants.CHUNK_HEIGHT);
        int chunkZ = Mathf.FloorToInt(pos.z / Constants.CHUNK_WIDTH);

        Vector3Int chunkCoord = new Vector3Int(chunkX, chunkY, chunkZ) * Constants.CHUNK_WIDTH;

        if (chunks.ContainsKey(chunkCoord))
        {
            return chunks[chunkCoord];
        }
        else
        {
            return null;
        }
    }

    public void EditTerrainSphere(RaycastHit hit, float radius, float value)
    {
        Vector3 center = hit.point;
        List<Chunk> modifiedChunks = new List<Chunk>();

        // Calculate the bounds of the sphere
        int minX = Mathf.FloorToInt(center.x - radius);
        int maxX = Mathf.CeilToInt(center.x + radius);
        int minY = Mathf.FloorToInt(center.y - radius);
        int maxY = Mathf.CeilToInt(center.y + radius);
        int minZ = Mathf.FloorToInt(center.z - radius);
        int maxZ = Mathf.CeilToInt(center.z + radius);

        // Iterate over the points in the chunk's volume
        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                for (int z = minZ; z <= maxZ; z++)
                {
                    Chunk chunk = GetChunkFromExactVector3(new Vector3(x, y, z));
                    ModifyChunk(chunk, x, y, z, center, radius, value);
                    if(!modifiedChunks.Contains(chunk))
                        modifiedChunks.Add(chunk);

                    if(x % Constants.CHUNK_WIDTH == 0 && y % Constants.CHUNK_HEIGHT == 0 && z % Constants.CHUNK_WIDTH == 0)
                    {
                        Chunk chunk2 = GetChunkFromExactVector3(new Vector3(x - 1, y - 1, z - 1));
                        ModifyChunk(chunk2, x, y, z, center, radius, value);
                        if(!modifiedChunks.Contains(chunk2))
                            modifiedChunks.Add(chunk2);
                    }

                    if (x % Constants.CHUNK_WIDTH == 0 && y % Constants.CHUNK_HEIGHT == 0)
                    {
                        Chunk chunk2 = GetChunkFromExactVector3(new Vector3(x - 1, y - 1, z));
                        ModifyChunk(chunk2, x, y, z, center, radius, value);
                        if(!modifiedChunks.Contains(chunk2))
                            modifiedChunks.Add(chunk2);
                    }

                    if (z % Constants.CHUNK_WIDTH == 0 && y % Constants.CHUNK_HEIGHT == 0)
                    {
                        Chunk chunk2 = GetChunkFromExactVector3(new Vector3(x, y - 1, z - 1));
                        ModifyChunk(chunk2, x, y, z, center, radius, value);
                        if(!modifiedChunks.Contains(chunk2))
                            modifiedChunks.Add(chunk2);
                    }

                    if(x % Constants.CHUNK_WIDTH == 0 && z % Constants.CHUNK_WIDTH == 0)
                    {
                        Chunk chunk2 = GetChunkFromExactVector3(new Vector3(x - 1, y, z - 1));
                        ModifyChunk(chunk2, x, y, z, center, radius, value);
                        if(!modifiedChunks.Contains(chunk2))
                            modifiedChunks.Add(chunk2);
                    }
                    
                    if (x % Constants.CHUNK_WIDTH == 0)
                    {
                        Chunk chunk2 = GetChunkFromExactVector3(new Vector3(x - 1, y, z));
                        ModifyChunk(chunk2, x, y, z, center, radius, value);
                        if(!modifiedChunks.Contains(chunk2))
                            modifiedChunks.Add(chunk2);
                    }

                    if (y % Constants.CHUNK_HEIGHT == 0)
                    {
                        Chunk chunk2 = GetChunkFromExactVector3(new Vector3(x, y - 1, z ));
                        ModifyChunk(chunk2, x, y, z, center, radius, value);
                        if(!modifiedChunks.Contains(chunk2))
                            modifiedChunks.Add(chunk2);
                    }

                    if (z % Constants.CHUNK_WIDTH == 0)
                    {
                        Chunk chunk2 = GetChunkFromExactVector3(new Vector3(x, y, z - 1));
                        ModifyChunk(chunk2, x, y, z, center, radius, value);
                        if(!modifiedChunks.Contains(chunk2))
                            modifiedChunks.Add(chunk2);
                    }
                }
            }
        }

        foreach (var item in modifiedChunks)
        {
            item.MarchingCubesTaskJob();
        }
    }

    private void ModifyChunk(Chunk chunk, int x, int y, int z, Vector3 center, float radius, float value)
    {
        if (chunk != null)
        {
            Vector3Int localPos = new Vector3Int(x, y, z) - chunk.ChunkPosition;
            if (Vector3.Distance(new Vector3(x, y, z), center) <= radius)
            {
                // chunk.TerrainMap[localPos.x, localPos.y, localPos.z].DistanceToSurface += value;
                chunk.ModifyTerrainMap(localPos, value);
            }
        }
    }

    private void PopulateTextureArray()
    {
        TerrainTexArray = new Texture2DArray(1024, 1024, TerrainTextures.Length, TextureFormat.ARGB32, false);

        for (int i = 0; i < TerrainTextures.Length; i++)
        {
            TerrainTexArray.SetPixels(TerrainTextures[i].GetPixels(0), i, 0);
        }

        TerrainTexArray.Apply();
    }

    // This is example code of a more optimized EditTerrain Method
    // public void EditTerrainSphere(RaycastHit hit, float radius, float value)
    // {
    //     Vector3 center = hit.point;

    //     // Calculate the bounds of the sphere in world space
    //     int minX = Mathf.FloorToInt(center.x - radius);
    //     int maxX = Mathf.CeilToInt(center.x + radius);
    //     int minY = Mathf.FloorToInt(center.y - radius);
    //     int maxY = Mathf.CeilToInt(center.y + radius);
    //     int minZ = Mathf.FloorToInt(center.z - radius);
    //     int maxZ = Mathf.CeilToInt(center.z + radius);

    //     // Calculate the range of chunks that need to be modified
    //     int minChunkX = minX / ChunkData.Width;
    //     int maxChunkX = maxX / ChunkData.Width;
    //     int minChunkZ = minZ / ChunkData.Width;
    //     int maxChunkZ = maxZ / ChunkData.Width;

    //     // Iterate over the chunks that need to be modified
    //     for (int chunkX = minChunkX; chunkX <= maxChunkX; chunkX++)
    //     {
    //         for (int chunkZ = minChunkZ; chunkZ <= maxChunkZ; chunkZ++)
    //         {
    //             // Get the chunk at the current position
    //             Chunk chunk = GetChunkFromExactVector3(new Vector3(chunkX * ChunkData.Width, 0, chunkZ * ChunkData.Width));

    //             // Modify the chunk
    //             ModifyChunk(chunk, center, radius, value);
    //         }
    //     }
    // }

    // private void ModifyChunk(Chunk chunk, Vector3 center, float radius, float value)
    // {
    //     if (chunk != null)
    //     {
    //         // Calculate the bounds of the sphere in chunk space
    //         Vector3 localCenter = center - chunk.ChunkPosition;
    //         int minX = Mathf.FloorToInt(localCenter.x - radius);
    //         int maxX = Mathf.CeilToInt(localCenter.x + radius);
    //         int minY = Mathf.FloorToInt(localCenter.y - radius);
    //         int maxY = Mathf.CeilToInt(localCenter.y + radius);
    //         int minZ = Mathf.FloorToInt(localCenter.z - radius);
    //         int maxZ = Mathf.CeilToInt(localCenter.z + radius);

    //         // Iterate over the points in the chunk's volume
    //         for (int x = minX; x <= maxX; x++)
    //         {
    //             for (int y = minY; y <= maxY; y++)
    //             {
    //                 for (int z = minZ; z <= maxZ; z++)
    //                 {
    //                     if (Vector3.Distance(new Vector3(x, y, z), localCenter) <= radius)
    //                     {
    //                         chunk.ModifyTerrain(x, y, z, value);
    //                     }
    //                 }
    //             }
    //         }
    //     }
    // }

    void OnDrawGizmos()
    {
        if (_drawGizmo)
        {
            foreach (Chunk child in chunks.Values)
            {
                Gizmos.color = Color.green;

                Vector3 chunkDrawSize = new Vector3(Constants.CHUNK_WIDTH, Constants.CHUNK_HEIGHT, Constants.CHUNK_WIDTH);
                Vector3 chunkDrawCenterPostion = new Vector3(Constants.CHUNK_WIDTH / 2, Constants.CHUNK_HEIGHT / 2, Constants.CHUNK_WIDTH / 2) + child.ChunkPosition;
                Gizmos.DrawWireCube(chunkDrawCenterPostion, chunkDrawSize);
            }
        }
    }
}