namespace CycleOpener;
public static partial class JobGuides {
 static void Summoner(Book b){
  b.Threshold=b.L<26?null:3;
  var filler=b.Aoe?b.Up("Outburst","Tri-disaster"):b.Up("Ruin","Ruin II","Ruin III");
  var major=b.Up("Aethercharge","Dreadwyrm Trance","Summon Bahamut","Summon Solar Bahamut");
  b.First="INVOCATION MAJEURE";b.Second="TROIS INVOCATIONS ÉLÉMENTAIRES";
  b.Add(b.Base,major,"À chaque retour ; Carbuncle invoqué avant le combat");
  b.Add(b.Base,b.L>=100?(b.Aoe?"Umbral Flare":"Umbral Impulse"):b.L>=58?(b.Aoe?"Astral Flare":"Astral Impulse"):filler,"Répéter pendant la phase ; insérer les aptitudes de l’invocation");
  b.Add(b.Burst,b.Up("Summon Topaz","Summon Titan","Summon Titan II"),"Puis consommer toutes ses charges de gemme");
  b.Add(b.Burst,b.Up("Summon Emerald","Summon Garuda","Summon Garuda II"),"Puis consommer toutes ses charges de gemme");
  b.Add(b.Burst,b.Up("Summon Ruby","Summon Ifrit","Summon Ifrit II"),"Puis consommer toutes ses charges de gemme ; prévoir les incantations");
  b.Note="Après chaque invocation élémentaire, utiliser les charges de Gemme/Brillance adaptées au nombre de cibles. L’ordre Titan / Garuda / Ifrit est adaptable au déplacement ; reprendre l’invocation majeure dès son retour.";
  b.Rule(b.Aoe?"Precious Brilliance":"Gemshine","Utiliser toutes les charges de l’élément actif avant d’en invoquer un autre.",b.Aoe?"3+ cibles":"Phase élémentaire");
  b.Rule("Astral Flow",b.L>=86?"Titan : suite après chaque sort de gemme. Garuda : zone de vent. Ifrit : charge puis frappe au corps à corps ; vérifier la sécurité.":"Pendant la transe / Bahamut, utiliser l’attaque associée avant la fin de la phase.","Action transformée selon l’invocation");
  b.Rule("Energy Drain","Récupérer les charges puis les dépenser avec l’attaque appropriée, idéalement sous buffs. Variante de gain de zone dès 3 cibles.","Flux d’éther");
  b.Rule(b.Up("Fester","Necrotize"),"Dépenser les charges sans les écraser ; Suppuration est la variante pour plusieurs cibles.","Charge disponible");
  b.Rule("Searing Light","Aligner le bonus du groupe avec la phase majeure ; utiliser la suite au niv. 96.","120 s");
  b.Rule("Summon Phoenix",b.L>=100?"Alterner Soleil → Bahamut → Soleil → Phénix. Utiliser les sorts transformés et l’Enkindle du demi actif.":"Alterner Bahamut et Phénix ; leurs sorts remplacent les sorts de la phase majeure.","Alternance des demi-invocations");
  b.Add(b.Open,filler,"Carbuncle invoqué ; précast si possible");b.Weave("Energy Drain");b.Add(b.Open,major);b.Weave("Searing Light");
  var phase=b.L>=100?(b.Aoe?"Umbral Flare":"Umbral Impulse"):b.L>=58?(b.Aoe?"Astral Flare":"Astral Impulse"):filler;
  b.Add(b.Open,phase);b.Weave(b.L>=100?"Enkindle Solar Bahamut":"Enkindle Bahamut",b.T>=2&&b.L>=40?"Painflare":b.Up("Fester","Necrotize"));
  b.Add(b.Open,phase);b.Weave(b.L>=100?"Sunflare":"Deathflare",b.T>=2&&b.L>=40?"Painflare":b.Up("Fester","Necrotize"));
  b.Add(b.Open,phase);b.Weave("Searing Flash");
  b.Add(b.Open,phase,"Continuer jusqu’à la fin de l’invocation majeure");
  b.Add(b.Open,b.L>=15?b.Up("Summon Topaz","Summon Titan","Summon Titan II"):"Summon Ruby","Seulement après la fin de la phase majeure ; charges élémentaires disponibles");b.Add(b.Open,b.Aoe?"Precious Brilliance":"Gemshine","Consommer les charges puis passer à l’élément suivant");
 }
 static void RedMage(Book b){
  b.Threshold=b.L<15?null:3;var jolt=b.Up("Jolt","Jolt II","Jolt III");var thunder=b.Up("Verthunder","Verthunder III");var aero=b.Up("Veraero","Veraero III");
  if(b.L==1)b.Add(b.Base,"Riposte");
  else if(b.Aoe&&b.L>=18){b.Add(b.Base,"Verthunder II",b.L>=22?"Incantation courte : choisir Extra VerVent si la mana blanche est plus basse":"Incantation courte");b.Add(b.Base,b.Up("Scatter","Impact"),"Sous Double sort : instantané");}
  else if(b.Aoe)b.Add(b.Base,"Scatter","Avant les sorts courts de zone : répéter");
  else{b.Add(b.Base,jolt,b.L>=26?"Incantation courte ; préférer VerFeu / VerTerre si le proc est présent":"Incantation courte");b.Add(b.Base,b.L>=4?thunder:jolt,b.L>=10?"Double sort : choisir VerVent si la mana blanche est plus basse":"Avec Double sort si disponible");}
  if(b.Aoe&&b.L>=52)b.Seq(b.Burst,"Enchanted Moulinet","Enchanted Moulinet Deux","Enchanted Moulinet Trois");else if(b.L>=2)b.Seq(b.Burst,"Enchanted Riposte","Enchanted Zwerchhau","Enchanted Redoublement");
  b.Add(b.Burst,b.L>=70?"Verholy":"Verflare","Après trois gemmes : choisir le finisher qui équilibre la mana");b.Seq(b.Burst,"Scorch","Resolution");
  b.Note=b.L>=50?"La chaîne enchantée demande 50 mana noire et blanche, ou les charges gratuites de Manafication. Garder l’écart entre les deux couleurs sous 30.":b.L>=35?"La chaîne enchantée à deux coups demande 35 de chaque mana. Avant les finishers, reprendre ensuite les incantations.":b.L>=2?"Riposte enchantée demande 20 de chaque mana ; ne pas utiliser la version non enchantée à la place des sorts.":"Riposte est le seul coup à ce niveau ; la génération de mana commence avec les sorts du niveau 2.";
  b.Rule("Verfire","Consommer les procs comme incantation courte à la place d’À-coup, en équilibrant les couleurs ; ne jamais incanter lentement les grands sorts en boucle.","Double sort");
  b.Rule("Verstone","Même priorité que VerFeu : dépenser le proc utile pour équilibrer les deux couleurs.","Proc disponible");
  b.Rule("Veraero II","En zone, alterner les deux sorts courts selon la mana manquante, puis lancer le grand sort de zone avec Double sort.","3+ cibles");
  b.Rule("Manafication","Lancer une chaîne enchantée gratuite avec les charges reçues ; éviter d’interrompre un combo déjà commencé.","Burst");
  b.Rule("Embolden","Aligner la chaîne enchantée et ses finishers avec le bonus du groupe.","120 s");
  b.Rule("Fleche","Utiliser les aptitudes de dégâts à recharge dans les fenêtres instantanées, sans décaler le prochain sort.","Inclut Contre de sixte");
  b.Rule("Acceleration","Préparer un instantané et son proc ; dès niv. 96, consommer Grand impact avant de remplacer l’effet.","Déplacement / burst");
  b.Rule("Prefulgence","Après consommation des charges de Manafication, insérer la suite. Enhardissement donne aussi Épine croisée au niv. 92.","Effet requis");
  b.Add(b.Open,b.L>=4?thunder:b.L>=2?jolt:"Riposte","Précast long en mono, puis Double sort");b.Add(b.Open,b.L>=10?aero:b.L>=2?jolt:"Riposte");b.Weave("Fleche","Contre Sixte");
  b.Add(b.Open,jolt);b.Add(b.Open,b.L>=4?thunder:jolt);b.Weave("Embolden","Manafication");
  if(b.L>=60){b.Open.AddRange(b.Burst);b.Weave("Vice of Thorns","Prefulgence");}
 }
 static void Pictomancer(Book b){
  b.Threshold=b.L<25?null:b.L is >=54 and <74?4:3;
  b.First="COULEURS ADDITIVES";b.Second="PALETTE SOUSTRACTIVE";
  b.Add(b.Base,b.Aoe?"Fire II in Red":"Fire in Red");
  b.Add(b.Base,b.Aoe&&b.L>=35?"Aero II in Green":"Aero in Green");b.Add(b.Base,b.Aoe&&b.L>=45?"Water II in Blue":"Water in Blue");
  var subArea=b.T>=3;
  b.Add(b.Burst,"Subtractive Palette","50 Palette ou effet gratuit de Imagi Ciel");
  b.Seq(b.Burst,subArea?"Blizzard II in Cyan":"Blizzard in Cyan",subArea?"Stone II in Yellow":"Stone in Yellow",subArea?"Thunder II in Magenta":"Thunder in Magenta");
  b.Add(b.Burst,"Comet in Black","Peinture noire disponible après la palette");
  b.Note="Dessiner les motifs hors combat ou pendant les pauses ; les muses les consomment. Les ressources conditionnent les couleurs soustractives et la peinture.";
  b.ThresholdDetail="Palette soustractive dès 3 cibles. Les muses, le marteau et la peinture restent prioritaires en zone. Paliers 74+ recoupés avec les puissances officielles 7.5.";
  b.Rule("Living Muse","Consommer les motifs de créature sans saturer les charges, puis redessiner le suivant. Utiliser le portrait obtenu après les créatures nécessaires.","Motif préparé");
  b.Rule("Steel Muse","Préparer le motif d’arme, activer la muse, puis consommer les trois coups de marteau. Prévoir leur usage pour les déplacements.","30 s pour les coups");
  b.Rule("Starry Muse","Préparer le paysage avant le combat. Placer les attaques fortes et la palette gratuite dans la fenêtre.","120 s");
  b.Rule("Holy in White","Dépenser la peinture pour bouger ; en zone, utiliser après les muses. La Comète remplace une charge lorsque la peinture noire est prête.","Peinture disponible");
  b.Rule("Star Prism","Consommer après Imagi Ciel. Les cinq sorts accélérés donnent aussi un Arc-en-ciel instantané.","Effets requis");
  b.Add(b.Open,b.L>=92?"Rainbow Drip":"Fire in Red","Motifs créature, arme et paysage préparés si disponibles ; précast");b.Weave("Pom Muse");
  if(b.L<92)b.Seq(b.Open,"Aero in Green","Water in Blue");b.Weave("Starry Muse");
  if(b.L>=70){b.Add(b.Open,"Subtractive Palette","Palette gratuite de Imagi Ciel");b.Seq(b.Open,subArea?"Blizzard II in Cyan":"Blizzard in Cyan",subArea?"Stone II in Yellow":"Stone in Yellow",subArea?"Thunder II in Magenta":"Thunder in Magenta");}
  b.Weave("Striking Muse");b.Add(b.Open,"Star Prism");b.Add(b.Open,"Comet in Black");
  b.Add(b.Open,"Hammer Stamp","Trois coups sous la muse ; avant niv. 86 répéter ce coup",b.L>=50&&b.L<86?3:1);b.Seq(b.Open,"Hammer Brush","Polishing Hammer");b.Open.AddRange(b.Base);
 }
}
