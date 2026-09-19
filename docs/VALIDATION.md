# Validation locale — 0.1.1

19 septembre 2026. Compilation locale réussie, **aucun essai FFXIV effectué**. Ces contrôles ne prouvent pas que la DLL est chargée dans le jeu.

## Reproduire

Prérequis : Windows, SDK .NET 10.0.400, assemblies Dalamud API 15 dans le répertoire Hooks/dev de XIVLauncher. Aucun package NuGet externe requis pour le plugin ou les contrôles métier.

```powershell
./build.ps1
```

Sur ce poste, le SDK partagé a été explicitement sélectionné :

```powershell
./build.ps1 -Dotnet ../.tools/dotnet/dotnet.exe
./tools/render.ps1 -Dotnet ../.tools/dotnet/dotnet.exe
python -m http.server 8742 --bind 127.0.0.1 --directory preview
```

`-DalamudHome` permet de fournir un autre répertoire d’assemblies. Le SDK partagé n’est pas une dépendance du dépôt. Les rendus nécessitent également Python, Pillow, beautifulsoup4, une connexion au guide officiel pour son cache local d’icônes et le cimgui.dll fourni par Dalamud. Le plugin compilé n’utilise pas ces outils.

## Résultats

- Compilation Release : zéro erreur, zéro avertissement. DLL version 0.1.1.0.
- 12 005 assertions : disponibilité des sorts sur tous les niveaux 1–100 et 1–8 cibles, séquences représentatives, paliers de Foudre, ouverture et politique de popup. Ce chiffre compte les invariants parcourus, pas des scénarios manuels indépendants.
- 327 rendus natifs ImGui produits par les mêmes composants que le plugin. La matrice comprend huit niveaux, trois profils de cibles et les cinq vues, y compris la largeur du duo ; largeur minimale, barre du panneau, réglages et popup à 100/150/200 % ; étapes de l’ouverture. Aucun défilement horizontal détecté dans cette matrice.
- Inspection visuelle des cinq vues principales, de variantes mono/multi, de largeurs minimales, de réglages et de popup. Vérification d’échantillons à 150/200 %, accents et retours à la ligne. Le rasteriseur hors jeu utilise un filtrage bilinéaire et toutes les pages de l’atlas de police.
- Comparateur servi sur localhost : changements de vue, niveau et cibles, parcours de l’ouverture, duo cycle/priorités + ouverture et simulation niveau 50 acceptée/refusée. Défilement indépendant des deux fiches à largeur réduite. Aucune erreur ou alerte JavaScript remontée pendant ces parcours.

Les images sont dans `preview/renders/`, les métriques dans `.artifacts/render-metrics.json`. Le navigateur affiche les images du rendu natif ; son popup de simulation est en HTML. Cela valide la proposition visuelle et la navigation du comparateur, pas les clics natifs dans FFXIV.

## Livrables

`build.ps1` copie et vérifie la DLL dans `plugin/` et `releases/0.1.1/`, avec le manifeste, l’icône originale et la licence. Le ZIP utilise une liste explicite de ces quatre fichiers et ses SHA256 sont écrits dans `releases/SHA256SUMS.txt`. Seul `plugin/CycleOpener.dll` est le chemin stable à renseigner dans Dev Plugin Locations. Conserver une seule entrée active pour ce plugin. Le chemin de l’assembly réellement chargé est disponible dans Diagnostic et dans le journal du plugin.

## À confirmer en jeu

Chargement/déchargement API 15, icônes locales, police Dalamud et Expressway si installée, interaction avec les étapes, fermeture/recentrage/verrouillage des deux panneaux, échelle globale, sauvegarde puis rechargement des réglages et de leurs positions indépendantes. Vérifier les changements de job, déconnexion, entrée/sortie de donjon et synchronisation réelle, popup différée après chargement/combat/cinématique, refus et désactivation persistante. Vérifier que l’apparence du guide n’affecte jamais les réglages.

Les contrôles statiques ne certifient ni l’optimalité de toutes les rotations ni l’intégration native. Les nouvelles valeurs de niveaux ou changements de patch nécessitent une révision des sources.
