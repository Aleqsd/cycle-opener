using CycleOpener;

public static class RequirementChecks {
 public static void Run(){
  var count=0;void Check(bool condition,string label){if(!condition)throw new Exception(label);count++;}
  string? Badge(GuideJob job,string name,int level=100)=>Requirements.For(new(level,Job:job),JobActions.Id(job,name))?.Badge;
  Check(Badge(GuideJob.Warrior,"Fell Cleave",69)=="50 jauge"&&Badge(GuideJob.Warrior,"Fell Cleave",70)=="50 jauge / gratuit","Free beast spender starts at Inner Release unlock");
  Check(Badge(GuideJob.DarkKnight,"Edge of Darkness",69)=="3 000 PM"&&Badge(GuideJob.DarkKnight,"Edge of Darkness",70)=="3 000 PM / gratuit","Free MP expenditure starts at TBN unlock");
  Check(Badge(GuideJob.RedMage,"Enchanted Riposte",59)=="20 / 20 mana"&&Badge(GuideJob.RedMage,"Enchanted Riposte",60)=="20/20 ou gratuit","Manafication alternative is level-aware");
  Check(Badge(GuideJob.Gunbreaker,"Double Down")=="2 cartouches","Double Down current official cost");
  Check(Badge(GuideJob.Reaper,"Perfectio")=="Récolte + Communio","Communio alone does not grant Perfectio");
  Check(Badge(GuideJob.Pictomancer,"Subtractive Palette",69)=="50 Palette"&&Badge(GuideJob.Pictomancer,"Subtractive Palette",70)=="50 Palette / gratuit","Free palette requires Starry Muse unlock");
  Check(Requirements.For(new(73,Job:GuideJob.WhiteMage),16535)==null,"No locked Blood Lily prerequisite");
  Check(Requirements.For(new(Job:GuideJob.WhiteMage),16535)?.Badge=="Lys de sang","Blood Lily cost visible");
  foreach(var job in Jobs.All){
   var seen=false;
   for(var level=1;level<=100;level++)foreach(var targets in new[]{1,3,8}){
    var state=new GuideContext(level,targets,Job:job.Job);var cycle=Guide.Cycle(state);
    var ids=cycle.Ice.Concat(cycle.Fire).Concat(Guide.Opener(state)).SelectMany(s=>new[]{s.Action}.Concat(s.Weaves??[])).Concat(Guide.Reminders(state).Select(r=>r.Action)).Distinct();
    foreach(var id in ids)if(Requirements.For(state,id) is {} requirement){seen=true;Check(!string.IsNullOrWhiteSpace(requirement.Badge)&&!string.IsNullOrWhiteSpace(requirement.Detail),"Every visible prerequisite has a short label and explanation");}
   }
   Check(seen,"Each job has prerequisites covered");
  }
  Spells.ConfigureNames(id=>Spells.Get(id).EnglishName);
  Check(Spells.LocalizeText(Requirements.For(new(Job:GuideJob.WhiteMage),37009)!.Badge)=="Presence of Mind","Prerequisite action name follows English client");
  Spells.ConfigureNames(_=>null);
  Console.WriteLine($"PASS {count} prerequisite checks: levels, costs, free alternatives, effects and English names.");
 }
}
