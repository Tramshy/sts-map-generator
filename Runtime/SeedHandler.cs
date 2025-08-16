using UnityEngine;

namespace StSMapGenerator
{
    public struct SeedContainer
    {
        public SeedContainer(int amountOfActs, int seed)
        {
            ActSeeds = new int[amountOfActs];
            var arbitrarySeedIncrease = 600;

            for (int i = 0; i < ActSeeds.Length; i++)
            {
                ActSeeds[i] = seed * (arbitrarySeedIncrease * (i + 1));
            }
        }

        // Add more for each act.
        public int[] ActSeeds;
    }

    public class SeedHandler : MonoBehaviour
    {
        public static SeedHandler Instance;

        public SeedContainer CurrentSeed { get; private set; }

        [SerializeField] private int _amountOfActs = 3;
        private int _currentAct;

        private void Awake()
        {
            transform.parent = null;
            DontDestroyOnLoad(gameObject);

            if (Instance == null)
                Instance = this;
            else
            {
                Destroy(gameObject);
            }
        }

        public void GenerateRandomSeed()
        {
            SetSeed((int)System.DateTime.Now.Ticks);
        }

        public int HashSubSeed(int floorN, int posX)
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + CurrentSeed.ActSeeds[_currentAct];
                hash = hash * 31 + posX;
                hash = hash * 31 + floorN;
                return hash;
            }
        }

        private int StringToSeed(string input)
        {
            unchecked
            {
                int hash = 23;

                foreach (char c in input)
                {
                    hash = hash * 31 + c;
                }

                return hash;
            }
        }

        public void SetSeed(int seed)
        {
            CurrentSeed = new SeedContainer(_amountOfActs, seed);
        }

        // Called from button event
        public void TrySetSeedFromString(string input)
        {
            if (input.Length == 0)
            {
                GenerateRandomSeed();

                return;
            }

            if (int.TryParse(input, out int seed))
            {
                SetSeed(seed);

                Debug.Log($"Set seed to: {seed}");

                return;
            }

            var hashedInput = StringToSeed(input);

            Debug.Log($"Hashed string input to: {hashedInput}. Setting seed to hashed");

            SetSeed(hashedInput);
        }

        public void UpdateSeedForAct(int act)
        {
            _currentAct = act;

            Random.InitState(CurrentSeed.ActSeeds[act]);
        }
    }
}
