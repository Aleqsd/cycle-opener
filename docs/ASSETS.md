# Ressources

`assets/icon.svg` est un dessin original créé en SVG avec l’aide de Codex pour Cycle & Opener : atlas ouvert et boussole. Aucun symbole ni asset de FFXIV n’a été repris. Le SVG et son export PNG sont couverts par la licence MIT du dépôt.

La composition prolonge les icônes vectorielles originales des autres plugins d’Aleqsd : fond anthracite, angles arrondis et contours sobres. L’identité reste indépendante des jobs qui seront ajoutés. Aucun générateur d’image raster n’a été utilisé.

Pour régénérer le PNG avec ImageMagick :

```powershell
magick -background none assets/icon.svg -strip -depth 8 -define png:color-type=6 assets/icon.png
```

Les captures de documentation sont des rendus du code ImGui hors jeu, avec des données de démonstration. Les icônes de sorts qu’elles montrent appartiennent à Square Enix. Elles ne sont pas empaquetées comme ressources de jeu : le plugin résout les icônes locales par les services Dalamud.

L’en-tête utilise les icônes locales des 21 jobs. `tools/fetch-icons.py` prépare uniquement pour les aperçus un cache XIVAPI de 610 icônes d’actions et de jobs. Les commandes de fenêtre sont des formes géométriques dessinées par le plugin. Aucun asset de jeu n’entre dans l’archive.

Les noms français/anglais, identifiants, niveaux et numéros d’icônes sélectionnés sont des métadonnées factuelles, consignées dans `imported-actions.json`. L’import épingle la révision XIVAPI `d71de329cc6ed30c6fb16b9108cdf7d29c653302`. Les CSV complets et les pages consultées restent dans un cache ignoré ; aucun guide ou schéma tiers n’est redistribué. Les explications françaises des nouvelles fiches sont rédigées pour ce plugin.

Ni police, ni assembly Dalamud, ni cache d’icônes ne sont redistribués. Expressway reste une option utilisant une installation locale ; les aperçus hors jeu utilisent Segoe UI.

Le code, les outils de validation et l’icône ont été préparés avec une aide substantielle de Codex, à partir des demandes et retours de design d’Aleqsd. Les contrôles automatisés et rendus hors jeu ne constituent pas une validation humaine en jeu de cette version.
