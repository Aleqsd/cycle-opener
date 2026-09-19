# Validation locale — 1.1.0

19 septembre 2026. Compilation locale réussie, **aucun essai FFXIV effectué pour cette version**. Ces contrôles ne prouvent pas que la DLL est chargée dans le jeu.

## Reproduire

Windows, SDK .NET 10.0.400, assemblies Dalamud API 15 dans le répertoire Hooks/dev de XIVLauncher. Aucun package NuGet externe requis pour le plugin ou les contrôles métier.

```powershell
./build.ps1
```

Commandes vérifiées sur le poste de développement :

```powershell
python tools/import-actions.py
./build.ps1 -Dotnet ../.tools/dotnet/dotnet.exe
./tools/render.ps1 -Dotnet ../.tools/dotnet/dotnet.exe
python -m http.server 8742 --bind 127.0.0.1 --directory preview
```

`-DalamudHome` accepte un autre répertoire d’assemblies. Le SDK partagé est facultatif. Les rendus utilisent Python et Pillow pour leur cache local d’icônes XIVAPI, puis cimgui.dll fourni par Dalamud. Le plugin compilé n’utilise pas ces outils ni le réseau. Les imports sont versionnés et reproductibles depuis la révision épinglée, avec empreintes des CSV.

## Résultats

- Release : zéro erreur, zéro avertissement ; assembly **1.1.0.0**.
- **520 217 assertions métier** : 81 903 contrôles existants/généralisés, 424 047 contrôles des jobs importés et 14 267 contrôles des prérequis (coûts, alternatives gratuites selon le niveau, effets et noms anglais). Parcours des 21 jobs × niveaux 1–100 × 1–8 cibles ; actions et aptitudes débloquées, listes non vides, classes de départ, valeurs sauvegardées BLM/WHM conservées, exclusion du Mage bleu, noms anglais, paliers de zone et regroupement des étapes. Ce nombre compte les invariants parcourus, pas autant de scénarios indépendants.
- Régressions ciblées : pas de cartouche inventée pour le combo de zone GNB au niveau 30 ; Faucheur bas niveau commençant par le premier coup ; Pictomancien rouge/vert/bleu ; Tendo au niveau 100 seulement ; surchauffe MCH préparée avant les tirs au niveau 66 ; pas d’attaque enchantée RDM au niveau 1 avant la génération de mana.
- **196 assertions natives de panneaux et commandes** : migrations v1/v2/v3 vers les deux contenus, état ouvert/fermé et préférences conservés, favoris sauvegardés et assainis, niveaux et cibles, fermeture verrouillée/repliée, styles restaurés. Clics réels cimgui sur les filtres Tanks/Favoris, ajout et retrait de Paladin, sélection et fermeture du menu. Le vrai callback de commande est exécuté sans les services Dalamud : commande vide/show, hide, config/cfg/setup, espaces et majuscules ; ouverture et dépliage du guide, réglages indépendants.
- **2 285 vues ImGui** dans la matrice : 21 jobs, cinq niveaux, trois profils de cibles, cinq affichages ; guides français/anglais, largeur normale/minimale, hauteur minimale, soins/protections, repli, verrouillage, réglages, sélecteur de jobs ouvert et popup à 100/150/200 %. Aucun défilement horizontal détecté. Les 447 vues de guide sont régénérées après le dernier changement des icônes ; le sélecteur est vérifié ouvert, avec favoris et avec une liste vide.
- Inspection visuelle : guides Pictomancien, Mage blanc, Pistosabreur et Mage rouge anglais, Chevalier dragon anglais à largeur minimale et échelle 200 %, réglages Astromancien à 150 %, sélecteurs Tanks/Favoris/vide et cadenas fermé à 200 %. Les prérequis longs passent à la ligne ; les commandes restent accessibles. Police d’aperçu Segoe UI ; Expressway absente.
- Galerie locale : titre 1.1.0, 21 choix, sélection Pictomancien → anglais puis Chevalier dragon → minimum → 200 %. La capture et les sources changent avec le job. Contrôle Playwright à 1440 × 1080 et 390 × 844 : page non vide, images chargées, pas de débordement horizontal, pas d’overlay d’erreur, console sans erreur. Le plugin Browser n’étant pas disponible, les outils Playwright ont été utilisés. La galerie ne teste pas les interactions en jeu.

Les métriques locales se trouvent dans `.artifacts/render-metrics.json`, les images dans `preview/renders/`. Ces caches de travail sont ignorés par Git. Le README contient deux captures actualisées du code ImGui hors jeu.

Les règles et seuils sont une adaptation pédagogique des [sources par job](JOBS.md), avec des contrôles d’actions officiels et des régressions de ressources. Les tests ne simulent pas tout le moteur de combat et ne certifient pas l’optimalité des rotations.

## Livrables

`build.ps1` copie la même DLL vérifiée dans `plugin/` et `releases/1.1.0/`. Le ZIP contient exactement quatre fichiers : DLL, manifeste, icône originale, licence. Aucune texture de jeu, police, assembly Dalamud, donnée locale ou dépendance de build. Les empreintes sont dans `releases/SHA256SUMS.txt`.

Seul `plugin/CycleOpener.dll` est le chemin stable à renseigner dans Dev Plugin Locations. Garder une seule installation active. Le chemin de l’assembly effectivement chargé reste consultable dans Diagnostic et dans le journal du plugin.

## À confirmer en jeu

Chargement/déchargement API 15, ressources locales FR/EN, police Dalamud et Expressway, clics/Échap après fermeture des réglages, déplacement, redimensionnement et restauration de position. Vérifier les changements de job, reconnexion, synchronisation réelle, entrée/sortie de donjon, refus et désactivation de la proposition, popup différée après chargement/combat/cinématique et absence de popup en ville. Vérifier que l’apparence du guide laisse les réglages fixes.

L’intégration native et les essais personnels en combat restent à effectuer. Les changements de patch nécessitent une révision des fiches et de leurs sources.
