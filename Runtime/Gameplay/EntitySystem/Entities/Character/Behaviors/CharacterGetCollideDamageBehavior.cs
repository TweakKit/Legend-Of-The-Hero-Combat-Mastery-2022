using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Runtime.Gameplay.CollisionDetection;

namespace Runtime.Gameplay.EntitySystem
{
    /// <summary>
    /// This behavior makes the character get damaged when being collided with others.<br/>
    /// </summary>
    public class CharacterGetCollideDamageBehavior : CharacterCollideBehavior, IUpdatable
    {
        #region Members

        private List<CollisionNewData> _collisionsData;
        private List<CollisionNewData> _removalCollisionsData;

        #endregion Members

        #region Class Methods

        public override bool InitModel(EntityModel model, Transform transform)
        {
            _collisionsData = new();
            _removalCollisionsData = new();
            return base.InitModel(model, transform);
        }

        public override void OnCollision(CollisionResult result, ICollisionBody other)
        {
            base.OnCollision(result, other);
            if (other.Collider != null)
            {
                if (result.collisionType == CollisionType.Enter)
                {
                    var entity = other.Collider.GetComponent<IEntity>();
                    if (entity != null)
                    {
                        var interactable = entity.GetBehavior<IInteractable>();
                        if (interactable != null && interactable.Model is CharacterModel)
                        {
                            if (!_collisionsData.Any(x => x.CollidingRefId == other.RefId))
                                _collisionsData.Add(new CollisionNewData(other.RefId, ownerTransform, interactable.Model as CharacterModel));
                        }
                    }
                }
                else if (result.collisionType == CollisionType.Exit)
                {
                    var collisionData = _collisionsData.FirstOrDefault(x => x.CollidingRefId == other.RefId);
                    if (collisionData != null)
                    {
                        if (_collisionsData.Contains(collisionData))
                        {
                            if (!_removalCollisionsData.Contains(collisionData))
                                _removalCollisionsData.Add(collisionData);
                        }
                    }
                }
            }
        }

        public void Update()
        {
            if (_removalCollisionsData.Count != 0)
            {
                foreach (var collisionData in _removalCollisionsData)
                {
                    if (_collisionsData.Contains(collisionData))
                        _collisionsData.Remove(collisionData);
                }
                _removalCollisionsData.Clear();
            }

            foreach (var collisionsData in _collisionsData)
                collisionsData.Update(Time.deltaTime);
        }

        #endregion Class Methods

        #region Owner Classes

        public sealed class CollisionNewData
        {
            #region Members

            private float _currentCountTime;
            private Transform _targetCollidedTransform;
            private CharacterModel _collidingModel;
            private int _collidingRefId;
            private float _recollideDelay;

            #endregion Members

            #region Properties

            public int CollidingRefId => _collidingRefId;

            #endregion Properties

            #region Class Methods

            public CollisionNewData(int refId, Transform targetCollidedTransform, CharacterModel collidingModel)
            {
                _recollideDelay = 1.0f;
                _collidingRefId = refId;
                _targetCollidedTransform = targetCollidedTransform;
                _collidingModel = collidingModel;
                _currentCountTime = _recollideDelay;
            }

            public void Update(float deltaTime)
            {
                if (_currentCountTime >= _recollideDelay)
                {
                    _currentCountTime = 0;
                    var entity = _targetCollidedTransform.GetComponent<IEntity>();
                    if (entity != null)
                    {
                        var interactable = entity.GetBehavior<IInteractable>();
                        if (interactable != null)
                        {
                            if (_collidingModel.EntityType.CanCauseDamage(interactable.Model.EntityType) && _collidingModel.CanCauseCollideDamage)
                            {
                                var damageDirection = (interactable.Model.Position - _collidingModel.Position).normalized;
                                interactable.GetHit(GetDamageInfo(interactable.Model), new DamageMetaData(damageDirection, _collidingModel.Position));
                            }
                        }
                    }
                }
                _currentCountTime += deltaTime;
            }

            private DamageInfo GetDamageInfo(EntityModel targetModel)
                   => new DamageInfo(DamageSource.FromCollide, _collidingModel.CollideDamage, _collidingModel, targetModel);

            #endregion Class Methods
        }

        #endregion Owner Classes
    }
}