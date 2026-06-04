using Configs;
using NGO.Client;
using NGO.Server;
using Simulation.Player;
using UnityEngine;
using Visual.Player;

namespace Binders
{
    public sealed class PlayerMovementBinder : MonoBehaviour
    {
        [SerializeField] private PlayerMovementServer _movementServer;
        [SerializeField] private LocalPlayerMovementPrediction _movementPrediction;
        [SerializeField] private RemotePlayerMovement _remotePlayerMovement;
        [SerializeField] private Transform _orientationTransform; 
        [SerializeField] private float _groundCheckRadius;
        [SerializeField] private float _groundCheckMaxDistance;
        [SerializeField] private float _height;
        [SerializeField] private float _radius;
        [SerializeField] private PlayerMovementConfig _config;
        [SerializeField] private LayerMask _groundLayerMask;
        [SerializeField] private LayerMask _obstacleLayerMask;
        [SerializeField] private PlayerCameraController _cameraController;

        public void Bind(bool isServer, bool isOwner)
        {
            var movementSimulation = new PlayerMovementSimulation(_config, _groundLayerMask, _obstacleLayerMask, _groundCheckRadius, _groundCheckMaxDistance, _height, _radius);
            _movementServer.SetClient(_movementPrediction, _remotePlayerMovement);
            
            if (isServer)
            {
                _movementServer.Construct(movementSimulation, _orientationTransform);
                _movementPrediction.SetServer(_movementServer);
            }
            
            if (isOwner)
            {
                _movementPrediction.Construct(movementSimulation, _orientationTransform);
                _cameraController.Construct(movementSimulation);
            }
            else if (!isServer)
            {
                _remotePlayerMovement.Construct();
            }
        }
    }
}