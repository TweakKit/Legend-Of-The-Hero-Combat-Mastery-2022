namespace Runtime.Gameplay.EntitySystem
{
    public interface IMechanicSystem
    {
        void Reset(HeroModel heroModel);
        void Disable();
    }
}