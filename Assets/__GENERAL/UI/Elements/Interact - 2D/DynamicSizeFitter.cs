using UnityEngine;

namespace HCIG.UI {

    //[ExecuteInEditMode]
    public class DynamicSizeFitter : MonoBehaviour {

        [Header("Parameters")]
        [SerializeField]
        private bool _orderLayout = false;

        [Header("Padding")]
        [Tooltip("Adds a buffer to the beginning of the whole complex")]
        [SerializeField]
        private float _top;
        [Tooltip("adds a buffer to the ending of the whole complex")]
        [SerializeField]
        private float _bottom;

        [Header("Spacing")]
        [Tooltip("Set the designated distance between each child")]
        [SerializeField]
        private float _distance;

        [Header("Conditions")]
        [Tooltip("If enabled, then we consider the height of the parent and place the last child so that it uses the entire field")]
        [SerializeField]
        private bool _considerParent = false;

        private float _initialParentHeight = 0;

        private RectTransform _myself;
        private RectTransform _parent;
        private RectTransform _child;

        private void Start() {

            _myself = transform as RectTransform;
            _parent = transform.parent as RectTransform;
        }

        /// <summary>
        /// Changes our height depending on the space needs of each child
        /// </summary>
        private void Update() {

            // Check parent size already initialized
            float parentHeight = Mathf.Max(_parent.rect.height, _parent.sizeDelta.y);

            if (parentHeight == 0) {
                return;
            }

            if (_initialParentHeight == 0) {
                _initialParentHeight = parentHeight;
            }

            // calculation variables
            float totalHeight = _top;

            float childPosition;
            float childHeight;


            // Child loop
            for (int i = 0; i < transform.childCount; i++) {
                _child = transform.GetChild(i) as RectTransform;

                if (!_child.gameObject.activeSelf) {
                    continue;
                } 

                childHeight = _child.sizeDelta.y;

                if (_orderLayout) {

                    if (i == transform.childCount - 1) {
                        // The last child in the row

                        if (totalHeight == _top) {
                            // we are the first and last active child

                            _child.anchoredPosition = new Vector2(_child.anchoredPosition.x, totalHeight);

                            if (_considerParent) {
                                // no size available ->  use parent as initial size
                                _child.sizeDelta = new Vector2(_child.sizeDelta.x, _initialParentHeight);
                                totalHeight = _initialParentHeight;
                            } else {
                                // we use our own size
                                _child.sizeDelta = new Vector2(_child.sizeDelta.x, childHeight);
                            }
                        } else if (_considerParent && _initialParentHeight > totalHeight + childHeight) {
                            // we have to be always as tall as our parent (but only when we stay same number of childs) 

                            _child.anchoredPosition = new Vector2(_child.anchoredPosition.x, -_initialParentHeight + _child.rect.height);
                            totalHeight = _initialParentHeight;
                        } else {
                            // we are tall enough... position us just underneath as all others before

                            _child.anchoredPosition = new Vector2(_child.anchoredPosition.x, -totalHeight);
                        }
                    } else {
                        // all other childs

                        _child.anchoredPosition = new Vector2(_child.anchoredPosition.x, -totalHeight);
                        childHeight = _child.rect.height + _distance;
                    }
                }

                childPosition = Mathf.Abs(_child.anchoredPosition.y);

                if (childPosition + childHeight > totalHeight) {
                    totalHeight = childPosition + childHeight;
                }
            }

            _myself.sizeDelta = new Vector2(_myself.sizeDelta.x, totalHeight + _bottom);
        }
    }
}

