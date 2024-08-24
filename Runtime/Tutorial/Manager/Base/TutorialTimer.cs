using System;
using System.Collections.Generic;
using UnityEngine;

namespace Runtime.Tutorial
{
    public class TutorialTimer : MonoBehaviour
    {
        #region Members

        private List<Timer> _timers;
        private List<int> _removalPending;
        private int _idCounter;
        private bool _useUnscaledTime;

        #endregion Members

        #region API Methods

        private void Update()
        {
            Remove();
            Tick();
        }

        #endregion API Methods

        #region Class Methods

        public void Init(bool useUnscaledTime)
        {
            _timers = new List<Timer>();
            _removalPending = new List<int>();
            _idCounter = 0;
            _useUnscaledTime = useUnscaledTime;
        }

        public int AddTimer(float rate, Action callback)
            => AddTimer(rate, 0, callback);

        public int AddTimer(float rate, int ticks, Action callback)
        {
            Timer newTimer = new Timer(++_idCounter, rate, ticks, callback);
            _timers.Add(newTimer);
            return newTimer.id;
        }

        public void RemoveTimer(int timerId)
        {
            if (!_removalPending.Contains(timerId))
                _removalPending.Add(timerId);
        }

        private void Remove()
        {
            if (_removalPending.Count > 0)
            {
                foreach (int id in _removalPending)
                {
                    for (int i = 0; i < _timers.Count; i++)
                    {
                        if (_timers[i].id == id)
                        {
                            _timers.RemoveAt(i);
                            break;
                        }
                    }
                }
                _removalPending.Clear();
            }
        }

        private void Tick()
        {
            var deltaTime = _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            for (int i = 0; i < _timers.Count; i++)
                _timers[i].Tick(deltaTime);
        }

        #endregion API Methods

        #region Owner Classes

        private class Timer
        {
            #region Members

            public int id;
            public bool isActive;
            public float rate;
            public int ticks;
            public int ticksElapsed;
            public float currentRate;
            public Action callback;

            #endregion Members

            #region Class Methods

            public Timer(int id, float rate, int ticks, Action callback)
            {
                this.id = id;
                this.rate = rate < 0 ? 0 : rate;
                this.ticks = ticks < 0 ? 0 : ticks;
                this.callback = callback;
                currentRate = 0;
                ticksElapsed = 0;
                isActive = true;
            }

            public void Tick(float deltaTime)
            {
                currentRate += deltaTime;
                if (isActive && currentRate >= rate)
                {
                    currentRate = 0;
                    ticksElapsed++;
                    callback.Invoke();
                    if (ticks > 0 && ticks == ticksElapsed)
                    {
                        isActive = false;
                        TutorialNavigator.CurrentTutorial.TutorialTimer.RemoveTimer(id);
                    }
                }
            }

            #endregion Class Methods
        }

        #endregion Owner Classes
    }
}