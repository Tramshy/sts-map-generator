#if DOTWEEN
using DG.Tweening;
#endif

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace StSMapGenerator
{
    public class PointOfInterest : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public List<PointOfInterest> NextPointsOfInterest { get; set; } = new List<PointOfInterest>();

        private System.Random _rng;
        private Image _thisImage;

        private Color _deactivatedColor;

        private int _subSeed;
        public int Weight;

        private bool _isAvailable;

        private void Awake()
        {
            _thisImage = GetComponent<Image>();

            _deactivatedColor = _thisImage.color;
        }

        public void SetUpSubSeed(int floorN, int xNum)
        {
            _subSeed = SeedHandler.Instance.HashSubSeed(floorN, xNum);

            _rng = new System.Random(_subSeed);
        }

        public void SetAvailability(bool isAvailable)
        {
            _isAvailable = isAvailable;

            var highlight = Color.white;
#if DOTWEEN
            var duration = 0.75f;
#endif

            if (isAvailable)
            {
#if DOTWEEN
                _thisImage
                .DOColor(highlight, duration)
                .SetLoops(-1, LoopType.Yoyo)
                .SetTarget(_thisImage);
#else
                _thisImage.color = highlight;
#endif
            }
            else
            {
#if DOTWEEN
                _thisImage.DOKill();
                _thisImage.DOColor(_deactivatedColor, duration * 0.5f)
                    .SetTarget(this);
#else
                _thisImage.color = _deactivatedColor;
#endif
            }
        }

        public void SetDisabledColor(Color color)
        {
            _deactivatedColor = color;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            // If you decide to add full keyboard controls:
            // Move SetScale to another component and call to that component from here.

            if (!_isAvailable)
                return;

            var scaleIncrease = 1.5f;
            var duration = 0.5f;

            SetScale(scaleIncrease, duration);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!_isAvailable)
                return;

            var scaleReset = 1;
            var duration = 0.5f;

            SetScale(scaleReset, duration);
        }

        private void SetScale(float scale, float duration)
        {
#if DOTWEEN
            transform.DOKill();

            (transform as RectTransform)
            .DOScale(scale, duration)
            .SetTarget(transform);
#else
            (transform as RectTransform).localScale = Vector2.one * scale;
#endif
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!_isAvailable)
                return;

            MapPlayerTracker.Instance.UpdateCurrentPOI(this);

            // Add some logic for randomizing battle here and all o' that jazz.
        }

        private void OnDisable()
        {
#if DOTWEEN
            DOTween.Kill(this);
            _thisImage.DOKill();
            transform.DOKill();
#endif
        }
    }
}
