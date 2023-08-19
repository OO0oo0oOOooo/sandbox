// using System;

// [Serializable]
public struct TerrainPoint
{
    // 1 is above / 0 is Terrain
    public float DistanceToSurface;

    public int TextureID;

    public TerrainPoint(float dst, int tex)
    {
        DistanceToSurface = dst;
        TextureID = tex;
    }
}
