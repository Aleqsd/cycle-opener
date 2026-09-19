using System.Numerics;
using Dalamud.Bindings.ImGui;
namespace CycleOpener;
public sealed record HudLook(float Scale=1,float Opacity=.86f,bool Outline=true);
public static class Hud
{
 public static readonly string[] Names=["Fiche express","Frise","Priorités","Deux phases","Ouverture"];
 public static readonly string[] Descriptions=["Le cycle condensé, avec ses répétitions.","L’enchaînement complet, comme un schéma.","Les règles conditionnelles et la boucle de base.","La récupération en glace face à la dépense en feu.","L’ouverture illustrée, à étudier à ton rythme."];
 public static readonly Vector4 Purple=new(.647f,.475f,.839f,1);
 const uint White=0xFFF2EFF5,Muted=0xFFB6ABB9,Violet=0xFFD679A5,Fire=0xFF83A8F0,Ice=0xFFF0C38B,Line=0xFF3B343F;
 public static Vector2 Preferred(Layout mode)=>mode switch{Layout.Focus=>new(440,400),Layout.Ruban=>new(770,380),Layout.Priorites=>new(560,600),Layout.Cycle=>new(640,500),_=>new(670,520)};
 public static float Draw(Layout mode,GuideContext context,Func<uint,ImTextureID?> texture,HudLook look,int selectedStep=0,bool preview=false,Action<int>? selectStep=null)
 {
  var origin=ImGui.GetCursorScreenPos();var dl=ImGui.GetWindowDrawList();float scale=look.Scale,w=Math.Max(240,ImGui.GetContentRegionAvail().X/scale);
  var plan=Guide.Cycle(context);
  List<string> Wrap(string value,float width,float fs) {
   var lines=new List<string>();var line="";
   foreach(var word in value.Split(' ')){var next=line.Length==0?word:line+" "+word;if(width>0&&line.Length>0&&ImGui.CalcTextSize(next).X*fs/ImGui.GetFontSize()>width*scale){lines.Add(line);line=word;}else line=next;}
   lines.Add(line);return lines;
  }
  float Text(string value,float x,float y,uint col=White,float size=16,float max=0){
   var fs=size*scale;var lines=Wrap(value,max,fs);var pos=origin+new Vector2(x,y)*scale;
   foreach(var line in lines){if(look.Outline)dl.AddText(ImGui.GetFont(),fs,pos+Vector2.One,0xCC000000,line);dl.AddText(ImGui.GetFont(),fs,pos,col,line);pos.Y+=fs*1.25f;}
   return lines.Count*size*1.25f;
  }
  void Rect(float x,float y,float width,float height,uint col)=>dl.AddRectFilled(origin+new Vector2(x,y)*scale,origin+new Vector2(x+width,y+height)*scale,col);
  void Icon(uint id,float x,float y,float size=40){
   var p=origin+new Vector2(x,y)*scale;var end=p+new Vector2(size)*scale;Rect(x-1,y-1,size+2,size+2,Line);
   var t=texture(id);if(t.HasValue)dl.AddImage(t.Value,p,end);else{Rect(x,y,size,size,Line);Text("?",x+10,y+5);}
   if(ImGui.IsMouseHoveringRect(p,end)){ImGui.BeginTooltip();ImGui.TextUnformatted(Spells.Get(id).Name);ImGui.EndTooltip();}
  }
  float Strip(Step[] steps,float y,bool compact,uint color){
   if(steps.Length==0)return y;
   if(!compact)steps=steps.SelectMany(st=>Enumerable.Repeat(st with {Count=1},st.Count)).ToArray();
   float cell=compact?102:110,icon=compact?37:48;int cols=Math.Max(2,(int)((w-24)/cell));cell=(w-24)/cols;float rowHeight=0;
   for(int n=0;n<steps.Length;n++){
    var st=steps[n];float x=12+n%cols*cell,yy=y;Icon(st.Action,x,yy,icon);
    if(st.Count>1){Rect(x+icon-12,yy+icon-15,30,21,0xFF29212F);Text("×"+st.Count,x+icon-9,yy+icon-14,Violet,15);}
    var name=Spells.Get(st.Action).Name;var textHeight=Text(name,x,yy+icon+8,White,14,cell-14);
    if(st.Note.Length>0)textHeight+=5+Text(st.Note,x,yy+icon+13+textHeight,Muted,12,cell-14);
    rowHeight=Math.Max(rowHeight,icon+8+textHeight+22);
    if(n%cols<cols-1&&n<steps.Length-1)Text("→",x+cell-21,yy+12,color,17);
    if(n%cols==cols-1||n==steps.Length-1){y+=rowHeight;rowHeight=0;}
   }
   return y;
  }
  float Phase(string title,Step[] steps,float y,uint color,bool compact){
   Text(title,12,y,color,14);y+=28;return Strip(steps,y,compact,color);
  }
  float ReminderRow(Reminder r,float y,bool numbered=false,int index=0){
   Icon(r.Action,12,y,34);var h=Text((numbered?$"{index+1}. ":"")+r.Text,60,y,White,15,w-76);
   if(!string.IsNullOrEmpty(r.Threshold)){h+=4+Text(r.Threshold,60,y+h+4,Violet,13,w-76);}
   return y+Math.Max(49,h+17);
  }
  Text("CYCLE & OPENER",12,8,Violet,13);Text(w<330?$"BLM · Niv. {context.Level}":$"MAGE NOIR · Niv. {context.Level}",w<330?12:163,w<330?29:8,Muted,14);
  Rect(0,w<330?52:35,w,1,Line);Text(Guide.Mode(context),12,w<330?64:48,White,14);Text("FICHE DE CYCLE",w<330?12:w-156,w<330?86:48,Muted,13);
  float y=w<330?118:80;
  if(mode==Layout.Ouverture){
   var steps=Guide.Opener(context.Level,context.Targets);
   if(steps.Count>0){
    selectedStep=Math.Clamp(selectedStep,0,steps.Count-1);var selected=steps[selectedStep];
    Text(Guide.OpenerName(context),12,y,Violet,16);y+=29;
    Icon(selected.Action,12,y,45);var height=Text(Spells.Get(selected.Action).Name,73,y,White,21,w-88);
    y+=Math.Max(73,height+8+Text(string.IsNullOrWhiteSpace(selected.Note)?"Séquence de référence · lecture libre":selected.Note,73,y+height+7,Muted,14,w-88)+12);
    int cols=Math.Max(3,(int)((w-24)/67));float cell=(w-24)/cols;
    for(int n=0;n<steps.Count;n++){float x=12+n%cols*cell,yy=y+n/cols*77;Icon(steps[n].Action,x,yy,41);Text($"{n+1:00}",x+10,yy+46,n==selectedStep?Violet:Muted,14);if(n==selectedStep)Rect(x,yy+67,41,2,Violet);
     if(selectStep!=null&&ImGui.IsMouseHoveringRect(origin+new Vector2(x,yy)*scale,origin+new Vector2(x+41,yy+67)*scale)&&ImGui.IsMouseClicked(ImGuiMouseButton.Left))selectStep(n);
    }
    y+=(steps.Count+cols-1)/cols*77;
    y+=Text("Les insertions sont détaillées sur l’étape sélectionnée.",12,y,Muted,14,w-24)+15;
   }
  }else if(mode==Layout.Cycle){
   bool stacked=w<510;float cw=stacked?w-24:(w-48)/2;
   float Column(string label,Step[] steps,float x,float top,uint color){
    Rect(x,top,cw,3,color);Text(label,x,top+13,color,15);top+=45;
    foreach(var st in steps){Icon(st.Action,x,top,34);var h=Text(Spells.Get(st.Action).Name+(st.Count>1?$" ×{st.Count}":""),x+47,top,White,17,cw-50);
     if(st.Note.Length>0)h+=4+Text(st.Note,x+47,top+h+4,Muted,13,cw-50);
     top+=Math.Max(58,h+16);}
    return top;
   }
   float iceEnd=Column("01 · GLACE / RECHARGER",plan.Ice,12,y,Ice);
   float fireEnd=Column("02 · FEU / DÉPENSER",plan.Fire,stacked?12:cw+36,stacked?iceEnd+18:y,Fire);y=Math.Max(iceEnd,fireEnd)+9;
   Text("Revenir à la phase de glace",12,y,Violet,15);y+=35;
  }else if(mode==Layout.Priorites){
   Text("BOUCLE DE BASE",12,y,Violet,14);y+=26;
   string Sequence(Step[] seq)=>string.Join(" → ",seq.Select(x=>Spells.Get(x.Action).Name+(x.Count>1?$" ×{x.Count}":"")));
   y+=Text("Glace : "+Sequence(plan.Ice),12,y,Ice,15,w-24)+10;
   y+=Text("Feu : "+Sequence(plan.Fire),12,y,Fire,15,w-24)+20;
   Rect(12,y,w-24,1,Line);y+=18;Text("PRIORITÉS CONDITIONNELLES",12,y,Violet,14);y+=29;
   int n=0;foreach(var reminder in Guide.Reminders(context))y=ReminderRow(reminder,y,true,n++);
  }else{
   y=Phase("01 · GLACE — RÉCUPÉRER",plan.Ice,y,Ice,mode==Layout.Focus);
   y=Phase("02 · FEU — DÉPENSER",plan.Fire,y+4,Fire,mode==Layout.Focus);
   Text("Reprendre depuis la glace",12,y,Violet,14);y+=34;
  }
  if(mode!=Layout.Ouverture){
   Rect(12,y,w-24,1,Line);y+=15;y+=Text(plan.Note,12,y,Muted,14,w-24)+17;
   if(mode!=Layout.Priorites){foreach(var reminder in Guide.Reminders(context).Take(mode==Layout.Focus?1:2))y=ReminderRow(reminder,y);}
  }
  Rect(12,y,w-24,1,Line);y+=13;
  var threshold=context.Level<12?"Pas de cycle de zone avant le niveau 12.":context.Level>=100?"ZONE : 2 cibles · Giga Glace à 2 / Gel à 3+":context.Targets==2?"2 cibles : base mono · zone complète dès 3+":"ZONE : au moins 3 cibles regroupées";
  y+=Text(threshold,12,y,Muted,13,w-24)+12;
  ImGui.Dummy(new Vector2(w*scale,y*scale));return y*scale;
 }
}
