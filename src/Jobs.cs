namespace CycleOpener;

public sealed record JobInfo(GuideJob Job,uint Id,uint ClassId,string Name,string English,string Code,string Slug,string Role,uint Color,string Patch);
public static class Jobs {
 public static readonly JobInfo[] All=[
  new(GuideJob.BlackMage,25,7,"Mage noir","Black Mage","BLM","black-mage","casters",0xA579D6,"7.55"),
  new(GuideJob.WhiteMage,24,6,"Mage blanc","White Mage","WHM","white-mage","healers",0xFFF0DC,"7.55"),
  new(GuideJob.Paladin,19,1,"Paladin","Paladin","PLD","paladin","tanks",0xA8D2E6,"7.4"),
  new(GuideJob.Warrior,21,3,"Guerrier","Warrior","WAR","warrior","tanks",0xCF2621,"7.4"),
  new(GuideJob.DarkKnight,32,0,"Chevalier noir","Dark Knight","DRK","dark-knight","tanks",0xD126CC,"7.5"),
  new(GuideJob.Gunbreaker,37,0,"Pistosabreur","Gunbreaker","GNB","gunbreaker","tanks",0x796D30,"7.4"),
  new(GuideJob.Scholar,28,0,"Érudit","Scholar","SCH","scholar","healers",0x8657FF,"7.4"),
  new(GuideJob.Astrologian,33,0,"Astromancien","Astrologian","AST","astrologian","healers",0xFFE74A,"7.3"),
  new(GuideJob.Sage,40,0,"Sage","Sage","SGE","sage","healers",0x90B0FF,"7.3"),
  new(GuideJob.Monk,20,2,"Moine","Monk","MNK","monk","melee",0xD69C00,"7.4"),
  new(GuideJob.Dragoon,22,4,"Chevalier dragon","Dragoon","DRG","dragoon","melee",0x4164CD,"7.5"),
  new(GuideJob.Ninja,30,29,"Ninja","Ninja","NIN","ninja","melee",0xAF1964,"7.4 / ouverture 7.1"),
  new(GuideJob.Samurai,34,0,"Samouraï","Samurai","SAM","samurai","melee",0xE46D04,"7.4 ; paliers non datés"),
  new(GuideJob.Reaper,39,0,"Faucheur","Reaper","RPR","reaper","melee",0x965A90,"7.55"),
  new(GuideJob.Viper,41,0,"Rôdeur vipère","Viper","VPR","viper","melee",0x108210,"7.55"),
  new(GuideJob.Bard,23,5,"Barde","Bard","BRD","bard","ranged",0x91BA5E,"7.4"),
  new(GuideJob.Machinist,31,0,"Machiniste","Machinist","MCH","machinist","ranged",0x6EE1D6,"7.2 / bases 7.3"),
  new(GuideJob.Dancer,38,0,"Danseur","Dancer","DNC","dancer","ranged",0xE2B0AF,"7.4"),
  new(GuideJob.Summoner,27,26,"Invocateur","Summoner","SMN","summoner","casters",0x2D9B78,"7.55"),
  new(GuideJob.RedMage,35,0,"Mage rouge","Red Mage","RDM","red-mage","casters",0xE87B7B,"7.4"),
  new(GuideJob.Pictomancer,42,0,"Pictomancien","Pictomancer","PCT","pictomancer","casters",0xFC92E1,"7.4 / officiel 7.5")
 ];
 public static JobInfo Get(GuideJob job)=>All[(int)job];
 public static GuideJob? FromId(uint id)=>All.FirstOrDefault(j=>j.Id==id||(j.ClassId!=0&&j.ClassId==id))?.Job;
 public static bool Healer(GuideJob job)=>Get(job).Role=="healers";
 public static bool Tank(GuideJob job)=>Get(job).Role=="tanks";
 public static readonly string[] Labels=All.Select(j=>$"{j.Name} · {j.Code}").ToArray();
 public static readonly string[] BilingualLabels=All.Select(j=>$"{j.Name} / {j.English}").ToArray();
}
