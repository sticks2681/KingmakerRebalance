﻿using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;


namespace CallOfTheWild
{
    partial class MysteryEngine
    {
        public BlueprintFeature createArmorOfBones(string name_prefix, string display_name, string description)
        {
            var mage_armor = library.Get<BlueprintAbility>("9e1ad5d6f87d19e4d8883d63a6e35568");
            var buff = Helpers.CreateBuff(name_prefix + "Buff",
                                          display_name,
                                          description,
                                          "",
                                          mage_armor.Icon,
                                          null,
                                          Helpers.CreateAddContextStatBonus(StatType.AC, ModifierDescriptor.Armor, rankType: AbilityRankType.Default, multiplier: 2),
                                          Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, progression: ContextRankProgression.StartPlusDivStep,
                                                                          classes: classes, stepLevel: 4, startLevel: -1, min: 2)
                                         );
            var buff2 = Helpers.CreateBuff(name_prefix + "2Buff",
                              display_name,
                              description,
                              "",
                              buff.Icon,
                              null,
                              Common.createContextFormDR(5, PhysicalDamageForm.Bludgeoning)
                             );
            buff2.SetBuffFlags(BuffFlags.HiddenInUi);

            var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(1, DurationRate.Hours), dispellable: false);
            var apply_buff2 = Common.createContextActionApplyBuff(buff2, Helpers.CreateContextDuration(1, DurationRate.Hours), dispellable: false);
            var resource = Helpers.CreateAbilityResource(name_prefix + "Resource", "", "", "", null);
            resource.SetIncreasedByLevel(0, 1, classes);

            var ability = Helpers.CreateAbility(name_prefix + "Ability",
                                                display_name,
                                                description,
                                                "",
                                                buff.Icon,
                                                AbilityType.Supernatural,
                                                Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Standard,
                                                AbilityRange.Personal,
                                                "One hour",
                                                "",
                                                mage_armor.GetComponent<AbilitySpawnFx>(),
                                                Helpers.CreateRunActions(Common.createRunActionsDependingOnContextValue(Helpers.CreateContextValue(AbilityRankType.StatBonus),
                                                                                                                        Helpers.CreateActionList(apply_buff),
                                                                                                                        Helpers.CreateActionList(apply_buff, apply_buff2)
                                                                                                                        )
                                                                        ),
                                                Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: classes,
                                                                                type: AbilityRankType.StatBonus, progression: ContextRankProgression.OnePlusDivStep,
                                                                                stepLevel: 13),
                                                Helpers.CreateResourceLogic(resource)
                                                );
            ability.setMiscAbilityParametersSelfOnly();
            var feature = Common.AbilityToFeature(ability);
            feature.AddComponent(Helpers.CreateAddAbilityResource(resource));

            return feature;
        }


        public BlueprintFeature createBleedingWounds(string name_prefix, string display_name, string description)
        {
            var bleed1d4 = library.Get<BlueprintBuff>("5eb68bfe186d71a438d4f85579ce40c1");
            var bleed1d6 = library.Get<BlueprintBuff>("75039846c3d85d940aa96c249b97e562");
            var bleed2d6 = library.Get<BlueprintBuff>("16249b8075ab8684ca105a78a047a5ef");
            var icon = bleed1d4.Icon;
            var buffs = new BlueprintBuff[] { bleed1d4, bleed1d6, bleed2d6 };

            var features = new BlueprintFeature[buffs.Length];

            for (int i = 0; i < features.Length; i++)
            {
                var apply_buff = Common.createContextActionApplyBuff(buffs[i], Helpers.CreateContextDuration(), is_permanent: true, is_from_spell: false);
                features[i] = Helpers.CreateFeature(name_prefix + $"{i + 1}Feature",
                                                    display_name,
                                                    description,
                                                    "",
                                                    icon,
                                                    FeatureGroup.None,
                                                    Helpers.Create<NewMechanics.ActionOnSpellDamage>(a =>
                                                    {
                                                        a.descriptor = SpellDescriptor.None;
                                                        a.use_energy = true;
                                                        a.energy = DamageEnergyType.NegativeEnergy;
                                                        a.action = Helpers.CreateActionList(apply_buff);
                                                    })
                                                    );
                features[i].HideInUI = true;
                if (i > 0)
                {
                    features[i].AddComponent(Common.createRemoveFeatureOnApply(features[i - 1]));
                }

            }

            var feature = Helpers.CreateFeature(name_prefix + "Feature",
                                    display_name,
                                    description,
                                    "",
                                    icon,
                                    FeatureGroup.None,
                                    Helpers.CreateAddFeatureOnClassLevel(features[0], 10, classes, before: true),
                                    Helpers.CreateAddFeatureOnClassLevel(features[1], 10, classes),
                                    Helpers.CreateAddFeatureOnClassLevel(features[2], 20, classes)
                                    );

            return feature;
        }


        public BlueprintFeature createDeathsTouch(string name_prefix, string display_name, string description)
        {
            var resource = Helpers.CreateAbilityResource(name_prefix + "Resource", "", "", "", null);
            resource.SetIncreasedByStat(3, stat);
            var inflict_light_wounds = library.Get<BlueprintAbility>("e5cb4c4459e437e49a4cd73fde6b9063");

            var dmg = Helpers.CreateActionDealDamage(DamageEnergyType.NegativeEnergy, Helpers.CreateContextDiceValue(DiceType.D6, 1, Helpers.CreateContextValue(AbilityRankType.Default)));
            var heal = Common.createContextActionHealTarget(Helpers.CreateContextDiceValue(DiceType.D6, 1, Helpers.CreateContextValue(AbilityRankType.Default)));

            var channel_resistance = Helpers.CreateBuff(name_prefix + "ChannelResistanceBuff",
                                                        "",
                                                        "",
                                                        "",
                                                        null,
                                                        null,
                                                        Helpers.Create<SavingThrowBonusAgainstSpecificSpells>(c =>
                                                        {
                                                            c.Spells = new BlueprintAbility[0];
                                                            c.Value = 2;
                                                            c.BypassFeatures = new BlueprintFeature[] { library.Get<BlueprintFeature>("3d8e38c9ed54931469281ab0cec506e9") }; //sun domain
                                                        }
                                                        )
                                                        );
            var apply_buff = Common.createContextActionApplyBuff(channel_resistance, Helpers.CreateContextDuration(1, DurationRate.Minutes), dispellable: false);
            var effect = Helpers.CreateConditional(Helpers.Create<UndeadMechanics.ContextConditionHasNegativeEnergyAffinity>(),
                                                   new GameAction[] { heal, apply_buff }, new GameAction[] { dmg });
            ChannelEnergyEngine.addChannelResitance(channel_resistance);

            var ability = Helpers.CreateAbility(name_prefix + "Ability",
                                                display_name,
                                                description,
                                                "",
                                                inflict_light_wounds.Icon,
                                                AbilityType.Supernatural,
                                                CommandType.Standard,
                                                AbilityRange.Touch,
                                                "",
                                                Helpers.savingThrowNone,
                                                Helpers.CreateRunActions(effect),
                                                inflict_light_wounds.GetComponent<AbilityTargetHasFact>(),
                                                inflict_light_wounds.GetComponent<AbilitySpawnFx>(),
                                                inflict_light_wounds.GetComponent<AbilityDeliverTouch>(),
                                                Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, classes: classes,
                                                                                progression: ContextRankProgression.Div2)
                                                );
            ability.setMiscAbilityParametersTouchHarmful(true);

            var feature = Common.AbilityToFeature(ability.CreateTouchSpellCast(resource), false);
            feature.AddComponent(resource.CreateAddAbilityResource());
            return feature;
        }


        public BlueprintFeature createNearDeath(string name_prefix, string display_name, string description)
        {
            var resource = Helpers.CreateAbilityResource(name_prefix + "Resource", "", "", "", null);
            resource.SetIncreasedByStat(3, stat);
            var icon = Helpers.GetIcon("d42c6d3f29e07b6409d670792d72bc82"); //banshee blast
            var feature2 = Helpers.CreateFeature(name_prefix + "2Feature",
                                                 "",
                                                 "",
                                                 "",
                                                 null,
                                                 FeatureGroup.None,
                                                 Common.createContextSavingThrowBonusAgainstDescriptor(Helpers.CreateContextValue(AbilityRankType.Default), ModifierDescriptor.Insight,
                                                                                                       SpellDescriptor.Death | SpellDescriptor.Stun | SpellDescriptor.Sleep),
                                                 Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel,
                                                                                 progression: ContextRankProgression.Custom,
                                                                                 classes: classes,
                                                                                 customProgression: new (int, int)[]{ (10, 2),
                                                                                                                      (20, 4)
                                                                                                                      }
                                                                                 )
                                                );
            feature2.HideInCharacterSheetAndLevelUp = true;
            var feature = Helpers.CreateFeature(name_prefix + "Feature",
                                                 display_name,
                                                 description,
                                                 "",
                                                 icon,
                                                 FeatureGroup.None,
                                                 Common.createContextSavingThrowBonusAgainstDescriptor(Helpers.CreateContextValue(AbilityRankType.Default), ModifierDescriptor.Insight,
                                                                                                       SpellDescriptor.Disease | SpellDescriptor.MindAffecting | SpellDescriptor.Poison),
                                                 Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel,
                                                                                 progression: ContextRankProgression.Custom,
                                                                                 classes: classes,
                                                                                 customProgression: new (int, int)[]{ (10, 2),
                                                                                                                      (20, 4)
                                                                                                                      }
                                                                                 ),
                                                 Helpers.CreateAddFeatureOnClassLevel(feature2, 7, classes)
                                                );
            return feature;
        }


        public BlueprintFeature createRaiseTheDead(string name_prefix, string display_name, string description)
        {
            var spell_ids = new string[]
            {
                "4b76d32feb089ad4499c3a1ce8e1ac27", //animate dead
                "9b75cb3bd3108a24c81329a3734f2248", //grave knight
                "43a1ea314c59c4a4eb2c193a1e17b805", //living armor
            };

            var resource = Helpers.CreateAbilityResource(name_prefix + "Resource", "", "", "", null);
            resource.SetIncreasedByLevelStartPlusDivStep(1, 10, 1, 10, 0, 0, 0, classes);

            var abilities = new BlueprintAbility[spell_ids.Length];

            for (int i = 0; i < spell_ids.Length; i++)
            {
                abilities[i] = Common.convertToSuperNatural(library.Get<BlueprintAbility>(spell_ids[i]), name_prefix, classes, stat, resource);
                Helpers.SetField(abilities[i], "m_IsFullRoundAction", false);
                abilities[i].ReplaceComponent<ContextRankConfig>(c =>
                                                                {
                                                                    Helpers.SetField(c, "m_BaseValueType", ContextRankBaseValueType.StatBonus);
                                                                    Helpers.SetField(c, "m_Stat", StatType.Charisma);
                                                                });
            }

            abilities[0].SetNameDescription(display_name, description);
            abilities[1].SetNameDescription(display_name + " (Grave Knight)", description);
            abilities[2].SetNameDescription(display_name + " (Living Armor)", description);

            var wrapper2 = Common.createVariantWrapper(name_prefix + "2Ability", "", abilities[1], abilities[2]);

            var feature1 = Common.AbilityToFeature(abilities[0]);
            var feature2 = Common.AbilityToFeature(wrapper2);

            var feature = Helpers.CreateFeature(name_prefix + "Feature",
                                                display_name,
                                                description,
                                                "",
                                                abilities[0].Icon,
                                                FeatureGroup.None,
                                                Helpers.CreateAddFeatureOnClassLevel(feature1, 15, classes, before: true),
                                                Helpers.CreateAddFeatureOnClassLevel(feature2, 15, classes),
                                                resource.CreateAddAbilityResource()
                                                );
            return feature;
        }


        public BlueprintFeature createResistLife(string name_prefix, string display_name, string description)
        {
            var icon = library.Get<BlueprintFeature>("b0acce833384b9b428f32517163c9117").Icon; //deaths_embrace
            var feature = Helpers.CreateFeature(name_prefix + "Feature",
                                                display_name,
                                                description,
                                                "",
                                                icon,
                                                FeatureGroup.None,
                                                Helpers.Create<UndeadMechanics.ConsiderUndeadForHealing>(),
                                                Helpers.Create<AddEnergyImmunity>(a => a.Type = DamageEnergyType.NegativeEnergy),
                                                Helpers.Create<NewMechanics.ContextSavingThrowBonusAgainstSpecificSpells>(c =>
                                                {
                                                    c.Spells = new BlueprintAbility[0];
                                                    c.Value = Helpers.CreateContextValue(AbilityRankType.Default);
                                                    c.BypassFeatures = new BlueprintFeature[] { library.Get<BlueprintFeature>("3d8e38c9ed54931469281ab0cec506e9") }; //sun domain
                                                }
                                                                                                                          ),
                                                Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel,
                                                                                classes: classes,
                                                                                progression: ContextRankProgression.Custom,
                                                                                customProgression: new (int, int)[]
                                                                                                                {
                                                                                                                    (6, 0),
                                                                                                                     (10, 2),
                                                                                                                     (20, 4)
                                                                                                                }
                                                                                )
                                          );
            ChannelEnergyEngine.addChannelResitance(feature);
            return feature;
        }


        public BlueprintFeature createSoulSiphon(string name_prefix, string display_name, string description)
        {
            var resource = Helpers.CreateAbilityResource(name_prefix + "Resource", "", "", "", null);
            resource.SetIncreasedByLevelStartPlusDivStep(1, 11, 1, 4, 1, 0, 0, classes);

            var enervation = library.Get<BlueprintAbility>("f34fb78eaaec141469079af124bcfa0f");

            var drain = Helpers.CreateActionEnergyDrain(Helpers.CreateContextDiceValue(DiceType.Zero, 0, 1),
                                                        Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.StatBonus), DurationRate.Minutes),
                                                        Kingmaker.RuleSystem.Rules.EnergyDrainType.Temporary);
            var heal = Common.createContextActionHealTarget(Helpers.CreateContextDiceValue(DiceType.Zero, 0, Helpers.CreateContextValue(AbilityRankType.Default)));
            var heal_caster = Common.createContextActionOnContextCaster(heal);
            var ability = Helpers.CreateAbility(name_prefix + "Ability",
                                                display_name,
                                                description,
                                                "",
                                                enervation.Icon,
                                                AbilityType.Supernatural,
                                                CommandType.Standard,
                                                AbilityRange.Close,
                                                $"{stat.ToString()} modifier minutes",
                                                Helpers.savingThrowNone,
                                                enervation.GetComponent<AbilityDeliverProjectile>(),
                                                Common.createAbilityTargetHasFact(true, Common.undead, Common.elemental, Common.construct),
                                                Helpers.CreateRunActions(drain, heal_caster),
                                                resource.CreateResourceLogic(),
                                                Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel,
                                                                                classes: classes),
                                                Helpers.CreateContextRankConfig(type: AbilityRankType.StatBonus, baseValueType: ContextRankBaseValueType.StatBonus,
                                                                                stat: stat)
                                                );
            ability.setMiscAbilityParametersSingleTargetRangedHarmful(true);

            var feature = Common.AbilityToFeature(ability, false);
            feature.AddComponent(resource.CreateAddAbilityResource());

            foreach (var c in classes)
            {
                feature.AddComponents(Helpers.PrerequisiteClassLevel(c, 7, any: true));
            }
            return feature;
        }


        public BlueprintFeature createUndeadServitude(string name_prefix, string display_name, string description)
        {
            var resource = Helpers.CreateAbilityResource(name_prefix + "Resource", "", "", "", null);
            resource.SetIncreasedByStat(3, stat);

            var ability = Common.convertToSuperNatural(NewSpells.control_undead, name_prefix, classes, stat, resource);
            ability.SetName(display_name);

            var feature = Common.AbilityToFeature(ability, false);
            feature.AddComponent(resource.CreateAddAbilityResource());
            feature.SetDescription(description);

            return feature;
        }
    }
}
