using System;

using ACE.Database.Models.Shard;
using ACE.Entity.Enum;

namespace ACE.Server.WorldObjects.Entity
{
    public class CreatureSkill
    {
        private readonly Creature creature;
        // This is the underlying database record
        public readonly BiotaPropertiesSkill BiotaPropertiesSkill;

        public readonly Skill Skill;

        public CreatureSkill(Creature creature, BiotaPropertiesSkill biotaPropertiesSkill)
        {
            this.creature = creature;
            this.BiotaPropertiesSkill = biotaPropertiesSkill;

            Skill = (Skill)biotaPropertiesSkill.Type;
        }

        public SkillAdvancementClass AdvancementClass
        {
            get => (SkillAdvancementClass)BiotaPropertiesSkill.SAC;
            set
            {
                if (BiotaPropertiesSkill.SAC != (uint)value)
                    creature.ChangesDetected = true;

                BiotaPropertiesSkill.SAC = (uint)value;
            }
        }

        /// <summary>
        /// Total experience for this skill,
        /// both spent and earned
        /// </summary>
        public uint ExperienceSpent
        {
            get => BiotaPropertiesSkill.PP;
            set
            {
                if (BiotaPropertiesSkill.PP != value)
                    creature.ChangesDetected = true;

                BiotaPropertiesSkill.PP = value;
            }
        }

        /// <summary>
        /// Total skill level due to
        /// directly raising the skill
        /// </summary>
        public ushort Ranks
        {
            get => BiotaPropertiesSkill.LevelFromPP;
            set
            {
                if (BiotaPropertiesSkill.LevelFromPP != value)
                    creature.ChangesDetected = true;

                BiotaPropertiesSkill.LevelFromPP = value;
            }
        }

        public uint Base
        {
            get
            {
                var formula = Skill.GetFormula();

                uint total = 0;

                if (formula != null)
                {
                    if ((AdvancementClass == SkillAdvancementClass.Untrained && Skill.GetUsability() != null && Skill.GetUsability().UsableUntrained) || AdvancementClass == SkillAdvancementClass.Trained || AdvancementClass == SkillAdvancementClass.Specialized)
                        total = formula.CalcBase(creature.Strength.Base, creature.Endurance.Base, creature.Coordination.Base, creature.Quickness.Base, creature.Focus.Base, creature.Self.Base);
                }

                total += InitLevel + Ranks;

                // TODO: augs

                return total;
            }
        }

        public uint Current
        {
            get
            {
                var formula = Skill.GetFormula();

                uint total = 0;

                if (formula != null)
                {
                    if ((AdvancementClass == SkillAdvancementClass.Untrained && Skill.GetUsability() != null && Skill.GetUsability().UsableUntrained) || AdvancementClass == SkillAdvancementClass.Trained || AdvancementClass == SkillAdvancementClass.Specialized)
                        total = formula.CalcBase(creature.Strength.Current, creature.Endurance.Current, creature.Coordination.Current, creature.Quickness.Current, creature.Focus.Current, creature.Self.Current);
                }

                total += InitLevel + Ranks;

                var skillMod = creature.EnchantmentManager.GetSkillMod(Skill);
                total += (uint)skillMod;    // can be negative?

                // TODO: include augs + any other modifiers
                if (creature is Player)
                {
                    var player = creature as Player;

                    if (player.HasVitae)
                        total = (uint)Math.Round(total * player.Vitae);
                }

                return total;
            }
        }

        public double GetPercentSuccess(uint difficulty)
        {
            return GetPercentSuccess(Current, difficulty);
        }

        public static double GetPercentSuccess(uint skillLevel, uint difficulty)
        {
            float delta = skillLevel - difficulty;
            var scalar = 1d + Math.Pow(Math.E, 0.03 * delta);
            var percentSuccess = 1d - (1d / scalar);
            return percentSuccess;
        }

        /// <summary>
        /// A bonus from character creation: +5 for trained, +10 for specialized
        /// </summary>
        public uint InitLevel
        {
            get => BiotaPropertiesSkill.InitLevel;
            set => BiotaPropertiesSkill.InitLevel = value;
        }
    }
}
