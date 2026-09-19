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
 public static float Draw(Layout mode,GuideContext context,Func<uint,ImTextureID?> texture,HudLook look,int selectedStep=0,bool preview=false,Action<int>? selectStep=null,bool embedded=false,bool healingOnly=false)
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
  float y=8;
  if(!embedded){
   y+=Text($"{Guide.JobName(context.Job).ToUpperInvariant()} · NIVEAU {context.Level}",12,y,accent,15,w-24)+7;
   y+=Text(Guide.Mode(context)+(preview?" · niveau manuel":" · niveau synchronisé"),12,y,Muted,13,w-24)+20;
  }else if(!healingOnly&&mode!=Layout.Ouverture)y+=Text(healer?"DÉGÂTS / PRIORITÉS":"CYCLE / PRIORITÉS",12,y,accent,15,w-24)+12;
  if(healingOnly){
   foreach(var reminder in Guide.Reminders(context).Where(r=>r.Healing))y=ReminderRow(reminder,y);
   ImGui.Dummy(new Vector2(w*scale,y*scale));return y*scale;
  }
  if(mode==Layout.Ouverture){
   var steps=Guide.Opener(context);
   if(steps.Count>0){
    selectedStep=Math.Clamp(selectedStep,0,steps.Count-1);var selected=steps[selectedStep];
    y+=Text(Guide.OpenerName(context),12,y,accent,15,w-24)+13;
    var groups=Guide.OpeningGroups(context);
    int cols=Math.Max(2,(int)((w-24)/94));float cell=(w-24)/cols;
    for(int n=0;n<groups.Count;n++){var group=groups[n];float x=12+n%cols*cell,yy=y+n/cols*106;
     var active=selectedStep>=group.Start&&selectedStep<group.Start+group.Count;
     if(active)Rect(x-4,yy-3,cell-7,99,PhaseBg);
     Text(group.Count>1?$"{group.Start+1:00}–{group.Start+group.Count:00}":$"{group.Start+1:00}",x,yy,active?accent:Muted,12);
     Icon(group.Step.Action,x,yy+22,group.Step.Action==149?26:39,group.Step.Note);
     if(group.Count>1)Text("×"+group.Count,x+44,yy+32,accent,14);
     if(group.Step.Weaves is {Length:>0} weaves){
      Rect(x+3,yy+68,1,12,Muted);Rect(x+3,yy+79,10,1,Muted);
      for(var k=0;k<weaves.Length;k++)Icon(weaves[k],x+19+k*25,yy+68,21,"À insérer après ce sort ; "+group.Step.Note);
     }
     if(active)Rect(x,yy+94,cell-16,2,accent);
     if(n%cols!=cols-1&&n<groups.Count-1)Text("→",x+cell-22,yy+32,Muted,13);
     if(selectStep!=null){
      // Native hit target for keyboard navigation as well as mouse selection.
      var restore=ImGui.GetCursorScreenPos();ImGui.SetCursorScreenPos(origin+new Vector2(x,yy)*scale);
      if(ImGui.InvisibleButton("Étape##"+group.Start,new Vector2(cell-14,64)*scale))selectStep(group.Start);
      ImGui.SetCursorScreenPos(restore);
     }
    }
    y+=(groups.Count+cols-1)/cols*106;
    y+=Text($"{selectedStep+1:00} · {Spells.Name(selected.Action)}",12,y,White,16,w-24)+5;
    y+=Text(string.IsNullOrWhiteSpace(selected.Note)?"Reprendre la séquence ; adapter les priorités si nécessaire.":selected.Note,12,y,Muted,13,w-24)+12;
    y+=Text("Petites icônes : aptitudes à insérer · clic : détail de l’étape",12,y,Muted,12,w-24)+10;
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
   int n=0;foreach(var reminder in Guide.Reminders(context).Where(r=>!embedded||!r.Healing))y=ReminderRow(reminder,y,true,n++);
  }else{
   y=Phase(first,plan.Ice,y,healer?accent:Ice,mode==Layout.Focus);
   y=Phase(second,plan.Fire,y+4,healer?accent:Fire,mode==Layout.Focus);
   y+=Text(loop,12,y,accent,14,w-24)+16;
  }
  if(mode!=Layout.Ouverture){
   Rect(12,y,w-24,1,Line);y+=15;y+=Text(plan.Note,12,y,Muted,14,w-24)+17;
   if(mode!=Layout.Priorites){
    var reminders=Guide.Reminders(context).Where(r=>!embedded||!r.Healing).Take(2).ToArray();
    if(reminders.Length>0){Text("PRIORITÉS À GARDER EN TÊTE",12,y,Violet,13);y+=28;}
    foreach(var reminder in reminders)y=ReminderRow(reminder,y);
   }
  }
  if(embedded){ImGui.Dummy(new Vector2(w*scale,y*scale));return y*scale;}
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
 public static void Sources(GuideContext state,bool opening,bool both,HudLook look){
  ImGui.Spacing();ImGui.Separator();ImGui.Spacing();
  var origin=ImGui.GetCursorScreenPos();var dl=ImGui.GetWindowDrawList();var size=12*look.Scale;var width=ImGui.GetContentRegionAvail().X;var y=0f;
  void Line(string value){
   var line="";
   foreach(var word in value.Split(' ')){
    var next=line.Length==0?word:line+" "+word;
    if(line.Length>0&&ImGui.CalcTextSize(next).X*size/ImGui.GetFontSize()>width){dl.AddText(ImGui.GetFont(),size,origin+new Vector2(0,y),Muted,line);y+=size*1.4f;line=word;}else line=next;
   }
   dl.AddText(ImGui.GetFont(),size,origin+new Vector2(0,y),Muted,line);y+=size*1.6f;
  }
  Line(Spells.LocalizeText(Guide.Threshold(state)));
  Line("Sources · Icy Veins 7.55 / The Balance · vérifié le 19/09/2026");
  var links=GuideSources.For(state.Job,opening).ToList();
  if(both){links.RemoveAt(2);links.AddRange(GuideSources.For(state.Job,true).Take(2).Select(s=>s with{Label=s.Label+" · ouverture"}));links.Add(GuideSources.For(state.Job,false)[2]);}
  if(!opening&&state.Job==GuideJob.WhiteMage)links.Add(new("Soins · The Balance","https://www.thebalanceffxiv.com/jobs/healers/white-mage/basic-guide/"));
  var x=0f;
  foreach(var source in links){
   var linkWidth=ImGui.CalcTextSize(source.Label).X*size/ImGui.GetFontSize();
   if(x>0&&x+linkWidth>width){x=0;y+=size*1.8f;}
   var position=origin+new Vector2(x,y);dl.AddText(ImGui.GetFont(),size,position,Muted,source.Label);
   ImGui.SetCursorScreenPos(position);
   if(ImGui.InvisibleButton(source.Label,new Vector2(linkWidth,size*1.5f)))Dalamud.Utility.Util.OpenLink(source.Url);
   if(ImGui.IsItemHovered())ImGui.SetTooltip(source.Url);
   x+=linkWidth+16*look.Scale;
  }
  ImGui.SetCursorScreenPos(origin);ImGui.Dummy(new Vector2(width,y+size*2));
 }
}
