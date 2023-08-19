using UnityEngine;

public static class Noise
{
	public static float FractalBrownianMotion(Vector3 p, int octaves, float freq, float amp, float lacunarity, float gain)
	{
		float sum = 0;
		for(int i = 0; i < octaves; i++)
		{
			float n = Perlin2D(p.x, p.y, freq, amp);// GetNoiseLayer(p * freq + Offset); //  + Offset
			sum += n*amp;
			freq *= lacunarity;
			amp *= gain;
		}

		return sum;
	}

	public static float DomainWarp(Vector3 p, float warpStrength, int octaves, float freq, float amp, float lacunarity, float gain)
	{
		float xOffset = FractalBrownianMotion(new Vector2(p.x + 0, p.y + 0), octaves, freq, amp, lacunarity, gain); // Another Offset?
		float yOffset = FractalBrownianMotion(new Vector2(p.x + 5.2f, p.y + 2.4f), octaves, freq, amp, lacunarity, gain); // Another Offset?

		return FractalBrownianMotion(new Vector2(p.x + warpStrength * xOffset, p.y + warpStrength * yOffset), octaves, freq, amp, lacunarity, gain); // Offset does kinda cool stuff here
	}

    public static float Perlin2D(float x, float y, float freq, float amp)
	{
		return Mathf.PerlinNoise((float)x / freq * amp + 0.001f, (float)y / freq * amp + 0.001f);
	}

	public static float Get2DPerlin(Vector2 position, float offset, float scale)
    {
        return Mathf.PerlinNoise((position.x + 0.1f) / Constants.CHUNK_WIDTH * scale + offset, (position.y + 0.1f) / Constants.CHUNK_WIDTH * scale + offset);
    }

    public static float Perlin3D(Vector3 p, float freq, float amp)
	{
		float AB = Perlin2D(p.x, p.y, freq, amp);
		float BC = Perlin2D(p.y, p.z, freq, amp);
		float AC = Perlin2D(p.x, p.z, freq, amp);

		float BA = Perlin2D(p.y, p.x, freq, amp);
		float CB = Perlin2D(p.z, p.y, freq, amp);
		float CA = Perlin2D(p.z, p.x, freq, amp);

		float ABC = AB + BC + AC + BA + CB + CA;
		return ABC / 6f;
	}

	public static float Redistribution(float noise, NoiseSettings settings)
	{
		return Mathf.Pow(noise * settings.RedistributionModifier, settings.Exponent);
	}

	public static float RemapNoise01(float value, float outputMin, float outputMax)
    {
        return outputMin + (value - 0) * (outputMax - outputMin) / (1 - 0);
    }

	private static float Temperature()
	{
		return -1;
	}

	private static float Humidity()
	{
		return -1;
	}

    private static float Continentalness()
    {
        return -1;
    }

    private static float Erosion()
    {
        return -1;
    }

    private static float Weridness()
	{
		return -1;
	}
}
