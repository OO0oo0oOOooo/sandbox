using System.Collections.Generic;
using UnityEngine;

public class ChunkNotJobified
{
    private World _world;

    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    private MeshCollider _meshCollider;

    private int _width { get { return Constants.CHUNK_WIDTH; } }
	private int _height { get { return Constants.CHUNK_HEIGHT; } }
	private float _terrainSurface { get { return Constants.TERRAIN_SURFACE; } }

    private bool _flatShaded = false;

	public TerrainPoint[,,] TerrainMap;
    private List<Vector3> _vertices = new List<Vector3>();
    private List<int> _triangles = new List<int>();
    private List<Vector2> _uvs = new List<Vector2>();

    public Vector3Int ChunkPosition;
    public GameObject _chunkObject;

    public ChunkNotJobified(Vector3Int pos, World w)
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

        _meshRenderer.material = Resources.Load<Material>("Materials/Toon Terrain");
        _meshRenderer.material.SetTexture("_TexArray", _world.TerrainTexArray);

        TerrainMap = new TerrainPoint[_width + 1, _height + 1, _width + 1];

        // Check if terrain map has been generated in a previous session
        // LoadVoxelField();

        GenerateTerrainMap();

        ClearMeshData();
        March();
        AssembleMesh();
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

                    float terrainHeight = _world.GetTerrainHeight(worldX, worldZ);
                    float surface = worldY - terrainHeight;

                    int textureID = 0;
                    
                    if(worldY < terrainHeight - 3)
                        textureID = 1;
                    if(worldY < terrainHeight - 15)
                        textureID = 2;
                    
                    // TerrainPoint terrainPoint = new TerrainPoint(surface, textureID);
                    TerrainMap[x, y, z] = new TerrainPoint(surface, textureID);

                    // TerrainMap[x, y, z] = new TerrainPoint(y <= noise * (float)_height ? 0 : 1, 0);
                    // _terrainMap[x, y, z] = SimplePerlin3D.Perlin3D((float)x / _frequency * _amplitude + 0.001f, (float)y / _frequency * _amplitude + 0.001f, (float)z / _frequency * _amplitude + 0.001f);
                }
            }
        }
    }

    private void ClearMeshData()
	{
		_vertices.Clear();
		_triangles.Clear();
        _uvs.Clear();
	}

    private void AssembleMesh()
    {
        Mesh mesh = new Mesh();
		mesh.vertices = _vertices.ToArray();
		mesh.triangles = _triangles.ToArray();
        mesh.uv = _uvs.ToArray();

		mesh.RecalculateNormals();

		_meshFilter.sharedMesh = mesh;
        _meshCollider.sharedMesh = mesh;
    }

    public void March()
    {
        ClearMeshData();
        for (int x = 0; x < _width; x++)
        {
            for (int z = 0; z < _width; z++)
            {
                for (int y = 0; y < _height; y++)
                {
                    Cube(new Vector3Int(x, y, z));
                }
            }
        }
        AssembleMesh();
    }

    private void Cube(Vector3Int position)
    {
        //sample terrain values at each corner of the cube.
		float[] cube = new float[8];
        for (int i = 0; i < 8; i++)
        {
			cube[i] = SampleTerrain(position + MarchingCubesTables.CornerTable[i]);
        }

		// Get the configuration index of this cube.
		int configIndex = GetCubeConfiguration(cube);

		// If the configuration of this cube is 0 or 255 (completely inside the terrain or completely outside of it) we don't need to do anything.
		if (configIndex == 0 || configIndex == 255)
			return;

        int edgeIndex = 0;
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                // Get the current indice. We increment triangleIndex through each loop.
				int indice = MarchingCubesTables.TriangleTable[configIndex, edgeIndex];

				// If the current edgeIndex is -1, there are no more indices and we can exit the function.
				if (indice == -1)
					return;

				// Get the vertices for the start and end of this edge.
				Vector3 vert1 = position + MarchingCubesTables.CornerTable[MarchingCubesTables.EdgeIndexes[indice, 0]];
				Vector3 vert2 = position + MarchingCubesTables.CornerTable[MarchingCubesTables.EdgeIndexes[indice, 1]];
				Vector3 vertPosition;

				// Get the terrain values at either end of our current edge from the cube array created above.
				float vert1Sample = cube[MarchingCubesTables.EdgeIndexes[indice, 0]];
				float vert2Sample = cube[MarchingCubesTables.EdgeIndexes[indice, 1]];

				// Calculate the difference Between the terrain values.
				float difference = vert2Sample - vert1Sample;

				// If the difference is 0, then the terrain passes through the middle.
				if (difference == 0)
					difference = _terrainSurface;
				else
					difference = (_terrainSurface - vert1Sample) / difference;

				// Calculate the point along the edge that passes through
				vertPosition = vert1 + ((vert2 - vert1) * difference);

				// Add to our vertices and triangles list and incremement the edgeIndex.
				if (_flatShaded)
				{
					_vertices.Add(vertPosition);
					_triangles.Add(_vertices.Count - 1);
                    _uvs.Add(new Vector2(TerrainMap[position.x, position.y, position.z].TextureID, 0));
				}
				else
					_triangles.Add(VertForIndice(vertPosition, position));

				edgeIndex++;
            }
        }
    }

    private int GetCubeConfiguration(float[] cube)
	{
		int configurationIndex = 0;
		for (int i = 0; i < 8; i++)
		{
			// If it is, use bit-magic to the set the corresponding bit to 1. So if only the 3rd point in the cube was below
			// the surface, the bit would look like 00100000, which represents the integer value 32.
			if (cube[i] > _terrainSurface)
				configurationIndex |= 1 << i;
		}
		return configurationIndex;
	}

    private float SampleTerrain(Vector3Int position)
    {
        // You can use this method to sample the terrain values at a specific position in the terrainMap array.
        // You may want to add additional logic here to interpolate between values or handle positions that are outside of the array bounds.
        return TerrainMap[position.x, position.y, position.z].DistanceToSurface;
    }

    int VertForIndice(Vector3 vert, Vector3Int point)
	{
		// Loop through all the vertices currently in the vertices list.
		for (int i = 0; i < _vertices.Count; i++)
		{
			// If we find a vert that matches ours, then simply return this index.
			if (_vertices[i] == vert)
				return i;

		}

		// If we didnt find a match, add this vert to the list and return last index.
		_vertices.Add(vert);
        _uvs.Add(new Vector2(TerrainMap[point.x, point.y, point.z].TextureID, 0));
		return _vertices.Count - 1;
	}
}