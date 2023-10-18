using System;
using UnityEngine;

namespace HCIG.UsefulTools {

    public class ColorPaletteManager : Singleton<ColorPaletteManager> {

        public Action<Color> OnColorChanged = (_) => { };

        public Color Color {
            get {
                return _color;
            }
            set {
                if (_color == value) {
                    return;
                }

                OnColorChanged.Invoke(_color = value);
            }
        }
        [Header("Color")]
        [SerializeField]
        private Color _color = Color.cyan;
    }
}
