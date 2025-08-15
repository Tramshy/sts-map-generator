using System.Collections.Generic;
using UnityEngine;

namespace StSMapGenerator
{
    public class MapPlayerTracker : MonoBehaviour
    {
        public static MapPlayerTracker Instance;

        private MapGeneration _map;
        private PointOfInterest _current;
        private List<PointOfInterest> _currentAvailablePOIs = new List<PointOfInterest>();

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

                _currentAvailablePOIs.Add(current);
            }
        }

        public void UpdateCurrentPOI(PointOfInterest newPOI)
        {
            newPOI.SetDisabledColor(Color.white);

            for (int i = 0; i < _currentAvailablePOIs.Count; i++)
            {
                _currentAvailablePOIs[i].SetAvailability(false);
            }

            _currentAvailablePOIs.Clear();
            _current = newPOI;

            PointOfInterest nextInLine = null;

            for (int i = 0; i < _current.NextPointsOfInterest.Count; i++)
            {
                nextInLine = _current.NextPointsOfInterest[i];

                nextInLine.SetAvailability(true);
                _currentAvailablePOIs.Add(nextInLine);
            }
        }
    }
}
