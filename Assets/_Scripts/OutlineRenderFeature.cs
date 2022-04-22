using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//public class OutlineRenderFeature : ScriptableRendererFeature
//{
//	public class OutlinePass : ScriptableRenderPass
//	{
//		Material material;

//		void Init()
//		{

//		}

//		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
//		{
//			throw new System.NotImplementedException();
//		}
//	}

//	[System.Serializable]
//	public class Settings
//    {
//        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
//        public Shader shader;
//    }

//    [SerializeField] Settings settings = new Settings();
//    private OutlinePass pass;

//    public override void Create()
//    {
//        name = "Outliner";
//        pass = new OutlinePass(settings);
//    }

//    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
//    {
//		pass.Init(renderer.cameraColorTarget);
//        renderer.EnqueuePass(pass);
//    }
//}

