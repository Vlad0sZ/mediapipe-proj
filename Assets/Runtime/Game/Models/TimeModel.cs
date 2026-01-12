using System;
using UnityEngine;

namespace Runtime.Game.Models
{
    public struct TimeModel
    {
        public int Minutes { get; }

        public int Seconds { get; }

        public readonly int TotalSeconds;

        public TimeModel(int seconds)
        {
            TotalSeconds = seconds;
            Minutes = TotalSeconds / 60;
            Seconds = TotalSeconds % 60;
        }

        public TimeModel(float seconds)
        {
            TotalSeconds = Mathf.RoundToInt(seconds);
            Minutes = TotalSeconds / 60;
            Seconds = TotalSeconds % 60;
        }


        public override string ToString()
        {
            if (Minutes > 0)
                return $"{Minutes:D2}:${Seconds:D2}";
            
            return $"{Seconds:D2}";
        }

        public override bool Equals(object obj)
        {
            return obj switch
            {
                null => false,
                float f => Mathf.Abs(TotalSeconds - f) < Mathf.Epsilon,
                int i => TotalSeconds == i,
                double d => Math.Abs(TotalSeconds - d) < Mathf.Epsilon,
                TimeModel tm => TotalSeconds == tm.TotalSeconds,
                _ => base.Equals(obj)
            };
        }

        public bool Equals(TimeModel other) => 
            TotalSeconds == other.TotalSeconds;

        public override int GetHashCode() => 
            HashCode.Combine(TotalSeconds, Minutes, Seconds);
    }
}