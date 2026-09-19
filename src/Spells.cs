namespace CycleOpener;
public sealed record Spell(uint Id, string Name, int Level, uint Icon);
public static class Spells {
 public static readonly Dictionary<uint, Spell> All = new() {
  [7447] = new(7447, "Extra Foudre", 26, 468),
  [141] = new(141, "Feu", 2, 451),
  [142] = new(142, "Glace", 1, 454),
  [144] = new(144, "Foudre", 6, 457),
  [147] = new(147, "Extra Feu", 18, 452),
  [149] = new(149, "Transposition", 4, 466),
  [152] = new(152, "Méga Feu", 35, 453),
  [153] = new(153, "Méga Foudre", 45, 459),
  [154] = new(154, "Méga Glace", 35, 456),
  [156] = new(156, "Blessure", 15, 462),
  [158] = new(158, "Vasque de mana", 30, 2651),
  [159] = new(159, "Gel", 40, 2653),
  [162] = new(162, "Brasier", 50, 2652),
  [3573] = new(3573, "Manalignements", 52, 2656),
  [3576] = new(3576, "Giga Glace", 58, 2659),
  [3577] = new(3577, "Giga Feu", 60, 2660),
  [7420] = new(7420, "Giga Foudre", 64, 2662),
  [7421] = new(7421, "Triple sort", 66, 2663),
  [7422] = new(7422, "Infect", 70, 2664),
  [7561] = new(7561, "Magie prompte", 18, 866),
  [16505] = new(16505, "Feu du désespoir", 72, 2665),
  [16506] = new(16506, "Âme ombrale", 35, 2666),
  [16507] = new(16507, "Xénoglossie", 80, 2667),
  [25793] = new(25793, "Extra Glace", 12, 2668),
  [25794] = new(25794, "Extra Feu majeur", 82, 2669),
  [25795] = new(25795, "Extra Glace majeure", 82, 2670),
  [25796] = new(25796, "Amplificateur", 86, 2671),
  [25797] = new(25797, "Paradoxe", 90, 2672),
  [36986] = new(36986, "Foudre majeure", 92, 2673),
  [36987] = new(36987, "Extra Foudre majeure", 92, 2674),
  [36989] = new(36989, "Astre flamboyant", 100, 2151),
 };
 public static Spell Get(uint id) => All[id];
}
