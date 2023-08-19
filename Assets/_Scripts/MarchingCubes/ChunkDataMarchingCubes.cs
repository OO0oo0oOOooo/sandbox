public static class ChunkDataMarchingCubes
{
    public static float Octaves = 3;
	public static float Frequency = 32f;
    public static float Amplitude =  1.5f;

	public static int Width = 32;
	public static int Height = 32;
	public static float terrainSurface = 0.5f;

	public static float BaseTerrainHeight = 5f; // Minimum height of terrain.
	public static float TerrainHeightRange = 24f; // The max height (above BaseTerrainHeight) our terrain can be.

    public static float GetTerrainHeight(int x, int z)
	{
		// return (float)TerrainHeightRange * Mathf.PerlinNoise((float)x / _frequency * _amplitude + 0.001f, (float)z / _frequency * _amplitude + 0.001f) + BaseTerrainHeight;
		
		// return (float)TerrainHeightRange * Noise.FractalBrownianNoise(Noise.NoiseType.Perlin2D, x, 0, z) + BaseTerrainHeight;
		return -1;
	}
}
