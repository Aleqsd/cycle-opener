using Dalamud.Bindings.ImGui;
namespace CycleOpener;
public static class Panels {
 public static void PushTheme() {
  ImGui.PushStyleColor(ImGuiCol.WindowBg,new System.Numerics.Vector4(.067f,.067f,.067f,.97f));
  ImGui.PushStyleColor(ImGuiCol.TitleBgActive,new System.Numerics.Vector4(.13f,.12f,.15f,1));
  ImGui.PushStyleColor(ImGuiCol.TitleBg,new System.Numerics.Vector4(.13f,.12f,.15f,1));
  ImGui.PushStyleColor(ImGuiCol.FrameBg,new System.Numerics.Vector4(.16f,.16f,.16f,1));
  ImGui.PushStyleColor(ImGuiCol.Header,new System.Numerics.Vector4(.2f,.17f,.24f,1));
  ImGui.PushStyleColor(ImGuiCol.Button,new System.Numerics.Vector4(.24f,.19f,.3f,1));
  ImGui.PushStyleColor(ImGuiCol.CheckMark,Hud.Purple);
  ImGui.PushStyleColor(ImGuiCol.SliderGrab,Hud.Purple);
 }
 public static void PopTheme()=>ImGui.PopStyleColor(8);
 public static bool GuideToolbar(Configuration config,Action settings,ref int selectedStep,int level) {
  PushTheme();var changed=false;var width=ImGui.GetContentRegionAvail().X;
  var layout=(int)config.Layout;ImGui.SetNextItemWidth(width*.54f);
  if(ImGui.Combo("##Vue du guide",ref layout,Hud.Names,Hud.Names.Length)){config.Layout=(Layout)layout;changed=true;}
  ImGui.SameLine();var target=Math.Min(2,config.Targets-1);string[] modes=["Monocible","2 cibles","3+ cibles"];ImGui.SetNextItemWidth(-1);
  if(ImGui.Combo("##Cibles du guide",ref target,modes,modes.Length)){config.Targets=target+1;changed=true;}
  if(ImGui.SmallButton("Réglages"))settings();
  if(config.Layout==Layout.Ouverture)OpenerToolbar(ref selectedStep,level,config.Targets);
  else changed|=ImGui.Checkbox("Ouverture à côté",ref config.ShowOpener);
  ImGui.Spacing();PopTheme();return changed;
 }
 public static void OpenerToolbar(ref int selectedStep,int level,int targets){
   PushTheme();
   var count=Guide.Opener(level,targets).Count;selectedStep=Math.Clamp(selectedStep,0,Math.Max(0,count-1));
   ImGui.BeginDisabled(selectedStep==0);if(ImGui.SmallButton("← Précédente"))selectedStep--;ImGui.EndDisabled();
   ImGui.SameLine();ImGui.BeginDisabled(selectedStep>=count-1);if(ImGui.SmallButton("Suivante →"))selectedStep++;ImGui.EndDisabled();
   ImGui.Text($"Étape {selectedStep+1} / {count} · lecture libre");
   ImGui.Spacing();PopTheme();
 }
 public static bool Settings(Configuration config, GuideContext state, bool expresswayAvailable, string dllPath, ref bool demo, ref int openerStep, Action resetPosition, Action rebuildFont, string? runtimeError=null) {
 bool changed=false;
            ImGui.TextColored(Hud.Purple,"MAGE NOIR · 0.1.1 expérimental");
            ImGui.TextWrapped(state.Available?$"Niveau effectif : {state.Level} · suivi automatique de la synchronisation.":"Connecte un Mage noir / occultiste, ou ouvre le mode manuel.");
            if(runtimeError!=null)ImGui.TextWrapped(runtimeError);
            ImGui.Separator();
            changed |=ImGui.Checkbox("Afficher le guide",ref config.ShowHud);
            changed |=ImGui.Checkbox("Afficher aussi l’ouverture",ref config.ShowOpener);
            changed |=ImGui.Checkbox("Verrouiller · laisser passer les clics",ref config.Locked);
            changed |=ImGui.Checkbox("Proposer le guide quand le niveau change",ref config.SuggestOnSync);
            ImGui.Text("Affichage");ImGui.SetNextItemWidth(-1);
            var layout=(int)config.Layout;if(ImGui.Combo("##Affichage",ref layout,Hud.Names,Hud.Names.Length)){config.Layout=(Layout)layout;changed=true;}
            ImGui.TextWrapped(Hud.Descriptions[layout]);
            ImGui.Spacing();ImGui.TextColored(Hud.Purple,"Cibles regroupées");
            ImGui.Text("Nombre manuel");ImGui.SetNextItemWidth(-1);
            if(ImGui.SliderInt("##Nombre manuel",ref config.Targets,1,8)){changed=true;openerStep=0;}
            ImGui.TextWrapped("Le prototype ne compte pas les ennemis. Choisis le cycle à consulter. Aucun suivi du combat.");
            ImGui.TextWrapped("Niveau 100 : cycle de zone dès 2 cibles ; Giga Glace à 2, Gel à 3+. En dessous : base prudente de zone à 3+.");
            ImGui.Separator();
            if(ImGui.Checkbox("Choisir un niveau manuellement",ref demo)) {config.ShowHud=true;changed=true;}
            if(demo){ImGui.Text("Niveau de la fiche");ImGui.SetNextItemWidth(-1);if(ImGui.SliderInt("##Niveau fiche",ref config.PreviewLevel,1,100)){openerStep=0;changed=true;}}
            if(ImGui.Button("Recentrer le guide")){resetPosition();config.ShowHud=true;changed=true;}
            if(config.Layout==Layout.Ouverture) {
                ImGui.Spacing();ImGui.TextWrapped("Schéma d’ouverture : sélectionne une étape pour lire les insertions.");
                if(ImGui.Button("Étape précédente"))openerStep=Math.Max(0,openerStep-1);
                ImGui.SameLine();if(ImGui.Button("Étape suivante"))openerStep=Math.Min(Guide.Opener(demo?config.PreviewLevel:state.Level,config.Targets).Count-1,openerStep+1);
                if(ImGui.Button("Recommencer l’ouverture"))openerStep=0;
                ImGui.TextWrapped("Lecture : /cycle next et /cycle prev.");
            }
            if(ImGui.CollapsingHeader("Apparence du guide")) {
                changed |=ImGui.SliderFloat("Échelle du guide",ref config.HudScale,.8f,1.8f,"%.2f");
                changed |=ImGui.SliderFloat("Opacité du fond",ref config.BackgroundOpacity,.15f,1,"%.2f");
                changed |=ImGui.Checkbox("Ombre du texte",ref config.TextOutline);
                if(ImGui.Checkbox("Expressway si installée",ref config.Expressway)){changed=true;rebuildFont();}
                ImGui.TextWrapped(!expresswayAvailable?"Expressway absente : police Dalamud utilisée.":"Expressway locale disponible. Aucun fichier de police embarqué.");
                if(ImGui.Button("Restaurer l’apparence")){config.HudScale=1;config.BackgroundOpacity=.86f;config.TextOutline=true;config.Expressway=true;rebuildFont();changed=true;}
            }
            if(ImGui.CollapsingHeader("Diagnostic et limites")) {
                ImGui.TextWrapped("Fiches pédagogiques recoupées avec Icy Veins 7.55 et The Balance. Quêtes de job supposées terminées. Les variantes optimisées restent dans les guides sources.");
                ImGui.TextWrapped("Seuls le job et le niveau sont lus. Aucun suivi des sorts, cibles, jauges ou dégâts.");
                ImGui.TextWrapped(dllPath);
            }
 return changed;
 }
 public static int Prompt(GuideContext state, Layout layout, ref bool disabled) {
 ImGui.TextColored(Hud.Purple,$"Niveau {state.Level} détecté");
 ImGui.TextWrapped("Afficher le guide Mage noir adapté à ce niveau ?");
 ImGui.TextWrapped($"{Hud.Names[(int)layout]} · {state.Targets} cible(s) manuelles · sorts adaptés au niveau");
 if(ImGui.Button("Afficher le guide")) return 1;
 ImGui.SameLine(); if(ImGui.Button("Pas maintenant")) return 2;
 if(ImGui.Checkbox("Ne plus proposer automatiquement",ref disabled)) return 3;
 return 0;
 }
}
