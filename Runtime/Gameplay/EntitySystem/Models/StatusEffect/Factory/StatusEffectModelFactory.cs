using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    public static class StatusEffectModelFactory
    {
        #region Class Methods

        public static StatusEffectModel GetStatusEffectModel(StatusEffectType statusEffectType, StatusEffectData statusEffectData)
        {
            switch (statusEffectType)
            {
                case StatusEffectType.Stun:
                    return new StunStatusEffectModel(statusEffectData);

                case StatusEffectType.KnockBack:
                    return new KnockBackStatusEffectModel(statusEffectData);

                case StatusEffectType.Bleed:
                    return new BleedStatusEffectModel(statusEffectData);

                case StatusEffectType.Heal:
                    return new HealStatusEffectModel(statusEffectData);

                case StatusEffectType.Regen:
                    return new RegenStatusEffectModel(statusEffectData);

                case StatusEffectType.Dread:
                    return new DreadStatusEffectModel(statusEffectData);

                case StatusEffectType.Haste:
                    return new HasteStatusEffectModel(statusEffectData);

                case StatusEffectType.Quick:
                    return new QuickStatusEffectModel(statusEffectData);

                case StatusEffectType.Tough:
                    return new ToughStatusEffectModel(statusEffectData);

                case StatusEffectType.Hardened:
                    return new HardenedStatusEffectModel(statusEffectData);

                case StatusEffectType.Taunt:
                    return new TauntStatusEffectModel(statusEffectData);

                case StatusEffectType.Terror:
                    return new TerrorStatusEffectModel(statusEffectData);

                case StatusEffectType.Pull:
                    return new PullStatusEffectModel(statusEffectData);

                case StatusEffectType.Root:
                    return new RootStatusEffectModel(statusEffectData);

                case StatusEffectType.Chill:
                    return new ChillStatusEffectModel(statusEffectData);

                case StatusEffectType.Freeze:
                    return new FreezeStatusEffectModel(statusEffectData);

                case StatusEffectType.TrappedPoison:
                case StatusEffectType.TrappedFire:
                    return new TrappedStatusEffectModel(statusEffectData);

                default:
                    return null;
            }
        }

        #endregion Class Methods
    }
}