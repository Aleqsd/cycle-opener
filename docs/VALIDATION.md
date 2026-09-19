# Validation locale — 0.2.1

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

- Compilation Release : zéro erreur, zéro avertissement. DLL version 0.2.1.0.
- 24 704 assertions métier : niveaux 1–100 et 1–8 cibles pour les deux jobs, disponibilité des sorts, paliers, seuils WHM à 45/72, séquences d’ouverture, noms anglais et annotations, repli français, liens des sources et politique de popup : sortie pendant chargement, retour en ville, autre téléportation, synchronisation en monde ouvert, hausse de niveau et retour dans un autre donjon. Ce chiffre compte les invariants parcourus, pas des scénarios manuels indépendants.
- 144 assertions de panneaux : migration des 40 combinaisons v1/v2 vers une seule fenêtre, conservation des préférences, rechargement, sélection manuelle du job/niveau et indépendance par rapport au niveau synchronisé. Clics dans un contexte natif ImGui : Auto/Manuel, niveau + et borne 100, mode hors ligne, fermeture du guide verrouillé sans réglages, bouton des réglages, cibles, Les deux/Ouverture, navigation de l’ouverture et croix native de fermeture.
- Référence visuelle 0.2.0 conservée : 201 rendus natifs ImGui issus des composants du plugin : cinq niveaux, trois profils de cibles et cinq vues pour chacun des deux jobs ; fenêtre complète, largeur minimale, noms anglais/français, réglages et popup à 100/150/200 %. Aucun défilement horizontal détecté dans cette matrice.
- En 0.2.1 : 20 rendus des réglages régénérés aux échelles 100/150/200 % après le changement de libellé. Les 144 contrôles de panneaux passent ; aucun défilement horizontal dans ces rendus.
- Inspection visuelle : fenêtre unifiée des deux jobs, noms anglais, sources en pied de fiche, largeur minimale manuelle à 150 % et 200 %, réglages normaux et apparence à 200 %, fiches BLM multicibles et WHM de progression. Les captures utilisent Segoe UI, pas Expressway ni la police du client.
- Comparateur local vérifié dans le navigateur intégré : bascule Mage blanc/client anglais, largeur minimale à 200 %, navigation vers les cinq fiches. Images chargées et libellés concordants. Ces contrôles web portent sur la galerie, pas sur les clics du plugin.

Les contrôles des boutons utilisent le code des panneaux et cimgui, hors jeu, avant chaque génération des images. Rendus dans `preview/renders/`, métriques dans `.artifacts/render-metrics.json`. La page locale présente ces images ; ses contrôles permettent de choisir une capture, sans simuler un personnage connecté.

Les sources du Mage blanc ont été recoupées avec les guides officiel, Icy Veins et The Balance. Le schéma d’ouverture standard a été inspecté visuellement. Les règles restent une aide pédagogique statique et ne sont pas une certification d’optimalité de chaque situation.

## Livrables

`build.ps1` copie et vérifie la DLL dans `plugin/` et `releases/0.2.1/`, avec le manifeste, l’icône originale et la licence. Le ZIP utilise une liste explicite de ces quatre fichiers et ses SHA256 sont écrits dans `releases/SHA256SUMS.txt`. Seul `plugin/CycleOpener.dll` est le chemin stable à renseigner dans Dev Plugin Locations. Conserver une seule entrée active pour ce plugin. Le chemin de l’assembly réellement chargé est disponible dans Diagnostic et dans le journal du plugin.

## À confirmer en jeu

Chargement/déchargement API 15, icônes locales, police Dalamud et Expressway si installée, interaction avec les étapes, fermeture/recentrage/verrouillage du guide, touche Échap après fermeture des réglages, échelle globale, sauvegarde puis rechargement des réglages et de la position du guide. Vérifier les clients français et anglais avec leurs ressources locales. Expressway n’est pas installée sur le poste de validation. Vérifier les changements de job, déconnexion, entrée/sortie de donjon et synchronisation réelle, absence de popup en ville et dans les transitions après sortie, popup différée après chargement/combat/cinématique, refus et désactivation persistante. Vérifier que l’apparence du guide n’affecte jamais les réglages.

Les contrôles statiques ne certifient ni l’optimalité de toutes les rotations ni l’intégration native. Les nouvelles valeurs de niveaux ou changements de patch nécessitent une révision des sources.
