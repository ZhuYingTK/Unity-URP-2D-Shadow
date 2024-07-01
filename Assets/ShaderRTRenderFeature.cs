using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ShaderRTRenderFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Setting
    {
        public string passTag = "RenderRTFeature";
        public string RTName = "GlobalRT";
        public RenderPassEvent renderEvent = RenderPassEvent.AfterRenderingTransparents;
        public string[] lightModeTags;//渲染标签
        public Color BackGroundColor = Color.clear;
    }
    
    class ShaderRTRenderPass : ScriptableRenderPass
    {
        private RenderTexture renderTexture; // 渲染目标
        private List<ShaderTagId> m_ShaderTagIdList = new List<ShaderTagId>();
        private Setting setting;
        private RTHandle _rt;  
        
        string m_ProfilerTag;
        //ProfilingSampler m_ProfilingSampler;

        public ShaderRTRenderPass(Setting setting)
        {
            this.setting = setting;
            renderPassEvent = setting.renderEvent;
            m_ProfilerTag = setting.passTag;
            //m_ProfilingSampler = new ProfilingSampler(setting.passTag);
            if (setting.lightModeTags != null && setting.lightModeTags.Length > 0)
            {
                for (int i = 0; i < setting.lightModeTags.Length; i++)
                {
                    m_ShaderTagIdList.Add(new ShaderTagId(setting.lightModeTags[i]));
                }
            }
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.camera != Camera.main) return;
            var cmd = CommandBufferPool.Get(m_ProfilerTag);
            cmd.ClearRenderTarget(true, true, setting.BackGroundColor);
            var filterSettings = new FilteringSettings(RenderQueueRange.transparent);
            var drawSettings = CreateDrawingSettings(m_ShaderTagIdList, ref renderingData, renderingData.cameraData.defaultOpaqueSortFlags);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref filterSettings);

        }
        
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            if (!renderTexture)
            {
                renderTexture = new RenderTexture(cameraTextureDescriptor);
                renderTexture.wrapMode = TextureWrapMode.Mirror;
                renderTexture.Create();
            }
            _rt = RTHandles.Alloc(renderTexture);
            ConfigureTarget(_rt);
            Shader.SetGlobalTexture(setting.RTName, renderTexture);
        }
    }

    private ShaderRTRenderPass dungeonFogRenderPass;
    public Setting setting = new Setting();

    public override void Create()
    {
        dungeonFogRenderPass = new ShaderRTRenderPass(setting);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(dungeonFogRenderPass);
    }
}