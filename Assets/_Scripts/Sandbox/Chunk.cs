using UnityEngine;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

public class Chunk
{
    private World _world;

    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    private MeshCollider _meshCollider;

    private int _width { get { return Constants.CHUNK_WIDTH; } }
	private int _height { get { return Constants.CHUNK_HEIGHT; } }
	private float _terrainSurface { get { return Constants.TERRAIN_SURFACE; } }

    private bool _flatShaded = true;

    public TerrainPoint[] TerrainMap;
    Mesh _mesh;

    public Vector3Int ChunkPosition;
    public GameObject _chunkObject;

    public Chunk(Vector3Int pos, World w)
    {
        _world = w;

        _chunkObject = new GameObject("Chunk");
        _chunkObject.name = string.Format("Chunk {0}, {1}, {2}", pos.x, pos.y, pos.z);
        _chunkObject.transform.tag = "Terrain";

        ChunkPosition = pos;
        _chunkObject.transform.position = ChunkPosition;

        _meshFilter = _chunkObject.AddComponent<MeshFilter>();
        _meshRenderer = _chunkObject.AddComponent<MeshRenderer>();
        _meshCollider = _chunkObject.AddComponent<MeshCollider>();

        _mesh = new Mesh();
        _mesh.hideFlags = HideFlags.HideAndDontSave;

        // _meshRenderer.material = Resources.Load<Material>("Materials/Toon Terrain");
        _meshRenderer.material = Resources.Load<Material>("Materials/Terrain");
        _meshRenderer.material.SetTexture("_TexArray", _world.TerrainTexArray);

        // TerrainMap = new TerrainPoint[_width + 1, _height + 1, _width + 1];
        // Debug.Log("Terrain Map: " + TerrainMap.Length);

        TerrainMap = new TerrainPoint[(_width + 1) * (_height + 1) * (_width + 1)];//TerrainMap.Length];

        // Check if terrain map has been generated in a previous session
        // LoadVoxelField();

        GenerateTerrainMap();
        MarchingCubesTaskJob();
    }

    public void ModifyTerrainMap(Vector3Int position, float value)
    {
        int index = GetIndex(position);
        TerrainMap[index].DistanceToSurface += value;
    }

    private void GenerateTerrainMap()
    {
        for (int x = 0; x < _width + 1; x++)
        {
            for (int z = 0; z < _width + 1; z++)
            {
                for (int y = 0; y < _height + 1; y++)
                {
                    int worldX = x + ChunkPosition.x;
                    int worldZ = z + ChunkPosition.z;
                    float worldY = (float)y + ChunkPosition.y;

                    TerrainPoint terrainPoint = _world.GetTerrainPoint(worldX, worldY, worldZ);

                    // float terrainHeight = _world.GetTerrainHeight(worldX, worldZ);
                    // terrainHeight = Noise.Redistribution(terrainHeight, settings);
                    // float surface = worldY - terrainHeight;
                    
                    // int textureID = 0;
                    
                    // if(worldY < terrainHeight - 3)
                    //     textureID = 1;
                    // if(worldY < terrainHeight - 15)
                    //     textureID = 2;

                    int index = GetIndex(x, y, z);
                    TerrainMap[index] = terrainPoint;
                    // TerrainMap[index] = new TerrainPoint(surface, textureID);
                    // TerrainMap[x, y, z] = new TerrainPoint(surface, textureID);
                }
            }
        }
    }

    public void MarchingCubesTaskJob()
    {
        MarchingCubesJob job = new MarchingCubesJob
        {
            Width = Constants.CHUNK_WIDTH,
            Height = Constants.CHUNK_HEIGHT,
            TerrainSurface = Constants.TERRAIN_SURFACE,
            FlatShaded = _flatShaded,

            TerrainMap = new NativeArray<TerrainPoint>(TerrainMap, Allocator.TempJob),
            Vertices = new NativeList<float3>(Allocator.TempJob),
            Triangles = new NativeList<int>(Allocator.TempJob),
            Uvs = new NativeList<float2>(Allocator.TempJob)
        };

        JobHandle handle = job.Schedule();
        handle.Complete();

        Vector3[] verticesArray = new Vector3[job.Vertices.Length];
        for (int i = 0; i < job.Vertices.Length; i++)
        {
            verticesArray[i] = job.Vertices[i];
        }
        
        int[] trianglesArray = new int[job.Triangles.Length];
        for (int i = 0; i < job.Triangles.Length; i++)
        {
            trianglesArray[i] = job.Triangles[i];
        }

        Vector2[] uvsArray = new Vector2[job.Uvs.Length];
        for (int i = 0; i < job.Uvs.Length; i++)
        {
            uvsArray[i] = job.Uvs[i];
        }

        // Mesh mesh = new Mesh();
        // mesh.hideFlags = HideFlags.HideAndDontSave;
        _mesh.Clear();
        _mesh.vertices = verticesArray;
        _mesh.triangles = trianglesArray;
        _mesh.uv = uvsArray;
        AssembleMesh();

        // Dispose of the NativeArray and NativeList containers
        job.TerrainMap.Dispose();
        job.Vertices.Dispose();
        job.Triangles.Dispose();
        job.Uvs.Dispose();

    }

    private void AssembleMesh()
    {
		_mesh.RecalculateNormals();

		_meshFilter.sharedMesh = _mesh;
        _meshCollider.sharedMesh = _mesh;
    }

    private int GetIndex(Vector3Int position) => position.x + (_width + 1) * (position.y + (_height + 1) * position.z);
    private int GetIndex(int x, int y, int z) => x + (_width + 1) * (y + (_height + 1) * z);

    // Latest Example
    // Create a NativeArray to hold the terrain data for this chunk
    // var terrainMap = new NativeArray<TerrainPoint>(chunkSize * chunkSize * chunkSize, Allocator.TempJob);

    // // Initialize the terrainMap with data for this chunk

    // // Create the NativeList containers for the mesh data
    // var vertices = new NativeList<float3>(Allocator.TempJob);
    // var triangles = new NativeList<int>(Allocator.TempJob);
    // var uvs = new NativeList<float2>(Allocator.TempJob);

    // // Create and schedule the GenerateChunkMeshJob
    // var job = new GenerateChunkMeshJob
    // {
    //     Width = ChunkData.Width,
    //     Height = ChunkData.Height,
    //     TerrainSurface = ChunkData.terrainSurface,
    //     FlatShaded = _flatShaded,
    //     TerrainMap = terrainMap,
    //     Vertices = vertices,
    //     Triangles = triangles,
    //     Uvs = uvs
    // };
    // var handle = job.Schedule();

    // // Wait for the job to complete
    // handle.Complete();

    // // Use the mesh data generated by the job
    // // ...

    // // Dispose of the NativeArray and NativeList containers
    // terrainMap.Dispose();
    // vertices.Dispose();
    // triangles.Dispose();
    // uvs.Dispose();

    // =============================================================

    // Original Example
    // Create a NativeArray to hold the terrain data for each chunk
    // var terrainMap = new NativeArray<TerrainPoint>(chunkSize * chunkSize * chunkSize, Allocator.TempJob);

    // // Initialize the terrainMap with data for each chunk

    // // Create a list of job handles
    // var handles = new List<JobHandle>();

    // // Schedule a GenerateChunkMeshJob for each chunk
    // for (int i = 0; i < numChunks; i++)
    // {
    //     var job = new GenerateChunkMeshJob
    //     {
    //         Width = ChunkData.Width,
    //         Height = ChunkData.Height,
    //         TerrainSurface = ChunkData.terrainSurface,
    //         FlatShaded = _flatShaded,
    //         TerrainMap = terrainMap,
    //         Vertices = new NativeList<float3>(Allocator.TempJob),
    //         Triangles = new NativeList<int>(Allocator.TempJob),
    //         Uvs = new NativeList<float2>(Allocator.TempJob)
    //     };

    //     var handle = job.Schedule();
    //     handles.Add(handle);
    // }

    // // Wait for all jobs to complete
    // JobHandle.CompleteAll(handles);

    // // Process the results of each job
    // for (int i = 0; i < handles.Count; i++)
    // {
    //     var handle = handles[i];
    //     var job = handle.GetJobData<GenerateChunkMeshJob>();

    //     // Use job.Vertices, job.Triangles, and job.Uvs to create a mesh for each chunk

    //     // Dispose of the NativeList containers used by each job
    //     job.Vertices.Dispose();
    //     job.Triangles.Dispose();
    //     job.Uvs.Dispose();
    // }

    // // Dispose of the terrainMap NativeArray
    // terrainMap.Dispose();

    // =============================================================

    // Init Native list
    // Create the NativeList containers for the mesh data
    // var vertices = new NativeList<float3>(Allocator.TempJob);
    // var triangles = new NativeList<int>(Allocator.TempJob);
    // var uvs = new NativeList<float2>(Allocator.TempJob);

    // // Create and schedule the MarchCubes job
    // var job = new MarchingCubesJob
    // {
    //     // ...
    //     Vertices = vertices,
    //     Triangles = triangles,
    //     Uvs = uvs
    // };
    // var handle = job.Schedule();

    // // Wait for the job to complete
    // handle.Complete();

    // // Use the mesh data generated by the job
    // // ...

    // // Dispose of the NativeList containers
    // vertices.Dispose();
    // triangles.Dispose();
    // uvs.Dispose();

    // =============================================================

    // Flatten 3D Array
    // Create a NativeArray to hold the terrain data for each chunk
    // var terrainMap = new NativeArray<TerrainPoint>(chunkSize * chunkSize * chunkSize, Allocator.TempJob);

    // // Flatten the multi-dimensional TerrainMap array into the one-dimensional terrainMap NativeArray
    // for (int z = 0; z < _width; z++)
    // {
    //     for (int y = 0; y < _width; y++)
    //     {
    //         for (int x = 0; x < _height; x++)
    //         {
    //             int index = x + y * _width + z * _width * _height;
    //             terrainMap[index] = TerrainMap[x, y, z];
    //         }
    //     }
    // }

    // // Pass the terrainMap NativeArray to the job
    // var job = new MarchingCubesJob
    // {
    //     // ...
    //     TerrainMap = terrainMap,
    //     // ...
    // };

    // =============================================================

    // IJobParallelFor
    // Create a NativeArray to hold the terrain data for all chunks
    // var terrainMaps = new NativeArray<TerrainPoint>(numChunks * chunkSize * chunkSize * chunkSize, Allocator.TempJob);

    // // Initialize the terrainMaps array with data for all chunks

    // // Create a NativeArray of NativeList containers for each chunk's mesh data
    // var vertices = new NativeArray<NativeList<float3>>(numChunks, Allocator.TempJob);
    // var triangles = new NativeArray<NativeList<int>>(numChunks, Allocator.TempJob);
    // var uvs = new NativeArray<NativeList<float2>>(numChunks, Allocator.TempJob);

    // // Schedule the GenerateChunkMeshesJob
    // var job = new GenerateChunkMeshesJob
    // {
    //     Width = ChunkData.Width,
    //     Height = ChunkData.Height,
    //     TerrainSurface = ChunkData.terrainSurface,
    //     FlatShaded = _flatShaded,
    //     TerrainMaps = terrainMaps,
    //     Vertices = vertices,
    //     Triangles = triangles,
    //     Uvs = uvs
    // };
    // var handle = job.Schedule(numChunks, 1);

    // // Wait for the job to complete
    // handle.Complete();

    // // Process the results of the job
    // for (int i = 0; i < numChunks; i++)
    // {
    //     // Use vertices[i], triangles[i], and uvs[i] to create a mesh for each chunk

    //     // Dispose of the NativeList containers used by each chunk
    //     vertices[i].Dispose();
    //     triangles[i].Dispose();
    //     uvs[i].Dispose();
    // }

    // // Dispose of the NativeArray containers
    // terrainMaps.Dispose();
    // vertices.Dispose();
    // triangles.Dispose();
    // uvs.Dispose();

    // =============================================================

    // Stuffed TerrainMap for IJobParallelFor
    // public struct GenerateChunkMeshesJob : IJobParallelFor
    // {
    //     // ...
    //     [ReadOnly] public NativeArray<TerrainPoint> TerrainMaps;
    //     // ...

    //     public void Execute(int index)
    //     {
    //         // Calculate the start and end indices of the slice that corresponds to this chunk's terrain map
    //         int start = index * Width * Height * Width;
    //         int length = Width * Height * Width;

    //         // Get a slice of the TerrainMaps array that corresponds to this chunk's terrain map
    //         var terrainMap = TerrainMaps.Slice(start, length);

    //         // Use the terrainMap slice to generate the mesh for this chunk
    //         // ...
    //     }
    // }

}