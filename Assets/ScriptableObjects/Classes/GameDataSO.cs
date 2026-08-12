using UnityEngine;
using UnityEngine.Serialization;

namespace ScriptableObjects.Classes
{
    [CreateAssetMenu(menuName = "ScriptableObjects/GameDataSO",  fileName = "GameDataSO" )]
    public class GameDataSO : ScriptableObject
    {
        [SerializeField] private int questionCount = 10;
        [SerializeField] private int timePerQuestion = 30;
        [SerializeField]  private float  waitBetweenQuestions = 3f;

        public int QuestionCount => questionCount;
        public int TimePerQuestion => timePerQuestion;
        public float WaitBetweenQuestions => waitBetweenQuestions;
    }
}