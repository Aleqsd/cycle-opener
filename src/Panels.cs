using System.Numerics;
using Dalamud.Bindings.ImGui;
namespace CycleOpener;

public static class Panels {
 static readonly Vector4 Muted=new(.72f,.72f,.75f,1);
 static readonly Vector4 Accent=new(.68f,.49f,.86f,1);
 public static void PushTheme() {
  ImGui.PushStyleColor(ImGuiCol.WindowBg,new Vector4(.067f,.067f,.067f,.97f));
  ImGui.PushStyleColor(ImGuiCol.TitleBgActive,new Vector4(.13f,.12f,.15f,1));
  ImGui.PushStyleColor(ImGuiCol.TitleBg,new Vector4(.13f,.12f,.15f,1));
  ImGui.PushStyleColor(ImGuiCol.FrameBg,new Vector4(.16f,.16f,.16f,1));
  ImGui.PushStyleColor(ImGuiCol.Header,new Vector4(.2f,.17f,.24f,1));
  ImGui.PushStyleColor(ImGuiCol.Button,new Vector4(.20f,.18f,.23f,1));
  ImGui.PushStyleColor(ImGuiCol.ButtonHovered,new Vector4(.32f,.26f,.39f,1));
  ImGui.PushStyleColor(ImGuiCol.ButtonActive,new Vector4(.40f,.30f,.51f,1));
  ImGui.PushStyleColor(ImGuiCol.CheckMark,Hud.Purple);
  ImGui.PushStyleColor(ImGuiCol.SliderGrab,Hud.Purple);
  ImGui.PushStyleVar(ImGuiStyleVar.FramePadding,new Vector2(10,6)*(ImGui.GetFontSize()/17));
  ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding,3);
 }
 public static void PopTheme(){ImGui.PopStyleVar(2);ImGui.PopStyleColor(10);}
 static void MutedText(string text){ImGui.PushStyleColor(ImGuiCol.Text,Muted);ImGui.TextWrapped(text);ImGui.PopStyleColor();}
 static void Section(string title){ImGui.Spacing();ImGui.Separator();ImGui.Spacing();ImGui.TextColored(Accent,title);ImGui.Spacing();}
 static bool Button(string label,float width=0,bool primary=false){
  if(primary)ImGui.PushStyleColor(ImGuiCol.Button,new Vector4(.34f,.23f,.46f,1));
  var clicked=ImGui.Button(label,new(width,0));
  if(primary)ImGui.PopStyleColor();return clicked;
 }
 // Stack when translated labels or global font scale cannot fit side by side.
 static void Pair(string left,Action onLeft,string right,Action onRight){
  var width=ImGui.GetContentRegionAvail().X;var gap=ImGui.GetStyle().ItemSpacing.X;
  var minimum=Math.Max(ImGui.CalcTextSize(left).X,ImGui.CalcTextSize(right).X)+2*ImGui.GetStyle().FramePadding.X;
  var beside=(width-gap)/2>=minimum;
  if(Button(left,beside?(width-gap)/2:width))onLeft();
  if(beside)ImGui.SameLine();
  if(Button(right,beside?(width-gap)/2:width))onRight();
 }
 static bool Targets(Configuration config){
  var changed=false;string[] labels=["Mono","2 cibles","3+ cibles"];
  var width=(ImGui.GetContentRegionAvail().X-2*ImGui.GetStyle().ItemSpacing.X)/3;
  for(var n=0;n<3;n++){
   if(n>0)ImGui.SameLine();
   var selected=Math.Min(config.Targets-1,2)==n;
   if(Button(labels[n],width,selected)){config.Targets=n+1;changed=true;}
  }
  return changed;
 }
 public static bool GuideToolbar(Configuration config,Action settings,ref int selectedStep,int level) {
  PushTheme();var changed=false;
  var layout=(int)config.Layout;ImGui.SetNextItemWidth(-1);
  if(ImGui.Combo("##Vue du guide",ref layout,Hud.Names,Hud.Names.Length)){config.Layout=(Layout)layout;changed=true;selectedStep=0;}
  if(Targets(config)){changed=true;selectedStep=0;}
  if(config.Layout==Layout.Ouverture)OpenerToolbar(ref selectedStep,level,config.Targets);
  else Pair(config.OpeningVisible?"Masquer l’ouverture":"Ouvrir l’ouverture",()=>{if(config.OpeningVisible)config.HideOpening();else config.OpenOpening();changed=true;},"Réglages",settings);
  if(config.Layout==Layout.Ouverture&&Button("Réglages",ImGui.GetContentRegionAvail().X))settings();
  ImGui.Spacing();ImGui.Separator();ImGui.Spacing();PopTheme();return changed;
 }
 public static bool OpeningPanelToolbar(Configuration config,Action settings,ref int selectedStep,int level){
  PushTheme();var changed=false;
  Pair(config.CycleVisible?"Masquer le cycle":"Ouvrir le cycle",()=>{if(config.CycleVisible)config.ShowHud=false;else config.OpenCycle();changed=true;},"Réglages",settings);
  if(Targets(config)){changed=true;selectedStep=0;}
  OpenerToolbar(ref selectedStep,level,config.Targets);
  ImGui.Spacing();ImGui.Separator();ImGui.Spacing();PopTheme();return changed;
 }
 public static void OpenerToolbar(ref int selectedStep,int level,int targets){
  var count=Guide.Opener(level,targets).Count;selectedStep=Math.Clamp(selectedStep,0,Math.Max(0,count-1));
  var width=(ImGui.GetContentRegionAvail().X-2*ImGui.GetStyle().ItemSpacing.X)/3;
  ImGui.BeginDisabled(selectedStep==0);if(Button("←",width))selectedStep--;ImGui.EndDisabled();
  ImGui.SameLine();if(Button("Début",width))selectedStep=0;
  ImGui.SameLine();ImGui.BeginDisabled(selectedStep>=count-1);if(Button("→",width))selectedStep++;ImGui.EndDisabled();
  MutedText($"Étape {selectedStep+1} / {count} · sélection libre");
 }
 public static bool Settings(Configuration config, GuideContext state, bool expresswayAvailable, string dllPath, ref bool demo, ref int openerStep, Action resetPosition, Action rebuildFont, string? runtimeError=null) {
  var changed=false;
  ImGui.TextColored(Accent,"MAGE NOIR");
  ImGui.SameLine();ImGui.TextColored(Muted,"0.1.2 · expérimental");
  var level=demo?config.PreviewLevel:state.Level;
  MutedText(demo?$"Fiche niveau {level} · niveau manuel":state.Available?$"Niveau {level} · synchronisation automatique":"Personnage indisponible · choisis un niveau manuel.");
  if(runtimeError!=null)ImGui.TextWrapped(runtimeError);

  ImGui.Spacing();
  ImGui.BeginDisabled(!demo&&!state.Available);
  if(Button("Ouvrir les deux panneaux",ImGui.GetContentRegionAvail().X,true)){config.OpenBoth();changed=true;}
  Pair(config.CycleVisible?"Masquer le cycle":"Ouvrir le cycle",()=>{if(config.CycleVisible)config.ShowHud=false;else config.OpenCycle();changed=true;},
       config.OpeningVisible?"Masquer l’ouverture":"Ouvrir l’ouverture",()=>{if(config.OpeningVisible)config.HideOpening();else config.OpenOpening();changed=true;});
  ImGui.EndDisabled();
  MutedText("Cycle et ouverture se déplacent et se ferment séparément.");

  Section("FICHE À CONSULTER");
  ImGui.Text("Niveau");
  var manual=demo;
  Pair(demo?"Suivre le personnage":"Automatique · actif",()=>manual=false,demo?"Manuel · actif":"Choisir manuellement",()=>manual=true);
  if(manual!=demo){demo=manual;openerStep=0;changed=true;}
  if(demo){ImGui.SetNextItemWidth(-1);if(ImGui.SliderInt("##Niveau fiche",ref config.PreviewLevel,1,100,"Niv. %d")){openerStep=0;changed=true;}}
  ImGui.Spacing();ImGui.Text("Cibles regroupées");
  if(Targets(config)){changed=true;openerStep=0;}
  MutedText(level<12?"Cycle de zone disponible à partir du niveau 12.":level>=100?"Zone dès 2 cibles · Giga Glace à 2, Gel à 3+.":"Cycle de zone dès 3 cibles regroupées.");
  ImGui.Spacing();ImGui.Text("Présentation du cycle");ImGui.SetNextItemWidth(-1);
  var layout=(int)config.Layout;
  if(ImGui.Combo("##Affichage",ref layout,Hud.Names,Hud.Names.Length)){config.Layout=(Layout)layout;openerStep=0;changed=true;}
  MutedText(Hud.Descriptions[layout]);

  Section("COMPORTEMENT");
  changed|=ImGui.Checkbox("Proposer après un changement de niveau",ref config.SuggestOnSync);
  changed|=ImGui.Checkbox("Verrouiller les panneaux",ref config.Locked);
  if(config.Locked)MutedText("Les clics passent vers le jeu. Déverrouille ici pour agir sur les panneaux.");
  Pair("Recentrer les panneaux",()=>{resetPosition();changed=true;},"Tout masquer",()=>{config.HideAll();changed=true;});
  ImGui.Spacing();
  if(ImGui.CollapsingHeader("Apparence des panneaux")) {
   ImGui.Text("Taille du contenu");ImGui.SetNextItemWidth(-1);changed|=ImGui.SliderFloat("##Échelle",ref config.HudScale,.8f,1.8f,"%.2f");
   ImGui.Text("Opacité du fond");ImGui.SetNextItemWidth(-1);changed|=ImGui.SliderFloat("##Opacité",ref config.BackgroundOpacity,.15f,1,"%.2f");
   changed|=ImGui.Checkbox("Ombre du texte",ref config.TextOutline);
   if(ImGui.Checkbox("Expressway si installée",ref config.Expressway)){changed=true;rebuildFont();}
   MutedText(!expresswayAvailable?"Expressway absente : police Dalamud.":"Expressway locale disponible.");
   if(Button("Restaurer l’apparence",ImGui.GetContentRegionAvail().X)){config.HudScale=1;config.BackgroundOpacity=.86f;config.TextOutline=true;config.Expressway=true;rebuildFont();changed=true;}
  }
  if(ImGui.CollapsingHeader("Sources et diagnostic")) {
   ImGui.TextWrapped("Fiches recoupées avec Icy Veins 7.55 et The Balance. Quêtes de job supposées terminées.");
   MutedText("Choix des cibles manuel. Aucun suivi des sorts, cibles, jauges ou dégâts.");
   ImGui.TextWrapped(dllPath);
  }
  return changed;
 }
 public static int Prompt(GuideContext state, Layout layout, ref bool disabled) {
  ImGui.TextColored(Hud.Purple,$"Niveau {state.Level} détecté");
  ImGui.TextWrapped("Le cycle et l’ouverture Mage noir sont prêts pour ce niveau.");
  MutedText($"{Hud.Names[(int)layout]} · {state.Targets} cible(s) manuelles");
  ImGui.Spacing();
  if(Button("Ouvrir les deux panneaux",ImGui.GetContentRegionAvail().X,true))return 1;
  if(Button("Pas maintenant",ImGui.GetContentRegionAvail().X))return 2;
  if(ImGui.Checkbox("Ne plus proposer automatiquement",ref disabled))return 3;
  return 0;
 }
}
