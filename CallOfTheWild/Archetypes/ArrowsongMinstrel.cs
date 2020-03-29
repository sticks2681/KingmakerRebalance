using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Shields;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.Localization;
using Kingmaker.ResourceLinks;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using static Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityResourceLogic;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;

namespace CallOfTheWild.Archetypes
{
    public class ArrowsongMinstrel
    {
        static public BlueprintArchetype archetype;
        static public BlueprintSpellbook spellbook;

        static public BlueprintFeature all_around_vision;
        static public BlueprintFeature awareness;
        static public BlueprintFeature armathor_cantrips;
        static public BlueprintFeatureSelection fighter_feat;
        static public BlueprintFeature perfect_memory;
        static public BlueprintFeature jack_of_all_trades;
        static public BlueprintFeature fast_movement;
        static public BlueprintFeature uncanny_dodge;
        static public BlueprintFeature improved_uncanny_dodge;
        static public BlueprintFeature stern_gaze;
        static public BlueprintFeature leaders_words;
        static public BlueprintFeature stalwart;
		
		static LibraryScriptableObject library => Main.library;


        internal static void create()
        {
            var paladin_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("bfa11238e7ae3544bbeb4d0b92e897ec"); //Changed to Paladin class
            
			createSpellbook();
			
            all_around_vision = library.CopyAndAdd<BlueprintFeature>("3248db42de0649040a9f9c1ae035641c", "ArmathorAllAroundVision", "");
            all_around_vision.SetNameDescription("All-Around Visoin", "Can see in all directions at once and cannot be flanked.");
            
			awareness = library.CopyAndAdd<BlueprintFeature>("236ec7f226d3d784884f066aa4be1570", "ArmathorAwareness", "");
            awareness.SetNameDescription("Preternatural Senses", "Senses so refined that they make invisibility and concealment (even magical darkness) irrelevant.");
            
			fighter_feat = library.CopyAndAdd<BlueprintFeatureSelection>("41c8486641f7d6d4283ca9dae4147a9f", "ArmathorBonusFeat", "");
            fighter_feat.SetDescription("At 1st level and every even level thereafter, an Armathor gains a bonus feat in addition to those gained from normal advancement. These bonus feats must be selected from those listed as combat feats.");
            
			armathor_cantrips = library.CopyAndAdd<BlueprintFeature>("c58b36ec3f759c84089c67611d1bcc21", "ArmathorCantrips", "");
            armathor_cantrips.SetNameDescription("Armathor Orisons",
                                                 "An Armathor can cast a number of orisons, or 0-level spells. These are cast like any other spell, but they are not expended when cast and may be used again.");
            armathor_cantrips.ReplaceComponent<LearnSpells>(l => l.CharacterClass = paladin_class);
            armathor_cantrips.ReplaceComponent<BindAbilitiesToClass>(b => { b.CharacterClass = paladin_class; b.Stat = StatType.Wisdom; });
            
			perfect_memory = library.CopyAndAdd<BlueprintFeature>("65cff8410a336654486c98fd3bacd8c5", "ArmathorPerfectMemory", "");
            perfect_memory.SetNameDescription("Perfect Memory", "The ability to perfectly recall anything seen or read allows the Armathor to add half his class level (minimum 1) to all Knowledge and Lore skill checks and may make all Knowledge and Lore skill checks untrained.");
            perfect_memory.ReplaceComponent<ContextRankConfig>(c => Helpers.SetField(c, "m_Class", getArmathorArray()));
            
			jack_of_all_trades = library.CopyAndAdd<BlueprintFeature>("21fbafd5dc42d4d488c4d6caed46bc99", "ArmathorJackofallTrades", "");
            jack_of_all_trades.SetNameDescription("Jack of all Trades", "The Armathor gains a +1 bonus on all skill checks.");
            
			fast_movement = library.Get<BlueprintFeature>("d294a5dddd0120046aae7d4eb6cbc4fc");
            fast_movement.SetDescription("A character's land speed is faster than the norm for her race by +10 feet. This benefit applies only when they are wearing no armor, light armor, or medium armor, and not carrying a heavy load. Apply this bonus before modifying the character's speed because of any load carried or armor worn. This bonus stacks with any other bonuses to the character's land speed.");
			
            uncanny_dodge = library.Get<BlueprintFeature>("3c08d842e802c3e4eb19d15496145709");
            improved_uncanny_dodge = library.Get<BlueprintFeature>("485a18c05792521459c7d06c63128c79");

            var evasion = library.Get<BlueprintFeature>("576933720c440aa4d8d42b0c54b77e80");
            var improved_evasion = library.Get<BlueprintFeature>("ce96af454a6137d47b9c6a1e02e66803");
            
			stern_gaze = library.CopyAndAdd<BlueprintFeature>("a6d917fd5c9bee0449bd01c92e3b0308", "ArmathorSternGaze", "");
            stern_gaze.SetNameDescription("Stern Gaze", "An Armathor skilled at sensing deception and intimidating their foes. An Armathor receives a morale bonus on all Persuasion skill checks made for intimidation and Perception checks equal to 1/2 their Armathor level (minimum +1).");
            stern_gaze.ReplaceComponent<ContextRankConfig>(c => Helpers.SetField(c, "m_Class", getArmathorArray()));
            
			leaders_words = library.CopyAndAdd<BlueprintFeature>("a6d917fd5c9bee0449bd01c92e3b0308", "ArmathorLeadersWords", "");
            leaders_words.SetNameDescription("Leader's Words", "An Armathor is skilled at speaking soothing words that keep the peace and bolster allies' resolve. An Armathor receives a morale bonus on all Persuasion skill checks (when used for diplomacy) equal to half their Armathor level (minimum +1).");
            leaders_words.ReplaceComponent<ContextRankConfig>(c => Helpers.SetField(c, "m_Class", getArmathorArray()));
			
            stalwart = library.CopyAndAdd<BlueprintFeature>("ec9dbc9a5fa26e446a54fe5df6779088", "ArmathorStalwart", "");

            var ac_bonus = library.CopyAndAdd<BlueprintFeature>("e241bdfd6333b9843a7bfd674d607ac4", "ACBonusArmathorACBonusFeature", "");
            ac_bonus.SetDescription("When unarmored and unencumbered, an Armathor adds their Wisdom bonus (if any) to their AC and CMD. In addition, an armathor gains a +1 bonus to AC and CMD at 4th level. This bonus increases by 1 for every four Armathor levels thereafter, up to a maximum of +5 at 20th level.");
            foreach (var c in ac_bonus.GetComponents<ContextRankConfig>().ToArray())
            {
                if (c.IsBasedOnClassLevel)
                {
                    var new_c = c.CreateCopy();
                    Helpers.SetField(new_c, "m_Class", getArmathorArray());
                    ac_bonus.ReplaceComponent(c, new_c);
                    break;
                }
            }

            var unlock_ac_bonus = Common.createMonkFeatureUnlock(ac_bonus, false);
			
            archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "ArrowsongMinstrelArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Armathor");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Arrowsong minstrels combine the elven traditions of archery, song, and spellcasting into a seamless harmony of dazzling magical effects.");
            });
            Helpers.SetField(archetype, "m_ParentClass", paladin_class);
            library.AddAsset(archetype, "");
            archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, library.Get<BlueprintFeature>("f8c91c0135d5fc3458fcc131c4b77e96")), //alignment restriction
                                                          Helpers.LevelEntry(11, library.Get<BlueprintFeature>("9f13fdd044ccb8a439f27417481cb00e")), //mark of justice
                                                       };
            archetype.ReplaceSpellbook = spellbook;
            archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, armathor_cantrips, fighter_feat, all_around_vision, awareness, perfect_memory, jack_of_all_trades, fast_movement, uncanny_dodge, stern_gaze, leaders_words, unlock_ac_bonus, library.Get<BlueprintFeature>("54ee847996c25cd4ba8773d7b8555174")),
                                                       Helpers.LevelEntry(2, fighter_feat),
                                                       Helpers.LevelEntry(4, fighter_feat),
                                                       Helpers.LevelEntry(5, evasion, improved_uncanny_dodge, stalwart),
                                                       Helpers.LevelEntry(6, fighter_feat),
                                                       Helpers.LevelEntry(8, fighter_feat),
                                                       Helpers.LevelEntry(10, fighter_feat, improved_evasion),
                                                       Helpers.LevelEntry(11, library.Get<BlueprintFeature>("d19ef94d056fd1a45bb08017b10a5e93")),
                                                       Helpers.LevelEntry(12, fighter_feat),
                                                       Helpers.LevelEntry(14, fighter_feat),
                                                       Helpers.LevelEntry(16, fighter_feat),
                                                       Helpers.LevelEntry(18, fighter_feat),
                                                       Helpers.LevelEntry(20, fighter_feat)};

            /*paladin_class.Progression.UIGroups[5].Features.Add(fighter_feat);
            paladin_class.Progression.UIGroups[5].Features.Add(fighter_feat);
            paladin_class.Progression.UIGroups[5].Features.Add(fighter_feat);
            paladin_class.Progression.UIGroups[5].Features.Add(fighter_feat);
            paladin_class.Progression.UIGroups[5].Features.Add(fighter_feat);
            paladin_class.Progression.UIGroups[5].Features.Add(fighter_feat);
            paladin_class.Progression.UIGroups[5].Features.Add(fighter_feat);
            paladin_class.Progression.UIGroups[5].Features.Add(fighter_feat);
            paladin_class.Progression.UIGroups[5].Features.Add(fighter_feat);
            paladin_class.Progression.UIGroups[5].Features.Add(fighter_feat);
            paladin_class.Progression.UIGroups[5].Features.Add(fighter_feat);
			
            paladin_class.Progression.UIGroups[6].Features.Add(stern_gaze);
            paladin_class.Progression.UIGroups[6].Features.Add(evasion);
            paladin_class.Progression.UIGroups[6].Features.Add(improved_evasion);
			
            paladin_class.Progression.UIGroups[7].Features.Add(leaders_words);
			
            paladin_class.Progression.UIGroups[8].Features.Add(uncanny_dodge);
            paladin_class.Progression.UIGroups[8].Features.Add(improved_uncanny_dodge);*/

            paladin_class.Progression.UIDeterminatorsGroup = paladin_class.Progression.UIDeterminatorsGroup.AddToArray(armathor_cantrips, all_around_vision, awareness, perfect_memory, jack_of_all_trades, fast_movement);
            paladin_class.Progression.UIGroups = paladin_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(fighter_feat, uncanny_dodge, stern_gaze, leaders_words, evasion, improved_uncanny_dodge, improved_evasion, stalwart, unlock_ac_bonus));
            paladin_class.Archetypes = paladin_class.Archetypes.AddToArray(archetype);

            archetype.ReplaceClassSkills = true;
            archetype.ClassSkills = paladin_class.ClassSkills.AddToArray(StatType.SkillAthletics, StatType.SkillMobility, StatType.SkillThievery, StatType.SkillStealth, StatType.SkillKnowledgeArcana, StatType.SkillKnowledgeWorld, StatType.SkillLoreNature, StatType.SkillLoreReligion, StatType.SkillPerception, StatType.SkillPersuasion, StatType.SkillUseMagicDevice);
        }


        static BlueprintCharacterClass[] getArmathorArray()
        {
            var paladin_class = library.Get<BlueprintCharacterClass>("bfa11238e7ae3544bbeb4d0b92e897ec");
            return new BlueprintCharacterClass[] { paladin_class };
        }


        static void createSpellbook()
        {
            var cleric_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("67819271767a9dd4fbfd4ae700befea0");
            var sorcerer_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("b3a505fb61437dc4097f43c3f8f9a4cf");
            var wizard_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("ba34257984f4c41408ce1dc2004e342e");
            var magus_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("45a4607686d96a1498891b3286121780");
            var paladin_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("bfa11238e7ae3544bbeb4d0b92e897ec");
            var rogue_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("299aa766dee3cbf4790da4efb8c72484");
            var bard_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("772c83a25e2268e448e841dcd548235f");
            var ranger_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("cda0615668a6df14eb36ba19ee881af6");

            spellbook = Helpers.Create<BlueprintSpellbook>();
            spellbook.name = "ArmathorSpellbook";
            library.AddAsset(spellbook, "");
            spellbook.Name = Helpers.CreateString("ArmathorSpellbook.Name", "Armathor");
            spellbook.SpellsKnown = bard_class.Spellbook.SpellsKnown;
            spellbook.Spontaneous = true;
            spellbook.IsArcane = false;
            spellbook.AllSpellsKnown = true;
            spellbook.CanCopyScrolls = false;
            spellbook.CastingAttribute = StatType.Wisdom;
            spellbook.CharacterClass = bard_class;
            spellbook.CasterLevelModifier = 0;
            spellbook.CantripsType = bard_class.Spellbook.CantripsType;
            spellbook.SpellsPerDay =  Common.createSpellsTable("ArmathorSpellsPerDay", "",
                                                                       Common.createSpellsLevelEntry(),  //0
                                                                       Common.createSpellsLevelEntry(0, 2),  //1
                                                                       Common.createSpellsLevelEntry(0, 4),  //2
                                                                       Common.createSpellsLevelEntry(0, 4, 2),  //3
                                                                       Common.createSpellsLevelEntry(0, 6, 4), //4
                                                                       Common.createSpellsLevelEntry(0, 6, 4, 2), //5
                                                                       Common.createSpellsLevelEntry(0, 6, 6, 4), //6
                                                                       Common.createSpellsLevelEntry(0, 8, 6, 4, 2), //7
                                                                       Common.createSpellsLevelEntry(0, 8, 6, 6, 4), //8
                                                                       Common.createSpellsLevelEntry(0, 8, 8, 6, 4, 2), //9
                                                                       Common.createSpellsLevelEntry(0, 8, 8, 6, 4, 4), //10
                                                                       Common.createSpellsLevelEntry(0, 8, 8, 8, 6, 4, 2), //11
                                                                       Common.createSpellsLevelEntry(0, 8, 8, 8, 6, 6, 4), //12
                                                                       Common.createSpellsLevelEntry(0, 8, 8, 8, 8, 6, 4, 2), //13
                                                                       Common.createSpellsLevelEntry(0, 8, 8, 8, 8, 6, 6, 4), //14
                                                                       Common.createSpellsLevelEntry(0, 8, 8, 8, 8, 8, 6, 4, 2), //15
                                                                       Common.createSpellsLevelEntry(0, 8, 8, 8, 8, 8, 6, 6, 4), //16
                                                                       Common.createSpellsLevelEntry(0, 8, 8, 8, 8, 8, 8, 6, 4, 2), //17
                                                                       Common.createSpellsLevelEntry(0, 8, 8, 8, 8, 8, 8, 6, 6, 4), //18
                                                                       Common.createSpellsLevelEntry(0, 8, 8, 8, 8, 8, 8, 8, 6, 6), //19
                                                                       Common.createSpellsLevelEntry(0, 8, 8, 8, 8, 4, 8, 8, 8, 8) //20
                                                                       );

            spellbook.SpellList = Helpers.Create<BlueprintSpellList>();
            spellbook.SpellList.name = "ArmathorSpellList";
            library.AddAsset(spellbook.SpellList, "");
            spellbook.SpellList.SpellsByLevel = new SpellLevelList[10];

            for (int i = 0; i < spellbook.SpellList.SpellsByLevel.Length; i++)
            {
                spellbook.SpellList.SpellsByLevel[i] = new SpellLevelList(i);
            }
            spellbook.SpellList.SpellsByLevel[0].SpellLevel = 0;

            BlueprintAbility[] extra_spells = new BlueprintAbility[]
            {
                 library.Get<BlueprintAbility>("9f10909f0be1f5141bf1c102041f93d9"), //snow ball
                 library.Get<BlueprintAbility>("3e9d1119d43d07c4c8ba9ebfd1671952"), //gravity bow
                 library.Get<BlueprintAbility>("2c38da66e5a599347ac95b3294acbe00"), //true strike
                 NewSpells.magic_weapon,
                 NewSpells.magic_weapon_greater,
                 library.Get<BlueprintAbility>("c28de1f98a3f432448e52e5d47c73208"), //protection from arrows
                 library.Get<BlueprintAbility>("9a46dfd390f943647ab4395fc997936d"), //acid arrow
                 NewSpells.flame_arrow
            };
            //add cleric spells      
            foreach (var spell_level_list in cleric_class.Spellbook.SpellList.SpellsByLevel)
            {
                int sp_level = spell_level_list.SpellLevel;
                foreach (var spell in spell_level_list.Spells)
                {
                    if (!spell.IsInSpellList(spellbook.SpellList))
                    {
                        ExtensionMethods.AddToSpellList(spell, spellbook.SpellList, sp_level);
                    }
                }
            }
            //add wizard spells 
            foreach (var spell_level_list in wizard_class.Spellbook.SpellList.SpellsByLevel)
            {
                int sp_level = spell_level_list.SpellLevel;
                foreach (var spell in spell_level_list.Spells)
                {
                    if (!spell.IsInSpellList(spellbook.SpellList))
                    {
                        ExtensionMethods.AddToSpellList(spell, spellbook.SpellList, sp_level);
                    }
                }
            }
            //add magus spells      
            foreach (var spell_level_list in magus_class.Spellbook.SpellList.SpellsByLevel)
            {
                int sp_level = spell_level_list.SpellLevel;
                foreach (var spell in spell_level_list.Spells)
                {
                    if (!spell.IsInSpellList(spellbook.SpellList))
                    {
                        ExtensionMethods.AddToSpellList(spell, spellbook.SpellList, sp_level);
                    }
                }
            }
            //add paladin spells      
            foreach (var spell_level_list in paladin_class.Spellbook.SpellList.SpellsByLevel)
            {
                int sp_level = spell_level_list.SpellLevel;
                foreach (var spell in spell_level_list.Spells)
                {
                    if (!spell.IsInSpellList(spellbook.SpellList))
                    {
                        ExtensionMethods.AddToSpellList(spell, spellbook.SpellList, sp_level);
                    }
                }
            }

        }
    }
}
