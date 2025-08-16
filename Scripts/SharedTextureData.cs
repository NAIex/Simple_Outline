using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class SharedTextureData : ContextItem
{
    public TextureHandle writtenTexture;

    public override void Reset()
    {
        writtenTexture = TextureHandle.nullHandle;
    }
}