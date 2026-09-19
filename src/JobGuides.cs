namespace CycleOpener;

// Curated reference sheets, never fed by live resources or cooldowns.
// The conditions are deliberately displayed instead of pretending every branch is available.
public sealed record JobSheet(CyclePlan Cycle,List<Step> Opening,List<Reminder> Priorities,int? AreaThreshold,string Threshold,string OpeningNote);
public static partial class JobGuides {
 static readonly Dictionary<(GuideJob,int,int),JobSheet> Cache=new();
 public static JobSheet For(GuideContext s){
  var key=(s.Job,Math.Clamp(s.Level,1,100),Math.Clamp(s.Targets,1,8));
  if(Cache.TryGetValue(key,out var cached))return cached;
  var b=new Book(s with{Level=key.Item2,Targets=key.Item3});
  switch(s.Job){
   case GuideJob.Paladin:Paladin(b);break;case GuideJob.Warrior:Warrior(b);break;
   case GuideJob.DarkKnight:DarkKnight(b);break;case GuideJob.Gunbreaker:Gunbreaker(b);break;
   case GuideJob.Scholar:Scholar(b);break;case GuideJob.Astrologian:Astrologian(b);break;case GuideJob.Sage:Sage(b);break;
   case GuideJob.Monk:Monk(b);break;case GuideJob.Dragoon:Dragoon(b);break;case GuideJob.Ninja:Ninja(b);break;
   case GuideJob.Samurai:Samurai(b);break;case GuideJob.Reaper:Reaper(b);break;case GuideJob.Viper:Viper(b);break;
   case GuideJob.Bard:Bard(b);break;case GuideJob.Machinist:Machinist(b);break;case GuideJob.Dancer:Dancer(b);break;
   case GuideJob.Summoner:Summoner(b);break;case GuideJob.RedMage:RedMage(b);break;case GuideJob.Pictomancer:Pictomancer(b);break;
   default:throw new ArgumentOutOfRangeException(nameof(s));
  }
  return Cache[key]=b.Build();
 }
 sealed class Book(GuideContext context){
  public int L=>context.Level;public int T=>context.Targets;public GuideJob Job=>context.Job;
  public List<Step> Base=[],Burst=[],Open=[];public List<Reminder> Rules=[];
  public string First="BOUCLE DE BASE",Second="RESSOURCES · SELON CONDITIONS",Note="",ThresholdDetail="";
  public string OpeningNote="Départ pédagogique, ressources initiales vides sauf préparation indiquée. Les procs et conditions restent à vérifier en jeu ; timings de raid dans les sources.";
  public int? Threshold;
  public bool Aoe=>Threshold.HasValue&&T>=Threshold;
  public uint Id(string name)=>JobActions.Id(Job,name);
  public bool Has(string name)=>Spells.Get(Id(name)).Level<=L;
  public string Up(params string[] names)=>names.LastOrDefault(Has)??names[0];
  public void Add(List<Step> list,string name,string note="",int count=1,params string[] weaves){
   if(!Has(name))return;
   list.Add(new(Id(name),note,count,weaves.Where(Has).Select(Id).ToArray()));
  }
  public void Seq(List<Step> list,params string[] names){foreach(var name in names)Add(list,name);}
  public void Rule(string name,string text,string condition="",bool support=false){if(Has(name))Rules.Add(new(Id(name),text,condition,support));}
  public void Weave(params string[] names){
   if(Open.Count==0)return;var s=Open[^1];var all=(s.Weaves??[]).Concat(names.Where(Has).Select(Id)).ToArray();
   Open[^1]=s with{Weaves=all.Take(2).ToArray(),Note=s.Note+(all.Length>2?" · À répartir sur les prochains GCD : "+string.Join(", ",all.Skip(2).Select(id=>Spells.Get(id).Name)):"")};
  }
  public JobSheet Build(){
   if(Open.Count==0)Open.AddRange(Base.Select(s=>s with{Count=1}));
   Open=Open.SelectMany(s=>Enumerable.Repeat(s with{Count=1},s.Count)).ToList();
   if(Open.Count>0)Open[0]=Open[0] with{Note=(Open[0].Note.Length>0?Open[0].Note+" · ":"")+OpeningNote};
   var text=Threshold is {} n?$"BOUCLE DE ZONE : {n}+ cibles regroupées.":"Pas encore de boucle de zone à ce niveau.";
   return new(new(Base.ToArray(),Burst.ToArray(),Note,First,Second,"Reprendre la boucle et appliquer les priorités"),Open,Rules,Threshold,text+" "+ThresholdDetail,OpeningNote);
  }
 }
 static void TankSupport(Book b,string stance,string shortMitigation,string major){
  b.Rule(stance,"Activer la posture pour tenir les ennemis. Vérifier après une synchronisation.","Tank principal",true);
  b.Rule(shortMitigation,"Anticiper les coups importants et répartir les protections sur le pack. Conserver une réponse pour la suite.","Réduction des dégâts",true);
  b.Rule(major,"Prévoir cette protection pour un passage dangereux ; ne pas empiler toutes les défenses par défaut.","Dégâts prévus",true);
 }
 static void Paladin(Book b){
  b.Threshold=b.L<6?null:b.L<40||b.L>=94?3:2;
  b.ThresholdDetail=b.L>=72?"Sort sacré de zone dès 3 cibles (2 avant niv. 94). Les aptitudes de zone restent utiles en mono.":"Le combo génère l’aggro sur tout le pack.";
  if(b.Aoe)b.Seq(b.Base,"Total Eclipse","Prominence");else b.Seq(b.Base,"Fast Blade","Riot Blade",b.Up("Rage of Halone","Royal Authority"));
  b.Add(b.Burst,b.Aoe&&b.L>=72?"Holy Circle":"Holy Spirit","Avec Puissance divine ; avant de la remplacer");
  if(!b.Aoe){b.Add(b.Burst,"Atonement","Après Autorité royale ; suivre la chaîne");b.Seq(b.Burst,"Supplication","Sepulchre");}
  b.Note="Le combo de base fournit les ressources. Les sorts instantanés et la chaîne d’expiation s’insèrent entre les combos ; ne pas les utiliser sans leur effet requis.";
  b.Rule("Fight or Flight","Regrouper les attaques fortes dans le bonus de dégâts, puis utiliser de nouveau à chaque recharge.","Fenêtre de 60 s");
  b.Rule(b.Up("Requiescat","Imperator"),b.L>=90?"Déclencher la magie : Confiteor → Lame de foi → Lame de vérité → Lame de vaillance.":b.L>=80?"Confiteor puis trois sorts sacrés sous les charges restantes.":"Utiliser les quatre charges sur les sorts sacrés.","Sous le bonus de dégâts");
  b.Rule(b.Up("Spirits Within","Expiacion"),"Insérer les aptitudes offensives, dont Cercle du destin, dans les fenêtres disponibles.","Ne pas retarder les GCD");
  b.Rule("Blade of Honor","Utiliser la suite après Lame de vaillance avant l’expiration de l’effet.","Après la chaîne magique");
  b.Seq(b.Open,b.Aoe?"Total Eclipse":"Fast Blade",b.Aoe?"Prominence":"Riot Blade");b.Weave("Fight or Flight",b.Up("Requiescat","Imperator"));
  if(b.L>=54){b.Add(b.Open,"Goring Blade","Pendant le bonus de dégâts");b.Weave(b.Up("Spirits Within","Expiacion"),"Circle of Scorn");}
  if(b.L>=80){b.Add(b.Open,"Confiteor");if(b.L>=90){b.Seq(b.Open,"Blade of Faith","Blade of Truth","Blade of Valor");b.Weave("Blade of Honor");}else b.Add(b.Open,b.Aoe?"Holy Circle":"Holy Spirit","Sous les charges de magie",3);}
  else if(b.L>=68)b.Add(b.Open,b.Aoe&&b.L>=72?"Holy Circle":"Holy Spirit","Sous les charges de magie",4);
  if(!b.Aoe){b.Add(b.Open,b.Up("Rage of Halone","Royal Authority"),"Terminer le combo commencé");b.Add(b.Open,"Holy Spirit","Avec Puissance divine");b.Seq(b.Open,"Atonement","Supplication","Sepulchre");}
  TankSupport(b,"Iron Will",b.Up("Sheltron","Holy Sheltron"),b.Up("Sentinel","Guardian"));
 }
 static void Warrior(Book b){
  b.Threshold=b.L<10?null:b.L<26||b.L is >=40 and <50?2:3;
  var spender=b.T>=(b.L>=94?4:3)?b.Up("Steel Cyclone","Decimate"):b.Up("Inner Beast","Fell Cleave");
  if(b.Aoe)b.Seq(b.Base,"Overpower","Mythril Tempest");else b.Seq(b.Base,"Heavy Swing","Maim","Storm's Path");
  b.Add(b.Burst,"Storm's Eye","À la place du dernier coup pour entretenir le bonus ; nécessite le combo");
  b.Add(b.Burst,spender,"50 de jauge, ou charge gratuite sous le buff");
  b.Add(b.Burst,b.L>=80&&b.T<4?"Inner Chaos":"Chaotic Cyclone","Après le gain de jauge de Cri de guerre ; 50 de jauge requis");
  b.Note="Entretenir le bonus de dégâts puis dépenser la jauge sans déborder. Les coups de jauge ne cassent pas le combo.";
  b.ThresholdDetail="Coups de jauge : zone à 3+ avant niv. 94, puis 4+. Cyclone chaotique remplace la dépense après Cri de guerre ; à partir du niv. 80, le préférer à 4+.";
  b.Rule(b.Up("Berserk","Inner Release"),b.L>=70?"Consommer les trois attaques gratuites sans utiliser celles de Cri de guerre à leur place.":"Placer les trois coups renforcés sur les attaques les plus fortes disponibles.","À chaque recharge");
  b.Rule("Infuriate","Générer 50 de jauge à 50 ou moins. Dépenser l’attaque renforcée avant de reprendre une charge.","Éviter le débordement");
  b.Rule("Primal Rend","Après le buff, utiliser l’attaque avant expiration ; vérifier que le déplacement est sûr.","Charge disponible");
  b.Rule("Primal Wrath","Après trois coups gratuits, utiliser la suite. Après Griffade primitive, terminer par Dévastateur au niveau 100.","Suites de burst");
  b.Seq(b.Open,b.Aoe?"Overpower":"Heavy Swing",b.Aoe?"Mythril Tempest":"Maim");
  if(!b.Aoe)b.Add(b.Open,b.L>=50?"Storm's Eye":"Storm's Path");
  b.Weave(b.Up("Berserk","Inner Release"),"Infuriate");
  if(b.L>=72)b.Add(b.Open,b.L>=80&&b.T<4?"Inner Chaos":"Chaotic Cyclone","Après Cri de guerre");
  b.Add(b.Open,"Primal Rend");b.Add(b.Open,"Primal Ruination");
  if(b.L>=70)b.Add(b.Open,spender,"Charges gratuites",3);
  else if(b.L>=50)b.Add(b.Open,spender,"50 de jauge fournis par Cri de guerre");
  b.Weave("Primal Wrath",b.T>=3&&b.L>=86?"Orogeny":"Upheaval");
  TankSupport(b,"Defiance",b.Up("Raw Intuition","Bloodwhetting"),b.Up("Vengeance","Damnation"));
 }
 static void DarkKnight(Book b){
  b.Threshold=b.L<6?null:b.L is >=26 and <40||b.L>=94?3:2;
  var edge=b.T>=3||b.L<40?b.Up("Flood of Darkness","Flood of Shadow"):b.Up("Edge of Darkness","Edge of Shadow");
  if(b.Aoe)b.Seq(b.Base,"Unleash","Stalwart Soul");else b.Seq(b.Base,"Hard Slash","Syphon Strike","Souleater");
  b.Add(b.Burst,b.T>=3?"Quietus":"Bloodspiller","50 de Sang ; éviter de déborder");
  if(b.L>=96){if(b.T>=3)b.Add(b.Burst,"Impalement","Sous Delirium de sang",3);else{b.Add(b.Burst,"Scarlet Delirium","Sous Delirium de sang");b.Seq(b.Burst,"Comeuppance","Torcleaver");}}
  b.Note="Maintenir le bonus Ténèbres avec les dépenses de PM. Dépenser le Sang et les charges gratuites ; reprendre ensuite le combo.";
  b.ThresholdDetail="PM, Sang et Drainage abyssal : variantes de zone dès 3 cibles.";
  b.Rule(edge,"Entretenir Ténèbres, éviter le plafond de PM et concentrer le surplus sous buffs.",b.L>=70?"Garder 3 000 PM pour le bouclier si nécessaire":"3 000 PM");
  b.Rule(b.Up("Blood Weapon","Delirium"),b.L>=68?"Consommer les trois charges gratuites. Dès niv. 96 : chaîne de trois coups en mono ou Empalement sanglant en zone.":"Toucher avec trois GCD pendant l’effet pour restaurer des PM.","À chaque recharge");
  b.Rule("Living Shadow","Invoquer l’ombre au début de la fenêtre de burst. Au niveau 100, lancer Sillon de morgue sous l’effet obtenu.","Burst de 120 s");
  b.Rule("Salted Earth","Placer la zone sur les ennemis ; utiliser sa suite dès qu’elle est disponible.","Niv. 86 : Sel et ténèbres");
  b.Seq(b.Open,b.Aoe?"Unleash":"Hard Slash");b.Weave(edge,"Living Shadow");
  b.Add(b.Open,b.Aoe?"Stalwart Soul":"Syphon Strike");b.Weave(b.Up("Blood Weapon","Delirium"),"Salted Earth");
  if(!b.Aoe)b.Add(b.Open,"Souleater");b.Weave(b.T>=3?"Abyssal Drain":"Carve and Spit");
  if(b.L>=96){if(b.T>=3)b.Add(b.Open,"Impalement","Charges gratuites",3);else b.Seq(b.Open,"Scarlet Delirium","Comeuppance","Torcleaver");}
  else if(b.L>=68)b.Add(b.Open,b.T>=3?"Quietus":"Bloodspiller","Charges gratuites",3);
  b.Add(b.Open,"Disesteem","Effet de l’ombre requis");b.Weave("Shadowbringer","Salt and Darkness");
  TankSupport(b,"Grit","The Blackest Night",b.Up("Shadow Wall","Shadowed Vigil"));
 }
 static void Gunbreaker(Book b){
  b.Threshold=b.L<10?null:b.L is >=26 and <40||b.L>=94?3:2;
  var spend=b.T>=2&&b.L>=72?"Fated Circle":"Burst Strike";
  if(b.Aoe)b.Seq(b.Base,"Demon Slice","Demon Slaughter");else b.Seq(b.Base,"Keen Edge","Brutal Shell","Solid Barrel");
  b.Add(b.Burst,spend,"Une cartouche ; libérer une place avant le prochain combo");
  var fangLimit=b.L>=94?3:b.L>=72?4:b.L>=70?6:4;
  if(b.T<fangLimit){b.Add(b.Burst,"Gnashing Fang","Une cartouche ; suivre les trois coups");b.Seq(b.Burst,"Savage Claw","Wicked Talon");}
  b.Note="Construire les cartouches avec le combo. Les suites de Consécution s’insèrent immédiatement après leur attaque ; ne pas les écraser.";
  b.ThresholdDetail="Tourbillon fatidique dès 2 cibles. Croc pugnace : à réserver au mono/duo à partir du niv. 94.";
  b.Rule("No Mercy","Garder les grandes attaques et assez de cartouches pour la fenêtre de dégâts.","Burst de 60 s");
  b.Rule("Continuation","Insérer la suite correspondante après chaque coup du combo spécial. Hypervitesse suit Frappe explosive au niv. 86.","Proc requis");
  b.Rule("Bloodfest","Remplir les cartouches quand elles sont vides. Dès niv. 100, donne aussi le combo Règne animal.","Éviter le débordement");
  b.Rule("Double Down","Placer cette attaque dans le bonus de dégâts ; deux cartouches requises.","Burst");
  b.Seq(b.Open,b.Aoe?"Demon Slice":"Keen Edge",b.Aoe?"Demon Slaughter":"Brutal Shell");
  if(!b.Aoe)b.Add(b.Open,"Solid Barrel");b.Weave("No Mercy");
  if(b.L>=60&&b.T<fangLimit){b.Add(b.Open,"Gnashing Fang");b.Weave("Jugular Rip");b.Add(b.Open,"Savage Claw");b.Weave("Abdomen Tear");b.Add(b.Open,"Wicked Talon");b.Weave("Eye Gouge");}
  else if(b.L>=30&&(!b.Aoe||b.L>=40)){b.Add(b.Open,spend,"Cartouche du combo requise");b.Weave(spend=="Fated Circle"?"Fated Brand":"Hypervelocity");}
  b.Weave("Bloodfest");b.Add(b.Open,"Double Down");
  b.Add(b.Open,"Sonic Break","Sous le bonus de dégâts");b.Weave(b.Up("Danger Zone","Blasting Zone"),"Bow Shock");
  b.Seq(b.Open,"Reign of Beasts","Noble Blood","Lion Heart");
  TankSupport(b,"Royal Guard",b.Up("Heart of Stone","Heart of Corundum"),b.Up("Nebula","Great Nebula"));
 }
}
