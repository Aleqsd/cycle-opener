# Cycle & Opener

<img src="assets/icon.png" width="80" height="80" alt="Icône Cycle & Opener">

Cycles, priorités et ouvertures par niveau pour les **21 jobs de combat**, du niveau 1 au 100. Mono et multi, avec le nombre minimal de cibles. Le cycle et l’ouverture partagent une seule fenêtre. Le Mage bleu viendra plus tard.

![Guide Pictomancien : ouverture et cycle dans un panneau compact](docs/images/fiche-express.png)

*Rendu ImGui hors jeu, avec Segoe UI. Ce n’est pas une capture FFXIV.*

**Auto** suit le job et le niveau synchronisé. **Manuel** permet de choisir le job et le niveau, avec saisie, boutons −/+ et paliers rapides. Une proposition facultative peut apparaître quand la synchronisation baisse le niveau en instance PvE, après le chargement ou le combat. Aucune proposition à la sortie du donjon, en ville ou lors d’une hausse de niveau.

L’ouverture reste au-dessus du cycle et des priorités. Des encarts indiquent les ressources ou effets requis par les actions ; le survol donne le détail. Les soigneurs ont une section **Soins et urgences**, les tanks une section **Protection et tanking**.

Les noms des sorts suivent la langue du client, notamment français et anglais. Les explications et l’interface restent en français. Les sources sont indiquées en petit sur les fiches.

Version **1.1.0**, API Dalamud 15. Guide statique : aucun suivi du combat ni lancement de sort. Quêtes de classe et de job supposées terminées. Les nouvelles ouvertures sont des départs pédagogiques ; les variantes optimisées de raid restent dans les sources liées.

## Installation

Ajouter ce dépôt personnalisé dans les réglages de Dalamud :

```text
https://raw.githubusercontent.com/Aleqsd/dalamud-plugins/main/repo.json
```

Rechercher **Cycle & Opener** dans `/xlplugins`.

[Télécharger le ZIP 1.1.0](https://github.com/Aleqsd/cycle-opener/releases/download/v1.1.0/CycleOpener-1.1.0.zip) · [Notes de version](https://github.com/Aleqsd/cycle-opener/releases/tag/v1.1.0)

En développement, extraire le ZIP puis ajouter sa DLL aux **Dev Plugin Locations**. Désactiver cette copie avant l’installation depuis le dépôt ; garder une seule installation active.

## Utilisation

Ouvrir le guide avec `/cycle`. L’ouverture et le cycle sont toujours réunis. En mode manuel, filtrer les jobs par rôle et utiliser l’étoile pour retrouver ses favoris. Le bouton **3+ cibles** permet de choisir précisément de 3 à 8 ennemis regroupés. Cliquer sur une icône de l’ouverture pour lire son étape ; les petites icônes montrent les aptitudes à insérer.

Glisser l’en-tête pour déplacer le panneau. Le cadenas verrouille sa position ; **−** le replie, **+** le déplie et l’engrenage ouvre les réglages. **×** ou **Échap** le ferme, même si les réglages sont fermés ou sa position verrouillée.

![Réglages et sélection manuelle](docs/images/reglages.png)

*Réglages ImGui hors jeu. Leur apparence reste fixe.*

- `/cycle show` et `/cycle hide` : afficher ou masquer le guide.
- `/cycle config`, `/cycle cfg` ou `/cycle setup` : ouvrir les réglages.
- `/cycle next` et `/cycle prev` : parcourir l’ouverture.
- Présentations du cycle : Fiche express, Frise, Priorités ou Deux sections.

L’intégration en jeu reste à tester. Ce dépôt personnalisé est distinct du catalogue officiel Dalamud.

[Jobs et sources](docs/JOBS.md) · [Règles des fiches](docs/RULES.md) · [Compilation et validation](docs/VALIDATION.md) · [Ressources et icône](docs/ASSETS.md)
