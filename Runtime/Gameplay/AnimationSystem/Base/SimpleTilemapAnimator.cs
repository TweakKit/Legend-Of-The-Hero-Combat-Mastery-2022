using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Runtime.Animation
{
    [RequireComponent(typeof(Tilemap))]
    public class SimpleTilemapAnimator : MonoBehaviour
    {
        #region Members

        [SerializeField]
        private TilemapAnimation _tilemapAnimation;

        private Tilemap _tileMap;
        private float _timePerFrame;
        private bool _isPlaying;

        private float _frameCountTime;
        private int _currentFrameIndex;
        private int _currentFrameIndexForCurrentTile;
        private List<Vector3Int> _tilePositions;

        #endregion Members

        #region Properties

        public int CurrentFrameIndex => _currentFrameIndex;
        public Action StopAction { get; set; }
        public TilemapAnimation TileMapAnimation => _tilemapAnimation;

        #endregion Properties

        #region API Methods

        private void Start()
        {
            _tileMap = GetComponent<Tilemap>();
            _timePerFrame = (float)1 / _tilemapAnimation.fps;

            var bounds = _tileMap.cellBounds.allPositionsWithin;
            _tilePositions = new();
            foreach (var pos in bounds)
            {
                Vector3Int localPlace = new Vector3Int(pos.x, pos.y, pos.z);
                var trapTile = _tileMap.GetTile<TrapWithIntervalTile>(localPlace);
                if (trapTile != null)
                {
                    _tilePositions.Add(localPlace);
                }
            }

            UpdateTiles(_tilemapAnimation.animations[0].tile);
        }

        private void Update()
        {
            if (!_isPlaying)
            {
                return;
            }

            if(_frameCountTime >= _timePerFrame)
            {
                if (_currentFrameIndexForCurrentTile >= _tilemapAnimation.animations[_currentFrameIndex].numberOfFrame - 1)
                {
                    if (_currentFrameIndex >= _tilemapAnimation.animations.Length - 1)
                    {
                        _isPlaying = false;
                        _currentFrameIndex = 0;
                        StopAction?.Invoke();
                    }
                    else
                    {
                        _currentFrameIndex++;
                        UpdateTiles(_tilemapAnimation.animations[_currentFrameIndex].tile);
                    }

                    _currentFrameIndexForCurrentTile = 0;
                }
                else
                {
                    _currentFrameIndexForCurrentTile++;
                }

                _frameCountTime = 0;
            }

            _frameCountTime += Time.deltaTime;
        }

        #endregion API Methods

        #region Class Methods

        public void Play()
        {
            _currentFrameIndex = 0;
            _currentFrameIndexForCurrentTile = 0;
            _frameCountTime = 0;
            if(_tilemapAnimation.animations.Length > _currentFrameIndex)
            {
                _isPlaying = true;
                UpdateTiles(_tilemapAnimation.animations[_currentFrameIndex].tile);
            }
        }

        public void UpdateTiles(TrapWithIntervalTile tile)
        {
            foreach (var tilePosition in _tilePositions)
            {
                _tileMap.SetTile(tilePosition, tile);
            }
        }

        #endregion Class Methods
    }
}
