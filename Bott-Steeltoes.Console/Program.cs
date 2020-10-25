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
    protected override string Name => "Bott_Steeltoes";

    public Program() : base(prefix: ";") { }

    public static void Main(string[] args) => Task.WaitAll(new Program().RunAsync());

    [Command("wild ?magic|wm", "wild magic|wm", "Roll on the Sorcerer's Wild Magic Table.")]
    public Output WildMagic(Input input)
    {
      var result = WildMagicTable.Roll(out var roll);

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

      return new Output(
        $"{input.MentionAuthor} You rolled a **{roll + 1}** for Wild Magic!",
        $"Effect: {result}" + (effectRoll.HasValue ? NewLine + $"Effect Roll: **{effectRoll}**" : "")
      );
    }
    private static Regex WildMagicDiceRegex { get; } = new Regex(
      "(?<num>^Roll a |\\d{1,3})d(?<die>\\d{1,3})((?<mod>[+-])(?<amt>\\d{1,3}))?(\\. )?",
      RegexOptions.Compiled | RegexOptions.ExplicitCapture
    );
    private static readonly Table<string> WildMagicTable = new Table<string>
    (
      (01.. 02, "Roll on this table at the start of each of your turns for the next minute, ignoring this result on subsequent rolls."),
      (03.. 04, "For the next minute, you can see any invisible creature if you have line of sight to it."),
      (05.. 06, "A modron chosen and controlled by the DM appears in an unoccupied space within 5 feet of you, then disappears 1 minute later."),
      (07.. 08, "You cast fireball as a 3rd-level spell centered on yourself."),
      (09.. 10, "You cast magic missile as a 5th-level spell."),
      (11.. 12, "Roll a d10. Your height changes by a number of inches equal to the roll. If the roll is odd, you shrink. If the roll is even, you grow."),
      (13.. 14, "You cast confusion centered on yourself."),
      (15.. 16, "For the next minute, you regain 5 hit points at the start of each of your turns."),
      (17.. 18, "You grow a long beard made of feathers that remains until you sneeze, at which point the feathers explode out from your face."),
      (19.. 20, "You cast grease centered on yourself."),
      (21.. 22, "Creatures have disadvantage on saving throws against the next spell you cast in the next minute that involves a saving throw."),
      (23.. 24, "Your skin turns a vibrant shade of blue. A remove curse spell can end this effect."),
      (25.. 26, "An eye appears on your forehead for the next minute. During that time, you have advantage on Wisdom (Perception) checks that rely on sight."),
      (27.. 28, "For the next minute, all your spells with a casting time of 1 action have a casting time of 1 bonus action."),
      (29.. 30, "You teleport up to 60 feet to an unoccupied space of your choice that you can see."),
      (31.. 32, "You are transported to the Astral Plane until the end of your next turn, after which time you return to the space you previously occupied or the nearest unoccupied space if that space is occupied."),
      (33.. 34, "Maximize the damage of the next damaging spell you cast within the next minute."),
      (35.. 36, "Roll a d10. Your age changes by a number of years equal to the roll. If the roll is odd, you get younger (minimum 1 year old). If the roll is even, you get older."),
      (37.. 38, "1d6 flumphs controlled by the DM appear in unoccupied spaces within 60 feet of you and are frightened of you. They vanish after 1 minute."),
      (39.. 40, "You regain 2d10 hit points."),
      (41.. 42, "You turn into a potted plant until the start of your next turn. While a plant, you are incapacitated and have vulnerability to all damage. If you drop to 0 hit points, your pot breaks, and your form reverts."),
      (43.. 44, "For the next minute, you can teleport up to 20 feet as a bonus action on each of your turns."),
      (45.. 46, "You cast levitate on yourself."),
      (47.. 48, "A unicorn controlled by the DM appears in a space within 5 feet of you, then disappears 1 minute later."),
      (49.. 50, "You can’t speak for the next minute. Whenever you try, pink bubbles float out of your mouth."),
      (51.. 52, "A spectral shield hovers near you for the next minute, granting you a +2 bonus to AC and immunity to magic missile."),
      (53.. 54, "You are immune to being intoxicated by alcohol for the next 5d6 days."),
      (55.. 56, "Your hair falls out but grows back within 24 hours."),
      (57.. 58, "For the next minute, any flammable object you touch that isn’t being worn or carried by another creature bursts into flame."),
      (59.. 60, "You regain your lowest-level expended spell slot."),
      (61.. 62, "For the next minute, you must shout when you speak."),
      (63.. 64, "You cast fog cloud centered on yourself."),
      (65.. 66, "Up to three creatures you choose within 30 feet of you take 4d10 lightning damage."),
      (67.. 68, "You are frightened by the nearest creature until the end of your next turn."),
      (69.. 70, "Each creature within 30 feet of you becomes invisible for the next minute. The invisibility ends on a creature when it attacks or casts a spell."),
      (71.. 72, "You gain resistance to all damage for the next minute."),
      (73.. 74, "A random creature within 60 feet of you becomes poisoned for 1d4 hours."),
      (75.. 76, "You glow with bright light in a 30-foot radius for the next minute. Any creature that ends its turn within 5 feet of you is blinded until the end of its next turn."),
      (77.. 78, "You cast polymorph on yourself. If you fail the saving throw, you turn into a sheep for the spell’s duration."),
      (79.. 80, "Illusory butterflies and flower petals flutter in the air within 10 feet of you for the next minute."),
      (81.. 82, "You can take one additional action immediately."),
      (83.. 84, "Each creature within 30 feet of you takes 1d10 necrotic damage. You regain hit points equal to the sum of the necrotic damage dealt."),
      (85.. 86, "You cast mirror image."),
      (87.. 88, "You cast fly on a random creature within 60 feet of you."),
      (89.. 90, "You become invisible for the next minute. During that time, other creatures can’t hear you. The invisibility ends if you attack or cast a spell."),
      (91.. 92, "If you die within the next minute, you immediately come back to life as if by the reincarnate spell."),
      (93.. 94, "Your size increases by one size category for the next minute."),
      (95.. 96, "You and all creatures within 30 feet of you gain vulnerability to piercing damage for the next minute."),
      (97.. 98, "You are surrounded by faint, ethereal music for the next minute."),
      (99..100, "You regain all expended sorcery points.")
    );

    [Command(
      pattern: "reincarnate", hint: "reincarnate",
      description: "Roll a random race (and subrace, where applicable) from Marcus' reincarnate table."
    )]
    public Output Reincarnate(Input input)
    {
      var race = Races.Roll(out var roll);

      var currentRace = race;
      var subracesList = new List<string>();
      while (Subraces.TryGetValue(currentRace, out var currentSubraces))
      {
        subracesList.Add(currentRace = currentSubraces.Roll());
      }
      var subracesString =
        $"{subracesList.Aggregate("", (s, subrace) => $"{s} (**{subrace}**")}{new string(')', subracesList.Count)}";

      var abilityRolls = Enumerable.Range(0, 6).Select(_ => Enumerable.Range(0, 4)
        .Select(i => Random.Next(6) + 1).OrderBy(i => i).Skip(1).Sum()
      ).ToList();

      return new Output(
        input.MentionAuthor,
        $"Your New Race (**{roll + 1}**): **{race}**{subracesString}",
        $"Ability Score Rolls: {string.Join(", ", abilityRolls.Select(s => $"**{s}**"))} (Sum: **{abilityRolls.Sum()}**)"
      );
    }
    private static readonly Table<string> Races = new Table<string>(new[]
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
      "Ooze-Kin"
    });
    private static readonly Table<string> HumanoidRaces = new Table<string>
    (
      "Dwarf", "Elf", "Gnome", "Halfling", "Half-Dwarf", "Half-Elf", "Half-Orc", 
      "Human", "Illumian", "Mongrelfolk", "Orc", "Vryloka", "Yuan-Ti Pureblood"
    );
    private static readonly Dictionary<string, Table<string>> Subraces = new Dictionary<string, Table<string>>
    {
      { "Aasimar",         new Table<string>("Fairy", "Fallen", "Far Traveler", "Protector", "Raven Queen", "Scourge")},
      { "Animated Object", new Table<string>("Animated Armor", "Flying Weapon", "Rug of Smothering", "Sentient Mimic")},
      { "Dragonborn",      new Table<string>("Black", "Blue", "Brass", "Bronze", "Copper", "Gold", "Green", "Red", "Silver", "White")},
      { "Dwarf",           new Table<string>("Aquatic", "Arctic", "Azer", "Cloud", "Duergar", "Gold", "Highland", "Hill", "Mountain", "Rune", "Shield", "Wild")},
      { "Elf",             new Table<string>("Aquatic", "Avariel", "Blood", "Drow", "Dust", "Eladrin", "Frost", "High", "Selvari", "Shadar-Kai", "Star", "Wood")},
      { "Gensai",          new Table<string>("Air", "Celestial", "Earth", "Eldritch", "Fire", "Water")},
      { "Gnome",           new Table<string>("Deep", "Desert", "Forest", "Frost", "Mountain", "Redcap", "Rock", "Salt")},
      { "Grung",           new Table<string>("Blue", "Gold", "Green", "Orange", "Purple", "Red", "Silver", "Yellow")},
      { "Halfling",        new Table<string>("Burly", "Ghostwise", "Lightfoot", "Quarterling", "Shadowfoot", "Strongheart", "Stout")},
      { "Half-Dwarf",      new Table<string>("Aquatic", "Arctic", "Azer", "Cloud", "Duergar", "Gold", "Highland", "Hill", "Mountain", "Rune", "Shield", "Wild")},
      { "Half-Elf",        new Table<string>("Aquatic", "Avariel", "Blood", "Drow", "Dust", "Eladrin", "Frost", "High", "Selvari", "Shadar-Kai", "Star", "Wood")},
      { "Half-Orc",        new Table<string>("Evergrowing", "Gray", "Green", "Mountain", "Orog", "Sharakim")},
      { "Human",           new Table<string>("Bound Born", "Chill Born", "Normal", "Saint Born", "Variant", "Void Born", "Volcano Born", "Water Born")},
      { "Lycanthrope",     new Table<string>("Werebat", "Werebear", "Wereboar", "Werecrocodile", "Werehyena", "Wererat", "Wereshark", "Weretiger", "Werewolf")},
      { "Minotaur",        new Table<string>("Imix", "Natural", "Shadow")},
      { "Ooze-Kin",        new Table<string>("Adhesive", "Corrosive", "Elastic", "Gelatinous", "Psionic")},
      { "Orc",             new Table<string>("Evergrowing", "Gray", "Green", "Mountain", "Orog", "Sharakim")},
      { "Saurial",         new Table<string>("Bladeback", "Finhead", "Flyer", "Hornhead", "Packrunner", "Stormjaw", "Waverider")},
      { "Shifter",         new Table<string>("Beasthide", "Cliffwalker", "Dreamsight", "Gorebrute", "Longstrider", "Longtooth", "Razorclaw", "Swiftwing", "Truediver", "Wildhunter")},
      { "Spirit Folk",     new Table<string>("Bamboo", "River", "Sea", "Valley")},
      { "Tiefling",        new Table<string>("Hellfire", "Infernal", "Red", "Winged")},
      { "Tortle",          new Table<string>("Desert", "Ocean", "Razorback", "Softshell")},
      { "Warforged",       new Table<string>("Brass", "Bronze", "Gold", "Platinum", "Silver", "Stone")},
      //Lycanthropes:
      { "Werebat",         HumanoidRaces },
      { "Werebear",        HumanoidRaces },
      { "Wereboar",        HumanoidRaces },
      { "Werecrocodile",   HumanoidRaces },
      { "Werehyena",       HumanoidRaces },
      { "Wererat",         HumanoidRaces },
      { "Wereshark",       HumanoidRaces },
      { "Weretiger",       HumanoidRaces },
      { "Werewolf",        HumanoidRaces },
      //Genasi
      { "Air",             HumanoidRaces },
      { "Celestial",       HumanoidRaces },
      { "Earth",           HumanoidRaces },
      { "Eldritch",        HumanoidRaces },
      { "Fire",            HumanoidRaces },
      { "Water",           HumanoidRaces },
    };

    [Command(
      "mad(ness)?( (?<type>short(-term)?|long(-term)?|indefinite|cure))?", "mad(ness)? (short|long|indefinite|cure)",
      "Roll on one of the Madness tables (from the DMG), or learn how madness can be cured."
    )]
    public Output Madness(Input input)
    {
      string type;
      if (input["type"].Success)
      {
        type = input["type"].Value;
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
          effect = ShortMadness.Roll(out roll);

          break;
        }
        case "long":
        {
          label = "Long-Term";
          duration = $"**{(Random.Next(10) + 1) * 10}** Hours";
          effect = LongMadness.Roll(out roll);

          break;
        }
        case "indefinite":
        {
          label = "Indefinite";
          duration = null;
          effect = IndefiniteMadness.Roll(out roll);

          break;
        }
        case "cure":
          return new Output(
           "A *calm emotions* spell can suppress the effects of madness, while a *lesser restoration* spell can rid a character of a short-term or long-term madness.",
           "Depending on the source of the madness, *remove curse* or *dispel evil and good* might also prove effective.",
           "A *greater restoration* spell or more powerful magic is required to rid a character of indefinite madness."
          );
        default: return null;
      }

      return new Output(
        $"{input.MentionAuthor} You rolled a **{roll + 1}** for {label} Madness!",
        $"Effect: {effect}",
        $"Duration: {duration ?? "**FOREVER**"}"
      );
    }
    private static readonly Table<string> ShortMadness = new Table<string>(
      (01.. 20, "The character retreats into his or her mind and becomes paralyzed. The effect ends if the character takes any damage."),
      (21.. 30, "The character becomes incapacitated and spends the duration screaming, laughing, or weeping."),
      (31.. 40, "The character becomes frightened and must use his or her action and movement each round to flee from the source of the fear."),
      (41.. 50, "The character begins babbling and is incapable of normal speech or spellcasting."),
      (51.. 60, "The character must use his or her action each round to attack the nearest creature."),
      (61.. 70, "The character experiences vivid hallucinations and has disadvantage on ability checks."),
      (71.. 75, "The character does whatever anyone tells him or her to do that isn’t obviously self-destructive."),
      (76.. 80, "The character experiences an overpowering urge to eat something strange such as dirt, slime, or offal."),
      (81.. 90, "The character is stunned."),
      (91..100, "The character falls unconscious.")
    );
    private static readonly Table<string> LongMadness = new Table<string>
    (
      (01.. 10, "The character feels compelled to repeat a specific activity over and over, such as washing hands, touching things, praying, or counting coins."),
      (11.. 20, "The character experiences vivid hallucinations and has disadvantage on ability checks."),
      (21.. 30, "The character suffers extreme paranoia. The character has disadvantage on Wisdom and Charisma checks."),
      (31.. 40, "The character regards something (usually the source of madness) with intense revulsion, as if affected by the antipathy effect of the antipathy/sympathy spell."),
      (41.. 45, "The character experiences a powerful delusion. Choose a potion. The character imagines that he or she is under its effects."),
      (46.. 55, "The character becomes attached to a “lucky charm,” such as a person or an object, and has disadvantage on attack rolls, ability checks, and saving throws while more than 30 feet from it."),
      (56.. 65, "The character is blinded (25%) or deafened (75%)."),
      (66.. 75, "The character experiences uncontrollable tremors or tics, which impose disadvantage on attack rolls, ability checks, and saving throws that involve Strength or Dexterity."),
      (76.. 85, "The character suffers from partial amnesia. The character knows who he or she is and retains racial traits and class features, but doesn’t recognize other people or remember anything that happened before the madness took effect."),
      (86.. 90, "Whenever the character takes damage, he or she must succeed on a DC 15 Wisdom saving throw or be affected as though he or she failed a saving throw against the confusion spell. The confusion effect lasts for 1 minute."),
      (91.. 95, "The character loses the ability to speak."),
      (96..100, "The character falls unconscious. No amount of jostling or damage can wake the character.")
    );
    private static readonly Table<string> IndefiniteMadness = new Table<string>
    (
      (01.. 15, "“Being drunk keeps me sane.”"),
      (16.. 25, "“I keep whatever I find.”"),
      (26.. 30, "“I try to become more like someone else I know — adopting his or her style of dress, mannerisms, and name.”"),
      (31.. 35, "“I must bend the truth, exaggerate, or outright lie to be interesting to other people.”"),
      (36.. 45, "“Achieving my goal is the only thing of interest to me, and I’ll ignore everything else to pursue it.”"),
      (46.. 50, "“I find it hard to care about anything that goes on around me.”"),
      (51.. 55, "“I don’t like the way people judge me all the time.”"),
      (56.. 70, "“I am the smartest, wisest, strongest, fastest, and most beautiful person I know.”"),
      (71.. 80, "“I am convinced that powerful enemies are hunting me, and their agents are everywhere I go. I am sure they’re watching me all the time.”"),
      (81.. 85, "“There’s only one person I can trust. And only I can see this special friend.”"),
      (86.. 95, "“I can’t take anything seriously. The more serious the situation, the funnier I find it.”"),
      (96..100, "“I’ve discovered that I really like killing people.”")
    );
  }
}
