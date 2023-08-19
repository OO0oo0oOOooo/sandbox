// Noise Library for Unity - https://github.com/keijiro/NoiseShader
// LICENSE https://github.com/keijiro/NoiseShader/blob/master/LICENSE
// 
// Translation and modification was made by Keijiro Takahashi.
//
// Copyright (C) 2011 by Ashima Arts (Simplex noise)
// Copyright (C) 2011-2016 by Stefan Gustavson (Classic noise and others)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

#ifndef _INCLUDE_JP_KEIJIRO_NOISESHADER_COMMON_HLSL_
#define _INCLUDE_JP_KEIJIRO_NOISESHADER_COMMON_HLSL_

float wglnoise_mod(float x, float y)
{
    return x - y * floor(x / y);
}

float2 wglnoise_mod(float2 x, float2 y)
{
    return x - y * floor(x / y);
}

float3 wglnoise_mod(float3 x, float3 y)
{
    return x - y * floor(x / y);
}

float4 wglnoise_mod(float4 x, float4 y)
{
    return x - y * floor(x / y);
}

float2 wglnoise_fade(float2 t)
{
    return t * t * t * (t * (t * 6 - 15) + 10);
}

float3 wglnoise_fade(float3 t)
{
    return t * t * t * (t * (t * 6 - 15) + 10);
}

float wglnoise_mod289(float x)
{
    return x - floor(x / 289) * 289;
}

float2 wglnoise_mod289(float2 x)
{
    return x - floor(x / 289) * 289;
}

float3 wglnoise_mod289(float3 x)
{
    return x - floor(x / 289) * 289;
}

float4 wglnoise_mod289(float4 x)
{
    return x - floor(x / 289) * 289;
}

float3 wglnoise_permute(float3 x)
{
    return wglnoise_mod289((x * 34 + 1) * x);
}

float4 wglnoise_permute(float4 x)
{
    return wglnoise_mod289((x * 34 + 1) * x);
}

#endif
