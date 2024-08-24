using UnityEngine;
using Runtime.Server.Models;
using Cysharp.Threading.Tasks;

namespace Runtime.Gameplay.BaseBuilder
{
    public interface IBuildStructure
    {
        #region Properties

        Transform RootTransform { get; }
        BuildStructureModel BuildStructureModel { get; }

        #endregion Properties

        #region Interface Methods

        UniTask Initialize(BuildStructureModel buildStructureModel);
        void CheckBuildStatus(ConstructionData constructionData);

        #endregion Interface Methods
    }
}