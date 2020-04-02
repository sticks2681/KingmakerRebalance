using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.EntitySystem.Stats;

namespace CallOfTheWild.Archetypes
{
    class Feywarden
    {
        static public BlueprintArchetype archetype;

        static public BlueprintSpellbook spellbook;
        static public BlueprintFeatureSelection fighter_feat;

        static LibraryScriptableObject library => Main.library;

        static public void create()
        {
            var cleric_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("67819271767a9dd4fbfd4ae700befea0");
            
			createSpellbook();
            
			fighter_feat = library.CopyAndAdd<BlueprintFeatureSelection>("41c8486641f7d6d4283ca9dae4147a9f", "FeywardenBonusFeat", "Test.");
            fighter_feat.SetDescription("Test.");

            archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "FeywardenArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Feywarden");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Test.");
            });
            Helpers.SetField(archetype, "m_ParentClass", cleric_class);
            library.AddAsset(archetype, "");

            archetype.ReplaceSpellbook = spellbook;

            archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, library.Get<BlueprintFeature>("48525e5da45c9c243a343fc6545dbdb9"), //first domain
                                                                                library.Get<BlueprintFeature>("43281c3d7fe18cc4d91928395837cd1e")), //second domain
                                                       };

            archetype.AddFeatures = new LevelEntry[] { Helpers.LevelEntry(1, fighter_feat),
                                                       Helpers.LevelEntry(3, fighter_feat),
                                                       Helpers.LevelEntry(6, fighter_feat),
                                                       Helpers.LevelEntry(9, fighter_feat),
                                                       Helpers.LevelEntry(12, fighter_feat),
                                                       Helpers.LevelEntry(15, fighter_feat),
                                                       Helpers.LevelEntry(18, fighter_feat),
                                                       Helpers.LevelEntry(20, fighter_feat)
													 };

            cleric_class.Progression.UIGroups = cleric_class.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(fighter_feat));
            cleric_class.Archetypes = cleric_class.Archetypes.AddToArray(archetype);
        }

        static void createSpellbook()
        {
            var cleric_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("67819271767a9dd4fbfd4ae700befea0");

            spellbook = Helpers.Create<BlueprintSpellbook>();
            spellbook.name = "FeywardenSpellbook";
            library.AddAsset(spellbook, "");
            spellbook.Name = Helpers.CreateString("FeywardenSpellbook.Name", "Feywarden");
            spellbook.SpellsKnown = cleric_class.Spellbook.SpellsKnown;
            spellbook.Spontaneous = true;
            spellbook.IsArcane = false;
            spellbook.AllSpellsKnown = true;
            spellbook.CanCopyScrolls = false;
            spellbook.CastingAttribute = StatType.Wisdom;
            spellbook.CharacterClass = cleric_class;
            spellbook.CasterLevelModifier = 0;
            spellbook.CantripsType = cleric_class.Spellbook.CantripsType;
            spellbook.SpellsPerDay =  Common.createSpellsTable("FeywardenSpellsPerDay", "",
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
            spellbook.SpellList.name = "FeywardenSpellList";
            library.AddAsset(spellbook.SpellList, "");
            spellbook.SpellList.SpellsByLevel = new SpellLevelList[10];

            for (int i = 0; i < spellbook.SpellList.SpellsByLevel.Length; i++)
            {
                spellbook.SpellList.SpellsByLevel[i] = new SpellLevelList(i);
            }
            spellbook.SpellList.SpellsByLevel[0].SpellLevel = 0;

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
        }
    }
}
