using UnityEngine.Rendering.Universal;

namespace HCIG.VisualEffects {

    public class ScreenFadeFeature : ScriptableRendererFeature {
        private ScreenFadePass _renderPass = null;

        public FadeSettings Settings = null;

        public override void Create() {
            _renderPass = new ScreenFadePass(Settings);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {

            if (Settings.AreValid()) {
                renderer.EnqueuePass(_renderPass);
            }
        }
    }
}
