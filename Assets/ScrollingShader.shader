Shader "Unlit/ScrollingShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ScrollSpeedX ("X Scroll Speed", Range(-5, 5)) = 1.0
        _ScrollSpeedY ("Y Scroll Speed", Range(-5, 5)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha // ���� ����
        Cull Off // �޸鵵 ���̵��� ����

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _ScrollSpeedX;
            float _ScrollSpeedY;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                // �ð��� ���� UV ��ǥ�� �̵���ŵ�ϴ�.
                float2 offset = float2(_ScrollSpeedX, _ScrollSpeedY) * _Time.y;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex) + offset;
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // �ؽ�ó�� ���ø��մϴ�.
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}
