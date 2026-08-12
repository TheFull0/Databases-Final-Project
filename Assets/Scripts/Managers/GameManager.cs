using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ScriptableObjects.Classes;
using Structs;
using UnityEngine;

namespace Managers
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private GameDataSO _gameData;
        private IEnumerable<Question> _questions;


        private async void Awake()
        {
            _questions = await ServerFunctions.GetRandomQuestions(_gameData.QuestionCount);
        }
    }
}