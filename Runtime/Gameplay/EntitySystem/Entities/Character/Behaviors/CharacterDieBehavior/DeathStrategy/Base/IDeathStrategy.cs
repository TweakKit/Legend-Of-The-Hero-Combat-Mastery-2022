using System.Threading;
using Cysharp.Threading.Tasks;
using Runtime.ConfigModel;

namespace Runtime.Gameplay.EntitySystem
{
    public interface IDeathStrategy
    {
        public UniTask Execute(EntityModel entityModel, DeathDataConfigItem deathDataConfig, CancellationToken cancellationToken);
    }
}