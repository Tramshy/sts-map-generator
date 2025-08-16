using UnityEngine;

namespace StSMapGenerator
{
    public class MapPlayerTracker : MonoBehaviour
    {
        public static MapPlayerTracker Instance;

        private MapGeneration _map;
        private PointOfInterest _current;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
            {
                Destroy(gameObject);
            }

            _map = GetComponent<MapGeneration>();
        }

        public void OnNewMapGenerated()
        {
            PointOfInterest current = null;

            for (int i = 0; i < _map.PointOfInterestsPerFloor[0].Length; i++)
            {
                current = _map.PointOfInterestsPerFloor[0][i];

                if (current == null)
                    continue;

                current.SetAvailability(true);
            }
        }

        /// <summary>
        /// Handles availability logic and calls UpdateCurrentPOIVisuals.
        /// By default, it will disable every node for previous floor and then enable new nodes.
        /// </summary>
        public virtual void UpdateCurrentPOI(PointOfInterest newPOI)
        {
            _current = newPOI;
            UpdateCurrentPOIVisuals();

            var thisFloor = _map.PointOfInterestsPerFloor[_current.FloorIndex];

            for (int i = 0; i < thisFloor.Length; i++)
            {
                if (thisFloor[i] != null)
                    thisFloor[i].SetAvailability(false);
            }

            PointOfInterest nextInLine = null;

            for (int i = 0; i < _current.NextPointsOfInterest.Count; i++)
            {
                nextInLine = _current.NextPointsOfInterest[i];

                nextInLine.SetAvailability(true);
            }
        }

        /// <summary>
        /// Handles visual changes to current node after updating.
        /// By default, it sets the deactivated color to white. This results in a path of nodes colored white.
        /// </summary>
        public virtual void UpdateCurrentPOIVisuals()
        {
            _current.SetDisabledColor(Color.white);
        }
    }
}
