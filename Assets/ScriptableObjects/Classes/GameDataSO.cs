using UnityEngine;

namespace ScriptableObjects.Classes
{
    [CreateAssetMenu(menuName = "ScriptableObjects/GameDataSO",  fileName = "GameDataSO" )]
    public class GameDataSO : ScriptableObject
    {
        [SerializeField] private int questionCount = 10;
        [SerializeField] private int timeLimitInSeconds = 30;

        public int QuestionCount => questionCount;
        public int TimeLimitInSeconds => timeLimitInSeconds;
    }
}