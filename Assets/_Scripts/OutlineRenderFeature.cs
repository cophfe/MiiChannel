using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class OutlineRenderFeature : ScriptableRendererFeature
{
	public class OutlinePass : ScriptableRenderPass
	{
		Material outlineMaterial;

		RenderTargetIdentifier src;
		RenderTargetHandle dest;
		RenderTargetHandle tempTexture;

		public OutlinePass(Material outlineMat, RenderPassEvent renderPassEvent)
		{
			outlineMaterial = outlineMat;
			this.renderPassEvent = renderPassEvent;
		}

		public void Init(ScriptableRenderer renderer)
		{
			src = renderer.cameraColorTarget;
			dest = RenderTargetHandle.CameraTarget;
		}

		public override void Execute(ScriptableRenderContext ctx, ref RenderingData renderingData)
		{
			CommandBuffer commandBuffer = CommandBufferPool.Get();
			var descriptor = renderingData.cameraData.cameraTargetDescriptor;
			//descriptor.depthBufferBits = 0; //if > 0 it goes black for some reason (update: only when blitting with material first instead of secxond

			//blit to middleman texture, then blit back to source
			//that way the camera texture can be read
			commandBuffer.GetTemporaryRT(tempTexture.id, descriptor, FilterMode.Point);
			Blit(commandBuffer, src, tempTexture.Identifier());
			Blit(commandBuffer, tempTexture.Identifier(), src, outlineMaterial, 0);

			ctx.ExecuteCommandBuffer(commandBuffer);
			CommandBufferPool.Release(commandBuffer);
		}

		public override void FrameCleanup(CommandBuffer cmd)
		{
			cmd.ReleaseTemporaryRT(tempTexture.id);
			base.FrameCleanup(cmd);
		}
	}

	[System.Serializable]
	public class Settings
	{
		public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
		public Material material;
		public LayerMask outlinedLayers;
		public int shaderPassIndex = 0;
	}

	public Settings settings = new Settings();
	OutlinePass pass;

	public override void Create()
	{
		pass = new OutlinePass(settings.material, settings.renderPassEvent);
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		if (settings.material == null)
		{
			Debug.LogWarning("Missing material for custom render feature.");
			return;
		}

		pass.Init(renderer);
		renderer.EnqueuePass(pass);
	}
}

