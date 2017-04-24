using UnityEngine;
using System;

namespace Gossage.System
{
    [Serializable]
    public class AsteroidSettings
    {
        public float SpawnDelayStart = 2.5f, SpawnDelayEnd = 0.5f;
        public float MinSpeed = 1, MaxSpeed = 2;
        public int PoolSize = 20;
        public float SuperChance = 0.1f;
        public float SuperScale = 2.0f;
        public int SuperHealth = 3;
    }

    [CreateAssetMenu(fileName = "Settings", menuName = "Gossage/ApplicationSettings")]
    public class ApplicationSettingsObject : ScriptableObject
    {
        public AsteroidSettings AsteroidSettings;
        public float GameDuration = 60;
        public int NumberOfShields = 5;
        public int ScorePerSecondSurvival = 10, ScorePerAsteroid = 100, ScorePerSuperAsteroid = 200;
        public float UiFadeTime = 2.0f;
        public float DamageFadeTime = 0.1f;
    }

}