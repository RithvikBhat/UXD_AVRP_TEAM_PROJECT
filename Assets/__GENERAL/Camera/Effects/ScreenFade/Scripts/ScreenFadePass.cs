using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace HCIG.VisualEffects {

    public class ScreenFadePass : ScriptableRenderPass {

        private FadeSettings _settings = null;

        public ScreenFadePass(FadeSettings newSettings) {
            if(newSettings != null) {
                _settings = newSettings;
                renderPassEvent = newSettings.RenderPassEvent;
            }
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {

            CommandBuffer command = CommandBufferPool.Get(_settings.ProfilerTag);

            RenderTargetIdentifier source = BuiltinRenderTextureType.CameraTarget;
            RenderTargetIdentifier target = BuiltinRenderTextureType.CurrentActive;

            command.Blit(source, target, _settings.RuntimeMaterial);

            context.ExecuteCommandBuffer(command);

            CommandBufferPool.Release(command);
        }
    }
}
