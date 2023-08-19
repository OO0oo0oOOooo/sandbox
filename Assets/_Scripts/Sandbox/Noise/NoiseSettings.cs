using UnityEngine;

[CreateAssetMenu(fileName = "Noise Settings", menuName = "Data/Noise Settings")]
public class NoiseSettings : ScriptableObject
{
    public int Octaves = 1;
	public float Frequency = 2;
    public float Amplitude =  1f;
	public float Lacunarity = 2f;
	public float Gain = 0.5f;

	public float WarpStrength = 0f;

    public float RedistributionModifier = 1.2f;

    public float Exponent = 1;
}
