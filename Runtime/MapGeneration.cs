using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace StSMapGenerator
{
    public class MapGeneration : MonoBehaviour
    {
        [SerializeField] private MapConfig _config;

        [SerializeField] private UnityEvent _onMapGenerated;
        [SerializeField] private RectTransform _boardContainer, _mapBoundingBox, _canvas;
        [SerializeField] private List<PointOfInterest> _pointsOfInterestPrefabs;
        [SerializeField] private GameObject _pathPrefab;

        public PointOfInterest[][] PointOfInterestsPerFloor { get; set; }
        private List<PointOfInterest> _pointsOfInterest = new();
        private List<int> _bossFloors = new List<int>();

        [SerializeField] private float _xPadding, _edgeYOffset = 100;
        private float _xMaxSize;
        private float _lineLength;
        private float _lineHeight;
        private float[] _totalPaddingPerFloor;

        private int _numberOfConnections = 0;
        private int[] _numberOfPointsOfInterestsPerFloor;

        private void Awake()
        {
            _lineLength = _pathPrefab.GetComponent<RectTransform>().rect.width;
            _lineHeight = _pathPrefab.GetComponent<RectTransform>().rect.height;
        }

        private void Start()
        {
            _xMaxSize = _mapBoundingBox.rect.width - _xPadding;
        }

        public void RecreateBoard(int actToRecreateBoard = 0)
        {
            _config.ClearMapLayerData();
            SeedHandler.Instance.UpdateSeedForAct(actToRecreateBoard);

            _numberOfConnections = 0;

            // Parent will get set in MapContentResizer, called by event.
            _boardContainer.SetParent(_canvas, false);

            DestroyAllChildren(_boardContainer);

            _pointsOfInterest.Clear();

            PointOfInterestsPerFloor = new PointOfInterest[_config.MapLength][];
            _numberOfPointsOfInterestsPerFloor = new int[_config.MapLength];
            _totalPaddingPerFloor = new float[_config.MapLength];

            for (int i = 0; i < PointOfInterestsPerFloor.Length; i++)
            {
                PointOfInterestsPerFloor[i] = new PointOfInterest[_config.MaxWidth];

                _totalPaddingPerFloor[i] = _config.LayerLayout[i].YPaddingFromPreviousLayer;

                if (i != 0)
                    _totalPaddingPerFloor[i] += _totalPaddingPerFloor[i - 1];
            }

            CreateMap();
        }

        private float GetOffset(int thisIndex, int finalNodesOnFloor)
        {
            if (finalNodesOnFloor <= 1)
                return 0;

            var rightEdgeWithExtraPadding = _xMaxSize - _xPadding * 4;
            float t = (float)thisIndex / (finalNodesOnFloor - 1);

            return Mathf.Lerp(0, rightEdgeWithExtraPadding, t) - rightEdgeWithExtraPadding * 0.5f;
        }

        private PointOfInterest InstantiatePointOfInterest(int floorN, int xNum)
        {
            var thisLayerType = _config.LayerLayout[floorN].LayerType;
            var isBossFloor = thisLayerType == NodeTypes.Boss;

            // Boss layers have xNum recalculated.
            if (PointOfInterestsPerFloor[floorN][xNum] != null && !isBossFloor)
                return PointOfInterestsPerFloor[floorN][xNum];

            if (_config.LayerLayout[floorN].AmountOfNodes != -1)
            {
                if (_numberOfPointsOfInterestsPerFloor[floorN] >= _config.LayerLayout[floorN].AmountOfNodes)
                    return GetClosestPOI(floorN, xNum, 0);
            }

            float xSize = _xMaxSize / _config.MaxWidth;
            float xPos = ((xSize * xNum) + (xSize * 0.5f)) - _xMaxSize * 0.5f;
            float yPos = _totalPaddingPerFloor[floorN] + _edgeYOffset;

            if (isBossFloor)
            {
                _bossFloors.Add(floorN);

                var bossesOnFloor = _config.LayerLayout[floorN].AmountOfNodes;

                xPos = GetOffset(_numberOfPointsOfInterestsPerFloor[floorN], bossesOnFloor);

                if (bossesOnFloor == 1)
                    xNum = Mathf.RoundToInt((float)(_config.MaxWidth - 1f) * 0.5f);
                else
                {
                    xNum = Mathf.RoundToInt(_numberOfPointsOfInterestsPerFloor[floorN] * (_config.MaxWidth - 1) / (float)(bossesOnFloor - 1));
                }
            }

            xPos += Random.Range(-xSize * 0.25f, xSize * 0.25f) * _config.LayerLayout[floorN].RandomizePosition;
            yPos += Random.Range(-_config.LayerLayout[1].YPaddingFromPreviousLayer * 0.25f, _config.LayerLayout[1].YPaddingFromPreviousLayer * 0.25f) * _config.LayerLayout[floorN].RandomizePosition;

            Vector2 pos = new Vector2(xPos, yPos);
            PointOfInterest instance = Instantiate(_config.GetRandomPOI(thisLayerType), _boardContainer);
            instance.SetUpSubSeed(floorN, xNum);
            _pointsOfInterest.Add(instance);
            _numberOfPointsOfInterestsPerFloor[floorN]++;

            instance.transform.localPosition = pos;
            PointOfInterestsPerFloor[floorN][xNum] = instance;

            int created = 0;

            void InstantiateNextPoint(int index_i, int index_j)
            {
                PointOfInterest nextPOI = InstantiatePointOfInterest(index_i, index_j);

                var isNextBossFloor = _config.LayerLayout[index_i].LayerType == NodeTypes.Boss;

                // Boss floors need to calculate closest POI's since their positions aren't the same as other floors.
                if (!isNextBossFloor)
                {
                    AddLineBetweenPoints(instance, nextPOI);
                    instance.NextPointsOfInterest.Add(nextPOI);
                }

                created++;
                _numberOfConnections++;
            }

            while (created == 0 && floorN < _config.MapLength - 1)
            {
                if (xNum > 0 && Random.Range(0f, 1f) < _config.ChancePathSide)
                {
                    if (_config.AllowPathCrossing || PointOfInterestsPerFloor[floorN + 1][xNum] == null)
                        InstantiateNextPoint(floorN + 1, xNum - 1);
                }

                if (xNum < _config.MaxWidth - 1 && Random.Range(0f, 1f) < _config.ChancePathSide)
                {
                    if (_config.AllowPathCrossing || PointOfInterestsPerFloor[floorN + 1][xNum] == null)
                        InstantiateNextPoint(floorN + 1, xNum + 1);
                }

                if (Random.Range(0f, 1f) < _config.ChancePathMiddle)
                    InstantiateNextPoint(floorN + 1, xNum);
            }

            return instance;
        }

        private void CreateMap()
        {
            int[] positions = GetRandomIndexes(_config.LayerLayout[0].AmountOfNodes);

            // Generate chaotic layout.
            for (int i = 0; i < positions.Length; i++)
            {
                InstantiatePointOfInterest(0, positions[i]);
            }

            // Fill remaining POI's.
            for (int i = 0; i < _numberOfPointsOfInterestsPerFloor.Length; i++)
            {
                if (_numberOfPointsOfInterestsPerFloor[i] >= _config.LayerLayout[i].AmountOfNodes)
                    continue;

                while (_numberOfPointsOfInterestsPerFloor[i] < _config.LayerLayout[i].AmountOfNodes)
                {
                    var xNum = GetRandomFreeIndex(i);
                    var newPOI = InstantiatePointOfInterest(i, xNum);

                    // When i is 0 it should never get past first check, but just to be safe.
                    if (i == 0)
                        continue;

                    var lastPOI = GetClosestPOI(i, xNum, -1);
                    lastPOI.NextPointsOfInterest.Add(newPOI);

                    AddLineBetweenPoints(lastPOI, newPOI);
                }
            }

            // Connect boss floors.
            for (int i = 0; i < _bossFloors.Count; i++)
            {
                var bossFloor = _bossFloors[i];
                var floorBeforeBoss = _bossFloors[i] - 1;

                // Connect boss floors to closest nodes to create minimum path
                for (int j = 0; j < PointOfInterestsPerFloor[bossFloor].Length; j++)
                {
                    var currentBoss = PointOfInterestsPerFloor[bossFloor][j];

                    if (currentBoss == null)
                        continue;

                    var closestNode = GetClosestPOI(bossFloor, j, -1);

                    if (closestNode.NextPointsOfInterest.Contains(currentBoss))
                        continue;

                    closestNode.NextPointsOfInterest.Add(currentBoss);
                    AddLineBetweenPoints(closestNode, currentBoss);
                }

                // Connect nodes before boss to closest boss so all paths lead to a boss.
                for (int j = 0; j < PointOfInterestsPerFloor[floorBeforeBoss].Length; j++)
                {
                    var currentNode = PointOfInterestsPerFloor[floorBeforeBoss][j];

                    if (currentNode == null)
                        continue;

                    var closestBoss = GetClosestPOI(floorBeforeBoss, j, 1);

                    if (currentNode.NextPointsOfInterest.Contains(closestBoss))
                        continue;

                    currentNode.NextPointsOfInterest.Add(closestBoss);
                    AddLineBetweenPoints(currentNode, closestBoss);
                }
            }

            if (_numberOfConnections <= _config.MapLength * _config.MultiplicativeNumberOfMinimumConnections)
            {
                Debug.Log($"Recreating board with {_numberOfConnections} connections");
                RecreateBoard();
                return;
            }

            Debug.Log($"Created board with {_numberOfConnections} connections");
            Debug.Log($"Created board with {_pointsOfInterest.Count} points");

            _onMapGenerated?.Invoke();
        }

        /// <summary>
        /// Finds the closest POI to target.
        /// </summary>
        /// <param name="floorN">The floor of searching POI</param>
        /// <param name="xNum">The position of the searching POI</param>
        /// <param name="lookDir">1 = increment upward, 0 = look adjacent, -1 = increment down</param>
        private PointOfInterest GetClosestPOI(int floorN, int xNum, int lookDir)
        {
            var row = PointOfInterestsPerFloor[floorN + lookDir];

            for (int dist = 0; dist < _config.MaxWidth; dist++)
            {
                // Check left side
                int leftIndex = xNum - dist;

                if (leftIndex >= 0 && row[leftIndex] != null)
                    return row[leftIndex];

                // Check right side (skip if same as leftIndex)
                int rightIndex = xNum + dist;

                if (rightIndex < _config.MaxWidth && rightIndex != leftIndex && row[rightIndex] != null)
                    return row[rightIndex];
            }

            throw new System.Exception("No floor nodes found.");
        }

        private void AddLineBetweenPoints(PointOfInterest thisPoint, PointOfInterest nextPoint)
        {
            var thisTransform = thisPoint.transform as RectTransform;
            var nextTransform = nextPoint.transform as RectTransform;

            Vector2 dir = (nextTransform.anchoredPosition - thisTransform.anchoredPosition).normalized;

            float distance = Vector3.Distance(thisTransform.anchoredPosition, nextTransform.anchoredPosition);

            //Number of lines (with padding) that fits inside the space from point A to B
            int num = (int)(distance / (_lineLength * _config.MultiplicativeSpaceBetweenLines));

            if (num <= 0)
                num = 1;

            //Find the real padding distance, since num is rounded to integer, the padding may increase
            float pad = (distance - (num * _lineLength)) / (num + 1);

            Vector2 pos_i = thisTransform.anchoredPosition + (dir * (pad + (_lineLength / 2f)));

            //Position all the lines
            for (int i = 0; i < num; i++)
            {
                Vector2 pos = pos_i + ((_lineLength + pad) * i * dir);
                GameObject lineCreated = Instantiate(_pathPrefab);
                var lineRectTransform = lineCreated.transform as RectTransform;

                lineRectTransform.SetParent(_boardContainer, false);
                lineRectTransform.anchoredPosition = pos;
                lineRectTransform.anchoredPosition -= Vector2.up * (_lineHeight / 2f);

                LookAt2D(lineRectTransform, nextPoint.transform.position);
            }
        }

        private void LookAt2D(RectTransform from, Vector3 target)
        {
            Vector3 dir = target - from.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            from.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        private int[] GetRandomIndexes(int xNum)
        {
            int[] numbers = new int[_config.MaxWidth];
            int[] indexes = new int[xNum];

            for (int i = 0; i < _config.MaxWidth; i++)
            {
                numbers[i] = i;
            }

            for (int i = numbers.Length - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                int temp = numbers[i];

                numbers[i] = numbers[j];
                numbers[j] = temp;
            }

            for (int i = 0; i < xNum; i++)
            {
                indexes[i] = numbers[i];
            }

            return indexes;
        }

        private int GetRandomFreeIndex(int floorN)
        {
            List<int> freeIndexes = new List<int>();

            for (int i = 0; i < _config.MaxWidth; i++)
            {
                if (PointOfInterestsPerFloor[floorN][i] == null)
                    freeIndexes.Add(i);
            }

            if (freeIndexes.Count == 0)
                throw new System.Exception($"All spaces on {floorN} are filled!");

            return freeIndexes[Random.Range(0, (freeIndexes.Count))];
        }

        private void DestroyAllChildren(Transform transform)
        {
            List<Transform> toKill = new();

            foreach (Transform child in transform)
            {
                toKill.Add(child);
            }

            for (int i = toKill.Count - 1; i >= 0; i--)
            {
                Destroy(toKill[i].gameObject);
            }
        }
    }
}
