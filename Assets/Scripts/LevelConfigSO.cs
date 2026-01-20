using UnityEngine;
namespace Core.Data
{
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "Block-Fast/Level Config")]
    public class LevelConfigSO : ScriptableObject
    {
        public int levelNumber;
        public int targetScore;
        public int maxMoves;
        public float difficulty;
        public string description;
    }
}
