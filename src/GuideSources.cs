namespace CycleOpener;
public sealed record GuideSource(string Label,string Url);
public static class GuideSources {
 public static GuideSource[] For(GuideJob job,bool opening){
  var slug=job==GuideJob.WhiteMage?"white-mage":"black-mage";
  var role=job==GuideJob.WhiteMage?"healers":"casters";
  return [
   new("Icy Veins",$"https://www.icy-veins.com/ffxiv/{slug}-"+(opening?(job==GuideJob.WhiteMage?"dps-rotation-for-healers":"pve-dps-rotation-openers-abilities"):"leveling")),
   new("The Balance",$"https://www.thebalanceffxiv.com/jobs/{role}/{slug}/"+(opening?"openers/":"leveling-guide/")),
   new("Guide officiel",$"https://fr.finalfantasyxiv.com/jobguide/{(job==GuideJob.WhiteMage?"whitemage":"blackmage")}/")];
 }
}
