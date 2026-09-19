using CycleOpener;
int count=0;
void Check(bool condition,string name){if(!condition)throw new Exception(name);count++;}
for(int level=1;level<=100;level++)for(int targets=1;targets<=8;targets++){
 var page=new GuideContext(level,targets);var cycle=Guide.Cycle(page);
 foreach(var step in cycle.Ice.Concat(cycle.Fire).Concat(Guide.Opener(level,targets)))Check(Spells.Get(step.Action).Level<=level,$"Locked action {level}/{targets}/{step.Action}");
 foreach(var r in Guide.Reminders(page))Check(Spells.Get(r.Action).Level<=level,$"Locked reminder {level}/{targets}/{r.Action}");
 Check(cycle.Ice.Length>0,$"Missing cycle {level}/{targets}");
}
Check(Guide.Cycle(new()).Fire.Where(x=>x.Action==3577).Sum(x=>x.Count)==6,"Six F4 in level 100 cycle");
Check(Guide.Cycle(new(Level:60)).Fire.Single(x=>x.Action==3577).Count==7,"Seven F4 below Paradox");
Check(Guide.Cycle(new(Targets:2)).Ice[0].Action==3576,"B4 for two targets");
Check(Guide.Cycle(new(Targets:3)).Ice[0].Action==159,"Freeze for three targets");
Check(Guide.Opener(100,1).Count(x=>x.Action==3577)==12,"5+7 opener");
Check(Guide.Opener(100,1).Count(x=>x.Action==36989)==2,"Two flare stars in opener");
Check(Guide.Opener(100,1)[7].Note.Contains("Vasque"),"Manafont insertion");
var sync=new SyncPromptPolicy();var t=new DateTime(2026,9,19,12,0,0);
Check(sync.Observe(100,t,false,false,true)==null,"No login popup");
Check(sync.Observe(100,t.AddSeconds(3),false,false,true)==null,"Initial stable baseline");
Check(sync.Observe(50,t.AddSeconds(4),false,false,true)==null,"Debounce level change");
Check(sync.Observe(50,t.AddSeconds(7),true,false,true)==null,"Do not interrupt combat/loading");
Check(sync.Observe(50,t.AddSeconds(8),false,false,true)==50,"Propose after combat");
sync.Dismiss();Check(sync.Observe(50,t.AddSeconds(10),false,false,true)==null,"Dismiss once per transition");
sync.Observe(60,t.AddSeconds(11),false,true,true);
Check(sync.Observe(60,t.AddSeconds(14),false,true,true)==null,"Visible HUD adapts without prompt");
sync.Observe(70,t.AddSeconds(15),false,false,false);
Check(sync.Observe(70,t.AddSeconds(18),false,false,false)==null,"Disabled preference");
sync.Reset();sync.Observe(90,t.AddSeconds(20),false,false,true);
Check(sync.Observe(90,t.AddSeconds(23),false,false,true)==null,"Relog resets baseline");
sync.Observe(null,t.AddSeconds(24),true,false,true);
sync.Observe(50,t.AddSeconds(25),true,false,true);
Check(sync.Observe(50,t.AddSeconds(28),false,false,true)==50,"Loading preserves previous level for sync proposal");
Check(Guide.Opener(20,1)[0].Action==142,"Low-level opener starts from ice, not Transpose in neutral");
Check(Guide.Cycle(new(Level:50)).Fire.Last().Action==162,"Flare finisher before Fire IV unlock");
Check(Guide.Cycle(new(Level:60)).Fire.All(x=>x.Action!=162),"Fire IV replaces low-level Flare finisher");
foreach(var level in new[]{26,44,64,80,91,100})Check(Guide.AreaThunder(new(level,2)),"Two-target area Thunder bracket "+level);
foreach(var level in new[]{6,25,45,63,92,99})Check(!Guide.AreaThunder(new(level,2)),"Two-target separate Thunder bracket "+level);
Console.WriteLine($"PASS {count} checks: levels 1–100, 1–8 targets, static cycles, openers and sync popup.");
