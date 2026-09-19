using System.Numerics;
using Dalamud.Bindings.ImGui;

namespace CycleOpener;

// One picker shared by the guide and settings. Only favourites are persisted.
public static class JobPicker {
 public static readonly string[] Filters=["Tous","Favoris","Tanks","Soins","Mêlée","Distance","Magie"];
 static readonly string[] Roles=["tanks","healers","melee","ranged","casters"];
 public static IEnumerable<JobInfo> Choices(Configuration config,int filter)=>Jobs.All.Where(j=>filter==0||filter==1&&config.FavoriteJobs.Contains(j.Job)||filter>=2&&filter<Filters.Length&&j.Role==Roles[filter-2]);
 public static bool Draw(Configuration config,string id,float width){
  var changed=false;var scale=ImGui.GetFontSize()/17;var job=Jobs.Get(config.ManualJob);
  ImGui.PushID(id);ImGui.SetNextItemWidth(width);
  var storage=ImGui.GetStateStorage();var filterId=ImGui.GetID("Filtre de rôle");var filter=storage.GetInt(filterId,0);
  ImGui.SetNextWindowSizeConstraints(new(Math.Max(width,290*scale),0),new(Math.Max(width,290*scale),440*scale));
  if(ImGui.BeginCombo("##Choisir un job",$"{job.Name} · {job.Code}")){
   float used=0;var available=ImGui.GetContentRegionAvail().X;var gap=ImGui.GetStyle().ItemSpacing.X;
   for(var n=0;n<Filters.Length;n++){
    var size=ImGui.CalcTextSize(Filters[n]).X+2*ImGui.GetStyle().FramePadding.X;
    if(used>0&&used+gap+size<=available){ImGui.SameLine();used+=gap;}else used=0;
    var selected=filter==n;
    if(selected)ImGui.PushStyleColor(ImGuiCol.Button,new Vector4(.34f,.23f,.46f,1));
    if(ImGui.Button(Filters[n])){filter=n;storage.SetInt(filterId,n);}
    if(selected)ImGui.PopStyleColor();
    used+=size;
   }
   ImGui.Spacing();ImGui.Separator();ImGui.Spacing();
   ImGui.PushStyleColor(ImGuiCol.Text,ImGui.GetStyle().Colors[(int)ImGuiCol.TextDisabled]);
   ImGui.TextWrapped("Étoile : ajouter ou retirer des favoris");ImGui.PopStyleColor();
   if(ImGui.BeginChild("Liste des jobs",new(0,260*scale),false)){
    var choices=Choices(config,filter).ToArray();
    if(choices.Length==0)ImGui.TextWrapped("Aucun favori. Choisis un rôle, puis utilise l’étoile d’un job.");
    foreach(var role in Roles){
     var group=choices.Where(j=>j.Role==role).ToArray();if(group.Length==0)continue;
     ImGui.TextDisabled(Filters[Array.IndexOf(Roles,role)+2]);
     foreach(var item in group){
      ImGui.PushID((int)item.Job);
      var h=ImGui.GetFrameHeight();var remaining=ImGui.GetContentRegionAvail().X;
      if(Star(config.FavoriteJobs.Contains(item.Job),h)){
       if(!config.FavoriteJobs.Remove(item.Job))config.FavoriteJobs.Add(item.Job);changed=true;
      }
      ImGui.SameLine();
      if(ImGui.Selectable($"{item.Name} · {item.Code}",item.Job==config.ManualJob,ImGuiSelectableFlags.DontClosePopups,new(remaining-h-ImGui.GetStyle().ItemSpacing.X,h))){
       config.ManualJob=item.Job;changed=true;ImGui.CloseCurrentPopup();
      }
      ImGui.PopID();
     }
     ImGui.Spacing();
    }
   }
   ImGui.EndChild();ImGui.EndCombo();
  }
  ImGui.PopID();return changed;
 }
 static bool Star(bool selected,float size){
  var p=ImGui.GetCursorScreenPos();var clicked=ImGui.InvisibleButton("Favori",new(size));var dl=ImGui.GetWindowDrawList();
  var hovered=ImGui.IsItemHovered();if(hovered)dl.AddRectFilled(p,p+new Vector2(size),ImGui.GetColorU32(ImGuiCol.ButtonHovered),3);
  var center=p+new Vector2(size/2);var color=ImGui.GetColorU32(selected?new Vector4(.95f,.79f,.41f,1):new Vector4(.68f,.68f,.72f,1));
  Vector2 Point(int n){var angle=-MathF.PI/2+n*MathF.PI/5;return center+new Vector2(MathF.Cos(angle),MathF.Sin(angle))*size*(n%2==0?.32f:.14f);}
  for(var i=0;i<10;i++){var a=Point(i);var b=Point((i+1)%10);if(selected)dl.AddTriangleFilled(center,a,b,color);else dl.AddLine(a,b,color,1.2f);}
  if(hovered)ImGui.SetTooltip(selected?"Retirer des favoris":"Ajouter aux favoris");
  return clicked;
 }
}
