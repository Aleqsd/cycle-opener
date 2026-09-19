using System.Numerics;
using Dalamud.Bindings.ImGui;

namespace CycleOpener;

// Presentation state is independent of the character and never drives combat.
public sealed class GuidePanelState {
 public bool Folded;
 public bool Healing;
 public Vector2 ExpandedSize;
 public bool RestoreSize;
 public float HeaderHeight=56;
 public GuideContext? Context;
}

public static class GuidePanel {
 public static readonly Vector4 Background=new(.045f,.045f,.05f,1);
 static readonly Vector4 Muted=new(.72f,.72f,.75f,1);
 public static Vector4 Accent(GuideJob job){
  var rgb=Jobs.Get(job).Color;var c=new Vector4((rgb>>16&255)/255f,(rgb>>8&255)/255f,(rgb&255)/255f,1);
  // LMeter job hues, lifted only for small text on charcoal (e.g. GNB / NIN / VPR).
  return Vector4.Lerp(c,Vector4.One,.22f);
 }
 public static void PushStyle(Configuration config){
  Panels.PushTheme();
  ImGui.PushStyleColor(ImGuiCol.WindowBg,Background with {W=config.BackgroundOpacity});
  ImGui.PushStyleColor(ImGuiCol.Border,new Vector4(.23f,.23f,.25f,.7f));
  ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize,1);
 }
 public static void PopStyle(){ImGui.PopStyleVar();ImGui.PopStyleColor(2);Panels.PopTheme();}

 // Controls use the user's global font scale, independent of the diagram's zoom.
 public static bool Header(Configuration config,GuideContext actual,GuidePanelState ui,Func<uint,ImTextureID?> icon,Action settings){
  var state=config.Resolve(actual);var accent=Accent(state.Job);var changed=false;
  var scale=ImGui.GetFontSize()/17;var start=ImGui.GetCursorScreenPos();var width=ImGui.GetContentRegionAvail().X;
  var small=width<440*scale;var size=29*scale;var gap=4*scale;var tools=4*size+3*gap;
  var titleWidth=small?width:width-tools-12*scale;var dl=ImGui.GetWindowDrawList();
  var texture=icon(Guide.JobIcon(state.Job));
  if(texture.HasValue)dl.AddImage(texture.Value,start+new Vector2(0,6)*scale,start+new Vector2(30,36)*scale);
  else dl.AddText(start+new Vector2(0,14)*scale,ImGui.GetColorU32(accent),Jobs.Get(state.Job).Code);
  var text=start+new Vector2(40,0)*scale;
  dl.AddText(ImGui.GetFont(),12*scale,text,ImGui.GetColorU32(Muted),"CYCLE & OPENER");
  dl.AddText(text+new Vector2(0,18)*scale,ImGui.GetColorU32(accent),$"{Guide.JobName(state.Job)} · {state.Level}");
  ImGui.InvisibleButton("Déplacer le guide",new(titleWidth,44*scale));
  if(ImGui.IsItemActive()&&!config.Locked&&ImGui.IsMouseDragging(ImGuiMouseButton.Left))ImGui.SetWindowPos(ImGui.GetWindowPos()+ImGui.GetIO().MouseDelta);
  if(ImGui.IsItemHovered())ImGui.SetTooltip(config.Locked?"Position verrouillée. Le cadenas permet de la déverrouiller.":"Glisser pour déplacer le guide.");
  ImGui.SetCursorScreenPos(start+new Vector2(width-tools,small?48*scale:5*scale));
  if(Tool("Replier le guide",ui.Folded?"expand":"fold",size,ui.Folded)){
   if(!ui.Folded)ui.ExpandedSize=ImGui.GetWindowSize();else ui.RestoreSize=true;
   ui.Folded=!ui.Folded;
  }
  ImGui.SameLine(0,gap);
  if(Tool("Verrouillage",config.Locked?"locked":"unlock",size,config.Locked)){config.Locked=!config.Locked;changed=true;}
  ImGui.SameLine(0,gap);if(Tool("Réglages","settings",size))settings();
  ImGui.SameLine(0,gap);if(Tool("Fermer le guide","close",size)){config.ShowGuide=false;changed=true;}
  var h=(small?82:46)*scale;
  ImGui.SetCursorScreenPos(start+new Vector2(0,h));
  ui.HeaderHeight=h+2*ImGui.GetStyle().WindowPadding.Y;
  if(!ui.Folded){ImGui.Separator();ImGui.Spacing();}
  return changed;
 }
 static bool Tool(string name,string glyph,float size,bool selected=false){
  var p=ImGui.GetCursorScreenPos();
  if(selected)ImGui.PushStyleColor(ImGuiCol.Button,new Vector4(.31f,.26f,.36f,1));
  var clicked=ImGui.Button("##"+name,new(size));if(selected)ImGui.PopStyleColor();
  var dl=ImGui.GetWindowDrawList();var color=ImGui.GetColorU32(ImGuiCol.Text);var s=size/29;var c=p+new Vector2(size/2);
  void Line(float x,float y,float xx,float yy)=>dl.AddLine(c+new Vector2(x,y)*s,c+new Vector2(xx,yy)*s,color,1.5f*s);
  switch(glyph){
   case "close":Line(-4,-4,4,4);Line(-4,4,4,-4);break;
   case "fold":Line(-5,0,5,0);break;
   case "expand":Line(-5,0,5,0);Line(0,-5,0,5);break;
   case "settings":
    // A continuous toothed outline reads as a gear at small sizes.
    Vector2 GearPoint(int n,float radius)=>c+new Vector2(MathF.Cos(n*MathF.PI/16),MathF.Sin(n*MathF.PI/16))*radius*s;
    for(var i=0;i<32;i++)dl.PathLineTo(GearPoint(i,i%4 is 1 or 2?8.5f:6));
    dl.PathStroke(color,ImDrawFlags.Closed,1.6f*s);
    dl.AddCircle(c,2.6f*s,color,24,1.5f*s);break;
   default:
    dl.AddRect(c+new Vector2(-6.5f,0)*s,c+new Vector2(6.5f,8)*s,color,2*s,ImDrawFlags.None,1.8f*s);
    var open=glyph=="unlock";var cx=open?3f:0f;var cy=open?-3f:-1f;
    Line(cx-4.5f,0,cx-4.5f,cy);
    for(var i=0;i<16;i++){
     var a=MathF.PI+i*MathF.PI/16;var b=a+MathF.PI/16;
     Line(cx+MathF.Cos(a)*4.5f,cy+MathF.Sin(a)*4.5f,cx+MathF.Cos(b)*4.5f,cy+MathF.Sin(b)*4.5f);
    }
    Line(cx+4.5f,cy,cx+4.5f,open?-1:0);
    dl.AddCircleFilled(c+new Vector2(0,3.5f)*s,1.25f*s,color,12);Line(0,4,0,6);break;
  }
  if(ImGui.IsItemHovered())ImGui.SetTooltip(name=="Replier le guide"&&selected?"Déplier le guide":name=="Verrouillage"?selected?"Déverrouiller la position":"Verrouiller la position":name);
  return clicked;
 }
 public static bool Controls(Configuration config,GuideContext actual,ref int step){
  var changed=false;var scale=ImGui.GetFontSize()/17;var width=ImGui.GetContentRegionAvail().X;
  ImGui.SetNextItemWidth(112*scale);
  if(ImGui.BeginCombo("##Mode du niveau",config.ManualLevel?"Manuel":"Auto")){
   ImGui.BeginDisabled(!actual.Available);
   if(ImGui.Selectable("Auto",!config.ManualLevel)&&config.ManualLevel){config.ManualLevel=false;changed=true;}
   ImGui.EndDisabled();
   if(ImGui.Selectable("Manuel",config.ManualLevel)&&!config.ManualLevel){config.ManualLevel=true;if(actual.Available)config.ManualJob=actual.Job;changed=true;}
   ImGui.EndCombo();
  }
  ImGui.SameLine();
  if(config.ManualLevel){
   ImGui.SetNextItemWidth(Math.Min(140*scale,width-120*scale));var level=config.PreviewLevel;
   if(ImGui.InputInt("##Niveau",ref level,1,10)){config.PreviewLevel=Math.Clamp(level,1,100);changed=true;}
   if(ImGui.IsItemHovered())ImGui.SetTooltip("Niveau 1 à 100. Saisie directe, −/+ ; Ctrl pour changer de 10 niveaux.");
   if(width>=440*scale)ImGui.SameLine();
   changed|=JobPicker.Draw(config,"Job",ImGui.GetContentRegionAvail().X);
  }else ImGui.TextColored(Muted,actual.Available?$"Niv. {actual.Level} · synchronisé":"Niveau indisponible");
  changed|=Panels.Targets(config,config.Resolve(actual));
  if(changed)step=0;
  var hint=Guide.LevelHint(config.Resolve(actual));
  if(hint.Length>0){ImGui.PushStyleColor(ImGuiCol.Text,Muted);ImGui.TextWrapped(hint);ImGui.PopStyleColor();}
  ImGui.Spacing();return changed;
 }
 public static void Content(Configuration config,GuideContext state,GuidePanelState ui,Func<uint,ImTextureID?> icon,HudLook look,ref int step){
  var selected=step;
  if(ui.Context!=state){ImGui.SetScrollY(0);ui.Context=state;}
  Hud.Draw(Layout.Ouverture,state,icon,look,selected,config.ManualLevel,n=>selected=n,embedded:true);
  ImGui.Spacing();ImGui.Separator();ImGui.Spacing();
  Hud.Draw(config.Layout==Layout.Ouverture?Layout.Focus:config.Layout,state,icon,look,selected,config.ManualLevel,embedded:true);
   if(Jobs.Healer(state.Job)||Jobs.Tank(state.Job)){
    if(ImGui.CollapsingHeader(Jobs.Tank(state.Job)?"Protection et tanking":"Soins et urgences",ui.Healing?ImGuiTreeNodeFlags.DefaultOpen:ImGuiTreeNodeFlags.None)){
     ImGui.TextWrapped("Des réponses à une situation, pas un cycle fixe. Anticiper les dégâts et garder une réponse disponible.");
     Hud.Draw(config.Layout,state,icon,look,embedded:true,healingOnly:true);
    }
   }
  Hud.Sources(state,false,true,look);
  step=selected;
 }
}
