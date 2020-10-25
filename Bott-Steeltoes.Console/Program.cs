using Discord_Bot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Environment;

namespace Bott_Steeltoes.Console
{
    /// <summary>
    /// A D&D-related bot I wrote for a D&D-related server.
    /// Named after a fun Beholder character in a Waterdeep: Dragon Heist campaign I played.
    /// </summary>
    class Program : Discord_Bot.Base
    {
        public Program() : base(prefix: ";") { }

        public static void Main(string[] args) => Task.WaitAll(new Program().RunAsync());

        [Command("wild ?magic|wm", "wild magic|wm", "Roll on the Sorcerer's Wild Magic Table.")]
        public static string WildMagic(Command command)
        {
            var roll = Random.Next(WildMagicTable.Length);
            var result = WildMagicTable[roll];

            int? effectRoll = null;
            result = WildMagicDiceRegex.Replace(result, m =>
            {
                var value = 0;

                bool numParsed;
                int num = (numParsed = int.TryParse(m.Groups["num"].Value, out num)) ? num : 1;
                var die = int.Parse(m.Groups["die"].Value);

                for (var i = 0; i < num; i++)
                {
                    value += Random.Next(die) + 1;
                }

                var modMatch = m.Groups["mod"];
                if (modMatch.Success)
                {
                    value += int.Parse(m.Groups["amt"].Value) * (modMatch.Value == "-" ? -1 : 1);
                }

                if (numParsed == false)
                {
                    effectRoll = value;

                    return "";
                }

                return $"**{value}**";
            });

            return string.Join(NewLine,
                $"{command.MentionAuthor} You rolled a **{roll + 1}** for Wild Magic!",
                $"Effect: {result}" + (effectRoll.HasValue ? NewLine + $"Effect Roll: **{effectRoll}**" : "")
            );
        }
        private static Regex WildMagicDiceRegex { get; } = new Regex(
            "(?<num>^Roll a |\\d{1,3})d(?<die>\\d{1,3})((?<mod>[+-])(?<amt>\\d{1,3}))?(\\. )?",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture
        );
        private static readonly string[] WildMagicTable = {
            /*001*/ "Roll on this table at the start of each of your turns for the next minute, ignoring this result on subsequent rolls.",
            /*002*/ "Roll on this table at the start of each of your turns for the next minute, ignoring this result on subsequent rolls.",
            /*003*/ "For the next minute, you can see any invisible creature if you have line of sight to it.",
            /*004*/ "For the next minute, you can see any invisible creature if you have line of sight to it.",
            /*005*/ "A modron chosen and controlled by the DM appears in an unoccupied space within 5 feet of you, then disappears 1 minute later.",
            /*006*/ "A modron chosen and controlled by the DM appears in an unoccupied space within 5 feet of you, then disappears 1 minute later.",
            /*007*/ "You cast fireball as a 3rd-level spell centered on yourself.",
            /*008*/ "You cast fireball as a 3rd-level spell centered on yourself.",
            /*009*/ "You cast magic missile as a 5th-level spell.",
            /*010*/ "You cast magic missile as a 5th-level spell.",
            /*011*/ "Roll a d10. Your height changes by a number of inches equal to the roll. If the roll is odd, you shrink. If the roll is even, you grow.",
            /*012*/ "Roll a d10. Your height changes by a number of inches equal to the roll. If the roll is odd, you shrink. If the roll is even, you grow.",
            /*013*/ "You cast confusion centered on yourself.",
            /*014*/ "You cast confusion centered on yourself.",
            /*015*/ "For the next minute, you regain 5 hit points at the start of each of your turns.",
            /*016*/ "For the next minute, you regain 5 hit points at the start of each of your turns.",
            /*017*/ "You grow a long beard made of feathers that remains until you sneeze, at which point the feathers explode out from your face.",
            /*018*/ "You grow a long beard made of feathers that remains until you sneeze, at which point the feathers explode out from your face.",
            /*019*/ "You cast grease centered on yourself.",
            /*020*/ "You cast grease centered on yourself.",
            /*021*/ "Creatures have disadvantage on saving throws against the next spell you cast in the next minute that involves a saving throw.",
            /*022*/ "Creatures have disadvantage on saving throws against the next spell you cast in the next minute that involves a saving throw.",
            /*023*/ "Your skin turns a vibrant shade of blue. A remove curse spell can end this effect.",
            /*024*/ "Your skin turns a vibrant shade of blue. A remove curse spell can end this effect.",
            /*025*/ "An eye appears on your forehead for the next minute.During that time, you have advantage on Wisdom (Perception) checks that rely on sight.",
            /*026*/ "An eye appears on your forehead for the next minute.During that time, you have advantage on Wisdom (Perception) checks that rely on sight.",
            /*027*/ "For the next minute, all your spells with a casting time of 1 action have a casting time of 1 bonus action.",
            /*028*/ "For the next minute, all your spells with a casting time of 1 action have a casting time of 1 bonus action.",
            /*029*/ "You teleport up to 60 feet to an unoccupied space of your choice that you can see.",
            /*030*/ "You teleport up to 60 feet to an unoccupied space of your choice that you can see.",
            /*031*/ "You are transported to the Astral Plane until the end of your next turn, after which time you return to the space you previously occupied or the nearest unoccupied space if that space is occupied.",
            /*032*/ "You are transported to the Astral Plane until the end of your next turn, after which time you return to the space you previously occupied or the nearest unoccupied space if that space is occupied.",
            /*033*/ "Maximize the damage of the next damaging spell you cast within the next minute.",
            /*034*/ "Maximize the damage of the next damaging spell you cast within the next minute.",
            /*035*/ "Roll a d10. Your age changes by a number of years equal to the roll. If the roll is odd, you get younger (minimum 1 year old). If the roll is even, you get older.",
            /*036*/ "Roll a d10. Your age changes by a number of years equal to the roll. If the roll is odd, you get younger (minimum 1 year old). If the roll is even, you get older.",
            /*037*/ "1d6 flumphs controlled by the DM appear in unoccupied spaces within 60 feet of you and are frightened of you.They vanish after 1 minute.",
            /*038*/ "1d6 flumphs controlled by the DM appear in unoccupied spaces within 60 feet of you and are frightened of you.They vanish after 1 minute.",
            /*039*/ "You regain 2d10 hit points.",
            /*040*/ "You regain 2d10 hit points.",
            /*041*/ "You turn into a potted plant until the start of your next turn.While a plant, you are incapacitated and have vulnerability to all damage.If you drop to 0 hit points, your pot breaks, and your form reverts.",
            /*042*/ "You turn into a potted plant until the start of your next turn.While a plant, you are incapacitated and have vulnerability to all damage.If you drop to 0 hit points, your pot breaks, and your form reverts.",
            /*043*/ "For the next minute, you can teleport up to 20 feet as a bonus action on each of your turns.",
            /*044*/ "For the next minute, you can teleport up to 20 feet as a bonus action on each of your turns.",
            /*045*/ "You cast levitate on yourself.",
            /*046*/ "You cast levitate on yourself.",
            /*047*/ "A unicorn controlled by the DM appears in a space within 5 feet of you, then disappears 1 minute later.",
            /*048*/ "A unicorn controlled by the DM appears in a space within 5 feet of you, then disappears 1 minute later.",
            /*049*/ "You can’t speak for the next minute.Whenever you try, pink bubbles float out of your mouth.",
            /*050*/ "You can’t speak for the next minute.Whenever you try, pink bubbles float out of your mouth.",
            /*051*/ "A spectral shield hovers near you for the next minute, granting you a +2 bonus to AC and immunity to magic missile.",
            /*052*/ "A spectral shield hovers near you for the next minute, granting you a +2 bonus to AC and immunity to magic missile.",
            /*053*/ "You are immune to being intoxicated by alcohol for the next 5d6 days.",
            /*054*/ "You are immune to being intoxicated by alcohol for the next 5d6 days.",
            /*055*/ "Your hair falls out but grows back within 24 hours.",
            /*056*/ "Your hair falls out but grows back within 24 hours.",
            /*057*/ "For the next minute, any flammable object you touch that isn’t being worn or carried by another creature bursts into flame.",
            /*058*/ "For the next minute, any flammable object you touch that isn’t being worn or carried by another creature bursts into flame.",
            /*059*/ "You regain your lowest-level expended spell slot.",
            /*060*/ "You regain your lowest-level expended spell slot.",
            /*061*/ "For the next minute, you must shout when you speak.",
            /*062*/ "For the next minute, you must shout when you speak.",
            /*063*/ "You cast fog cloud centered on yourself.",
            /*064*/ "You cast fog cloud centered on yourself.",
            /*065*/ "Up to three creatures you choose within 30 feet of you take 4d10 lightning damage.",
            /*066*/ "Up to three creatures you choose within 30 feet of you take 4d10 lightning damage.",
            /*067*/ "You are frightened by the nearest creature until the end of your next turn.",
            /*068*/ "You are frightened by the nearest creature until the end of your next turn.",
            /*069*/ "Each creature within 30 feet of you becomes invisible for the next minute.The invisibility ends on a creature when it attacks or casts a spell.",
            /*070*/ "Each creature within 30 feet of you becomes invisible for the next minute.The invisibility ends on a creature when it attacks or casts a spell.",
            /*071*/ "You gain resistance to all damage for the next minute.",
            /*072*/ "You gain resistance to all damage for the next minute.",
            /*073*/ "A random creature within 60 feet of you becomes poisoned for 1d4 hours.",
            /*074*/ "A random creature within 60 feet of you becomes poisoned for 1d4 hours.",
            /*075*/ "You glow with bright light in a 30-foot radius for the next minute.Any creature that ends its turn within 5 feet of you is blinded until the end of its next turn.",
            /*076*/ "You glow with bright light in a 30-foot radius for the next minute.Any creature that ends its turn within 5 feet of you is blinded until the end of its next turn.",
            /*077*/ "You cast polymorph on yourself.If you fail the saving throw, you turn into a sheep for the spell’s duration.",
            /*078*/ "You cast polymorph on yourself.If you fail the saving throw, you turn into a sheep for the spell’s duration.",
            /*079*/ "Illusory butterflies and flower petals flutter in the air within 10 feet of you for the next minute.",
            /*080*/ "Illusory butterflies and flower petals flutter in the air within 10 feet of you for the next minute.",
            /*081*/ "You can take one additional action immediately.",
            /*082*/ "You can take one additional action immediately.",
            /*083*/ "Each creature within 30 feet of you takes 1d10 necrotic damage. You regain hit points equal to the sum of the necrotic damage dealt.",
            /*084*/ "Each creature within 30 feet of you takes 1d10 necrotic damage. You regain hit points equal to the sum of the necrotic damage dealt.",
            /*085*/ "You cast mirror image.",
            /*086*/ "You cast mirror image.",
            /*087*/ "You cast fly on a random creature within 60 feet of you.",
            /*088*/ "You cast fly on a random creature within 60 feet of you.",
            /*089*/ "You become invisible for the next minute.During that time, other creatures can’t hear you.The invisibility ends if you attack or cast a spell.",
            /*090*/ "You become invisible for the next minute.During that time, other creatures can’t hear you.The invisibility ends if you attack or cast a spell.",
            /*091*/ "If you die within the next minute, you immediately come back to life as if by the reincarnate spell.",
            /*092*/ "If you die within the next minute, you immediately come back to life as if by the reincarnate spell.",
            /*093*/ "Your size increases by one size category for the next minute.",
            /*094*/ "Your size increases by one size category for the next minute.",
            /*095*/ "You and all creatures within 30 feet of you gain vulnerability to piercing damage for the next minute.",
            /*096*/ "You and all creatures within 30 feet of you gain vulnerability to piercing damage for the next minute.",
            /*097*/ "You are surrounded by faint, ethereal music for the next minute.",
            /*098*/ "You are surrounded by faint, ethereal music for the next minute.",
            /*099*/ "You regain all expended sorcery points.",
            /*100*/ "You regain all expended sorcery points.",
        };

        [Command(
            pattern: "reincarnate", hint: "reincarnate",
            description: "Roll a random race (and subrace, where applicable) from Marcus' reincarnate table."
        )]
        public static string Reincarnate(Command command)
        {
            var roll = Random.Next(Races.Length);
            var race = Races[roll];

            var current = race;
            var resultSubraces = new List<string>();
            while (Subraces.TryGetValue(current, out var subraces))
            {
                resultSubraces.Add(current = subraces[Random.Next(subraces.Length)]);
            }

            var abilityRolls = Enumerable.Range(0, 6).Select(_ => Enumerable.Range(0, 4)
                .Select(i => Random.Next(6) + 1).OrderBy(i => i).Skip(1).Sum()
            ).ToList();

            //(subrace != null ? $" (**{subrace}**)" : "")
            return string.Join(NewLine,
                command.MentionAuthor,
                $"Your New Race (**{roll + 1}**): **{race}**{resultSubraces.Aggregate("", (s, subrace) => $"{s} (**{subrace}**")}{new string(')', resultSubraces.Count)}",
                $"Ability Score Rolls: {string.Join(", ", abilityRolls.Select(s => $"**{s}**"))} (Sum: **{abilityRolls.Sum()}**)"
            );
        }
        private static readonly string[] Races =
        {
            "Gargoyle",
            "Lizardfolk",
            "Yakfolk",
            "Half-Elf",
            "Lycanthrope",
            "Vedalken",
            "Wight",
            "Medusa",
            "Simic Hybrid",
            "Lupin",
            "Deva",
            "Triton",
            "Ghost",
            "Planetar",
            "Shifter",
            "Half Ogre",
            "Orc",
            "Kalashtar",
            "Minotaur",
            "Vryloka",
            "Scarecrow",
            "Pixie",
            "Goblin",
            "Zombie",
            "Hill Giant",
            "Kenku",
            "Bearfolk",
            "Giff",
            "Storm Giant",
            "Bugbear",
            "Satyr",
            "Githzerai",
            "Loxo",
            "Cloud Giant",
            "Warforged",
            "Vanara",
            "Yuan Ti Pureblood",
            "Zoirus",
            "Bullywug",
            "Firbolg",
            "Kuo Toa",
            "Sobruaro",
            "Aasimar",
            "Ratfolk",
            "Viashino",
            "Skeleton",
            "Rhox",
            "Alseid",
            "Hobgoblin",
            "Dwarf",
            "Mind Flayer",
            "Tabaxi",
            "Mummy",
            "Centaur",
            "Grung",
            "Animated Object",
            "Aarakocra",
            "Saurial",
            "Wilden",
            "Wemic",
            "Tiefling",
            "Half-Orc",
            "Quickling",
            "Thri Kreen",
            "Fire Giant",
            "Chitine",
            "Goliath",
            "Githyanki",
            "Ghoul",
            "Trollkin",
            "Mongrelfolk",
            "Half-Dwarf",
            "Subek",
            "Gnoll",
            "Tortle",
            "Halfling",
            "Burrowling",
            "Fomorian",
            "Grimlock",
            "Human",
            "Spirit Folk",
            "Elf",
            "Illumian",
            "Dragonborn",
            "Incubus/Succubus",
            "Gnome",
            "Hamadryad",
            "Kobold",
            "Stone Giant",
            "Myconid",
            "Solara",
            "Gensai",
            "Monolith",
            "Shardmind",
            "Frost Giant",
            "Revenant",
            "Fey’ri",
            "Changeling",
            "Cyclops",
            "Ooze-Kin",
        };
        private static readonly Dictionary<string, string[]> Subraces = new Dictionary<string, string[]>
        {
            { "Aasimar",         new [] { "Fairy", "Fallen", "Far Traveler", "Protector", "Raven Queen", "Scourge" } },
            { "Animated Object", new [] { "Animated Armor", "Flying Weapon", "Rug of Smothering", "Sentient Mimic" } },
            { "Dragonborn",      new [] { "Black", "Blue", "Brass", "Bronze", "Copper", "Gold", "Green", "Red", "Silver", "White" } },
            { "Dwarf",           new [] { "Aquatic", "Arctic", "Azer", "Cloud", "Duergar", "Gold", "Highland", "Hill", "Mountain", "Rune", "Shield", "Wild" } },
            { "Elf",             new [] { "Aquatic", "Avariel", "Blood", "Drow", "Dust", "Eladrin", "Frost", "High", "Selvari", "Shadar-Kai", "Star", "Wood" } },
            { "Gensai",          new [] { "Air", "Celestial", "Earth", "Eldritch", "Fire", "Water" } },
            { "Gnome",           new [] { "Deep", "Desert", "Forest", "Frost", "Mountain", "Redcap", "Rock", "Salt" } },
            { "Grung",           new [] { "Blue", "Gold", "Green", "Orange", "Purple", "Red", "Silver", "Yellow" } },
            { "Halfling",        new [] { "Burly", "Ghostwise", "Lightfoot", "Quarterling", "Shadowfoot", "Strongheart", "Stout" } },
            { "Half-Dwarf",      new [] { "Aquatic", "Arctic", "Azer", "Cloud", "Duergar", "Gold", "Highland", "Hill", "Mountain", "Rune", "Shield", "Wild" } },
            { "Half-Elf",        new [] { "Aquatic", "Avariel", "Blood", "Drow", "Dust", "Eladrin", "Frost", "High", "Selvari", "Shadar-Kai", "Star", "Wood" } },
            { "Half-Orc",        new [] { "Evergrowing", "Gray", "Green", "Mountain", "Orog", "Sharakim" } },
            { "Human",           new [] { "Bound Born", "Chill Born", "Normal", "Saint Born", "Variant", "Void Born", "Volcano Born", "Water Born" } },
            { "Lycanthrope",     new [] { "Werebat", "Werebear", "Wereboar", "Werecrocodile", "Werehyena", "Wererat", "Wereshark", "Weretiger", "Werewolf" } },
            { "Minotaur",        new [] { "Imix", "Natural", "Shadow" } },
            { "Ooze-Kin",        new [] { "Adhesive", "Corrosive", "Elastic", "Gelatinous", "Psionic" } },
            { "Orc",             new [] { "Evergrowing", "Gray", "Green", "Mountain", "Orog", "Sharakim" } },
            { "Saurial",         new [] { "Bladeback", "Finhead", "Flyer", "Hornhead", "Packrunner", "Stormjaw", "Waverider" } },
            { "Shifter",         new [] { "Beasthide", "Cliffwalker", "Dreamsight", "Gorebrute", "Longstrider", "Longtooth", "Razorclaw", "Swiftwing", "Truediver", "Wildhunter" } },
            { "Spirit Folk",     new [] { "Bamboo", "River", "Sea", "Valley" } },
            { "Tiefling",        new [] { "Hellfire", "Infernal", "Red", "Winged" } },
            { "Tortle",          new [] { "Desert", "Ocean", "Razorback", "Softshell" } },
            { "Warforged",       new [] { "Brass", "Bronze", "Gold", "Platinum", "Silver", "Stone" } },
            //Lycanthropes:
            { "Werebat",         new [] { "Dwarf", "Elf", "Gnome", "Halfling", "Half-Dwarf", "Half-Elf", "Half-Orc", "Human", "Illumian", "Mongrelfolk", "Orc", "Vryloka", "Yuan-Ti Pureblood" } },
            { "Werebear",        new [] { "Dwarf", "Elf", "Gnome", "Halfling", "Half-Dwarf", "Half-Elf", "Half-Orc", "Human", "Illumian", "Mongrelfolk", "Orc", "Vryloka", "Yuan-Ti Pureblood" } },
            { "Wereboar",        new [] { "Dwarf", "Elf", "Gnome", "Halfling", "Half-Dwarf", "Half-Elf", "Half-Orc", "Human", "Illumian", "Mongrelfolk", "Orc", "Vryloka", "Yuan-Ti Pureblood" } },
            { "Werecrocodile",   new [] { "Dwarf", "Elf", "Gnome", "Halfling", "Half-Dwarf", "Half-Elf", "Half-Orc", "Human", "Illumian", "Mongrelfolk", "Orc", "Vryloka", "Yuan-Ti Pureblood" } },
            { "Werehyena",       new [] { "Dwarf", "Elf", "Gnome", "Halfling", "Half-Dwarf", "Half-Elf", "Half-Orc", "Human", "Illumian", "Mongrelfolk", "Orc", "Vryloka", "Yuan-Ti Pureblood" } },
            { "Wererat",         new [] { "Dwarf", "Elf", "Gnome", "Halfling", "Half-Dwarf", "Half-Elf", "Half-Orc", "Human", "Illumian", "Mongrelfolk", "Orc", "Vryloka", "Yuan-Ti Pureblood" } },
            { "Wereshark",       new [] { "Dwarf", "Elf", "Gnome", "Halfling", "Half-Dwarf", "Half-Elf", "Half-Orc", "Human", "Illumian", "Mongrelfolk", "Orc", "Vryloka", "Yuan-Ti Pureblood" } },
            { "Weretiger",       new [] { "Dwarf", "Elf", "Gnome", "Halfling", "Half-Dwarf", "Half-Elf", "Half-Orc", "Human", "Illumian", "Mongrelfolk", "Orc", "Vryloka", "Yuan-Ti Pureblood" } },
            { "Werewolf",        new [] { "Dwarf", "Elf", "Gnome", "Halfling", "Half-Dwarf", "Half-Elf", "Half-Orc", "Human", "Illumian", "Mongrelfolk", "Orc", "Vryloka", "Yuan-Ti Pureblood" } },
            //Genasi
            { "Air",             new [] { "Dwarf", "Elf", "Gnome", "Halfling", "Half-Dwarf", "Half-Elf", "Half-Orc", "Human", "Illumian", "Mongrelfolk", "Orc", "Vryloka", "Yuan-Ti Pureblood" } },
            { "Celestial",       new [] { "Dwarf", "Elf", "Gnome", "Halfling", "Half-Dwarf", "Half-Elf", "Half-Orc", "Human", "Illumian", "Mongrelfolk", "Orc", "Vryloka", "Yuan-Ti Pureblood" } },
            { "Earth",           new [] { "Dwarf", "Elf", "Gnome", "Halfling", "Half-Dwarf", "Half-Elf", "Half-Orc", "Human", "Illumian", "Mongrelfolk", "Orc", "Vryloka", "Yuan-Ti Pureblood" } },
            { "Eldritch",        new [] { "Dwarf", "Elf", "Gnome", "Halfling", "Half-Dwarf", "Half-Elf", "Half-Orc", "Human", "Illumian", "Mongrelfolk", "Orc", "Vryloka", "Yuan-Ti Pureblood" } },
            { "Fire",            new [] { "Dwarf", "Elf", "Gnome", "Halfling", "Half-Dwarf", "Half-Elf", "Half-Orc", "Human", "Illumian", "Mongrelfolk", "Orc", "Vryloka", "Yuan-Ti Pureblood" } },
            { "Water",           new [] { "Dwarf", "Elf", "Gnome", "Halfling", "Half-Dwarf", "Half-Elf", "Half-Orc", "Human", "Illumian", "Mongrelfolk", "Orc", "Vryloka", "Yuan-Ti Pureblood" } },
        };

        [Command(
            "mad(ness)?( (?<type>short(-term)?|long(-term)?|indefinite|cure))?", "mad(ness)? (short|long|indefinite|cure)", 
            "Roll on one of the Madness tables (from the DMG), or learn how madness can be cured."
        )]
        public static string Madness(Command command)
        {
            string type;
            if (command["type"].Success)
            {
                type = command["type"].Value;
            }
            else
            {
                var typeRoll = Random.Next(100);

                if (typeRoll == 0) { type = "indefinite"; }
                else if (typeRoll < 10) { type = "long"; }
                else { type = "short"; }
            }

            int roll;
            string label, effect, duration;
            switch (type.ToLower())
            {
                case "short":
                {
                    label = "Short-Term";
                    duration = $"**{Random.Next(10) + 1}** Minutes";
                    roll = Random.Next(ShortMadnesses.Length);
                    effect = ShortMadnesses[roll];

                    break;
                }
                case "long":
                {
                    label = "Long-Term";
                    duration = $"**{(Random.Next(10) + 1) * 10}** Hours";
                    roll = Random.Next(LongMadnesses.Length);
                    effect = LongMadnesses[roll];

                    break;
                }
                case "indefinite":
                {
                    label = "Indefinite";
                    duration = null;
                    roll = Random.Next(IndefiniteMadnesses.Length);
                    effect = IndefiniteMadnesses[roll];

                    break;
                }
                case "cure": return string.Join(NewLine,
                    "A *calm emotions* spell can suppress the effects of madness, while a *lesser restoration* spell can rid a character of a short-term or long-term madness.",
                    "Depending on the source of the madness, *remove curse* or *dispel evil and good* might also prove effective.",
                    "A *greater restoration* spell or more powerful magic is required to rid a character of indefinite madness."
                );
                default: return null;
            }

            return string.Join(NewLine,
                $"{command.MentionAuthor} You rolled a **{roll + 1}** for {label} Madness!",
                $"Effect: {effect}",
                $"Duration: {duration ?? "**FOREVER**"}"
            );
        }
        private static readonly string[] ShortMadnesses = new List<IEnumerable<string>>
        {
            Enumerable.Repeat("The character retreats into his or her mind and becomes paralyzed. The effect ends if the character takes any damage.", 20),
            Enumerable.Repeat("The character becomes incapacitated and spends the duration screaming, laughing, or weeping.", 10),
            Enumerable.Repeat("The character becomes frightened and must use his or her action and movement each round to flee from the source of the fear.", 10),
            Enumerable.Repeat("The character begins babbling and is incapable of normal speech or spellcasting.", 10),
            Enumerable.Repeat("The character must use his or her action each round to attack the nearest creature.", 10),
            Enumerable.Repeat("The character experiences vivid hallucinations and has disadvantage on ability checks.", 10),
            Enumerable.Repeat("The character does whatever anyone tells him or her to do that isn’t obviously self-destructive.", 5),
            Enumerable.Repeat("The character experiences an overpowering urge to eat something strange such as dirt, slime, or offal.", 5),
            Enumerable.Repeat("The character is stunned.", 10),
            Enumerable.Repeat("The character falls unconscious.", 10),
        }.SelectMany(e => e).ToArray();
        private static readonly string[] LongMadnesses = new List<IEnumerable<string>>
        {
            Enumerable.Repeat("The character feels compelled to repeat a specific activity over and over, such as washing hands, touching things, praying, or counting coins.", 10),
            Enumerable.Repeat("The character experiences vivid hallucinations and has disadvantage on ability checks.", 10),
            Enumerable.Repeat("The character suffers extreme paranoia. The character has disadvantage on Wisdom and Charisma checks.", 10),
            Enumerable.Repeat("The character regards something (usually the source of madness) with intense revulsion, as if affected by the antipathy effect of the antipathy/sympathy spell.", 10),
            Enumerable.Repeat("The character experiences a powerful delusion. Choose a potion. The character imagines that he or she is under its effects.", 5),
            Enumerable.Repeat("The character becomes attached to a “lucky charm,” such as a person or an object, and has disadvantage on attack rolls, ability checks, and saving throws while more than 30 feet from it.", 10),
            Enumerable.Repeat("The character is blinded (25%) or deafened (75%).", 10),
            Enumerable.Repeat("The character experiences uncontrollable tremors or tics, which impose disadvantage on attack rolls, ability checks, and saving throws that involve Strength or Dexterity.", 10),
            Enumerable.Repeat("The character suffers from partial amnesia. The character knows who he or she is and retains racial traits and class features, but doesn’t recognize other people or remember anything that happened before the madness took effect.", 10),
            Enumerable.Repeat("Whenever the character takes damage, he or she must succeed on a DC 15 Wisdom saving throw or be affected as though he or she failed a saving throw against the confusion spell. The confusion effect lasts for 1 minute.", 5),
            Enumerable.Repeat("The character loses the ability to speak.", 5),
            Enumerable.Repeat("The character falls unconscious. No amount of jostling or damage can wake the character.", 5),
        }.SelectMany(e => e).ToArray();
        private static readonly string[] IndefiniteMadnesses = new List<IEnumerable<string>>
        {
            Enumerable.Repeat("“Being drunk keeps me sane.”", 15),
            Enumerable.Repeat("“I keep whatever I find.”", 10),
            Enumerable.Repeat("“I try to become more like someone else I know — adopting his or her style of dress, mannerisms, and name.”", 5),
            Enumerable.Repeat("“I must bend the truth, exaggerate, or outright lie to be interesting to other people.”", 5),
            Enumerable.Repeat("“Achieving my goal is the only thing of interest to me, and I’ll ignore everything else to pursue it.”", 10),
            Enumerable.Repeat("“I find it hard to care about anything that goes on around me.”", 5),
            Enumerable.Repeat("“I don’t like the way people judge me all the time.”", 5),
            Enumerable.Repeat("“I am the smartest, wisest, strongest, fastest, and most beautiful person I know.”", 15),
            Enumerable.Repeat("“I am convinced that powerful enemies are hunting me, and their agents are everywhere I go. I am sure they’re watching me all the time.”", 10),
            Enumerable.Repeat("“There’s only one person I can trust. And only I can see this special friend.”", 5),
            Enumerable.Repeat("“I can’t take anything seriously. The more serious the situation, the funnier I find it.”", 10),
            Enumerable.Repeat("“I’ve discovered that I really like killing people.”", 5),
        }.SelectMany(e => e).ToArray();
    }
}
