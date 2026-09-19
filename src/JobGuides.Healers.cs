namespace CycleOpener;
public static partial class JobGuides {
 static void Scholar(Book b){
  b.Threshold=b.L<46?null:2;
  var filler=b.Up("Ruin","Broil","Broil II","Broil III","Broil IV");var dot=b.Up("Bio","Bio II","Biolysis");var aoe=b.Up("Art of War","Art of War II");
  var dotLimit=b.L>=94?4:b.L>=82?3:b.L>=72?4:2;
  if(b.T<=dotLimit)b.Add(b.Base,dot,"Sur chaque cible qui vivra assez longtemps ; renouveler à expiration");
  b.Add(b.Base,b.Aoe?aoe:filler,"Répéter entre les soins nécessaires");
  b.Add(b.Burst,"Baneful Impaction","Après Stratagèmes entrelacés ; avant expiration");
  b.Note="Invoquer la fée avant le combat. Privilégier ses soins et les aptitudes, puis les soins incantés si nécessaires ; les dégâts passent après la survie.";
  b.ThresholdDetail=$"DoT sur 1 à {dotLimit} cibles durables ; au-delà, privilégier le sort de zone.";
  b.Rule("Aetherflow","Récupérer les charges en combat sans en perdre ; réserver les charges nécessaires aux soins.","À chaque recharge");
  b.Rule("Energy Drain","Dépenser uniquement les charges qui ne seront pas utiles aux soins ; avant le prochain gain de charges.","Surplus de Flux d’éther");
  b.Rule("Chain Stratagem","Placer le bonus sur la cible durable pendant le burst du groupe, puis utiliser la suite au niv. 92.","120 s");
  b.Rule("Lucid Dreaming","Récupérer les PM régulièrement, sans attendre d’être à sec.","PM manquants");
  b.Add(b.Open,b.Aoe?aoe:filler,"Fée déjà invoquée ; précast en mono");b.Weave("Aetherflow");
  if(b.T<=dotLimit)b.Add(b.Open,dot);b.Add(b.Open,b.Aoe?aoe:filler);b.Weave("Chain Stratagem");
  b.Add(b.Open,b.Aoe?aoe:filler);b.Weave("Baneful Impaction");
  b.Add(b.Open,b.Aoe?aoe:filler,"Reprendre les priorités ; Drain d’énergie seulement avec un surplus de charges");
  b.Rule("Summon Eos","Garder la fée invoquée avant le départ ; ses soins accompagnent ceux du groupe.","Préparation",true);
  b.Rule("Whispering Dawn","Utiliser les soins de fée pour les dégâts réguliers ; conserver les gros outils pour les pics.","Soins gratuits",true);
  b.Rule("Lustrate","Employer une charge pour remonter une cible en danger ; ne pas sacrifier le soin pour Drain d’énergie.","Charge de Flux d’éther",true);
  b.Rule("Adloquium","Préparer le bouclier avant le combat ou des dégâts dangereux ; compléter si les aptitudes ne suffisent pas.","Bouclier",true);
  b.Rule("Recitation","Rendre gratuit et critique un soin compatible ; planifier avec Traité de l’excogitation ou de l’indomitabilité.","Soin planifié",true);
  b.Rule("Seraphism","Renforcer la réponse aux dégâts prolongés ; employer les sorts transformés selon les besoins.","Urgence / soins soutenus",true);
 }
 static void Astrologian(Book b){
  b.Threshold=b.L<45?null:2;
  var filler=b.Up("Malefic","Malefic II","Malefic III","Malefic IV","Fall Malefic");var dot=b.Up("Combust","Combust II","Combust III");var aoe=b.Up("Gravity","Gravity II");
  if(!b.Aoe)b.Add(b.Base,dot,"Entretenir sur les cibles durables");b.Add(b.Base,b.Aoe?aoe:filler,"Répéter entre les soins");
  b.Add(b.Burst,"Oracle","Après Divination ; avant expiration");
  b.Note="Les cartes de dégâts vont aux partenaires adaptés ; les autres cartes et les soins répondent à la situation. La survie reste prioritaire.";
  b.Rule("Astral Draw","Alterner les deux pioches. Donner la Balance à un DPS de mêlée, l’Épieu à un DPS à distance ; jouer avant de repiocher.","Cartes de dégâts");
  b.Rule("Divination","Aligner le bonus du groupe et les cartes de dégâts ; utiliser Oracle au niv. 92.","Burst de 120 s");
  b.Rule("Earthly Star","Placer l’étoile en avance là où resteront alliés et ennemis. Attendre son renforcement si le soin peut attendre.","Préparation / dégâts et soin");
  b.Rule("Lord of Crowns","Utiliser la carte offensive obtenue par la pioche astrale sans l’écraser à la prochaine pioche.","Carte disponible");
  b.Rule("Lucid Dreaming","Utiliser régulièrement quand les PM baissent. Continuer les pioches qui rendent aussi des PM.","Gestion des PM");
  b.Add(b.Open,b.Aoe?aoe:filler,"Étoile et pioche préparées hors combat si disponibles");
  if(!b.Aoe)b.Add(b.Open,dot);b.Add(b.Open,b.Aoe?aoe:filler);b.Weave("Divination","The Balance");
  b.Add(b.Open,b.Aoe?aoe:filler);b.Weave("Oracle","Lord of Crowns");
  b.Rule("Essential Dignity","Remonter rapidement une cible blessée ; ne pas attendre une valeur de vie dangereuse.","Soin monocible",true);
  b.Rule("Celestial Opposition","Soigner le groupe entre deux sorts de dégâts ; employer les soins incantés si nécessaire.","Groupe",true);
  b.Rule("Aspected Benefic","Entretenir le tank exposé à des dégâts réguliers ; utiliser un soin direct si le danger est immédiat.","Soin sur la durée",true);
  b.Rule("Macrocosmos","Préparer avant une série de dégâts puis récupérer les soins au bon moment.","Dégâts prévus",true);
  b.Rule("Sun Sign","Après Secte neutre, protéger le groupe avant des dégâts importants.","Effet requis",true);
  b.Rule("Benefic", "À bas niveau, soigner dès que nécessaire puis reprendre les dégâts.","Soins",true);
 }
 static void Sage(Book b){
  b.Threshold=b.L<46?null:b.L>=94?3:2;
  var filler=b.Up("Dosis","Dosis II","Dosis III");var dot=b.Up("Eukrasian Dosis","Eukrasian Dosis II","Eukrasian Dosis III");var aoe=b.Up("Dyskrasia","Dyskrasia II");var phlegma=b.Up("Phlegma","Phlegma II","Phlegma III");
  if(b.L>=30){b.Add(b.Base,"Eukrasia","Pour renouveler le DoT uniquement");b.Add(b.Base,b.T>=2&&b.L>=82?"Eukrasian Dyskrasia":dot,"Sur cibles durables ; ne pas superposer les deux DoT");}
  b.Add(b.Base,b.Aoe?aoe:filler,"Répéter jusqu’au prochain besoin de soin ou renouvellement");
  b.Add(b.Burst,phlegma,"Dépenser les charges sans en perdre, idéalement sous buffs");
  b.Add(b.Burst,b.Up("Toxikon","Toxikon II"),"Charge déjà disponible ; pour bouger ou en zone");
  b.Note="Placer Kardia sur le tank avant le combat. Eukrasia est une étape à part entière, pas une aptitude à intercaler.";
  b.ThresholdDetail="Phlegma et Pneuma utiles en zone ; le DoT de zone remplace celui de la cible, il ne s’y ajoute pas.";
  b.Rule(phlegma,"Éviter de rester au maximum de charges ; placer les utilisations possibles sous les buffs.","Charge disponible");
  b.Rule("Psyche","Utiliser l’aptitude offensive à chaque recharge, en commençant sous les buffs du groupe.","60 s");
  b.Rule(b.Up("Toxikon","Toxikon II"),"Dépenser les charges déjà obtenues ; ne pas lancer des boucliers en combat uniquement pour en générer.","Déplacement / zone");
  b.Rule("Lucid Dreaming","Utiliser quand les PM baissent ; les soins d’Addersgall rendent aussi des PM.","Gestion des PM");
  b.Add(b.Open,b.Aoe?aoe:filler,"Kardia et bouclier de préparation si utile ; précast en mono");
  if(b.L>=30){b.Add(b.Open,"Eukrasia");b.Add(b.Open,b.T>=2&&b.L>=82?"Eukrasian Dyskrasia":dot);}
  b.Add(b.Open,b.Aoe?aoe:filler);b.Add(b.Open,phlegma,"Première charge");b.Weave("Psyche");b.Add(b.Open,phlegma,"Seconde charge disponible");b.Add(b.Open,b.Aoe?aoe:filler,"Reprendre les priorités");
  b.Rule("Kardia","Lier au tank ; les sorts de dégâts déclenchent les petits soins.","Avant le combat",true);
  b.Rule("Druochole","Soigner avec les charges en gardant une réponse aux prochains dégâts ; éviter de saturer la jauge.","Addersgall",true);
  b.Rule("Kerachole","Réduire les dégâts à venir ; dès niv. 78, ajoute une régénération. Sa mitigation ne s’additionne pas à Taurochole.","Groupe / tank",true);
  b.Rule("Haima","Protéger le tank face aux coups répétés ; alterner les grandes protections entre les packs.","Dégâts répétés",true);
  b.Rule("Pneuma","En mono, réserver pour son soin de groupe ; en zone, dégâts et soin peuvent être combinés. Renforcer avec Zoe si utile.","Soin et dégâts",true);
  b.Rule("Diagnosis","Compléter avec les soins incantés lorsque les aptitudes disponibles ne suffisent pas.","Survie prioritaire",true);
  b.Rule("Philosophia","Prévoir une période de dégâts prolongés où le groupe profitera des soins déclenchés par les sorts.","Soins soutenus",true);
 }
}
