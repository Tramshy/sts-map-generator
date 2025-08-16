using UnityEngine;

namespace StSMapGenerator
{
    public enum LayerTypes
    {
        [Tooltip("Most likely used for first floor")] MinorEnemy,
        RandomNode,
        RestSpot,
        Treasure,
        [Tooltip("Most likely used for last floor")] Boss,
        Custom
    }

    [System.Serializable]
    public class MapLayer
    {
        public LayerTypes LayerType = LayerTypes.RandomNode;

        public float YPaddingFromPreviousLayer = 200;
        public float RandomizePosition = 1;

        public bool UseRandomRange;

        [Tooltip("-1 for random")]
        [SerializeField] private int _amountOfNodes = 4;

        [Tooltip("Range SHOULD be exclusive")]
        [SerializeField] private Vector2Int _randomNodeRange;
        [System.NonSerialized] private int? _randomNodeAmount = null;

        /// <summary>
        /// This is only used for Custom layer types.
        /// </summary>
        public string LayerID;

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
        [HideInInspector] public LayerTypes ThisLayerType;

        /// <summary>
        /// This is only used for Custom layer types.
        /// </summary>
        [HideInInspector] public string LayerID;

        public PointOfInterest[] PointsOfInterestForThisLayerType;
    }

    [CreateAssetMenu(fileName = "New Map Config", menuName = "StS Map Generation/Map Config")]
    public class MapConfig : ScriptableObject
    {
        [field: SerializeField] public MapLayer[] LayerLayout;
        public POIPrefabs[] PrefabsForLayerTypes;

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

        [Tooltip("Here you can add new custom layer types. " +
                "If you want a layer to use your custom layer type, you will have to select 'Other' when selecting the LayerType. " +
                "You will then get additional options to determine a custom layer Type")]
        public string[] CustomLayerTypes;

        public PointOfInterest GetRandomPOI(LayerTypes LayerType)
        {
            return GetRandomFromArray(GetPOIList(LayerType), LayerType.ToString());
        }

        public PointOfInterest GetRandomPOI(string layerType)
        {
            return GetRandomFromArray(GetPOIList(layerType), layerType);
        }

        private PointOfInterest GetRandomFromArray(PointOfInterest[] poiList, string layerType)
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

            throw new System.Exception($"POI array for {layerType} layer type is empty.");
        }

        private PointOfInterest[] GetPOIList(LayerTypes layerType)
        {
            for (int i = 0; i < PrefabsForLayerTypes.Length; i++)
            {
                if (PrefabsForLayerTypes[i].ThisLayerType == layerType)
                    return PrefabsForLayerTypes[i].PointsOfInterestForThisLayerType;
            }

            throw new System.Exception("No POI list found for layer type, this should not be possible.");
        }

        private PointOfInterest[] GetPOIList(string layerType)
        {
            for (int i = 0; i < PrefabsForLayerTypes.Length; i++)
            {
                if (PrefabsForLayerTypes[i].LayerID == layerType)
                    return PrefabsForLayerTypes[i].PointsOfInterestForThisLayerType;
            }

            throw new System.Exception("No POI list found for layer type, this should not be possible.");
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
