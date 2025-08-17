#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace StSMapGenerator
{
    public abstract class PointOfInterest : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public List<PointOfInterest> NextPointsOfInterest { get; set; } = new List<PointOfInterest>();

        /// <summary>
        /// Created based on sub-seed.
        /// Use this for all random values for this node to remain consistent per seed.
        /// </summary>
        protected System.Random rnd { get; private set; }
        /// <summary>
        /// The image of this node.
        /// </summary>
        protected Image thisImage;

        /// <summary>
        /// Color set in SetAvailability method.
        /// </summary>
        protected Color deactivatedColor, activatedColor;

        public int FloorIndex { get; private set; }
        protected int subSeed { get; private set; }

        [Tooltip("A higher value will result in a higher probability of this node being spawned.")]
        [field: SerializeField] public int Weight { get; private set; }

        /// <summary>
        /// Set in SetAvailability method by default.
        /// </summary>
        protected bool isAvailable;

#if UNITY_EDITOR
        private void OnValidate()
        {
            Weight = Mathf.Clamp(Weight, 0, int.MaxValue);
        }
#endif

        /// <summary>
        /// Please call base.Awake for field initialization.
        /// </summary>
        protected virtual void Awake()
        {
            thisImage = GetComponent<Image>();

            if (thisImage == null)
                throw new System.NullReferenceException("PointOfInterest object needs an Image component!");

            deactivatedColor = Color.gray;
            activatedColor = Color.white;
        }

        internal void SetUpSubSeed(int floorN, int xNum)
        {
            subSeed = SeedHandler.Instance.HashSubSeed(floorN, xNum);

            FloorIndex = floorN;
            rnd = new System.Random(subSeed);
        }

        /// <summary>
        /// Dictates whether or not this node can be selected.
        /// </summary>
        public void SetAvailability(bool isAvailable)
        {
            this.isAvailable = isAvailable;

            SetAvailabilityVisuals();
        }

        /// <summary>
        /// By default, applies color changes to node depending on isAvailable.
        /// </summary>
        protected virtual void SetAvailabilityVisuals()
        {
            if (isAvailable)
                thisImage.color = activatedColor;
            else
            {
                thisImage.color = deactivatedColor;
            }
        }

        public void SetDisabledColor(Color color)
        {
            deactivatedColor = color;
        }

        public void SetEnabledColor(Color color)
        {
            activatedColor = color;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnHoverEnter();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnHoverExit();
        }

        /// <summary>
        /// By default, updates scale of node on map.
        /// </summary>
        protected virtual void OnHoverEnter()
        {
            if (!isAvailable)
                return;

            var scaleIncrease = 1.5f;
            var duration = 0.5f;

            SetScale(scaleIncrease, duration);
        }

        /// <summary>
        /// By default, resets scale increase.
        /// </summary>
        protected virtual void OnHoverExit()
        {
            if (!isAvailable)
                return;

            var scaleReset = 1;
            var duration = 0.5f;

            SetScale(scaleReset, duration);
        }

        private void SetScale(float scale, float duration)
        {
            (transform as RectTransform).localScale = Vector2.one * scale;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!isAvailable)
                return;

            OnNodeEnter();
        }

        /// <summary>
        /// Called when a node is available and is selected.
        /// Make sure to call MapPlayerTracker.Instance.UpdateCurrentPOI(this); at some point when this method is called.
        /// </summary>
        public abstract void OnNodeEnter();
    }
}
