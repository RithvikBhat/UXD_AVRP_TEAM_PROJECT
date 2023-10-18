using HCIG.Network;

using System;
using System.Collections;

using UnityEngine;
using UnityEngine.UI;

namespace HCIG.VisualEffects {

    public class ScreenFadeManager : Singleton<ScreenFadeManager> {

        /// <summary>
        /// Gets called when we reach the endpositions of the fade (0 / 1) 
        /// </summary>
        public Action<bool> OnFadeState = (_) => { };

        public bool IsActive {
            get {
                return _isActive;
            }
        }
        private bool _isActive = true;


        [Header("Feature")]
        [SerializeField]
        ScreenFadeFeature _screenFadeFeature = null;

        private Material _material;
        private Image _image;

        readonly int _shaderAlphaParameter = Shader.PropertyToID("_Alpha");

        protected override void Awake() {
            base.Awake();

            // Events
            NetworkManager.Instance.OnDisconnect += (_) => HandleDisconnect();

            // Initialization
            if(ApplicationManager.Instance.IsAndroid) {
                _material = Instantiate(_screenFadeFeature.Settings.Material);
                _screenFadeFeature.Settings.RuntimeMaterial = _material;

                // Initial-Fade
                _material.SetFloat(_shaderAlphaParameter, 1);
            } else {
                _image = GetComponent<Image>();

                _image.color = new Color(0, 0, 0, 1);
            }

            // Initial-State
            _isActive = true;
        }

        /// <summary>
        /// Switches instantly to black if we had an unexpected disconnect
        /// </summary>
        private void HandleDisconnect() {
            if (!_isActive) {
                if (ApplicationManager.Instance.IsAndroid) {
                    _material.SetFloat(_shaderAlphaParameter, 1);
                } else {
                    _image.color = new Color(0, 0, 0, 1);
                }
                _isActive = true;
            }
        }

        /// <summary>
        /// Fades the screen to black within a defined time 
        /// </summary>
        /// <param name="duration"></param>
        public void FadeOut(float duration = 0.25f) {
            if (_isActive) {
                return;
            }

            StartCoroutine(FadeRoutine(duration));
        }

        /// <summary>
        /// Makes the screen visible again within a defined time
        /// </summary>
        /// <param name="duration"></param>
        public void FadeIn(float duration = 0.25f) {
            if (!_isActive) {
                return;
            }

            StartCoroutine(FadeRoutine(duration));
        }

        /// <summary>
        /// Fades the screen to black and visible again within a defined time
        /// </summary>
        /// <param name="duration"></param>
        public void Fade(float duration = 0.5f) {
            if (_isActive) {
                return;
            }

            StartCoroutine(FadeRoutine(duration / 2, true));
        }

        /// <summary>
        /// the routine that implements the fade process
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="complete"></param>
        /// <returns></returns>
        private IEnumerator FadeRoutine(float duration, bool complete = false) {

            float refTime = Time.realtimeSinceStartup;
            float curTime = 0;

            if (_isActive) {
                // Fade In

                while (curTime < duration) {

                    yield return null;

                    if (ApplicationManager.Instance.IsAndroid) {
                        _material.SetFloat(_shaderAlphaParameter, Mathf.Lerp(1, 0, (curTime = Time.realtimeSinceStartup - refTime) / duration));
                    } else {
                        _image.color = new Color(0, 0, 0, Mathf.Lerp(1, 0, (curTime = Time.realtimeSinceStartup - refTime) / duration));
                    }
                }

                OnFadeState.Invoke(_isActive = false);
            } else {
                // fade Out

                _isActive = true;
                OnFadeState.Invoke(false);

                while (curTime < duration) {

                    yield return null;

                    if (ApplicationManager.Instance.IsAndroid) {
                        _material.SetFloat(_shaderAlphaParameter, Mathf.Lerp(0, 1, (curTime = Time.realtimeSinceStartup - refTime) / duration));
                    } else {
                        _image.color = new Color(0, 0, 0, Mathf.Lerp(0, 1, (curTime = Time.realtimeSinceStartup - refTime) / duration));
                    }
                }

                OnFadeState.Invoke(_isActive);

                if (complete) {
                    StartCoroutine(FadeRoutine(duration));
                }
            }
        }
    }
}
