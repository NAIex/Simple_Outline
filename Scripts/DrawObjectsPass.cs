using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class DrawObjectsPass : ScriptableRenderPass
{

    private DrawObjectsSettings settings;
    private static readonly ShaderTagId[] shaderTags = new ShaderTagId[]
    {
        new ShaderTagId("UniversalForward"), // standard URP opaque pass
        new ShaderTagId("SRPDefaultUnlit")   // unlit objects
    };

    public DrawObjectsPass(DrawObjectsSettings settings)
    {
        this.settings = settings;
        this.renderPassEvent = this.settings.renderPassEvent;
    }

    public void Setup(FirstPassSettings settings)
    {
        this.settings = settings;
        this.renderPassEvent = this.settings.renderPassEvent;
    }

    private class DrawPassData
    {
        public RendererListHandle rendererListHandle;
    }

    private void InitRendererLists(ContextContainer frameData, ref DrawPassData passData, RenderGraph renderGraph)
    {
        // Access the relevant frame data from the Universal Render Pipeline
        UniversalRenderingData universalRenderingData = frameData.Get<UniversalRenderingData>();
        UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
        UniversalLightData lightData = frameData.Get<UniversalLightData>();

        SortingCriteria sortFlags = cameraData.defaultOpaqueSortFlags;
        RenderQueueRange renderQueueRange = RenderQueueRange.opaque;

        // setting the objects we want to draw
        FilteringSettings filterSettings = new FilteringSettings(renderQueueRange, settings.layers);
        DrawingSettings drawSettings = RenderingUtils.CreateDrawingSettings(shaderTags.ToList(), universalRenderingData, cameraData, lightData, sortFlags);
        if (settings.overrideMaterial != null)
        {
            // setting another material for drawing the objects -> you can choose one which draws objects as white.
            drawSettings.overrideMaterial = settings.overrideMaterial;
        }

        var param = new RendererListParams(universalRenderingData.cullResults, drawSettings, filterSettings);
        passData.rendererListHandle = renderGraph.CreateRendererList(param);
    }
    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        var res = frameData.Get<UniversalResourceData>();
        var shared = frameData.GetOrCreate<SharedTextureData>();

        // Create an RG texture with the camera's descriptor
        var desc = res.activeColorTexture.GetDescriptor(renderGraph);
        desc.depthBufferBits = 0;
        desc.name = "_Texture";

        shared.writtenTexture = renderGraph.CreateTexture(desc);

        using var builder = renderGraph.AddRasterRenderPass<DrawPassData>(
            "Draw Objects", out var passData);

        // init objects to be rendered
        InitRendererLists(frameData, ref passData, renderGraph);
        
        if (!passData.rendererListHandle.IsValid()) return;

        builder.UseRendererList(passData.rendererListHandle);


        builder.SetRenderAttachment(shared.writtenTexture, 0); // set texture we want to render on

        builder.SetRenderFunc((DrawPassData data, RasterGraphContext ctx) =>
        {
            // Draw into shared.texture
            ctx.cmd.ClearRenderTarget(true, true, Color.clear);  // clear image from previous frame
            ctx.cmd.DrawRendererList(passData.rendererListHandle); // draw the objects with the custom material on the texture

        });
    }
}