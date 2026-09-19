# Cycle & Opener

[Socle commun](../AGENTS.md) — ce document le complète pour ce plugin uniquement.

Guide statique par niveau pour Mage noir et occultiste, tâche de référence « Publier Cycle & Opener ». Correction explicite de l'utilisateur le 19 septembre : schémas de cycle et d'ouverture, listes de priorités comme Icy Veins ; pas de conseil de prochain sort en temps réel. Seuls le job, le niveau et les conditions nécessaires à différer la proposition de synchronisation sont lus. Aucun suivi des cibles, jauges, sorts ou dégâts.

- `src/Core.cs` : règles pures, paliers, seuils et séquences. `Plugin.cs` : lecture des services Dalamud et fenêtres. `Hud.cs` : les cinq compositions partageant le même état. `tests/` : cas métier. `tools/Preview/` : rendu ImGui hors jeu. `preview/` : comparateur local.
- SDK .NET 10, Dalamud API 15. `build.ps1` compile puis exécute les contrôles ; `tools/render.ps1` produit les aperçus natifs. Les commandes effectivement validées sont consignées dans `docs/VALIDATION.md`.
- Panneau LMeter, violet BLM, feu orange et glace bleue nommés. Settings fixes. Cinq modes : Fiche express, Frise, Priorités, Deux phases, Ouverture. Lecture libre, pas de progression liée au combat.
- Demande explicite du 19 septembre : afficher simultanément ouverture et cycle/priorités. Deux fenêtres déplaçables avec le même niveau et le même profil de cibles ; bouton principal pour ouvrir les deux, boutons séparés pour ouvrir ou masquer chacun indépendamment. Le comparateur propose aussi ce duo, avec défilement indépendant à largeur réduite.
- Niveau synchronisé lu sur le personnage ; choix manuel d'un autre niveau possible. Mono, 2 cibles et 3+ sont des variantes de fiche. Seuils pour ennemis regroupés. Les optimisations ne doivent pas être présentées comme une unique séquence universelle.
- Actualisation via Framework.Update, aucun IO ni calcul natif lourd dans Draw. Icônes locales par ITextureProvider, aucune icône extraite incluse dans les binaires. Cache web d'icônes réservé aux aperçus locaux, ignoré par Git. Expressway facultative et non redistribuée.
- Une capture ImGui hors jeu ne prouve pas l'intégration FFXIV. Vérifier ensuite en jeu : niveau synchronisé, job/zone, reconnexion, popup après chargement et hors combat, fermeture, déplacement et restauration du panneau. Ne pas annoncer une publication ou un chargement effectif sans preuve.
- Livrable local dans `plugin/CycleOpener.dll`, paquet versionné dans `releases/`. Sources et releases dans Aleqsd/cycle-opener ; intégration du dépôt personnalisé confiée à la tâche de publication du socle.

- Choix utilisateur : Fiche express (affichage 1) par défaut, avec ouverture à côté. Préserver les préférences déjà sauvegardées. L’icône originale est dans `assets/`, sa provenance dans `docs/ASSETS.md`.
- Dans un clone autonome, le socle parent peut être absent. Ce dépôt compile indépendamment ; appliquer alors les instructions locales disponibles sans créer un socle concurrent.

- Nom public et identité de chargement : Cycle & Opener / `CycleOpener`, dépôt `Aleqsd/cycle-opener`, commande `/cycle`. Le dossier local historique reste `astral-guide/` ; aucun déplacement implicite. Seul le Mage noir est pris en charge actuellement.
- Diffusion autorisée uniquement sur le GitHub d’Aleqsd et son dépôt personnalisé. Demande explicite : aucune PR ni soumission au catalogue officiel Dalamud ; ne pas activer son suivi.

- Panneau de consultation : actions d’ouverture par boutons, jamais par cases à cocher. Niveau et cibles regroupés, comportement et apparence secondaires. Fiche express en lignes de sorts compactes, deux rappels conditionnels visibles. Le clic manuel sur une étape ne suit aucune action du joueur.
- Configuration v2 : migrer l’ancienne ouverture dépendante en conservant les fenêtres effectivement visibles et les autres préférences. Un panneau fermé ne ferme plus l’autre. Ne pas créer deux fenêtres d’ouverture si la vue principale utilise déjà Ouverture. Les contrôles natifs et la migration sont vérifiés dans `tools/Preview/PanelChecks.cs`.
