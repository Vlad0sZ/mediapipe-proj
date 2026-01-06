// Copyright (c) 2023 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace SensorPack.Addons.Mediapipe.Solutions.Runners
{
    public abstract class BaseRunner : MonoBehaviour
    {
        protected virtual string Tag => GetType().Name;

        protected bool IsPaused { get; private set; }

        private readonly Stopwatch _stopwatch = new();

        /// <summary>
        ///   Start the main program from the beginning.
        /// </summary>
        public virtual void Play()
        {
            IsPaused = false;
            _stopwatch.Restart();
        }


        /// <summary>
        ///     Pause the main program.
        /// </summary>
        public virtual void Pause()
        {
            IsPaused = true;
        }

        /// <summary>
        ///    Resume the main program.
        ///    If the main program has not begun, it'll do nothing.
        /// </summary>
        public virtual void Resume()
        {
            IsPaused = false;
        }

        /// <summary>
        ///   Stops the main program.
        /// </summary>
        public virtual void Stop()
        {
            IsPaused = true;
            _stopwatch.Stop();
        }

        protected long GetCurrentTimestampMillisec() =>
            _stopwatch.IsRunning ? _stopwatch.ElapsedTicks / TimeSpan.TicksPerMillisecond : -1;
    }
}