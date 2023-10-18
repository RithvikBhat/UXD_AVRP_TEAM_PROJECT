using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace HCIG.VisualEffects {

    [Serializable]
    public class FadeSettings {

        public bool IsEnabled = true;
        public String ProfilerTag = "Screen Fade";

        public RenderPassEvent RenderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        public Material Material = null;

        [NonSerialized] public Material RuntimeMaterial = null;

        public bool AreValid() {
            return RuntimeMaterial != null && IsEnabled;
        }
    }
}
