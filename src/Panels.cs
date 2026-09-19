using System.Numerics;
using Dalamud.Bindings.ImGui;
namespace CycleOpener;

public static class Panels {
 public static ImGuiWindowFlags GuideFlags(bool locked)=>ImGuiWindowFlags.NoTitleBar|ImGuiWindowFlags.NoCollapse|ImGuiWindowFlags.NoMove|ImGuiWindowFlags.NoScrollbar|ImGuiWindowFlags.NoScrollWithMouse|ImGuiWindowFlags.NoFocusOnAppearing|(locked?ImGuiWindowFlags.NoResize:0);
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
 public static bool Targets(Configuration config,GuideContext? context=null){
  var threshold=context==null?null:Guide.MultiThreshold(context);
  var changed=false;string[] labels=["1 cible",threshold==2?"Multi · 2":"2 cibles",config.Targets>=3?$"{config.Targets} cibles":"3+ cibles"];
  var width=(ImGui.GetContentRegionAvail().X-2*ImGui.GetStyle().ItemSpacing.X)/3;
  for(var n=0;n<3;n++){
   if(n>0)ImGui.SameLine();
   var selected=Math.Min(config.Targets-1,2)==n;
   if(Button(labels[n],width,selected)){
    if(n==2)ImGui.OpenPopup("Nombre de cibles");
    else if(!selected){config.Targets=n+1;changed=true;}
   }
   if(n==2&&ImGui.IsItemHovered())ImGui.SetTooltip("Choisir le nombre exact : de 3 à 8 cibles.");
  }
  if(ImGui.BeginPopup("Nombre de cibles")){
   for(var n=3;n<=8;n++)if(ImGui.Selectable($"{n} cibles"+(threshold==n?" · seuil de zone":""),config.Targets==n)){config.Targets=n;changed=true;}
   ImGui.EndPopup();
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
   if(ImGui.Combo("##Job manuel",ref job,Jobs.BilingualLabels,Jobs.BilingualLabels.Length)){config.ManualJob=(GuideJob)job;changed=true;}
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
 public static bool ViewSelector(Configuration config){
  string[] labels=["Les deux","Cycle","Ouverture"];var changed=false;
  var width=(ImGui.GetContentRegionAvail().X-2*ImGui.GetStyle().ItemSpacing.X)/3;
  for(var n=0;n<3;n++){if(n>0)ImGui.SameLine();if(Button(labels[n],width,(int)config.View==n)){config.View=(GuideView)n;changed=true;}}
  return changed;
 }
 public static bool Settings(Configuration config, GuideContext state, bool expresswayAvailable, string dllPath, ref int openerStep, Action resetPosition, Action rebuildFont, string? runtimeError=null) {
  var changed=false;
  ImGui.TextColored(Accent,Guide.JobName(config.Resolve(state).Job).ToUpperInvariant());
  if(ImGui.GetContentRegionAvail().X>ImGui.CalcTextSize(Guide.JobName(config.Resolve(state).Job).ToUpperInvariant()).X+ImGui.CalcTextSize("1.0.0 · expérimental").X+ImGui.GetStyle().ItemSpacing.X)ImGui.SameLine();
  ImGui.TextColored(Muted,"1.0.0 · expérimental");
  var level=config.Resolve(state).Level;
  MutedText(config.ManualLevel?$"Fiche niveau {level} · niveau manuel":state.Available?$"Niveau {level} · synchronisation automatique":"Personnage indisponible · choisis un niveau manuel.");
  if(runtimeError!=null)ImGui.TextWrapped(runtimeError);

  ImGui.Spacing();
  ImGui.BeginDisabled(!config.ManualLevel&&!state.Available);
  if(Button(config.ShowGuide?"Masquer le guide":"Ouvrir le guide",ImGui.GetContentRegionAvail().X,true)){config.ShowGuide=!config.ShowGuide;changed=true;}
  ImGui.EndDisabled();
  changed|=ViewSelector(config);
  MutedText("Une seule fenêtre pour le cycle et l’ouverture. Fermeture par × ou Échap, même verrouillée.");

  Section("FICHE À CONSULTER");
  ImGui.Text("Niveau");
  changed|=LevelSelector(config,state,ref openerStep);
  level=config.Resolve(state).Level;
  ImGui.Spacing();ImGui.Text("Cibles regroupées");
  if(Targets(config,config.Resolve(state))){changed=true;openerStep=0;}
  MutedText(Spells.LocalizeText(Guide.Threshold(config.Resolve(state))));
  ImGui.Spacing();ImGui.Text("Présentation du cycle");ImGui.SetNextItemWidth(-1);
  var layout=(int)config.Layout;
  if(ImGui.Combo("##Affichage",ref layout,Hud.CycleNames,Hud.CycleNames.Length)){config.Layout=(Layout)layout;openerStep=0;changed=true;}
  MutedText(layout==3&&config.Resolve(state).Job!=GuideJob.BlackMage?"Boucle de base et ressources conditionnelles.":Hud.Descriptions[layout]);

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
   ImGui.TextWrapped(GuideSources.Credit(config.Resolve(state).Job)+". Consultation : 19/09/2026. Quêtes de job supposées terminées.");
   MutedText("21 jobs de combat. Mage bleu exclu : ses sorts dépendent des apprentissages. Les départs pédagogiques ne remplacent pas une ouverture de raid adaptée au groupe.");
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
