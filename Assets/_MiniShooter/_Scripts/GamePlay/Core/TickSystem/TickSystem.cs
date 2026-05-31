using System;
using System.Collections.Generic;
using System.Diagnostics;
using Core.DIServiceLocator;
using TriInspector;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Core.TicksSystem
{
    public sealed class TickSystem : MonoBehaviour, IService
    {
        [SerializeField] private int _ticksPerSecond = 60;

        private readonly SortedDictionary<TickPhase, List<ITickable>> _tickables = new();
        private readonly Queue<ITickable> _registerQueue = new();
        private readonly Queue<ITickable> _unregisterQueue = new();

        private float _tickInterval;
        private float _accumulator;
        private bool _isConstruct;
        private bool _isTicksStarted;
        private int _tickCounter;
        private int _ticksCount;
        private float _tickTimer;
        private Stopwatch _stopwatch = new();

        public int TargetTickRate => _ticksPerSecond;
        public int RealTickRate { get; private set; }
        public float TickExecutionTimeMs { get; private set; }
        public int Tick => _ticksCount;

        public void Construct(int tick = 0)
        {
            ServiceLocator.Register(this);

            SetTickSettings();

            foreach (TickPhase phase in Enum.GetValues(typeof(TickPhase)))
            {
                _tickables[phase] = new List<ITickable>();
            }

            _ticksCount = tick;
            _isConstruct = true;
        }

        public void StartTicks()
        {
            if (!_isConstruct)
                return;

            _isTicksStarted = true;
            Debug.Log("[TickSystem] Ticks started");
        }

        public void StopTicks()
        {
            _isTicksStarted = false;
            Debug.Log("[TickSystem] Ticks stopped");
        }

        public void Register(ITickable tickable) => _registerQueue.Enqueue(tickable);
        public void Unregister(ITickable tickable) => _unregisterQueue.Enqueue(tickable);

        private void Update()
        {
            if (!_isConstruct)
                return;

            _tickTimer += Time.unscaledDeltaTime;
            if (_tickTimer >= 1f)
            {
                RealTickRate = _tickCounter;
                _tickCounter = 0;
                _tickTimer -= 1f;
            }

            _accumulator += Time.deltaTime;
            while (_accumulator >= _tickInterval)
            {
                _stopwatch.Restart();

                OnTick(_tickInterval);
                _ticksCount++;
                _tickCounter++;

                _stopwatch.Stop();
                TickExecutionTimeMs = (float)_stopwatch.Elapsed.TotalMilliseconds;

                _accumulator -= _tickInterval;
            }
        }

        private void OnTick(float deltaTime)
        {
            foreach (var tickables in _tickables.Values)
            {
                foreach (var tickable in tickables)
                {
                    if (tickable.UpdatePhase is TickPhase.InputPhase)
                    {
                        tickable.Tick(deltaTime);
                        continue;
                    }

                    if (_isTicksStarted)
                    {
                        tickable.Tick(deltaTime);
                    }
                }
            }

            ReleaseRegisterQueue();
            ReleaseUnregisterQueue();
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister(this);
        }

        private void ReleaseRegisterQueue()
        {
            while (_registerQueue.Count > 0)
            {
                var newTickable = _registerQueue.Dequeue();
                var phase = newTickable.UpdatePhase;

                if (_tickables[phase].Contains(newTickable))
                    continue;

                _tickables[phase].Add(newTickable);
                Debug.Log($"[TickSystem] Registered tickable {newTickable.GetType().Name}");
            }
        }

        private void ReleaseUnregisterQueue()
        {
            while (_unregisterQueue.Count > 0)
            {
                var tickable = _unregisterQueue.Dequeue();
                var phase = tickable.UpdatePhase;

                if (!_tickables[phase].Contains(tickable))
                    continue;

                _tickables[phase].Remove(tickable);
                Debug.Log($"[TickSystem] Unregistered tickable {tickable.GetType().Name}");
            }
        }

        [Button(ButtonSizes.Medium, "Recalculate ticks")]
        private void SetTickSettings()
        {
            _tickInterval = 1f / _ticksPerSecond;
            _accumulator = 0f;
        }
    }
}