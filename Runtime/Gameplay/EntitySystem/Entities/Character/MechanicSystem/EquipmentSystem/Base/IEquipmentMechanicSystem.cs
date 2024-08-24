namespace Runtime.Gameplay.EntitySystem
{
    public interface IEquipmentMechanicSystem : IMechanicSystem
    {
        void Init(EquipmentSystemModel equipmentSystemModel, HeroModel heroModel);
    }
}