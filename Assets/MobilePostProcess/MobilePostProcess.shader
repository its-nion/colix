Shader "SupGames/Mobile/PostProcess"
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "" {}
	}

	CGINCLUDE

#include "UnityCG.cginc"

	struct appdata {
		fixed4 pos : POSITION;
		fixed2 uv : TEXCOORD0;
	};

	struct v2fb {
		fixed4 pos : POSITION;
		fixed4 uv : TEXCOORD0;
	};
	struct v2f {
		fixed4 pos : POSITION;
		fixed4 uv  : TEXCOORD0;
		fixed2 data  : TEXCOORD1;
	};

	struct v2fbs
	{
		fixed4 pos  : SV_POSITION;
		fixed2  uv  : TEXCOORD0;
		fixed4  uv1 : TEXCOORD1;
		fixed4  uv2 : TEXCOORD2;
	};

	sampler2D _MainTex;
	sampler2D _LutTex2D;
	sampler3D _LutTex3D;
	sampler2D _MaskTex;
	sampler2D _BlurTex;
	fixed _LutAmount;
	fixed _BloomThreshold;
	fixed4 _Color;
	fixed _BloomAmount;
	fixed _BlurAmount;
	fixed _LutDimension;
	fixed _Contrast;
	fixed _Brightness;
	fixed _Saturation;
	fixed _Exposure;
	fixed _Gamma;
	fixed _Offset;
	fixed _Vignette;
	fixed4 _MainTex_TexelSize;

	v2fb vertBlur(appdata i)
	{
		v2fb o;
		o.pos = UnityObjectToClipPos(i.pos);
		fixed2 offset = _MainTex_TexelSize.xy * _BlurAmount;
		o.uv = fixed4(i.uv - offset, i.uv + offset);
		return o;
	}

	v2fbs vertBlurSingle(appdata i)
	{
		v2fbs o;
		o.pos = UnityObjectToClipPos(i.pos);
		o.uv = i.uv;
		o.uv1 = fixed4(i.uv - _MainTex_TexelSize.xy * fixed2(1.3846153846h, 1.3846153846h)*_BlurAmount, i.uv + _MainTex_TexelSize.xy * fixed2(1.3846153846h, 1.3846153846h)*_BlurAmount);
		o.uv2 = fixed4(i.uv - _MainTex_TexelSize.xy * fixed2(3.2307692308h, 3.2307692308h)*_BlurAmount, i.uv + _MainTex_TexelSize.xy * fixed2(3.2307692308h, 3.2307692308h)*_BlurAmount);
		return o;
	}

	v2f vert(appdata i)
	{
		v2f o;
		o.pos = UnityObjectToClipPos(i.pos);
		o.uv.xy = i.uv;
		o.uv.zw = i.uv - 0.5h;
		o.data.x = i.uv.x - _Offset * _MainTex_TexelSize.x;
		o.data.y = i.uv.x + _Offset * _MainTex_TexelSize.x;
		return o;
	}

	fixed4 fragBloom(v2fb i) : COLOR
	{
		fixed4 result = tex2D(_MainTex, i.uv.xy);
		result += tex2D(_MainTex, i.uv.xw);
		result += tex2D(_MainTex, i.uv.zy);
		result += tex2D(_MainTex, i.uv.zw);
		return max(result * 0.25h - _BloomThreshold, 0.0h);
	}

	fixed4 fragBlurSingle(v2fbs i) : COLOR
	{
		fixed4 result = tex2D(_MainTex, i.uv)*0.227027027h;
		result += tex2D(_MainTex, i.uv1.xy)*0.3162162162h;
		result += tex2D(_MainTex, i.uv1.zw)*0.3162162162h;
		result += tex2D(_MainTex, i.uv2.xy)*0.0702702703h;
		result += tex2D(_MainTex, i.uv2.zw)*0.0702702703h;
#if defined(BLOOM)
		return max(result - _BloomThreshold, 0.0h);
#endif
		return result;
	}

	fixed4 fragBlur(v2fb i) : COLOR
	{
		fixed4 result = tex2D(_MainTex, i.uv.xy);
		result += tex2D(_MainTex, i.uv.xw);
		result += tex2D(_MainTex, i.uv.zy);
		result += tex2D(_MainTex, i.uv.zw);
		return result * 0.25h;
	}

	fixed4 fragAll2D(v2f i) : COLOR
	{
		fixed4 c;
		fixed bx;
		fixed by;

#if !defined(CHROMA)
		c = tex2D(_MainTex, i.uv);
#else   
		c.r = tex2D(_MainTex, fixed2(i.data.r, i.uv.y)).r;
		c.g = tex2D(_MainTex, i.uv).g;
		c.b = tex2D(_MainTex, fixed2(i.data.g, i.uv.y)).b;
		c.a = 1.0h;
#endif

#if defined(BLUR) || defined(BLOOM)
		fixed4 b = tex2D(_BlurTex, i.uv);
#endif

#if defined(BLUR) && !defined(BLOOM)
		fixed4 m = tex2D(_MaskTex, i.uv);
#endif

#if defined(LUT)
		bx = floor(c.b * 256.0h);
		by = floor(bx * 0.0625h);
		c = lerp(c, tex2D(_LutTex2D, c.rg * 0.05859375h + 0.001953125h + fixed2(floor(bx - by * 16.0h), by) * 0.0625h), _LutAmount);
#endif

#if defined(LUT) && (defined(BLOOM) || defined(BLUR))
		bx = floor(b.b * 256.0h);
		by = floor(bx * 0.0625h);
		b = lerp(b, tex2D(_LutTex2D, b.rg * 0.05859375h + 0.001953125h + fixed2(floor(bx - by * 16.0h), by) * 0.0625h), _LutAmount);
#endif

#if defined(BLOOM)
		c = (c + b * _BloomAmount * _Color) * 0.5h;
#elif defined(BLUR)
		c = lerp(c, b, m.r);
#endif

#if defined(FILTER)
		c.rgb = (c.rgb - 0.5f) * _Contrast + _Brightness;
		c.rgb = lerp(dot(c.rgb, fixed3(0.299h, 0.587h, 0.114h)), c.rgb, _Saturation);
		c.rgb *= pow(2, _Exposure) - _Gamma;
#endif
		c.rgb *= 1.0h - dot(i.uv.zw, i.uv.zw) * _Vignette;
		return c;
	}


		fixed4 fragAll3D(v2f i) : COLOR
	{
		fixed4 c;

#if !defined(CHROMA)
		c = tex2D(_MainTex, i.uv);
#else   
		c.r = tex2D(_MainTex, fixed2(i.data.r, i.uv.y)).r;
		c.g = tex2D(_MainTex, i.uv).g;
		c.b = tex2D(_MainTex, fixed2(i.data.g, i.uv.y)).b;
		c.a = 1.0h;
#endif

#if defined(BLUR) || defined(BLOOM)
		fixed4 b = tex2D(_BlurTex, i.uv);
#endif

#if defined(BLUR) && !defined(BLOOM)
		fixed4 m = tex2D(_MaskTex, i.uv);
#endif

#if defined(LUT)
		c = lerp(c, tex3D(_LutTex3D, c.rgb * 0.9375h + 0.03125h), _LutAmount);
#endif

#if defined(LUT) && (defined(BLOOM)|| defined(BLUR))
		b = lerp(b, tex3D(_LutTex3D, b.rgb * 0.9375h + 0.03125h), _LutAmount);
#endif

#if defined(BLOOM)
		c = (c + b * _BloomAmount * _Color) * 0.5h;
#elif defined(BLUR)
		c = lerp(c, b, m.r);
#endif

#if defined(FILTER)
		c.rgb = (c.rgb - 0.5f) * _Contrast + _Brightness;
		c.rgb = lerp(dot(c.rgb, fixed3(0.299h, 0.587h, 0.114h)), c.rgb, _Saturation);
		c.rgb *= pow(2, _Exposure) - _Gamma;
#endif
		c.rgb *= 1.0h - dot(i.uv.zw, i.uv.zw) * _Vignette;
		return c;
	}
	ENDCG


	Subshader
	{
		Pass //0
		{
		  ZTest Always Cull Off ZWrite Off
		  Fog { Mode off }
		  CGPROGRAM
		  #pragma vertex vertBlurSingle
		  #pragma fragment fragBlurSingle
		  #pragma shader_feature BLOOM
		  #pragma fragmentoption ARB_precision_hint_fastest
		  ENDCG
		}
		Pass //1
		{
		  ZTest Always Cull Off ZWrite Off
		  Fog { Mode off }
		  CGPROGRAM
		  #pragma vertex vertBlur
		  #pragma fragment fragBlur
		  #pragma fragmentoption ARB_precision_hint_fastest
		  ENDCG
		}
		Pass //2
		{
		  ZTest Always Cull Off ZWrite Off
		  Fog { Mode off }
		  CGPROGRAM
		  #pragma vertex vertBlur
		  #pragma fragment fragBloom
		  #pragma fragmentoption ARB_precision_hint_fastest
		  ENDCG
		}
		Pass //3
		{
		  ZTest Always Cull Off ZWrite Off
		  Fog { Mode off }
		  CGPROGRAM
		  #pragma vertex vert
		  #pragma fragment fragAll2D
		  #pragma fragmentoption ARB_precision_hint_fastest
		  #pragma shader_feature BLOOM
		  #pragma shader_feature BLUR
		  #pragma shader_feature CHROMA
		  #pragma shader_feature LUT
		  #pragma shader_feature FILTER
		  ENDCG
		}
		Pass //4
		{
		  ZTest Always Cull Off ZWrite Off
		  Fog { Mode off }
		  CGPROGRAM
		  #pragma vertex vert
		  #pragma fragment fragAll3D
		  #pragma fragmentoption ARB_precision_hint_fastest
		  #pragma shader_feature BLOOM
		  #pragma shader_feature BLUR
		  #pragma shader_feature CHROMA
		  #pragma shader_feature LUT
		  #pragma shader_feature FILTER
		  ENDCG
		}
	}
	Fallback off
}