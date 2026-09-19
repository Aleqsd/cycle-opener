# Fiches et sources

Relevé le 19 septembre 2026. Les règles sont versionnées avec le plugin ; elles ne se mettent pas à jour depuis un site pendant le jeu.

## Références

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

## Comportement de la proposition

La première lecture stable du niveau sert de référence, sans popup de connexion. Après un changement, attendre deux secondes de stabilité et la fin du combat, du chargement ou de la cinématique. Les panneaux déjà ouverts s’adaptent ensemble. Sinon, proposer « Afficher le guide », « Pas maintenant » ou désactiver les propositions. Accepter ouvre aussi l’ouverture si l’option est active. Un refus vaut pour cette transition ; un nouveau changement peut donner une nouvelle proposition.

Une absence temporaire du personnage pendant le chargement conserve le niveau précédent. Une déconnexion ou un changement vers un autre job réinitialise la référence. Le niveau manuel permet de consulter une fiche librement ; seul le suivi automatique utilise le niveau du personnage.

## Ressources et limites

Dans le jeu, les icônes sont chargées depuis les ressources locales par `ITextureProvider`. Les binaires ne contiennent aucun asset extrait. Pour les aperçus locaux seulement, `tools/fetch-icons.py` utilise les icônes du guide officiel ; ce cache et les rendus de travail sont ignorés par Git. Les icônes restent la propriété de Square Enix. Les captures documentaires montrent le panneau du plugin.

Expressway n’est pas fournie. Si un fichier local est trouvé, le guide peut l’utiliser avec un repli de glyphes ; sinon il utilise la police Dalamud. Les rendus hors jeu utilisent Segoe UI. Les réglages ont une présentation fixe indépendante de l’apparence du guide.

Pas de réseau pendant l’utilisation du plugin, pas de télémétrie, pas de lecture de jauges, de cibles ou d’historique de sorts. Les seules données de jeu exploitées sont le job, le niveau et les conditions qui permettent de différer ou masquer les fenêtres.
