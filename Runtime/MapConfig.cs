using UnityEngine;

namespace StSMapGenerator
{
    public enum NodeTypes
    {
        RandomNode,
        [Tooltip("Most likely used for first floor")] MinorEnemy,
        [Tooltip("Most likely used for last floor")] Boss,
        Treasure,
        Custom
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

        /// <summary>
        /// This is only used for Custom node types.
        /// </summary>
        public string NodeID;

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

        /// <summary>
        /// This is only used for Custom node types.
        /// </summary>
        [HideInInspector] public string NodeID;

        public PointOfInterest[] PointsOfInterestForThisNodeType;
    }

    [CreateAssetMenu(fileName = "New Map Config", menuName = "StS Map Generation/Map Config")]
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

        [Space(20)]

        [Tooltip("Here you can add new custom node types. " +
                "If you want a layer to use your custom node type, you will have to select 'Other' when selecting the LayerType. " +
                "You will then get additional options to determine a custom Node Type")]
        public string[] CustomNodeTypes;

        public PointOfInterest GetRandomPOI(NodeTypes nodeType)
        {
            return GetRandomFromArray(GetPOIList(nodeType), nodeType.ToString());
        }

        public PointOfInterest GetRandomPOI(string nodeType)
        {
            return GetRandomFromArray(GetPOIList(nodeType), nodeType);
        }

        private PointOfInterest GetRandomFromArray(PointOfInterest[] poiList, string nodeType)
        {
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

        private PointOfInterest[] GetPOIList(string nodeType)
        {
            for (int i = 0; i < PrefabsForNodeTypes.Length; i++)
            {
                if (PrefabsForNodeTypes[i].NodeID == nodeType)
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
