namespace Runtime.Gameplay.EntitySystem
{
    public enum CharacterAnimationState
    {
        None = 0,
        Idle = 1,
        Run = 2,
        GetHurt = 3,
        FirstAttack = 4,
        UseFirstSkill = 5,
        Die = 6,
        PrecastFirstSkill = 7,
        BackSwingFirstSkill = 8,
        UseSecondSkill = 9,
        PrecastSecondSkill = 10,
        BackSwingSecondSkill = 11,
        UseThirdSkill = 12,
        PrecastThirdSkill = 13,
        BackSwingThirdSkill = 14,
        SecondAttack = 15,
    }
}