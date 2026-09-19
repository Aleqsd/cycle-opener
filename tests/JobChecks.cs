using CycleOpener;
public static class JobChecks {
 public static int Run(){
  int count=0;void Check(bool condition,string label){if(!condition)throw new Exception(label);count++;}
  Check(Jobs.All.Length==21,"All standard combat jobs");
  Check((int)GuideJob.BlackMage==0&&(int)GuideJob.WhiteMage==1,"Saved job values remain stable");
  foreach(var job in Jobs.All){
   Check(Guide.JobFromId(job.Id)==job.Job,"Automatic job ID "+job.Code);
   if(job.ClassId!=0)Check(Guide.JobFromId(job.ClassId)==job.Job,"Starting class alias "+job.Code);
   for(int level=1;level<=100;level++)for(int targets=1;targets<=8;targets++){
    var s=new GuideContext(level,targets,Job:job.Job);var cycle=Guide.Cycle(s);var open=Guide.Opener(s);
    Check(cycle.Ice.Length>0&&open.Count>0,$"Empty reference {job.Code}/{level}/{targets}");
    foreach(var action in cycle.Ice.Concat(cycle.Fire).Concat(open)){
     Check(action.Count>0&&Spells.Get(action.Action).Level<=level,$"Locked step {job.Code}/{level}/{targets}/{action.Action}");
     Check((action.Weaves?.Length??0)<=2,"No triple weaving instruction");
     foreach(var weave in action.Weaves??[])Check(Spells.Get(weave).Level<=level,"Locked weave");
    }
    foreach(var r in Guide.Reminders(s))Check(Spells.Get(r.Action).Level<=level,"Locked reminder");
    var threshold=Guide.MultiThreshold(s);Check(threshold==null||threshold is >=2 and <=5,"Valid minimum target threshold");
    if(job.Job!=GuideJob.BlackMage)Check(!cycle.First.Contains("GLACE")&&!cycle.Second.Contains("FEU"),"No BLM headings for other jobs");
   }
  }
  Check(Guide.JobFromId(26)==GuideJob.Summoner&&Guide.JobFromId(28)==GuideJob.Scholar,"Arcanist does not resolve to Scholar");
  foreach(var id in new uint[]{0,8,16,36,43,999})Check(Guide.JobFromId(id)==null,"Unsupported class/job remains unavailable");
  void Threshold(GuideJob job,int level,int? expected)=>Check(Guide.MultiThreshold(new(level,Job:job))==expected,$"Threshold {job}/{level}");
  Threshold(GuideJob.Paladin,5,null);Threshold(GuideJob.Paladin,39,3);Threshold(GuideJob.Paladin,40,2);Threshold(GuideJob.Paladin,94,3);
  Threshold(GuideJob.DarkKnight,25,2);Threshold(GuideJob.DarkKnight,26,3);Threshold(GuideJob.DarkKnight,40,2);Threshold(GuideJob.DarkKnight,94,3);
  Threshold(GuideJob.Gunbreaker,25,2);Threshold(GuideJob.Gunbreaker,26,3);Threshold(GuideJob.Gunbreaker,40,2);Threshold(GuideJob.Gunbreaker,94,3);
  Threshold(GuideJob.Sage,45,null);Threshold(GuideJob.Sage,46,2);Threshold(GuideJob.Sage,93,2);Threshold(GuideJob.Sage,94,3);
  Threshold(GuideJob.Astrologian,45,2);Threshold(GuideJob.Ninja,100,5);Threshold(GuideJob.Pictomancer,100,3);
  uint Id(GuideJob job,string name)=>JobActions.Id(job,name);
  List<Step> Open(GuideJob job,int l=100,int t=1)=>Guide.Opener(new(l,t,Job:job));
  Check(Open(GuideJob.Reaper,5)[0].Action==Id(GuideJob.Reaper,"Slice"),"Low-level Reaper starts a combo");
  Check(Guide.Cycle(new(1,Job:GuideJob.RedMage)).Fire.Length==0,"No enchanted attacks before mana generation unlocks");
  Check(!Open(GuideJob.Gunbreaker,30,3).Any(s=>s.Action==Id(GuideJob.Gunbreaker,"Burst Strike")),"No cartridge from incomplete low-level AoE combo");
  var pct=Guide.Cycle(new(Job:GuideJob.Pictomancer));
  Check(pct.Ice.Select(s=>s.Action).SequenceEqual(new[]{"Fire in Red","Aero in Green","Water in Blue"}.Select(n=>Id(GuideJob.Pictomancer,n))),"PCT red-green-blue order");
  Check(Open(GuideJob.Samurai).Any(s=>s.Action==Id(GuideJob.Samurai,"Tendo Setsugekka")),"Tendo opener at 100 after Meikyo");
  Check(!Open(GuideJob.Samurai,99).Any(s=>s.Action==Id(GuideJob.Samurai,"Tendo Setsugekka")),"No Tendo below 100");
  Check(Open(GuideJob.Machinist,66).Any(s=>s.Weaves?.SequenceEqual(new[]{Id(GuideJob.Machinist,"Wildfire"),Id(GuideJob.Machinist,"Hypercharge")})==true),"Low-level MCH starts overheat before Heat Blast");
  Spells.ConfigureNames(id=>Spells.Get(id).EnglishName);
  foreach(var action in Spells.All.Values)Check(Spells.Name(action.Id)==action.EnglishName,"Every imported name supports an English client");
  Spells.ConfigureNames(_=>null);
  Console.WriteLine($"PASS {count} additional checks: 21 jobs, every level 1–100, 1–8 targets, unlocks, aliases and resource regressions.");
  return count;
 }
}
