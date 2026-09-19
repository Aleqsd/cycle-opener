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
Check(sync.Observe(100,t,false,false,true,inDuty:true)==null,"No login popup");
Check(sync.Observe(100,t.AddSeconds(3),false,false,true,inDuty:true)==null,"Initial stable baseline");
Check(sync.Observe(50,t.AddSeconds(4),false,false,true,inDuty:true)==null,"Debounce level change");
Check(sync.Observe(50,t.AddSeconds(7),true,false,true,inDuty:true)==null,"Do not interrupt combat/loading");
Check(sync.Observe(50,t.AddSeconds(8),false,false,true,inDuty:true)==50,"Propose after combat");
sync.Dismiss();Check(sync.Observe(50,t.AddSeconds(10),false,false,true,inDuty:true)==null,"Dismiss once per transition");
sync.Observe(60,t.AddSeconds(11),false,true,true,inDuty:true);
Check(sync.Observe(60,t.AddSeconds(14),false,true,true,inDuty:true)==null,"Visible HUD adapts without prompt");
sync.Observe(70,t.AddSeconds(15),false,false,false,inDuty:true);
Check(sync.Observe(70,t.AddSeconds(18),false,false,false,inDuty:true)==null,"Disabled preference");
sync.Reset();sync.Observe(90,t.AddSeconds(20),false,false,true,inDuty:true);
Check(sync.Observe(90,t.AddSeconds(23),false,false,true,inDuty:true)==null,"Relog resets baseline");
sync.Observe(null,t.AddSeconds(24),true,false,true,inDuty:true);
sync.Observe(50,t.AddSeconds(25),true,false,true,inDuty:true);
Check(sync.Observe(50,t.AddSeconds(28),false,false,true,inDuty:true)==50,"Loading preserves previous level for sync proposal");
Check(Guide.Opener(20,1)[0].Action==142,"Low-level opener starts from ice, not Transpose in neutral");
Check(Guide.Cycle(new(Level:50)).Fire.Last().Action==162,"Flare finisher before Fire IV unlock");
Check(Guide.Cycle(new(Level:60)).Fire.All(x=>x.Action!=162),"Fire IV replaces low-level Flare finisher");
foreach(var level in new[]{26,44,64,80,91,100})Check(Guide.AreaThunder(new(level,2)),"Two-target area Thunder bracket "+level);
foreach(var level in new[]{6,25,45,63,92,99})Check(!Guide.AreaThunder(new(level,2)),"Two-target separate Thunder bracket "+level);
Console.WriteLine($"PASS {count} checks: levels 1–100, 1–8 targets, static cycles, openers and sync popup.");
for(int level=1;level<=100;level++)for(int targets=1;targets<=8;targets++){
 var page=new GuideContext(level,targets,Job:GuideJob.WhiteMage);var cycle=Guide.Cycle(page);
 foreach(var step in cycle.Ice.Concat(cycle.Fire).Concat(Guide.Opener(page)))Check(Spells.Get(step.Action).Level<=level,$"WHM locked action {level}/{targets}/{step.Action}");
 foreach(var r in Guide.Reminders(page))Check(Spells.Get(r.Action).Level<=level,$"WHM locked reminder {level}/{targets}/{r.Action}");
 Check(cycle.Ice.Any(x=>x.Action==(WhiteMage.Area(page)?WhiteMage.Holy(level):WhiteMage.Filler(level))),$"WHM filler {level}/{targets}");
 Check(!cycle.Ice.Concat(cycle.Fire).Any(x=>new uint[]{141,142,149,3577,36989}.Contains(x.Action)),"No BLM actions in WHM cycle");
}
Check(!WhiteMage.Area(new(44,8,Job:GuideJob.WhiteMage)),"No Holy before 45");
Check(WhiteMage.Area(new(45,2,Job:GuideJob.WhiteMage)),"Holy at two targets from 45");
Check(WhiteMage.Area(new(71,2,Job:GuideJob.WhiteMage)),"Holy at two targets until 71");
Check(!WhiteMage.Area(new(72,2,Job:GuideJob.WhiteMage)),"Glare at two targets from 72");
Check(WhiteMage.Area(new(72,3,Job:GuideJob.WhiteMage)),"Holy at three targets from 72");
var whmOpener=Guide.Opener(new(Job:GuideJob.WhiteMage));
Check(whmOpener.Select(s=>s.Action).SequenceEqual(new uint[]{25859,16532,25859,25859,37009,16535,37009,37009,25859,25859,25859,25859,25859,16532}),"WHM standard early DoT refresh opener");
Check(whmOpener[5].Note.Contains("sinon"),"WHM opener requires ready Blood Lily");
Check(Guide.JobFromId(6)==GuideJob.WhiteMage&&Guide.JobFromId(24)==GuideJob.WhiteMage&&Guide.JobFromId(7)==GuideJob.BlackMage&&Guide.JobFromId(25)==GuideJob.BlackMage&&Guide.JobFromId(1)==null,"Locale independent job IDs");
Spells.ConfigureNames(id=>Spells.Get(id).EnglishName);
Check(Spells.Name(25859)=="Glare III"&&Spells.Name(3577)=="Fire IV","English spell names");
Check(Spells.LocalizeText("Insérer Présence d'esprit puis Assises")=="Insérer Presence of Mind puis Assize","English names in notes");
Check(Spells.LocalizeText("Extra Soin remplace Soin ; Méga Chatoiement")=="Cure II remplace Cure ; Glare III","Longest name replacement without cascading");
Check(Spells.LocalizeText("phase de feu ; Glace")=="phase de feu ; Blizzard","French explanatory prose retained");
Spells.ConfigureNames(_=>null);
Check(Spells.Name(25859)=="Méga Chatoiement","Missing local data falls back to French");
foreach(var job in Enum.GetValues<GuideJob>())foreach(var opening in new[]{false,true}){
 var sources=GuideSources.For(job,opening);Check(sources.Length==3&&sources.All(s=>Uri.TryCreate(s.Url,UriKind.Absolute,out var uri)&&uri.Scheme=="https"),"Three explicit source links");
 Check(sources[1].Url.Contains(opening?"openers":"leveling-guide"),"Source matches displayed content");
}
var duty=new SyncPromptPolicy();var clock=t;
int? Observe(int? level,bool inDuty,bool busy=false){clock=clock.AddSeconds(3);return duty.Observe(level,clock,busy,false,true,inDuty);}
Check(Observe(100,false)==null&&Observe(100,false)==null,"City establishes baseline without popup");
Check(Observe(50,false)==null&&Observe(50,false)==null,"Outdoor level sync never proposes guide");
Check(Observe(100,false)==null&&Observe(100,false)==null,"Return to city never proposes guide");
Check(Observe(null,false,true)==null,"Loading into duty does not propose");
Check(Observe(50,true,true)==null&&Observe(50,true)==50,"Duty sync lower level proposes after loading");
Check(Observe(50,false,true)==null&&duty.Pending==null,"Leaving duty immediately cancels pending proposal");
Check(Observe(null,false,true)==null&&Observe(100,false)==null&&Observe(100,false)==null,"Restored level after dungeon never proposes");
Check(Observe(100,false)==null,"Another city teleport stays quiet");
Check(Observe(60,true,true)==null&&Observe(60,true)==60,"Next dungeon compares with restored outside level");
Check(Observe(100,true,true)==null&&duty.Pending==null&&Observe(100,true)==null,"Restoring level cancels proposal even before duty flags clear");
Check(Observe(70,true,true)==null&&Observe(70,true)==70,"New genuine downward sync can propose");
Check(Observe(80,true)==null&&Observe(80,true)==null,"Level increase inside duty stays quiet");
Check(Observe(60,false,true)==null&&Observe(60,true)==60,"Duty flag may arrive after synced level during loading");
clock=clock.AddSeconds(3);Check(duty.Observe(60,clock,true,false,false,true)==null&&duty.Pending==null,"Disabling while busy cancels proposal immediately");
Console.WriteLine($"PASS {count} total checks: both jobs, 1–100 / 1–8 targets, English names, source links and duty-only downward sync.");
