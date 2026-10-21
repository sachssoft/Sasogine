#define SV_POSITION POSITION
float4x4 Projection;
texture Texture0;
sampler2D Sampler0 = sampler_state {
    Texture = <Texture0>;
    MinFilter = Linear; MagFilter = Linear; MipFilter = Linear;
    AddressU = Clamp; AddressV = Clamp;
};
texture Texture1;
sampler2D Sampler1 = sampler_state {
    Texture = <Texture1>;
    MinFilter = Linear; MagFilter = Linear; MipFilter = Linear;
    AddressU = Clamp; AddressV = Clamp;
};
texture Texture2;
sampler2D Sampler2 = sampler_state {
    Texture = <Texture2>;
    MinFilter = Linear; MagFilter = Linear; MipFilter = Linear;
    AddressU = Clamp; AddressV = Clamp;
};
texture Texture3;
sampler2D Sampler3 = sampler_state {
    Texture = <Texture3>;
    MinFilter = Linear; MagFilter = Linear; MipFilter = Linear;
    AddressU = Clamp; AddressV = Clamp;
};
texture Texture4;
sampler2D Sampler4 = sampler_state {
    Texture = <Texture4>;
    MinFilter = Linear; MagFilter = Linear; MipFilter = Linear;
    AddressU = Clamp; AddressV = Clamp;
};
texture Texture5;
sampler2D Sampler5 = sampler_state {
    Texture = <Texture5>;
    MinFilter = Linear; MagFilter = Linear; MipFilter = Linear;
    AddressU = Clamp; AddressV = Clamp;
};
texture Texture6;
sampler2D Sampler6 = sampler_state {
    Texture = <Texture6>;
    MinFilter = Linear; MagFilter = Linear; MipFilter = Linear;
    AddressU = Clamp; AddressV = Clamp;
};
texture Texture7;
sampler2D Sampler7 = sampler_state {
    Texture = <Texture7>;
    MinFilter = Linear; MagFilter = Linear; MipFilter = Linear;
    AddressU = Clamp; AddressV = Clamp;
};
struct Input {float4 Position:POSITION0;float4 Color:COLOR0;float2 UV:TEXCOORD0;float Slot:TEXCOORD1;};
struct Output {float4 Position:SV_POSITION;float4 Color:COLOR0;float2 UV:TEXCOORD0;float Slot:TEXCOORD1;};
Output VS(Input input) {
    Output output;
    output.Position=mul(input.Position,Projection);
    output.Color=input.Color;output.UV=input.UV;output.Slot=input.Slot;
    return output;
}
float4 PS(Output input):COLOR0 {
    float4 color;
    if (input.Slot < 0.5f) color=tex2D(Sampler0,input.UV);
    else if (input.Slot < 1.5f) color=tex2D(Sampler1,input.UV);
    else if (input.Slot < 2.5f) color=tex2D(Sampler2,input.UV);
    else if (input.Slot < 3.5f) color=tex2D(Sampler3,input.UV);
    else if (input.Slot < 4.5f) color=tex2D(Sampler4,input.UV);
    else if (input.Slot < 5.5f) color=tex2D(Sampler5,input.UV);
    else if (input.Slot < 6.5f) color=tex2D(Sampler6,input.UV);
    else color=tex2D(Sampler7,input.UV);
    return color*input.Color;
}
technique UITextures {pass P0 {VertexShader=compile vs_3_0 VS();PixelShader=compile ps_3_0 PS();}}
