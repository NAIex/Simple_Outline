using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;


public class DrawOutlinePass : ScriptableRenderPass
{
    private DrawOutlinePassSettings settings;

    public DrawOutlinePass(DrawOutlinePassSettings settings)
    {
        this.settings = settings;
        this.renderPassEvent = this.settings.renderPassEvent;
    }
    public void Setup(DrawOutlinePassSettings settings)
    {
        this.settings = settings;
        this.renderPassEvent = this.settings.renderPassEvent;

        this.settings.material.SetFloat("_WidthMultiplier", this.settings.widthMultipler);
        this.settings.material.SetFloat("_HeightMultiplier", this.settings.heightMultiplier);
        this.settings.material.SetColor("_Color", this.settings.color);
    }

    private class PassData
    {

    }
    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        var res = frameData.Get<UniversalResourceData>();
        var shared = frameData.Get<SharedTextureData>();
        if (shared == null || !shared.writtenTexture.IsValid()) return;

        using var builder = renderGraph.AddRasterRenderPass<PassData>("Composite", out var passData);

        // We will READ the produced texture…
        builder.UseTexture(shared.writtenTexture);
        // …and WRITE to the camera color
        builder.SetRenderAttachment(res.activeColorTexture, 0);

        builder.SetRenderFunc((PassData _, RasterGraphContext ctx) =>
        {
            settings.material.SetTexture("_Texture", shared.writtenTexture); // this is the texture we wrote in the previous pass
            Blitter.BlitTexture(ctx.cmd, Vector2.one, settings.material, 0); // settings.material -> our blur logic applied to the texture
        });
    }
}