# Validation locale — 0.3.0

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

- Compilation Release : zéro erreur, zéro avertissement. DLL version 0.3.0.0.
- 29 065 assertions métier : niveaux 1–100 et 1–8 cibles pour les deux jobs, disponibilité des sorts, paliers, seuils WHM à 45/72, séquences d’ouverture, noms anglais et annotations, repli français, liens des sources et politique de popup : sortie pendant chargement, retour en ville, autre téléportation, synchronisation en monde ouvert, hausse de niveau et retour dans un autre donjon. Ce chiffre compte les invariants parcourus, pas des scénarios manuels indépendants.
- 150 assertions de panneaux : migrations v1/v2, sauvegarde/rechargement, mode manuel et bornes, disponibilité hors ligne. Clics natifs sur l’en-tête intégré : fermeture verrouillée et repliée, accès aux réglages, repli/dépli et conservation de taille, verrouillage, cible, vues sans perte de l’étape de lecture, niveau + et sélection des étapes. Vérification de la restauration des styles après le guide.
- 215 rendus natifs ImGui de la version : cinq niveaux, trois profils de cibles et cinq vues pour chacun des deux jobs ; panneau complet, clients français/anglais, largeur minimale, repli, soins, pied de fiche, réglages et popup à 100/150/200 %. Six rendus supplémentaires couvrent le minimum 320 × 560 en mode manuel anglais, soit **221 rendus** sans défilement horizontal.
- Inspection visuelle : guide BLM et WHM complet, petites icônes d’aptitudes, répétitions, icônes de jobs, mode manuel anglais à petite largeur et 150/200 %, soins WHM dépliés, pied de fiche avec tous les liens à 200 %, panneau replié et réglages à largeur minimale. Les captures utilisent Segoe UI ; Expressway n’est pas installée.
- Les règles de regroupement préservent chaque action et son ordre sur les deux jobs, tous les niveaux et les profils 1/2/3 cibles. Les étapes avec condition ou insertion restent séparées. Disponibilité des aptitudes intercalées vérifiée à chaque palier, avec cas précis d’ouverture BLM/WHM. Les soins sont séparés des dégâts sans déplacer Assises hors des priorités de dégâts. Repères de niveau vérifiés en anglais.
- La galerie web sert uniquement à parcourir les rendus : ce n’est pas un test des interactions du plugin. Aucun nouveau contrôle en jeu ni essai Expressway n’a été effectué.

Les contrôles des boutons utilisent le code des panneaux et cimgui, hors jeu, avant chaque génération des images. Rendus dans `preview/renders/`, métriques dans `.artifacts/render-metrics.json`. La page locale présente ces images ; ses contrôles permettent de choisir une capture, sans simuler un personnage connecté.

Les sources du Mage blanc ont été recoupées avec les guides officiel, Icy Veins et The Balance. Le schéma d’ouverture standard a été inspecté visuellement. Les règles restent une aide pédagogique statique et ne sont pas une certification d’optimalité de chaque situation.

## Livrables

`build.ps1` copie et vérifie la DLL dans `plugin/` et `releases/0.3.0/`, avec le manifeste, l’icône originale et la licence. Le ZIP utilise une liste explicite de ces quatre fichiers et ses SHA256 sont écrits dans `releases/SHA256SUMS.txt`. Seul `plugin/CycleOpener.dll` est le chemin stable à renseigner dans Dev Plugin Locations. Conserver une seule entrée active pour ce plugin. Le chemin de l’assembly réellement chargé est disponible dans Diagnostic et dans le journal du plugin.

## À confirmer en jeu

Chargement/déchargement API 15, icônes locales, police Dalamud et Expressway si installée, interaction avec les étapes, déplacement par l’en-tête, redimensionnement, fermeture/recentrage/verrouillage/repli du guide, touche Échap après fermeture des réglages, échelle globale, sauvegarde puis rechargement des réglages et de la position du guide. Vérifier les clients français et anglais avec leurs ressources locales. Expressway n’est pas installée sur le poste de validation. Vérifier les changements de job, déconnexion, entrée/sortie de donjon et synchronisation réelle, absence de popup en ville et dans les transitions après sortie, popup différée après chargement/combat/cinématique, refus et désactivation persistante. Vérifier que l’apparence du guide n’affecte jamais les réglages.

Les contrôles statiques ne certifient ni l’optimalité de toutes les rotations ni l’intégration native. Les nouvelles valeurs de niveaux ou changements de patch nécessitent une révision des sources.
