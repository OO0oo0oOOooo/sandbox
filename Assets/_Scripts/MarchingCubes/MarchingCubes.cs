using System;
using System.Collections.Generic;
using UnityEngine;

// This is the Chunk

public class MarchingCubes
{
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    private MeshCollider _meshCollider;

    private int _width { get { return ChunkDataMarchingCubes.Width; } }
	private int _height { get { return ChunkDataMarchingCubes.Height; } }
	private float _terrainSurface { get { return ChunkDataMarchingCubes.terrainSurface; } }

    [SerializeField] private bool _flatShaded = true;

	private float[,,] _terrainMap;
    private List<Vector3> _vertices = new List<Vector3>();
    private List<int> _triangles = new List<int>();

    private Vector3Int _chunkPosition;
    public GameObject _chunkObject;

    public MarchingCubes(Vector3Int pos)
    {
        _chunkObject = new GameObject("Chunk");
        _chunkObject.name = string.Format("Chunk {0}, {1}", pos.x, pos.z);

        _chunkPosition = pos;
        _chunkObject.transform.position = _chunkPosition;

        _meshFilter = _chunkObject.AddComponent<MeshFilter>();
        _meshRenderer = _chunkObject.AddComponent<MeshRenderer>();
        _meshCollider = _chunkObject.AddComponent<MeshCollider>();

        _meshRenderer.material = Resources.Load<Material>("Materials/Toon Terrain");

        _terrainMap = new float[_width + 1, _height + 1, _width + 1];

        // Check if terrain map has been generated in a previous session
        // LoadVoxelField();

        GenerateTerrainMap();
        March();
    }

    private void GenerateTerrainMap()
    {
        for (int x = 0; x < _width + 1; x++)
        {
            for (int z = 0; z < _width + 1; z++)
            {
                for (int y = 0; y < _height + 1; y++)
                {
                    // float noise = 0.5f;
                    // float noise = Mathf.PerlinNoise((float)x / _frequency * _amplitude + 0.001f, (float)z / _frequency * _amplitude + 0.001f);
                    float noise = ChunkDataMarchingCubes.GetTerrainHeight(x + _chunkPosition.x, z + _chunkPosition.z);
                    _terrainMap[x, y, z] = (float)y - noise;
                    // _terrainMap[x, y, z] = y <= noise * (float)_height ? 0 : 1;
                    // _terrainMap[x, y, z] = SimplePerlin3D.Perlin3D((float)x / _frequency * _amplitude + 0.001f, (float)y / _frequency * _amplitude + 0.001f, (float)z / _frequency * _amplitude + 0.001f);
                }
            }
        }
    }

    private void March()
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
				}
				else
					_triangles.Add(VertForIndice(vertPosition));

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
        return _terrainMap[position.x, position.y, position.z];
    }

    int VertForIndice(Vector3 vert)
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
		return _vertices.Count - 1;
	}

    private void ClearMeshData()
	{
		_vertices.Clear();
		_triangles.Clear();
	}

    private void AssembleMesh()
    {
        Mesh mesh = new Mesh();
		mesh.vertices = _vertices.ToArray();
		mesh.triangles = _triangles.ToArray();
		mesh.RecalculateNormals();

		_meshFilter.sharedMesh = mesh;
        _meshCollider.sharedMesh = mesh;
    }
}
