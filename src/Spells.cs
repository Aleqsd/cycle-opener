namespace CycleOpener;
public sealed record Spell(uint Id, string Name, int Level, uint Icon, string EnglishName);
public static class Spells {
 public static readonly Dictionary<uint,Spell> All=new(){
  [119]=new(119,"Terre",1,403,"Stone"),
  [120]=new(120,"Soin",2,405,"Cure"),
  [121]=new(121,"Vent",4,401,"Aero"),
  [124]=new(124,"Médica",10,408,"Medica"),
  [125]=new(125,"Vie",12,411,"Raise"),
  [127]=new(127,"Extra Terre",18,404,"Stone II"),
  [131]=new(131,"Méga Soin",40,407,"Cure III"),
  [132]=new(132,"Extra Vent",46,402,"Aero II"),
  [133]=new(133,"Extra Médica",50,409,"Medica II"),
  [135]=new(135,"Extra Soin",30,406,"Cure II"),
  [136]=new(136,"Présence d'esprit",30,2626,"Presence of Mind"),
  [137]=new(137,"Récup",35,2628,"Regen"),
  [139]=new(139,"Miracle",45,2629,"Holy"),
  [140]=new(140,"Bénédiction",50,2627,"Benediction"),
  [141]=new(141,"Feu",2,451,"Fire"),
  [142]=new(142,"Glace",1,454,"Blizzard"),
  [144]=new(144,"Foudre",6,457,"Thunder"),
  [147]=new(147,"Extra Feu",18,452,"Fire II"),
  [149]=new(149,"Transposition",4,466,"Transpose"),
  [152]=new(152,"Méga Feu",35,453,"Fire III"),
  [153]=new(153,"Méga Foudre",45,459,"Thunder III"),
  [154]=new(154,"Méga Glace",35,456,"Blizzard III"),
  [156]=new(156,"Blessure",15,462,"Scathe"),
  [158]=new(158,"Vasque de mana",30,2651,"Manafont"),
  [159]=new(159,"Gel",40,2653,"Freeze"),
  [162]=new(162,"Brasier",50,2652,"Flare"),
  [3568]=new(3568,"Méga Terre",54,2631,"Stone III"),
  [3569]=new(3569,"Asile",52,2632,"Asylum"),
  [3570]=new(3570,"Tétragramme",60,2633,"Tetragrammaton"),
  [3571]=new(3571,"Assises",56,2634,"Assize"),
  [3573]=new(3573,"Manalignements",52,2656,"Ley Lines"),
  [3576]=new(3576,"Giga Glace",58,2659,"Blizzard IV"),
  [3577]=new(3577,"Giga Feu",60,2660,"Fire IV"),
  [7420]=new(7420,"Giga Foudre",64,2662,"Thunder IV"),
  [7421]=new(7421,"Triple sort",66,2663,"Triplecast"),
  [7422]=new(7422,"Infect",70,2664,"Foul"),
  [7430]=new(7430,"Sponte",58,2636,"Thin Air"),
  [7431]=new(7431,"Giga Terre",64,2637,"Stone IV"),
  [7432]=new(7432,"Faveur divine",66,2638,"Divine Benison"),
  [7433]=new(7433,"Indulgence plénière",70,2639,"Plenary Indulgence"),
  [7447]=new(7447,"Extra Foudre",26,468,"Thunder II"),
  [7561]=new(7561,"Magie prompte",18,866,"Swiftcast"),
  [7562]=new(7562,"Rêve lucide",14,865,"Lucid Dreaming"),
  [7568]=new(7568,"Guérison",10,884,"Esuna"),
  [16505]=new(16505,"Feu du désespoir",72,2665,"Despair"),
  [16506]=new(16506,"Âme ombrale",35,2666,"Umbral Soul"),
  [16507]=new(16507,"Xénoglossie",80,2667,"Xenoglossy"),
  [16531]=new(16531,"Offrande de réconfort",52,2640,"Afflatus Solace"),
  [16532]=new(16532,"Lumen",72,2641,"Dia"),
  [16533]=new(16533,"Chatoiement",72,2642,"Glare"),
  [16534]=new(16534,"Offrande de ravissement",76,2643,"Afflatus Rapture"),
  [16535]=new(16535,"Offrande de misère",74,2644,"Afflatus Misery"),
  [16536]=new(16536,"Tempérance",80,2645,"Temperance"),
  [25793]=new(25793,"Extra Glace",12,2668,"Blizzard II"),
  [25794]=new(25794,"Extra Feu majeur",82,2669,"High Fire II"),
  [25795]=new(25795,"Extra Glace majeure",82,2670,"High Blizzard II"),
  [25796]=new(25796,"Amplificateur",86,2671,"Amplifier"),
  [25797]=new(25797,"Paradoxe",90,2672,"Paradox"),
  [25859]=new(25859,"Méga Chatoiement",82,2646,"Glare III"),
  [25860]=new(25860,"Méga Miracle",82,2647,"Holy III"),
  [25861]=new(25861,"Aquavoile",86,2648,"Aquaveil"),
  [25862]=new(25862,"Tintinnabule",90,2649,"Liturgy of the Bell"),
  [36986]=new(36986,"Foudre majeure",92,2673,"High Thunder"),
  [36987]=new(36987,"Extra Foudre majeure",92,2674,"High Thunder II"),
  [36989]=new(36989,"Astre flamboyant",100,2151,"Flare Star"),
  [37009]=new(37009,"Giga Chatoiement",92,2126,"Glare IV"),
  [37010]=new(37010,"Méga Médica",96,2127,"Medica III"),
  [37011]=new(37011,"Caresse divine",100,2128,"Divine Caress"),
 };
 static Spells(){JobActions.Register(All);}
 private static Dictionary<uint,string> names=new();
 private static System.Text.RegularExpressions.Regex? namePattern;
 private static Dictionary<string,string> replacements=new();
 public static Spell Get(uint id)=>All[id];
 public static string Name(uint id)=>names.GetValueOrDefault(id,Get(id).Name);
 public static void ConfigureNames(Func<uint,string?> resolve){
  names=All.Values.ToDictionary(s=>s.Id,s=>{var value=resolve(s.Id);return string.IsNullOrWhiteSpace(value)?s.Name:value;});
  replacements=All.Values.Where(s=>s.Name!=names[s.Id]).GroupBy(s=>s.Name).ToDictionary(g=>g.Key,g=>names[g.First().Id]);
  namePattern=replacements.Count==0?null:new System.Text.RegularExpressions.Regex(@"(?<![\p{L}])("+string.Join("|",replacements.Keys.OrderByDescending(n=>n.Length).Select(System.Text.RegularExpressions.Regex.Escape))+@")(?![\p{L}])");
 }
 public static string LocalizeText(string value)=>namePattern?.Replace(value,m=>replacements[m.Value])??value;
}
