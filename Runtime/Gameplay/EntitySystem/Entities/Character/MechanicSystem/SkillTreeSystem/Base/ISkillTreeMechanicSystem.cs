namespace Runtime.Gameplay.EntitySystem
{
    public interface ISkillTreeMechanicSystem : IMechanicSystem
    {
        void Init(SkillTreeSystemModel skillTreeSystemModel, HeroModel heroModel);
    }

}