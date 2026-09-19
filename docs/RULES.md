# Fiches et sources

Relevé le 19 septembre 2026. Les règles sont versionnées avec le plugin ; elles ne se mettent pas à jour depuis un site pendant le jeu.

## Mage noir — références

- [Guide officiel français](https://fr.finalfantasyxiv.com/jobguide/blackmage/) : noms, actions, niveaux et icônes.
- [Icy Veins, progression 7.55](https://www.icy-veins.com/ffxiv/black-mage-leveling) : paliers, variantes à deux cibles, fin de phase Brasier aux niveaux 50–59.
- [Icy Veins, rotation 7.55](https://www.icy-veins.com/ffxiv/black-mage-pve-dps-rotation-openers-abilities) : cycle au niveau 100, seuil de deux cibles et aptitudes.
- [The Balance, progression 7.4](https://www.thebalanceffxiv.com/jobs/casters/black-mage/leveling-guide/) : boucles de base et transitions.
- [The Balance, ouvertures](https://www.thebalanceffxiv.com/jobs/casters/black-mage/openers/) : ouverture standard 5+7. L’ordre des GCD est retranscrit ; les annotations expliquent les insertions à la main.
- [Métadonnées publiques XIVAPI](https://github.com/xivapi/ffxiv-datamining/blob/master/csv/fr/Action.csv) : identifiants d’actions. Le plugin utilise aussi la feuille locale Action pour résoudre les icônes.

## Périmètre pédagogique

Les fiches couvrent les niveaux 1–100. Elles présentent une boucle de base et des règles écrites, sans évaluer leur condition en jeu. Le niveau effectif suffit à sélectionner les actions ; leur déblocage par quête est supposé acquis.

La zone complète commence à trois ennemis regroupés pendant la progression, puis à deux au niveau 100. À ce dernier palier, Giga Glace remplace Gel pour deux cibles. Infect possède son propre seuil de deux cibles dès que Xénoglossie est disponible. Les variantes de Foudre à deux cibles suivent les paliers du guide de progression : zone aux niveaux 26–44 et 64–91, entretien séparé aux autres paliers avant 100.

La vue Ouverture montre le 5+7 au niveau 100 en mono. Pour les autres situations, elle propose un départ pédagogique à partir de la glace. Elle ne promet pas un opener optimisé pour chaque niveau, vitesse de sort, durée de combat ou composition de groupe. Le Paradoxe de glace n’existe pas au premier départ neutre. Infect et Foudre restent conditionnels dans les départs de zone.

La liste Priorités explique aussi les aptitudes. Elle n’impose pas un classement universel entre toutes les actions. Les variantes avancées et les ajustements aux buffs sont à retrouver dans les sources.

## Mage blanc — références et limites

- [Guide officiel français, 7.5](https://fr.finalfantasyxiv.com/jobguide/whitemage/) : actions et niveaux.
- [Icy Veins, progression 7.55](https://www.icy-veins.com/ffxiv/white-mage-leveling) et [dégâts 7.55](https://www.icy-veins.com/ffxiv/white-mage-dps-rotation-for-healers) : paliers, dégâts, ressources et seuils de zone.
- [The Balance, progression 7.3](https://www.thebalanceffxiv.com/jobs/healers/white-mage/leveling-guide/) : changements des sorts au fil des niveaux, recoupés avec les références 7.55.
- [The Balance, ouvertures 7.4](https://www.thebalanceffxiv.com/jobs/healers/white-mage/openers/) et son [schéma standard avec renouvellement anticipé du DoT](https://www.thebalanceffxiv.com/img/jobs/whm/white-mage-early-dot-refresh-opener.png) : séquence à haut niveau.
- [Métadonnées XIVAPI en anglais](https://github.com/xivapi/ffxiv-datamining/blob/master/csv/en/Action.csv) et en français : identifiants stables, libellés et paliers.

La boucle de dégâts entretient Vent / Extra Vent / Lumen sur les cibles durables, puis répète Terre et ses améliorations. Miracle devient disponible au niveau 45 : deux cibles suffisent jusqu’au niveau 71 ; à partir de 72, le seuil est trois cibles. À deux cibles au niveau 72 et au-delà, conserver les dégâts monocibles et entretenir le DoT sur chacune si elles vivent assez longtemps.

Le soin reste prioritaire selon les dégâts reçus ou prévus. Les rappels présentent les aptitudes, les Lys et la gestion des PM ; ils ne forment pas une rotation de soins obligatoire. Offrande de misère suppose un Lys de sang prêt et Giga Chatoiement suppose Présence d’esprit. Le guide ne lit pas ces ressources et ne prétend pas connaître leur disponibilité.

À partir du niveau 92 en mono, l’ouverture retranscrit le schéma standard The Balance : précast Méga Chatoiement, Lumen, deux Méga Chatoiement, Présence d’esprit, Giga Chatoiement et Assises, Offrande de misère, deux Giga Chatoiement, cinq Méga Chatoiement, puis Lumen. Le Lys de sang doit être prêt ; sinon l’étape Misère indique de revenir au sort de dégâts habituel. Les séquences de progression, à deux cibles et de pack sont des départs pédagogiques, pas des optimisations universelles. Le double Misère avancé n’est pas couvert.

## Langue et attribution

Les classes/jobs sont reconnus par leurs identifiants (occultiste 7, Mage noir 25, élémentaliste 6, Mage blanc 24), indépendamment de la langue du client. Noms et icônes sont résolus au chargement depuis la feuille Action locale. Les noms présents dans les annotations suivent aussi le client ; l’interface et les explications restent françaises. Les libellés français servent de repli si une ligne locale est absente. Les noms anglais sont également couverts par les contrôles hors jeu.

Chaque fiche affiche en petit Icy Veins 7.55, The Balance, la date de vérification et des liens adaptés au job et au contenu (progression ou ouverture), ainsi que le guide officiel. Le patch précis de chaque référence est détaillé ci-dessus. Le clic ouvre le site dans le navigateur ; le plugin ne télécharge pas de nouvelles règles en cours de jeu.

## Comportement de la proposition

La première lecture stable du niveau sert de référence, sans popup de connexion. Après un changement, attendre deux secondes de stabilité et la fin du combat, du chargement ou de la cinématique. Un guide déjà ouvert s’adapte. Sinon, proposer « Ouvrir le guide », « Pas maintenant » ou désactiver les propositions. Un refus vaut pour cette transition ; un nouveau changement peut donner une nouvelle proposition.

Une absence temporaire du personnage pendant le chargement conserve le niveau précédent. Déconnexion et changement de job réinitialisent la référence. Le mode Auto suit le job et le niveau effectif. Le mode Manuel conserve le niveau et le job choisis, fonctionne sans personnage disponible et suspend la proposition automatique. Niveau et cibles restent communs aux deux parties du guide.

Une seule fenêtre propose Les deux / Cycle / Ouverture. Les contenus sont côte à côte à grande largeur, empilés à petite largeur ; chaque colonne large défile indépendamment. Fermer les réglages laisse les commandes du guide accessibles. Le verrouillage concerne déplacement et redimensionnement, jamais les clics ni la fermeture. Les anciennes visibilités v1/v2 migrent vers la vue correspondante, sans réinitialiser les préférences fonctionnelles ; la nouvelle fenêtre peut devoir être repositionnée.

## Ressources et limites

Dans le jeu, les icônes sont chargées depuis les ressources locales par `ITextureProvider`. Les binaires ne contiennent aucun asset extrait. Pour les aperçus locaux seulement, `tools/fetch-icons.py` utilise les icônes des deux guides officiels ; ce cache et les rendus de travail sont ignorés par Git. Les icônes restent la propriété de Square Enix. Les captures documentaires montrent le panneau du plugin.

Expressway n’est pas fournie. Si un fichier local est trouvé, le guide peut l’utiliser avec un repli de glyphes ; sinon il utilise la police Dalamud. Les rendus hors jeu utilisent Segoe UI. Les réglages ont une présentation fixe indépendante de l’apparence du guide.

Pas de télémétrie, pas de lecture de jauges, de cibles ou d’historique de sorts. Les seules données de jeu exploitées sont le job, le niveau et les conditions qui permettent de différer ou masquer les fenêtres.
