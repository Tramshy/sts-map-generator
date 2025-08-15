using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StSMapGenerator
{
    public enum NodeTypes
    {
        RandomNode,
        MinorEnemy,
        Boss,
        Treasure
    }

    [System.Serializable]
    public class MapLayer
    {
        public NodeTypes LayerType = NodeTypes.RandomNode;

        public float YPaddingFromPreviousLayer = 200;
        [Range(0f, 1f)] public float RandomizePosition = 1;

        public bool UseRandomRange;

        [Tooltip("-1 for random")]
        [SerializeField] private int _amountOfNodes = 4;

        [Tooltip("Range SHOULD be exclusive")]
        [SerializeField] private Vector2Int _randomNodeRange;
        [System.NonSerialized] private int? _randomNodeAmount = null;

        public int AmountOfNodes
        {
            get
            {
                if (UseRandomRange)
                {
                    if (_randomNodeAmount == null)
                        _randomNodeAmount = Random.Range(_randomNodeRange.x, _randomNodeRange.y);

                    return (int)_randomNodeAmount;
                }

                return _amountOfNodes;
            }
        }

        public void ClearStoredRandomNodeAmount()
        {
            if (UseRandomRange)
                _randomNodeAmount = null;
        }
    }

    [System.Serializable]
    public class POIPrefabs
    {
        [HideInInspector] public NodeTypes ThisNodeType;

        public PointOfInterest[] PointsOfInterestForThisNodeType;
    }

    [CreateAssetMenu(fileName = "New Map Config", menuName = "Scriptable Objects/Map Config")]
    public class MapConfig : ScriptableObject
    {
        [field: SerializeField] public MapLayer[] LayerLayout;
        public POIPrefabs[] PrefabsForNodeTypes;

        [Space(30)]

        [Range(0.2f, 1)] public float ChancePathMiddle;
        [Range(0.2f, 1)] public float ChancePathSide;
        [Range(0.9f, 5)] public float MultiplicativeSpaceBetweenLines = 2.5f;
        [Range(1, 5.5f)] public float MultiplicativeNumberOfMinimumConnections = 3;

        [Space(30)]

        public int MaxWidth = 5;
        public int MapLength
        {
            get
            {
                return LayerLayout.Length;
            }
        }

        public bool AllowPathCrossing = false;

        public PointOfInterest GetRandomPOI(NodeTypes nodeType)
        {
            var poiList = GetPOIList(nodeType);
            var totalWeight = 0;

            for (int i = 0; i < poiList.Length; i++)
            {
                totalWeight += poiList[i].Weight;
            }

            var random = Random.Range(0, totalWeight);

            for (int i = 0; i < poiList.Length; i++)
            {
                if (random < poiList[i].Weight || random == 0)
                    return poiList[i];

                random -= poiList[i].Weight;
            }

            throw new System.Exception($"POI array for {nodeType} node type is empty.");
        }

        private PointOfInterest[] GetPOIList(NodeTypes nodeType)
        {
            for (int i = 0; i < PrefabsForNodeTypes.Length; i++)
            {
                if (PrefabsForNodeTypes[i].ThisNodeType == nodeType)
                    return PrefabsForNodeTypes[i].PointsOfInterestForThisNodeType;
            }

            throw new System.Exception("No POI list found for node type, this should not be possible.");
        }

        public void ClearMapLayerData()
        {
            for (int i = 0; i < LayerLayout.Length; i++)
            {
                LayerLayout[i].ClearStoredRandomNodeAmount();
            }
        }
    }
}
