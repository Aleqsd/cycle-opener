namespace CycleOpener;
public sealed record GuideSource(string Label,string Url);
public static class GuideSources {
 public static GuideSource[] For(GuideJob job,bool opening){
  var info=Jobs.Get(job);var slug=info.Slug;var role=info.Role;
  return [
   new("Icy Veins",$"https://www.icy-veins.com/ffxiv/{slug}-"+(opening?(Jobs.Healer(job)?"dps-rotation-for-healers":Jobs.Tank(job)?"pve-tank-rotation-openers-abilities":"pve-dps-rotation-openers-abilities"):"leveling")),
   new("The Balance",$"https://www.thebalanceffxiv.com/jobs/{role}/{slug}/"+(opening?"openers/":"leveling-guide/")),
   new("Guide officiel",$"https://fr.finalfantasyxiv.com/jobguide/{slug.Replace("-","")}/")];
 }
 public static string Credit(GuideJob job)=>(int)job<2?"Sources · Icy Veins 7.55 / The Balance":"Source · The Balance "+Jobs.Get(job).Patch+" · adaptation pédagogique";
}
