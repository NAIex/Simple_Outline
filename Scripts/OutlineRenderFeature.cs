using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;
// import DrawOutlinePass
// import DrawObjectsPass


public class OutlineRenderFeature : ScriptableRendererFeature
{
    [Header("First Pass")]
    [SerializeField]
    private DrawObjectsSettings objectsPassSettings;
    private DrawObjectsPass objectsPass;

    [Header("Second Pass")]
    [SerializeField]
    private DrawOutlineSettings secondPassSettings;
    private DrawOutlinePass outlinePass;

    [Serializable]
    public class DrawObjectsSettings
    {
        public RenderPassEvent renderPassEvent;

        public Shader shader;
        [HideInInspector]
        public Material material;

        public LayerMask layers;

        public Material overrideMaterial;
    }

    [Serializable]
    public class DrawOutlineSettings
    {
        public Shader shader;
        [HideInInspector]
        public Material material;

        public RenderPassEvent renderPassEvent;
        [Space]

        public float widthMultipler;
        public float heightMultiplier;
        public Color color;
    }
    

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType != CameraType.Game) return;

        if (objectsPass != null)
        {
            objectsPass.Setup(objectsPassSettings);
            renderer.EnqueuePass(objectsPass);
        }
        if (outlinePass != null)
        {
            outlinePass.ConfigureInput(ScriptableRenderPassInput.Color);
            outlinePass.Setup(outlinePassSettings);
            renderer.EnqueuePass(outlinePass);
        }
    }

    public override void Create()
    {
        if (objectsPassSettings.shader != null)
        {
            objectsPassSettings.material = new Material(objectsPassSettings.shader);
            objectsPass = new DrawObjectsPass(objectsPassSettings);
        }
        if (outlinePassSettings.shader != null)
        {
            outlinePassSettings.material = new Material(outlinePassSettings.shader);
            outlinePass = new DrawOutlinePass(outlinePassSettings);
        }
    }
}
