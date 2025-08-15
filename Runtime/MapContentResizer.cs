using System.Linq;
using UnityEngine;

namespace StSMapGenerator
{
    public class MapContentResizer : MonoBehaviour
    {
        [SerializeField] private RectTransform _contentRect, _mapRect;
        private RectTransform[] _mapNodes;
        private RectTransform _thisRectTransform;

        private void Awake()
        {
            _thisRectTransform = GetComponent<RectTransform>();
        }

        public void OnGeneratedMap()
        {
            _mapNodes = _contentRect.GetComponentsInChildren<RectTransform>()
                .Where(rTransform => rTransform != _contentRect)
                .ToArray();

            ResizeContentToFitNodes();
        }

        public void ResizeContentToFitNodes()
        {
            if (_mapNodes == null || _mapNodes.Length == 0)
            {
                Debug.LogWarning("No map nodes assigned!");
                return;
            }

            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minY = float.MaxValue;
            float maxY = float.MinValue;

            foreach (var node in _mapNodes)
            {
                Vector3[] corners = new Vector3[4];
                node.GetWorldCorners(corners);

                for (int i = 0; i < 4; i++)
                {
                    Vector3 corner = _thisRectTransform.InverseTransformPoint(corners[i]);

                    if (corner.x < minX) minX = corner.x;
                    if (corner.x > maxX) maxX = corner.x;
                    if (corner.y < minY) minY = corner.y;
                    if (corner.y > maxY) maxY = corner.y;
                }
            }

            // Calculate new size
            float width = maxX - minX;
            float height = maxY - minY;

            var extraHeightPadding = 300;
            height += extraHeightPadding;

            // Resize content rect to fit all nodes
            _thisRectTransform.sizeDelta = new Vector2(width, height);

            ScrollerResizeFinished();
        }

        private void PositionRectTransformAtBottom(RectTransform toPosition, RectTransform target)
        {
            Vector3[] corners = new Vector3[4];
            target.GetWorldCorners(corners);

            Vector3 centerBottomWorld = (corners[0] + corners[3]) / 2f;
            Vector3 localPoint = toPosition.parent.InverseTransformPoint(centerBottomWorld);

            toPosition.anchoredPosition = localPoint;
        }

        private void ScrollerResizeFinished()
        {
            PositionRectTransformAtBottom(_contentRect, _thisRectTransform);
            _contentRect.SetParent(_thisRectTransform, true);
            // This is to set the bottom of the map to the front of the view.
            _thisRectTransform.anchoredPosition += Vector2.up * 4000;
        }
    }
}
