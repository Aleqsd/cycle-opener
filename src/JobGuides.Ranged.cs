namespace CycleOpener;
public static partial class JobGuides {
 static void Bard(Book b){
  b.Threshold=b.L<18?null:2;var shot=b.Up("Heavy Shot","Burst Shot");var proc=b.Up("Straight Shot","Refulgent Arrow");var poison=b.Up("Venomous Bite","Caustic Bite");var wind=b.Up("Windbite","Stormbite");var area=b.Up("Quick Nock","Ladonsbite");var areaProc=b.Up("Wide Volley","Shadowbite");
  if(!b.Aoe){b.Add(b.Base,poison,"Appliquer puis entretenir avec Mâchoires de fer dès niv. 56");b.Add(b.Base,wind,"Sur une cible durable");}
  b.Add(b.Base,b.Aoe?areaProc:proc,"Seulement avec le proc ; sinon le coup de base");b.Add(b.Base,b.Aoe?area:shot,"Répéter et surveiller les procs");
  b.First="PRIORITÉS DES TIRS";b.Second="CHANSONS · DANS CET ORDRE";
  b.Add(b.Burst,"The Wanderer's Minuet","Environ 43 s");b.Add(b.Burst,"Mage's Ballad",b.L>=52?"Environ 43 s":"Jusqu’à la fin");b.Add(b.Burst,"Army's Paeon","Jusqu’au retour de la première chanson");
  b.Note="Maintenir une chanson dès qu’elle est disponible. Les procs sont conditionnels : la fiche ne suppose pas qu’ils apparaissent à chaque tir.";
  b.Rule("Iron Jaws","Renouveler les deux DoT avant leur expiration ; cette action ne pose pas un DoT absent.","Deux DoT déjà actifs");
  b.Rule("Pitch Perfect","Dépenser à trois charges, ou avant de quitter le Menuet ; éviter de perdre une charge.","Sous le Menuet");
  b.Rule(b.Up("Bloodletter","Heartbreak Shot"),"Éviter la saturation des charges ; préférer Pluie mortelle sur un groupe de cibles.","Aptitudes entre les tirs");
  b.Rule("Empyreal Arrow","Utiliser à chaque recharge, sans bloquer le prochain GCD.","15 s");
  b.Rule("Apex Arrow","Dépenser la jauge avant le débordement, en visant les buffs. Dès niv. 86, dépenser au moins 80 pour la suite.","Jauge d’âme");
  b.Rule("Radiant Finale","Aligner avec Voix de combat et Tir furieux. Un seul coda suffit à l’ouverture ; trois ensuite.","Burst de 120 s");
  b.Rule("Resonant Arrow","Utiliser après Barrage avant expiration ; Final radieux donne aussi sa suite au niv. 100.","Suites de burst");
  b.Add(b.Open,b.Aoe?area:wind,b.Aoe?"Attendre que les ennemis soient regroupés":"Si ce DoT n’est pas encore appris, commencer par le tir de base");
  if(b.Open.Count==0)b.Add(b.Open,shot);
  b.Weave(b.L>=52?"The Wanderer's Minuet":"Mage's Ballad","Raging Strikes");
  b.Add(b.Open,b.Aoe?area:poison);b.Weave("Battle Voice","Radiant Finale");
  b.Add(b.Open,b.Aoe?area:shot);b.Weave("Barrage","Empyreal Arrow");
  b.Add(b.Open,b.Aoe&&b.L>=25?areaProc:proc,"Sous Barrage ; à bas niveau uniquement si le proc est disponible");
  b.Add(b.Open,"Resonant Arrow");b.Add(b.Open,"Radiant Encore");
 }
 static void Machinist(Book b){
  b.Threshold=b.L<18?null:b.L<64||b.L is >=82 and <84?2:3;
  var a=b.Up("Split Shot","Heated Split Shot");var c=b.Up("Slug Shot","Heated Slug Shot");var e=b.Up("Clean Shot","Heated Clean Shot");var aoe=b.Up("Spread Shot","Scattergun");var heat=b.T>=3&&b.L>=52?"Auto Crossbow":b.Up("Heat Blast","Blazing Shot");var anchor=b.Up("Hot Shot","Air Anchor");var gauss=b.Up("Gauss Round","Double Check");var rico=b.Up("Ricochet","Checkmate");
  if(b.Aoe)b.Add(b.Base,aoe);else b.Seq(b.Base,a,c,e);
  b.Add(b.Burst,anchor,"Utiliser à chaque recharge");b.Add(b.Burst,b.T>=3&&b.L>=72?"Bioblaster":"Drill","Prioritaire sur le combo quand disponible");b.Add(b.Burst,"Chain Saw","Génère de la batterie");b.Add(b.Burst,"Excavator","Après Scie circulaire");
  b.Note="Faire passer les outils à recharge avant le combo. La phase de surchauffe ne doit pas retarder leur retour ; intercaler une seule aptitude entre les tirs rapides.";
  b.ThresholdDetail="Arbalète automatique dès 3 cibles. Bio-blaster gagne de la valeur si le DoT peut durer ; partager sa recharge avec Foreuse impose un choix.";
  b.Rule("Hypercharge",b.L>=35?"50 Chaleur ou effet gratuit : cinq tirs rapides. Dépenser les charges d’aptitudes pour ne pas les écraser.":"50 Chaleur : renforcer les prochains coups ; ne pas activer sans ressource.","Fenêtre sans outil imminent");
  b.Rule("Wildfire","Associer à la surchauffe : cinq tirs rapides puis un sixième GCD avant l’explosion.","Cible durable / 120 s");
  b.Rule("Reassemble","Renforcer un gros outil ou un coup de zone. Ne pas utiliser sur un coup déjà garanti critique/direct.","Avant le GCD choisi");
  b.Rule(b.Up("Rook Autoturret","Automaton Queen"),"Déployer avec au moins 50 Batterie, sans laisser la jauge déborder ; viser les buffs du groupe.","Batterie disponible");
  b.Rule("Full Metal Field","Après Stabilisateur de canon ; ne consomme pas utilement Réassemblage.","Effet requis");
  b.Add(b.Open,"Reassemble","Préparation avant le combat");b.Add(b.Open,b.L>=58?"Drill":b.Aoe?aoe:anchor);b.Weave(gauss,b.L>=66?"Barrel Stabilizer":rico);
  if(b.L>=58){b.Add(b.Open,anchor);b.Add(b.Open,"Chain Saw");b.Add(b.Open,"Excavator");b.Add(b.Open,"Full Metal Field");}
  if(b.L>=66){b.Weave("Wildfire","Hypercharge");for(var i=0;i<5;i++){b.Add(b.Open,heat);b.Weave(i%2==0?gauss:rico);}b.Add(b.Open,b.Aoe?aoe:a,"Sixième coup dans Flambée ; reprendre les outils prioritaires");}
  else b.Open.AddRange(b.Base);
 }
 static void Dancer(Book b){
  b.Threshold=b.L<15?null:2;
  b.Add(b.Base,b.Aoe?"Windmill":"Cascade");b.Add(b.Base,b.Aoe?"Rising Windmill":"Reverse Cascade","Uniquement si le proc correspondant est présent");b.Add(b.Base,b.Aoe?"Bladeshower":"Fountain");b.Add(b.Base,b.Aoe?"Bloodshower":"Fountainfall","Uniquement si le proc correspondant est présent");
  b.Add(b.Burst,"Standard Step","Exécuter les deux pas indiqués puis le Final classique");b.Add(b.Burst,"Saber Dance","50 Esprit ; éviter de dépasser 100");
  b.Note="Les procs et plumes ne sont jamais garantis. Les pas sont ceux affichés par le jeu ; ils sont résumés dans une seule étape du schéma.";
  b.Rule("Standard Step","Maintenir le bonus et utiliser les dégâts de la danse. Au niv. 96, Mouvement final remplace certaines danses après Apothéose.","Deux pas corrects puis final");
  b.Rule("Technical Step","Préparer quatre pas puis le final ; associer Tango endiablé et les attaques fortes.","Burst de 120 s");
  b.Rule("Flourish","Consommer les procs avant de les remplacer ; utiliser les suites d’éventail fournies.","60 s");
  b.Rule(b.T>=2&&b.L>=50?"Fan Dance II":"Fan Dance","Garder des plumes pour le burst sans atteindre le plafond ; jouer Danse de l'éventail III avant de remplacer son proc.","Plumes / procs");
  b.Rule("Saber Dance","Dépenser à 80 Esprit environ pour éviter le débordement ; prioriser la fenêtre technique si possible.","50 Esprit minimum");
  b.Rule("Closed Position","Choisir un partenaire DPS avant le départ et conserver le lien.","Préparation");
  if(b.L>=15){b.Add(b.Open,"Standard Step","Préparer les deux pas avant le pull");b.Add(b.Open,"Double Standard Finish","Au pull, après les deux pas");}
  if(b.L>=70){b.Add(b.Open,"Technical Step","Exécuter les quatre pas indiqués");b.Add(b.Open,"Quadruple Technical Finish");b.Weave("Devilment");
   b.Add(b.Open,"Tillana");b.Weave("Flourish");
   if(b.L>=82)b.Add(b.Open,b.L>=100?"Dance of the Dawn":"Saber Dance","Esprit de Tillana requis");b.Weave("Fan Dance IV");
   b.Add(b.Open,"Last Dance");if(b.L>=72)b.Weave("Fan Dance III");b.Add(b.Open,"Finishing Move","Effet d’Apothéose requis");b.Add(b.Open,"Starfall Dance");
  }else b.Weave("Devilment");
  b.Open.AddRange(b.Base);
 }
}
