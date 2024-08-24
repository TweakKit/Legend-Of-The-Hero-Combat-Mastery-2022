using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    #region Class Methods

    public static class StatusEffectFactory
    {
        public static IStatusEffect GetStatusEffect(StatusEffectType statusEffectType)
        {
            switch (statusEffectType)
            {
                case StatusEffectType.Stun:
                    return new StunStatusEffect();

                case StatusEffectType.KnockBack:
                    return new KnockBackStatusEffect();

                case StatusEffectType.Bleed:
                    return new BleedStatusEffect();

                case StatusEffectType.Heal:
                    return new HealStatusEffect();

                case StatusEffectType.Regen:
                    return new RegenStatusEffect();

                case StatusEffectType.Dread:
                    return new DreadStatusEffect();

                case StatusEffectType.Haste:
                    return new HasteStatusEffect();

                case StatusEffectType.Quick:
                    return new QuickStatusEffect();

                case StatusEffectType.Tough:
                    return new ToughStatusEffect();

                case StatusEffectType.Hardened:
                    return new HardenedStatusEffect();

                case StatusEffectType.Taunt:
                    return new TauntStatusEffect();

                case StatusEffectType.Terror:
                    return new TerrorStatusEffect();

                case StatusEffectType.Pull:
                    return new PullStatusEffect();

                case StatusEffectType.Root:
                    return new RootStatusEffect();

                case StatusEffectType.Chill:
                    return new ChillStatusEffect();

                case StatusEffectType.Freeze:
                    return new FreezeStatusEffect();

                case StatusEffectType.TrappedPoison:
                case StatusEffectType.TrappedFire:
                    return new TrappedStatusEffect();

                case StatusEffectType.DamageReductionDebuff:
                    return new DamageReductionDebuffStatusEffect();

                default:
                    return null;
            }
        }

        #endregion Class Methods
    }
}