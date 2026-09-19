namespace CycleOpener;
public static partial class JobGuides {
 static void Monk(Book b){
  b.Threshold=b.L<26?null:3;var opo=b.Up("Bootshine","Leaping Opo");var rap=b.Up("True Strike","Rising Raptor");var coe=b.Up("Snap Punch","Pouncing Coeurl");
  if(b.Aoe){b.Add(b.Base,b.Up("Arm of the Destroyer","Shadow of the Destroyer"));b.Add(b.Base,b.L>=45?"Four-point Fury":rap);b.Add(b.Base,b.L>=30?"Rockbreaker":coe);}
  else{b.Add(b.Base,opo,b.L>=50?"Forme opo-opo : dépenser la furie, sinon Tacle du dragon":"Première forme");b.Add(b.Base,rap,b.L>=18?"Forme raptor : dépenser la furie, sinon Serpents jumeaux":"Deuxième forme");b.Add(b.Base,coe,b.L>=30?"Forme coeurl : dépenser la furie, sinon Démolition":"Troisième forme");}
  b.Add(b.Burst,"Dragon Kick","Sous Équilibre parfait : trois formes identiques, alterner avec le coup opo-opo");
  b.Add(b.Burst,b.Up("Elixir Field","Elixir Burst"),"Trois chakras de même forme : Nadi lunaire");
  b.Add(b.Burst,b.Up("Flint Strike","Rising Phoenix"),"Trois formes différentes : Nadi solaire");
  b.Add(b.Burst,b.Up("Tornado Kick","Phantom Rush"),"Les deux Nadi + trois nouveaux chakras");
  b.Note="Le schéma décrit les trois formes. Choisir le générateur quand sa furie est vide, puis le coup renforcé ; les blitz sont des alternatives selon les Nadi.";
  b.Rule("Perfect Balance","Trois coups libres : trois formes identiques pour la Lune, trois différentes pour le Soleil ; réunir les deux pour le finisher.",b.L>=60?"Chakras de formes":"Avant niv. 60 : alterner les coups opo-opo");
  b.Rule(b.Up("Steel Peak","the Forbidden Chakra"),"Dépenser les cinq chakras sans déborder. Variante de zone : à 2 cibles avant niv. 54, 4 entre 54–73, puis 3.","Chakras disponibles");
  b.Rule("Riddle of Fire","Regrouper les blitz et ripostes dans le bonus ; Fraternité accompagne une fenêtre sur deux.","60 / 120 s");
  b.Rule("Fire's Reply","Après Énigme du feu, utiliser la riposte puis profiter de la forme libre pour le coup opo-opo.","Effet requis");
  b.Rule("Wind's Reply","Utiliser la riposte accordée par Énigme du vent avant expiration.","Effet requis");
  b.Add(b.Open,b.L>=50?"Dragon Kick":opo,"Avant : chakras préparés ; Changement de posture si disponible");b.Add(b.Open,b.L>=18?"Twin Snakes":rap);b.Add(b.Open,b.L>=30?"Demolish":coe);b.Weave("Riddle of Fire","Brotherhood");
  if(b.L>=50){b.Add(b.Open,"Dragon Kick");b.Weave("Perfect Balance");b.Seq(b.Open,opo,"Dragon Kick",opo);b.Add(b.Open,b.Up("Elixir Field","Elixir Burst"));}
  b.Add(b.Open,"Fire's Reply");b.Weave("Riddle of Wind");b.Add(b.Open,"Wind's Reply");
 }
 static void Dragoon(Book b){
  b.Threshold=b.L<40?null:3;var buff=b.Up("Disembowel","Spiral Blow");var hit=b.Up("Vorpal Thrust","Lance Barrage");var dot=b.Up("Chaos Thrust","Chaotic Spring");var fin=b.Up("Full Thrust","Heavens' Thrust");
  if(b.Aoe){b.Seq(b.Base,"Doom Spike","Sonic Thrust","Coerthan Torment");b.Second="APTITUDES À INSÉRER";}
  else{b.First="COMBO DU BONUS / DOT";b.Second="COMBO DE DÉGÂTS";b.Seq(b.Base,"True Thrust",buff,dot,"Wheeling Thrust","Drakesbane");b.Seq(b.Burst,"True Thrust",hit,fin,"Fang and Claw","Drakesbane");}
  b.Note=b.L>=64?"Alterner les deux combos en mono ; finir chaque chaîne. Le premier coup devient Percée Raiden après une chaîne complète au niv. 76.":"Entretenir le bonus puis utiliser le combo de dégâts ; les extensions se débloquent avec le niveau.";
  b.Rule("Life Surge","Garantir le critique du plus gros coup de combo disponible, pas du DoT.","Finisher de combo");
  b.Rule("Geirskogul","Utiliser dans le bonus de dégâts, puis les attaques de Vie du dragon. Insérer les sauts sans bloquer le prochain GCD.","À chaque recharge");
  b.Rule("Battle Litany","Aligner la Litanie avec les buffs du groupe ; Lance acérée accompagne aussi les fenêtres intermédiaires.","120 s");
  b.Rule("Wyrmwind Thrust","Dépenser les deux charges avant le prochain coup qui en génère une.","Deux charges");
  b.Rule("Starcross","Utiliser la suite après Plongeon céleste ; après Piqué du dragon, employer Vol du dragon au niv. 92.","Suites de sauts");
  b.Open.AddRange(b.Base);if(b.Open.Count>2){var item=b.Open[2];b.Open[2]=item with{Weaves=new[]{"Lance Charge","Battle Litany"}.Where(b.Has).Select(b.Id).ToArray()};}
  else b.Weave("Lance Charge","Battle Litany");
  b.Weave("Geirskogul");b.Add(b.Open,b.Aoe?"Doom Spike":"True Thrust");b.Weave(b.Up("Jump","High Jump"),"Dragonfire Dive");
  b.Add(b.Open,b.Aoe?"Sonic Thrust":hit);b.Weave("Life Surge","Nastrond");b.Add(b.Open,b.Aoe?"Coerthan Torment":fin);b.Weave("Stardiver");
  if(!b.Aoe)b.Seq(b.Open,"Fang and Claw","Drakesbane");b.Weave("Rise of the Dragon","Starcross");
 }
 static void Ninja(Book b){
  b.Threshold=b.L<38?null:b.L>=94?5:3;
  if(b.Aoe)b.Seq(b.Base,"Death Blossom","Hakke Mujinsatsu");else{b.Seq(b.Base,"Spinning Edge","Gust Slash");b.Add(b.Base,"Aeolian Edge",b.L>=54?"Arrière ; remplacer par Perce-armure (flanc) quand Kazematoi est vide":"Arrière de la cible");}
  b.Add(b.Burst,b.L>=35?(b.T>=3?"Katon":"Raiton"):"Fuma Shuriken",b.L>=35?(b.T>=3?"Chi → Ten → Ninjutsu":"Ten → Chi → Ninjutsu"):"Ten → Ninjutsu");
  b.Add(b.Burst,"Suiton","Ten → Chi → Jin → Ninjutsu : prépare l’attaque de dos à venir");
  b.Add(b.Burst,b.T>=2?"Goka Mekkyaku":"Hyosho Ranryu",b.T>=2?"Sous Kassatsu : Chi → Ten → Ninjutsu":"Sous Kassatsu : Ten → Jin → Ninjutsu");
  b.Note="Les cartes de ninjutsu représentent le résultat des signes indiqués. Ne pas insérer une autre action entre les mudras. Les procs Raiju doivent être utilisés avant de reprendre les coups normaux.";
  b.ThresholdDetail=b.L>=94?"À 3–4 cibles, le combo de zone devient intéressant sous Doton ou Bunshin. Katon à 3+, Goka et grenouille de zone à 2+.":"Katon dès 3 cibles ; Doton seulement si le pack reste dans la zone et vit assez longtemps.";
  b.Rule(b.Up("Trick Attack","Kunai's Bane"),"Préparer Suiton puis regrouper ninjutsu et attaques fortes dans la vulnérabilité. Ne pas tenter l’attaque sans sa condition.","Depuis niv. 45 : Suiton");
  b.Rule("Armor Crush","Générer Kazematoi lorsqu’il est vide ; ne pas dépasser le maximum. Dépenser avec Lame éolienne.","Flanc / arrière");
  b.Rule(b.L>=68&&b.T<2?"Bhavacakra":"Hellfrog Medium","Dépenser le Ninki sans dépasser 100. Réserver 50 pour Bunshin lorsque sa recharge approche.","50 Ninki");
  b.Rule("Doton","Sur pack immobile et durable ; ne pas utiliser sur une cible unique.","3+ cibles, durée utile");
  b.Rule("Fleeting Raiju","Après Raiton, consommer le proc avant un coup de combo normal.","Proc Raiju");
  b.Rule("Ten Chi Jin","Exécuter les trois ninjutsu sans bouger ; utiliser Tenri Jindo après la séquence au niv. 100.","Fenêtre immobile");
  b.Rule("Meisui","Après Suiton de Ten Chi Jin, convertir l’effet en Ninki ; garder assez de place dans la jauge.","50 Ninki générés");
  b.Rule("Phantom Kamaitachi","Consommer le coup obtenu après Bunshin pendant les buffs ou pour rester à distance.","Effet requis");
  b.Rule("Tenri Jindo","Insérer l’attaque après Ten Chi Jin avant expiration de l’effet.","Suite de burst");
  if(b.L>=45)b.Add(b.Open,"Suiton","Préparer les signes avant le pull, lancer au départ");
  b.Add(b.Open,b.Aoe?"Death Blossom":"Spinning Edge");b.Weave(b.Up("Mug","Dokumori"));
  b.Add(b.Open,b.Aoe?"Hakke Mujinsatsu":"Gust Slash");
  if(!b.Aoe)b.Add(b.Open,b.L>=54?"Armor Crush":"Aeolian Edge");b.Weave("Bunshin");
  b.Add(b.Open,b.Aoe?"Death Blossom":"Spinning Edge");if(b.L>=45)b.Weave(b.Up("Trick Attack","Kunai's Bane"),"Kassatsu");
  if(b.L>=76)b.Add(b.Open,b.T>=2?"Goka Mekkyaku":"Hyosho Ranryu","Kassatsu requis ; signes indiqués dans le cycle");
  else if(b.L>=30)b.Add(b.Open,b.L>=35?(b.T>=3?"Katon":"Raiton"):"Fuma Shuriken");
  if(b.L>=35){b.Add(b.Open,b.T>=3?"Katon":"Raiton");if(b.T<3)b.Add(b.Open,"Fleeting Raiju");}
  b.Weave(b.Up("Assassinate","Dream Within a Dream"));
 }
 static void Samurai(Book b){
  b.Threshold=b.L<26?null:3;var start=b.Up("Hakaze","Gyofu");
  if(b.Aoe){b.Add(b.Base,b.Up("Fuga","Fuko"));b.Add(b.Base,"Mangetsu","Alterner avec Ôka quand disponible ; obtenir deux Sen différents");}
  else{b.Seq(b.Base,start,"Jinpu","Gekko");if(b.L>=18)b.Seq(b.Base,start,"Shifu","Kasha");if(b.L>=50)b.Seq(b.Base,start,"Yukikaze");}
  b.Add(b.Burst,"Higanbana","Un Sen, cible durable ; renouveler près de l’expiration");
  if(b.Aoe&&b.L>=45)b.Add(b.Burst,"Tenka Goken","Deux Sen différents");else if(b.L>=50)b.Add(b.Burst,"Midare Setsugekka","Trois Sen différents");
  b.Add(b.Burst,b.Aoe?"Kaeshi: Goken":"Kaeshi: Setsugekka","Après l’iaijutsu correspondant ; avant de le remplacer");
  b.Note="Entretenir les deux bonus et obtenir des Sen différents. Meikyô Shisui permet des finishers directs ; au niv. 100, il prépare les iaijutsu Tendo renforcés.";
  b.Rule("Meikyo Shisui","Utiliser pour les finishers de combo ; privilégier ceux qui donnent les bonus et éviter d’écraser un Sen déjà obtenu.","Trois coups libres");
  b.Rule(b.T>=3&&b.L>=62?"Hissatsu: Kyuten":"Hissatsu: Shinten","Dépenser le surplus de Kenki sans déborder ; garder la ressource du gros coup à recharge.","25 Kenki");
  b.Rule("Ikishoten","Gagner du Kenki sans dépasser le maximum. Dès niv. 90, lancer Ogi namikiri puis sa répétition.","Burst de 120 s");
  b.Rule("Shoha","Utiliser après trois Méditations avant d’en générer une autre.","Trois charges");
  b.Rule("Zanshin","Utiliser la suite d’Ikishoten pendant sa fenêtre, avec assez de Kenki.","50 Kenki");
  if(b.L>=50&&!b.Aoe){
   b.Add(b.Open,"Meikyo Shisui","Préparation hors combat");b.Seq(b.Open,"Gekko","Higanbana","Kasha","Yukikaze",start,"Jinpu","Gekko",b.Up("Midare Setsugekka","Tendo Setsugekka"));
   b.Add(b.Open,b.Up("Kaeshi: Setsugekka","Tendo Kaeshi Setsugekka"));b.Weave("Ikishoten");b.Seq(b.Open,"Ogi Namikiri","Kaeshi: Namikiri");
  }else if(b.Aoe){b.Seq(b.Open,b.Up("Fuga","Fuko"),"Mangetsu",b.Up("Fuga","Fuko"),"Oka");if(b.L>=45)b.Add(b.Open,"Tenka Goken");}
  else b.Open.AddRange(b.Base);
 }
 static void Reaper(Book b){
  b.Threshold=b.L<25?null:3;
  b.Add(b.Base,b.Aoe&&b.L>=35?"Whorl of Death":"Shadow of Death","Entretenir le malus avant les dégâts");
  if(b.Aoe)b.Seq(b.Base,"Spinning Scythe","Nightmare Scythe");else b.Seq(b.Base,"Slice","Waxing Slice","Infernal Slice");
  b.Add(b.Burst,b.Aoe&&b.L>=65?"Soul Scythe":"Soul Slice","Génère 50 Âme : utiliser à 50 ou moins");
  b.Add(b.Burst,b.Aoe?"Grim Swathe":"Blood Stalk","50 Âme ; ne pas écraser les charges déjà obtenues");
  b.Add(b.Burst,b.Aoe?"Guillotine":"Gibbet",b.Aoe?"Dépenser la charge obtenue":"Dépenser la charge ; alterner Gibet (flanc) et Potence (arrière)");
  b.Note="Ne pas interrompre les coups de Faucheur d’âmes obtenus. Le burst de Linceul est une phase distincte ; revenir ensuite au combo.";
  b.Rule("Gluttony","Garder 50 Âme pour cette attaque puis consommer ses deux charges ; versions Exécuteur au niv. 96.","À chaque recharge");
  b.Rule("Enshroud",b.L>=90?"50 Linceul : alterner quatre attaques transformées, insérer la dépense de Lémures tous les deux coups, puis Communio.":"50 Linceul : cinq coups transformés ; alterner les versions mono ou répéter celle de zone.","Phase rapide");
  b.Rule("Arcane Circle","Regrouper le burst sous le bonus ; Récolte abondante devient utilisable après la préparation de 6 s au niv. 88.","120 s");
  b.Rule("Perfectio","Après Communio, utiliser la suite avant de reprendre les coups normaux.","Effet requis");
  b.Add(b.Open,b.Aoe&&b.L>=35?"Whorl of Death":"Shadow of Death","Soulsow préparée hors combat si disponible");
  if(b.L>=60){b.Add(b.Open,b.Aoe&&b.L>=65?"Soul Scythe":"Soul Slice");b.Weave("Arcane Circle",b.L>=76?"Gluttony":b.Aoe&&b.L>=55?"Grim Swathe":"Blood Stalk");}
  if(b.L>=70){b.Add(b.Open,b.Aoe?b.Up("Guillotine","Executioner's Guillotine"):b.Up("Gibbet","Executioner's Gibbet"));if(b.L>=76)b.Add(b.Open,b.Aoe?b.Up("Guillotine","Executioner's Guillotine"):b.Up("Gallows","Executioner's Gallows"));}
  if(b.L>=88){b.Add(b.Open,"Plentiful Harvest","Attendre 6 s après Cercle arcanique ; effet requis");b.Weave("Enshroud");
   for(var n=0;n<4;n++){b.Add(b.Open,b.Aoe?"Grim Reaping":n%2==0?"Void Reaping":"Cross Reaping");if(n==0)b.Weave("Sacrificium");if(n%2==1)b.Weave(b.Aoe?"Lemure's Scythe":"Lemure's Slice");}
   b.Add(b.Open,b.L>=90?"Communio":b.Aoe?"Grim Reaping":"Void Reaping");b.Add(b.Open,"Perfectio");
  }
  b.Open.AddRange(b.Base.Skip(b.Has("Shadow of Death")?1:0));
 }
 static void Viper(Book b){
  b.Threshold=b.L<25?null:3;
  if(b.Aoe){b.Add(b.Base,"Steel Maw",b.L>=35?"Alterner avec l’autre premier coup de zone selon le bonus":"Début du combo");b.Add(b.Base,"Hunter's Bite","Alterner avec le deuxième coup alternatif pour entretenir les deux bonus");b.Add(b.Base,"Jagged Maw","Choisir le finisher renforcé ; sinon l’autre finisher de zone");}
  else{b.Add(b.Base,"Steel Fangs",b.L>=10?"Alterner avec l’autre premier coup selon le bonus":"Début du combo");b.Add(b.Base,"Hunter's Sting","Alterner avec le deuxième coup alternatif pour les deux bonus");b.Add(b.Base,"Flanksting Strike","Finisher renforcé : flanc sur cette branche, arrière sur l’autre");}
  b.Add(b.Burst,b.Aoe?"Vicepit":"Vicewinder","Utiliser une charge avant d’atteindre le maximum");b.Add(b.Burst,b.Aoe?"Hunter's Den":"Hunter's Coil");b.Add(b.Burst,b.Aoe?"Swiftskin's Den":"Swiftskin's Coil");
  b.Note="Choisir le coup éclairé par les bonus du job, pas une unique chaîne figée. Insérer les suites de Crochets jumeaux et de Queue serpentine après le coup qui les rend disponibles.";
  b.Rule("Serpent's Tail","Utiliser la suite du finisher avant le prochain coup ; pendant l’Éveil, insérer l’Héritage correspondant au niv. 100.","Suite disponible");
  b.Rule("Uncoiled Fury","Dépenser les anneaux sans déborder ; dès niv. 92, suivre avec les deux aptitudes associées.","Anneau disponible");
  b.Rule("Reawaken","50 Offrandes ou effet de Communion ophidienne : quatre Générations dans l’ordre, puis Ouroboros dès niv. 96. Chaque Génération a sa suite au niv. 100.","Burst / éviter la saturation");
  b.Rule("Serpent's Ire","Utiliser pour la fenêtre de groupe ; fournit un Éveil sans coût et un anneau.","120 s");
  b.Rule("Reaving Fangs","Alterner avec Crochets d’acier quand le bonus correspondant est disponible.","Début du combo mono");
  b.Rule("Swiftskin's Sting","Choisir la branche qui entretient le bonus et mène au finisher renforcé.","Deuxième coup");
  b.Rule("Hindsting Strike","La branche arrière alterne avec la branche de flanc ; suivre le finisher renforcé, pas toujours la même icône.","Positionnel");
  b.Rule("Reaving Maw","En zone, même logique d’alternance du premier coup ; les deux bonus viennent du deuxième coup.","3+ cibles");
  if(b.L>=65){
   b.Add(b.Open,b.Aoe&&b.L>=70?"Vicepit":"Vicewinder");b.Weave("Serpent's Ire");
   b.Add(b.Open,b.Aoe&&b.L>=70?"Hunter's Den":"Hunter's Coil");b.Weave(b.Aoe&&b.L>=70?"Twinfang Thresh":"Twinfang Bite",b.Aoe&&b.L>=70?"Twinblood Thresh":"Twinblood Bite");
   b.Add(b.Open,b.Aoe&&b.L>=70?"Swiftskin's Den":"Swiftskin's Coil");b.Weave(b.Aoe&&b.L>=70?"Twinblood Thresh":"Twinblood Bite",b.Aoe&&b.L>=70?"Twinfang Thresh":"Twinfang Bite");
   if(b.L>=90){b.Add(b.Open,"Reawaken","Éveil gratuit accordé par Ire");string[] gens=["First Generation","Second Generation","Third Generation","Fourth Generation"];string[] legacy=["First Legacy","Second Legacy","Third Legacy","Fourth Legacy"];for(var n=0;n<4;n++){b.Add(b.Open,gens[n]);b.Weave(legacy[n]);}b.Add(b.Open,"Ouroboros");}
   b.Add(b.Open,"Uncoiled Fury","Anneau du combo ou de Communion ophidienne");b.Weave("Uncoiled Twinfang","Uncoiled Twinblood");
  }else b.Open.AddRange(b.Base);
 }
}
