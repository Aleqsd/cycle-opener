using System.Numerics;
using Dalamud.Bindings.ImGui;
namespace CycleOpener;

public static class Panels {
 public static ImGuiWindowFlags GuideFlags(bool locked)=>ImGuiWindowFlags.NoFocusOnAppearing|(locked?ImGuiWindowFlags.NoMove|ImGuiWindowFlags.NoResize:0);
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
 public static bool Targets(Configuration config){
  var changed=false;string[] labels=["Mono","2 cibles","3+ cibles"];
  var width=(ImGui.GetContentRegionAvail().X-2*ImGui.GetStyle().ItemSpacing.X)/3;
  for(var n=0;n<3;n++){
   if(n>0)ImGui.SameLine();
   var selected=Math.Min(config.Targets-1,2)==n;
   if(Button(labels[n],width,selected)){config.Targets=n+1;changed=true;}
  }
  return changed;
 }
 public static bool LevelSelector(Configuration config,GuideContext actual,ref int selectedStep){
  ImGui.PushID("LevelSelector");var changed=false;
  var width=ImGui.GetContentRegionAvail().X;var gap=ImGui.GetStyle().ItemSpacing.X;
  var autoLabel=actual.Available?$"Auto · niv. {actual.Level}":"Auto · indisponible";
  var half=(width-gap)/2;
  var stacked=ImGui.CalcTextSize(autoLabel).X+2*ImGui.GetStyle().FramePadding.X>half;
  ImGui.BeginDisabled(!actual.Available);
  if(Button(autoLabel,stacked?width:half,!config.ManualLevel)&&config.ManualLevel){config.ManualLevel=false;changed=true;}
  ImGui.EndDisabled();
  if(!stacked)ImGui.SameLine();
  if(Button(config.ManualLevel?"Manuel · actif":"Manuel",stacked?width:half,config.ManualLevel)&&!config.ManualLevel){config.ManualLevel=true;if(actual.Available)config.ManualJob=actual.Job;changed=true;}
  if(config.ManualLevel){
   var job=(int)config.ManualJob;ImGui.SetNextItemWidth(-1);
   if(ImGui.Combo("##Job manuel",ref job,new[]{"Mage noir / Black Mage","Mage blanc / White Mage"},2)){config.ManualJob=(GuideJob)job;changed=true;}
   // Exact typing plus native +/- buttons; Ctrl uses the ten-level increment.
   var presetWidth=ImGui.CalcTextSize("Paliers").X+2*ImGui.GetStyle().FramePadding.X+ImGui.GetFrameHeight();
   ImGui.SetNextItemWidth(width-presetWidth-gap);var level=config.PreviewLevel;
   if(ImGui.InputInt("##Niveau manuel",ref level,1,10)){var bounded=Math.Clamp(level,1,100);if(config.PreviewLevel!=bounded){config.PreviewLevel=bounded;changed=true;}}
   if(ImGui.IsItemHovered())ImGui.SetTooltip("Niveau de la fiche : 1 à 100. Saisie directe ou −/+ ; Ctrl pour changer de 10 niveaux.");
   ImGui.SameLine();ImGui.SetNextItemWidth(presetWidth);
   if(ImGui.BeginCombo("##Palier","Paliers")){
    foreach(var preset in new[]{15,30,40,50,60,70,80,90,100})
     if(ImGui.Selectable($"Niveau {preset}",config.PreviewLevel==preset)&&config.PreviewLevel!=preset){config.PreviewLevel=preset;changed=true;}
    ImGui.EndCombo();
   }
  }
  if(changed)selectedStep=0;
  ImGui.PopID();return changed;
 }
 public static bool GuideToolbar(Configuration config,Action settings,ref int selectedStep,GuideContext actual) {
  PushTheme();var changed=false;
  Pair("Réglages",settings,"Fermer le guide",()=>{config.ShowGuide=false;changed=true;});
  changed|=LevelSelector(config,actual,ref selectedStep);
  if(Targets(config)){changed=true;selectedStep=0;}
  changed|=ViewSelector(config);
  if(config.View!=GuideView.Opening){
   var layout=(int)config.Layout;ImGui.SetNextItemWidth(-1);
   if(ImGui.Combo("##Vue du cycle",ref layout,Hud.CycleNames,Hud.CycleNames.Length)){config.Layout=(Layout)layout;changed=true;}
  }
  ImGui.Spacing();ImGui.Separator();ImGui.Spacing();PopTheme();return changed;
 }
 static bool ViewSelector(Configuration config){
  string[] labels=["Les deux","Cycle","Ouverture"];var changed=false;
  var width=(ImGui.GetContentRegionAvail().X-2*ImGui.GetStyle().ItemSpacing.X)/3;
  for(var n=0;n<3;n++){if(n>0)ImGui.SameLine();if(Button(labels[n],width,(int)config.View==n)){config.View=(GuideView)n;changed=true;}}
  return changed;
 }
 public static void GuideContent(Configuration config,GuideContext state,Func<uint,ImTextureID?> icon,HudLook look,ref int step){
  var selected=step;
  void Cycle(){ImGui.TextUnformatted("CYCLE / PRIORITÉS");Hud.Draw(config.Layout,state,icon,look,selected,config.ManualLevel);}
  void Opening(){ImGui.TextUnformatted("OUVERTURE");PushTheme();OpenerToolbar(ref selected,state);PopTheme();Hud.Draw(Layout.Ouverture,state,icon,look,selected,config.ManualLevel,n=>selected=n);}
  var paired=config.View==GuideView.Both;
  if(paired&&ImGui.GetContentRegionAvail().X>=760*look.Scale&&ImGui.BeginTable("GuideColumns",2,ImGuiTableFlags.SizingStretchSame|ImGuiTableFlags.BordersInnerV)){
   ImGui.TableNextColumn();ImGui.BeginChild("CycleSheet",new(0,Math.Max(200*look.Scale,ImGui.GetContentRegionAvail().Y-16*look.Scale)),false);Cycle();ImGui.EndChild();
   ImGui.TableNextColumn();ImGui.BeginChild("OpeningSheet",new(0,Math.Max(200*look.Scale,ImGui.GetContentRegionAvail().Y-16*look.Scale)),false);Opening();ImGui.EndChild();
   ImGui.EndTable();
  }else{
   if(config.View!=GuideView.Opening)Cycle();
   if(paired){ImGui.Spacing();ImGui.Separator();ImGui.Spacing();}
   if(config.View!=GuideView.Cycle)Opening();
  }
  step=selected;
 }
 public static void OpenerToolbar(ref int selectedStep,GuideContext context){
  var count=Guide.Opener(context).Count;selectedStep=Math.Clamp(selectedStep,0,Math.Max(0,count-1));
  var width=(ImGui.GetContentRegionAvail().X-2*ImGui.GetStyle().ItemSpacing.X)/3;
  ImGui.BeginDisabled(selectedStep==0);if(Button("←",width))selectedStep--;ImGui.EndDisabled();
  ImGui.SameLine();if(Button("Début",width))selectedStep=0;
  ImGui.SameLine();ImGui.BeginDisabled(selectedStep>=count-1);if(Button("→",width))selectedStep++;ImGui.EndDisabled();
  MutedText($"Étape {selectedStep+1} / {count} · sélection libre");
 }
 public static bool Settings(Configuration config, GuideContext state, bool expresswayAvailable, string dllPath, ref int openerStep, Action resetPosition, Action rebuildFont, string? runtimeError=null) {
  var changed=false;
  ImGui.TextColored(Accent,Guide.JobName(config.Resolve(state).Job).ToUpperInvariant());
  ImGui.SameLine();ImGui.TextColored(Muted,"0.2.1 · expérimental");
  var level=config.Resolve(state).Level;
  MutedText(config.ManualLevel?$"Fiche niveau {level} · niveau manuel":state.Available?$"Niveau {level} · synchronisation automatique":"Personnage indisponible · choisis un niveau manuel.");
  if(runtimeError!=null)ImGui.TextWrapped(runtimeError);

  ImGui.Spacing();
  ImGui.BeginDisabled(!config.ManualLevel&&!state.Available);
  if(Button(config.ShowGuide?"Masquer le guide":"Ouvrir le guide",ImGui.GetContentRegionAvail().X,true)){config.ShowGuide=!config.ShowGuide;changed=true;}
  ImGui.EndDisabled();
  changed|=ViewSelector(config);
  MutedText("Une seule fenêtre pour le cycle et l’ouverture. Fermeture par ×, Échap ou Fermer le guide.");

  Section("FICHE À CONSULTER");
  ImGui.Text("Niveau");
  changed|=LevelSelector(config,state,ref openerStep);
  level=config.Resolve(state).Level;
  ImGui.Spacing();ImGui.Text("Cibles regroupées");
  if(Targets(config)){changed=true;openerStep=0;}
  MutedText(Spells.LocalizeText(Guide.Threshold(config.Resolve(state))));
  ImGui.Spacing();ImGui.Text("Présentation du cycle");ImGui.SetNextItemWidth(-1);
  var layout=(int)config.Layout;
  if(ImGui.Combo("##Affichage",ref layout,Hud.CycleNames,Hud.CycleNames.Length)){config.Layout=(Layout)layout;openerStep=0;changed=true;}
  MutedText(layout==3&&config.Resolve(state).Job==GuideJob.WhiteMage?"Dégâts courants et ressources conditionnelles.":Hud.Descriptions[layout]);

  Section("COMPORTEMENT");
  changed|=ImGui.Checkbox("Proposer à la synchronisation en instance",ref config.SuggestOnSync);
  changed|=ImGui.Checkbox("Verrouiller la position du guide",ref config.Locked);
  if(config.Locked)MutedText("La fenêtre reste interactive et peut toujours être fermée.");
  if(Button("Recentrer le guide",ImGui.GetContentRegionAvail().X)){resetPosition();changed=true;}
  ImGui.Spacing();
  if(ImGui.CollapsingHeader("Apparence du guide")) {
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
   ImGui.TextWrapped("Noms des sorts : langue du client. Explications et interface : français.");
   ImGui.TextWrapped(dllPath);
  }
  return changed;
 }
 public static int Prompt(GuideContext state, Layout layout, ref bool disabled) {
  ImGui.TextColored(Hud.Purple,$"Niveau {state.Level} détecté");
  ImGui.TextWrapped("Le cycle et l’ouverture sont prêts pour ce niveau.");
  MutedText($"{Hud.Names[(int)layout]} · {state.Targets} cible(s) manuelles");
  ImGui.Spacing();
  if(Button("Ouvrir le guide",ImGui.GetContentRegionAvail().X,true))return 1;
  if(Button("Pas maintenant",ImGui.GetContentRegionAvail().X))return 2;
  if(ImGui.Checkbox("Ne plus proposer automatiquement",ref disabled))return 3;
  return 0;
 }
}
