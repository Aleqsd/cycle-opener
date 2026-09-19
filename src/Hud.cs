using System.Numerics;
using Dalamud.Bindings.ImGui;
namespace CycleOpener;
public sealed record HudLook(float Scale=1,float Opacity=.86f,bool Outline=true);
public static class Hud
{
 public static readonly string[] Names=["Fiche express","Frise","Priorités","Deux sections","Ouverture"];
 public static readonly string[] CycleNames=["Fiche express","Frise","Priorités","Deux sections"];
 public static readonly string[] Descriptions=["Le cycle condensé, avec ses répétitions.","L’enchaînement complet, comme un schéma.","Les règles conditionnelles et la boucle de base.","La récupération en glace face à la dépense en feu.","L’ouverture illustrée, à étudier à ton rythme."];
 public static readonly Vector4 Purple=new(.647f,.475f,.839f,1);
 const uint White=0xFFF2EFF5,Muted=0xFFB6ABB9,Violet=0xFFD679A5,Fire=0xFF83A8F0,Ice=0xFFF0C38B,Line=0xFF3B343F,PhaseBg=0x482B2530;
 public static Vector2 Preferred(Layout mode)=>mode switch{Layout.Focus=>new(490,400),Layout.Ruban=>new(770,380),Layout.Priorites=>new(560,600),Layout.Cycle=>new(640,500),_=>new(600,520)};
 public static float Draw(Layout mode,GuideContext context,Func<uint,ImTextureID?> texture,HudLook look,int selectedStep=0,bool preview=false,Action<int>? selectStep=null)
 {
  var origin=ImGui.GetCursorScreenPos();var dl=ImGui.GetWindowDrawList();float scale=look.Scale,w=Math.Max(240,ImGui.GetContentRegionAvail().X/scale);
  var plan=Guide.Cycle(context);var healer=context.Job==GuideJob.WhiteMage;
  var accent=healer?0xFFDCF0FFu:Violet;
  var first=healer?"DÉGÂTS COURANTS":"GLACE · RÉCUPÉRER";
  var second=healer?"RESSOURCES ET BURST":"FEU · DÉPENSER";
  var loop=healer?"Reprendre les dégâts entre les soins nécessaires":"Reprendre depuis la glace";
  List<string> Wrap(string value,float width,float fs) {
   var lines=new List<string>();var line="";
   foreach(var word in value.Split(' ')){var next=line.Length==0?word:line+" "+word;if(width>0&&line.Length>0&&ImGui.CalcTextSize(next).X*fs/ImGui.GetFontSize()>width*scale){lines.Add(line);line=word;}else line=next;}
   lines.Add(line);return lines;
  }
  float Text(string value,float x,float y,uint col=White,float size=16,float max=0){
   value=Spells.LocalizeText(value);
   var fs=size*scale;var lines=Wrap(value,max,fs);var pos=origin+new Vector2(x,y)*scale;
   foreach(var line in lines){if(look.Outline)dl.AddText(ImGui.GetFont(),fs,pos+Vector2.One,0xCC000000,line);dl.AddText(ImGui.GetFont(),fs,pos,col,line);pos.Y+=fs*1.25f;}
   return lines.Count*size*1.25f;
  }
  void Rect(float x,float y,float width,float height,uint col)=>dl.AddRectFilled(origin+new Vector2(x,y)*scale,origin+new Vector2(x+width,y+height)*scale,col);
  void Icon(uint id,float x,float y,float size=40,string note=""){
   var p=origin+new Vector2(x,y)*scale;var end=p+new Vector2(size)*scale;Rect(x-1,y-1,size+2,size+2,Line);
   var t=texture(id);if(t.HasValue)dl.AddImage(t.Value,p,end);else{Rect(x,y,size,size,Line);Text("?",x+10,y+5);}
   if(ImGui.IsMouseHoveringRect(p,end)&&ImGui.IsWindowHovered()){
    ImGui.BeginTooltip();ImGui.PushTextWrapPos(ImGui.GetFontSize()*23);
    var spell=Spells.Get(id);ImGui.TextUnformatted(Spells.Name(id));ImGui.TextDisabled($"Disponible au niveau {spell.Level}");
    if(note.Length>0)ImGui.TextWrapped(Spells.LocalizeText(note));
    ImGui.PopTextWrapPos();ImGui.EndTooltip();
   }
  }
  float Strip(Step[] steps,float y,bool compact,uint color){
   if(steps.Length==0)return y;
   if(!compact)steps=steps.SelectMany(st=>Enumerable.Repeat(st with {Count=1},st.Count)).ToArray();
   float cell=compact?102:110,icon=compact?37:48;int cols=Math.Max(2,(int)((w-24)/cell));cell=(w-24)/cols;float rowHeight=0;
   for(int n=0;n<steps.Length;n++){
    var st=steps[n];float x=12+n%cols*cell,yy=y;Icon(st.Action,x,yy,icon);
    if(st.Count>1){Rect(x+icon-12,yy+icon-15,30,21,0xFF29212F);Text("×"+st.Count,x+icon-9,yy+icon-14,Violet,15);}
    var name=Spells.Name(st.Action);var textHeight=Text(name,x,yy+icon+8,White,14,cell-14);
    if(st.Note.Length>0)textHeight+=5+Text(st.Note,x,yy+icon+13+textHeight,Muted,12,cell-14);
    rowHeight=Math.Max(rowHeight,icon+8+textHeight+22);
    if(n%cols<cols-1&&n<steps.Length-1)Text("→",x+cell-21,yy+12,color,17);
    if(n%cols==cols-1||n==steps.Length-1){y+=rowHeight;rowHeight=0;}
   }
   return y;
  }
  float Phase(string title,Step[] steps,float y,uint color,bool compact){
   if(steps.Length==0)return y;
   var headingHeight=Text(title,20,y+8,color,14,w-40);
   Rect(12,y,3,headingHeight+16,color);y+=headingHeight+28;
   if(!compact)return Strip(steps,y,false,color);
   // Short horizontal cells keep the diagram readable without tall note columns.
   var cols=Math.Max(1,(int)((w-24)/145));var cell=(w-24)/cols;float rowHeight=0;
   for(var n=0;n<steps.Length;n++){
    var st=steps[n];var x=12+n%cols*cell;
    Icon(st.Action,x,y,32,st.Note);
    var h=Text(Spells.Name(st.Action),x+42,y,White,14,cell-58);
    if(st.Count>1)h+=Text("×"+st.Count,x+42,y+h,color,14,cell-58);
    rowHeight=Math.Max(rowHeight,Math.Max(32,h)+18);
    if(n%cols!=cols-1&&n<steps.Length-1)Text("→",x+cell-17,y+8,color,14);
    if(n%cols==cols-1||n==steps.Length-1){y+=rowHeight;rowHeight=0;}
   }
   foreach(var st in steps.Where(st=>st.Note.Length>0))
    y+=Text(Spells.Name(st.Action)+" : "+st.Note,12,y,Muted,13,w-24)+5;
   return y+4;
  }
  float ReminderRow(Reminder r,float y,bool numbered=false,int index=0){
   Icon(r.Action,12,y,34);var h=Text((numbered?$"{index+1}. ":"")+r.Text,60,y,White,15,w-76);
   if(!string.IsNullOrEmpty(r.Threshold)){h+=4+Text(r.Threshold,60,y+h+4,Violet,13,w-76);}
   return y+Math.Max(49,h+17);
  }
  Text($"{Guide.JobName(context.Job).ToUpperInvariant()} · NIVEAU {context.Level}",12,8,accent,15,w-24);
  Text(Guide.Mode(context)+(preview?" · niveau manuel":" · niveau synchronisé"),12,34,Muted,13,w-24);
  float y=68;
  if(mode==Layout.Ouverture){
   var steps=Guide.Opener(context);
   if(steps.Count>0){
    selectedStep=Math.Clamp(selectedStep,0,steps.Count-1);var selected=steps[selectedStep];
    y+=Text(Guide.OpenerName(context),12,y,Violet,16,w-24)+13;
    Icon(selected.Action,12,y,45);var height=Text(Spells.Name(selected.Action),73,y,White,21,w-88);
    y+=Math.Max(73,height+8+Text(string.IsNullOrWhiteSpace(selected.Note)?"Séquence de référence · lecture libre":selected.Note,73,y+height+7,Muted,14,w-88)+12);
    int cols=Math.Max(3,(int)((w-24)/67));float cell=(w-24)/cols;
    for(int n=0;n<steps.Count;n++){float x=12+n%cols*cell,yy=y+n/cols*77;
     if(n==selectedStep)Rect(x-4,yy-4,49,73,PhaseBg);
     Icon(steps[n].Action,x,yy,41,steps[n].Note);Text($"{n+1:00}"+(steps[n].Note.Length>0?" ·":""),x+5,yy+46,n==selectedStep?Violet:Muted,14);if(n==selectedStep)Rect(x,yy+67,41,2,Violet);
     if(n%cols!=cols-1&&n<steps.Count-1)Text("→",x+cell-21,yy+12,Muted,13);
     if(selectStep!=null&&ImGui.IsWindowHovered()&&ImGui.IsMouseHoveringRect(origin+new Vector2(x,yy)*scale,origin+new Vector2(x+41,yy+67)*scale)&&ImGui.IsMouseClicked(ImGuiMouseButton.Left))selectStep(n);
    }
    y+=(steps.Count+cols-1)/cols*77;
    y+=Text("Clique une icône pour lire l’étape. Le point · signale une précision ou une insertion.",12,y,Muted,13,w-24)+15;
   }
  }else if(mode==Layout.Cycle){
   bool stacked=w<510;float cw=stacked?w-24:(w-48)/2;
   float Column(string label,Step[] steps,float x,float top,uint color){
    Rect(x,top,cw,3,color);top+=13+Text(label,x,top+13,color,15,cw)+13;
    foreach(var st in steps){Icon(st.Action,x,top,34);var h=Text(Spells.Name(st.Action)+(st.Count>1?$" ×{st.Count}":""),x+47,top,White,17,cw-50);
     if(st.Note.Length>0)h+=4+Text(st.Note,x+47,top+h+4,Muted,13,cw-50);
     top+=Math.Max(58,h+16);}
    return top;
   }
   float iceEnd=Column(first,plan.Ice,12,y,healer?accent:Ice);
   float fireEnd=plan.Fire.Length>0?Column(second,plan.Fire,stacked?12:cw+36,stacked?iceEnd+18:y,healer?accent:Fire):iceEnd;y=Math.Max(iceEnd,fireEnd)+9;
   y+=Text(loop,12,y,accent,15,w-24)+15;
  }else if(mode==Layout.Priorites){
   Text("BOUCLE DE BASE",12,y,Violet,14);y+=26;
   string Sequence(Step[] seq)=>string.Join(" → ",seq.Select(x=>Spells.Name(x.Action)+(x.Count>1?$" ×{x.Count}":"")));
   y+=Text((healer?"Dégâts : ":"Glace : ")+Sequence(plan.Ice),12,y,healer?accent:Ice,15,w-24)+10;
   if(plan.Fire.Length>0)y+=Text((healer?"Selon ressources : ":"Feu : ")+Sequence(plan.Fire),12,y,healer?accent:Fire,15,w-24)+20;
   Rect(12,y,w-24,1,Line);y+=18;Text("PRIORITÉS CONDITIONNELLES",12,y,Violet,14);y+=29;
   int n=0;foreach(var reminder in Guide.Reminders(context))y=ReminderRow(reminder,y,true,n++);
  }else{
   y=Phase(first,plan.Ice,y,healer?accent:Ice,mode==Layout.Focus);
   y=Phase(second,plan.Fire,y+4,healer?accent:Fire,mode==Layout.Focus);
   y+=Text(loop,12,y,accent,14,w-24)+16;
  }
  if(mode!=Layout.Ouverture){
   Rect(12,y,w-24,1,Line);y+=15;y+=Text(plan.Note,12,y,Muted,14,w-24)+17;
   if(mode!=Layout.Priorites){
    var reminders=Guide.Reminders(context).Take(2).ToArray();
    if(reminders.Length>0){Text("PRIORITÉS À GARDER EN TÊTE",12,y,Violet,13);y+=28;}
    foreach(var reminder in reminders)y=ReminderRow(reminder,y);
   }
  }
  Rect(12,y,w-24,1,Line);y+=13;
  var threshold=Guide.Threshold(context);
  y+=Text(threshold,12,y,Muted,13,w-24)+12;
  y+=Text("Sources · Icy Veins 7.55 / The Balance",12,y,Muted,12,w-24)+4;
  float linkX=12;
  foreach(var source in GuideSources.For(context.Job,mode==Layout.Ouverture)){
   var width=ImGui.CalcTextSize(source.Label).X*12/ImGui.GetFontSize();
   if(linkX+width>w-12){linkX=12;y+=19;}
   Text(source.Label,linkX,y,Muted,12);
   var p=origin+new Vector2(linkX,y)*scale;
   if(ImGui.IsWindowHovered()&&ImGui.IsMouseHoveringRect(p,p+new Vector2(width,16)*scale)){
    ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);ImGui.SetTooltip(source.Url);
    if(ImGui.IsMouseClicked(ImGuiMouseButton.Left))Dalamud.Utility.Util.OpenLink(source.Url);
   }
   linkX+=width+14;
  }
  y+=24;
  y+=Text("Vérifié le 19/09/2026 · noms et icônes : jeu local",12,y,Muted,12,w-24)+10;
  ImGui.Dummy(new Vector2(w*scale,y*scale));return y*scale;
 }
}
