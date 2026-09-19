namespace CycleOpener;
public enum Layout { Focus, Ruban, Priorites, Cycle, Ouverture }
public enum GuideJob { BlackMage, WhiteMage }
public enum GuideView { Both, Cycle, Opening }
public sealed record GuideContext(int Level = 100, int Targets = 1, bool Available = true, GuideJob Job = GuideJob.BlackMage);
public sealed record Step(uint Action, string Note = "", int Count = 1, uint[]? Weaves = null);
public sealed record Reminder(uint Action,string Text,string Threshold="",bool Healing=false);
public sealed record CyclePlan(Step[] Ice,Step[] Fire,string Note);
public sealed record OpeningGroup(int Start,int Count,Step Step);

public static class Guide
{
    public static GuideJob? JobFromId(uint id)=>id switch{7 or 25=>GuideJob.BlackMage,6 or 24=>GuideJob.WhiteMage,_=>null};
    public static string JobName(GuideJob job)=>job==GuideJob.WhiteMage?"Mage blanc":"Mage noir";
    public static uint JobIcon(GuideJob job)=>job==GuideJob.WhiteMage?62124u:62125u;
    public static int? MultiThreshold(GuideContext s)=>s.Job==GuideJob.WhiteMage
        ?s.Level<45?null:WhiteMage.AreaThreshold(s.Level):s.Level<12?null:AreaThreshold(s.Level);
    public static string LevelHint(GuideContext s){
        uint[] milestones=s.Job==GuideJob.WhiteMage?[139,16531,3571,16535,16534,37009,37011]:[149,152,162,3576,3577,7422,16505,16507,25797,36989];
        var missing=milestones.Where(id=>Spells.Get(id).Level>s.Level).Take(2).Select(id=>$"{Spells.Name(id)} (niv. {Spells.Get(id).Level})");
        return s.Level>=100?"":"À ce niveau : sans "+string.Join(" · ",missing);
    }
    public static List<Step> Opener(GuideContext s)=>s.Job==GuideJob.WhiteMage?WhiteMage.Opener(s):Opener(s.Level,s.Targets);
    public static List<OpeningGroup> OpeningGroups(GuideContext s){
        var steps=Opener(s);var groups=new List<OpeningGroup>();
        for(var i=0;i<steps.Count;i++){
            var start=i;var item=steps[i];
            while(i+1<steps.Count&&item.Note.Length==0&&(item.Weaves?.Length??0)==0&&steps[i+1].Note.Length==0&&(steps[i+1].Weaves?.Length??0)==0&&steps[i+1].Action==item.Action)i++;
            groups.Add(new(start,i-start+1,item));
        }
        return groups;
    }
    public static string Threshold(GuideContext s)=>s.Job==GuideJob.WhiteMage?WhiteMage.Threshold(s):s.Level<12?"Pas de cycle de zone avant le niveau 12.":s.Level>=100?"ZONE : 2 cibles · Giga Glace à 2 / Gel à 3+":s.Targets==2?"2 cibles : base mono · zone complète dès 3+":"ZONE : au moins 3 cibles regroupées";
    // Reference sheets only: no gauge, target, cooldown or next-action engine.
    public static int AreaThreshold(int level) => level >= 100 ? 2 : 3;
    public static bool Area(GuideContext s) => s.Level >= 12 && s.Targets >= AreaThreshold(s.Level);
    public static string Mode(GuideContext s) => s.Targets == 1 ? "MONOCIBLE" : s.Targets == 2 ? "2 CIBLES" : $"{s.Targets}+ CIBLES";
    public static uint Thunder(int level,bool area) => area && level>=26
        ? level>=92?36987u:level>=64?7420u:7447u : level>=92?36986u:level>=45?153u:144u;
    public static uint Poly(int level,int targets) => level>=80 && targets==1?16507u:7422u;
    public static bool AreaThunder(GuideContext s) => Area(s) || s.Targets==2 && (s.Level is >=26 and <45 or >=64 and <92);
    public static string Band(int level) => level<12?"1–11":level<18?"12–17":level<35?"18–34":level<40?"35–39":level<50?"40–49":level<58?"50–57":level<60?"58–59":level<72?"60–71":level<90?"72–89":level<100?"90–99":"100";
    public static CyclePlan Cycle(GuideContext s)
    {
        if(s.Job==GuideJob.WhiteMage)return WhiteMage.Cycle(s);
        var l=s.Level;
        if(l is <1 or >100)return new([],[],"Niveau non pris en charge.");
        if(Area(s)) {
            if(l<18)return new([new(25793,"Répéter")],[],"Extra Feu arrive au niveau 18. Reste en glace.");
            if(l<35)return new([new(25793,"Jusqu’aux PM pleins"),new(149)], [new(147,"",3),new(149)],"Répète sur au moins 3 ennemis regroupés.");
            if(l<40)return new([new(25793,"",2)],[new(147,"",4)],"Extra Feu et Extra Glace changent directement de phase.");
            if(l<50)return new([new(159,"Répéter")],[new(149),new(147,"",3),new(149)],"Reste sur Gel. La boucle feu sert seulement à obtenir Électrifié pour renouveler Foudre.");
            if(l<58)return new([new(159,"",2),new(149)],[new(147),new(162),new(149)],"Un Brasier par phase de feu à ce niveau.");
            var ice=new List<Step>{new(s.Targets==2?3576u:159u)};
            ice.Add(new(l>=70?7422u:Thunder(l,true),"Filler si disponible / utile"));ice.Add(new(149,"Si disponible"));
            var fire=new List<Step>{new(162,"",2)};if(l==100)fire.Add(new(36989));fire.Add(new(149,"Retour en glace"));
            return new(ice.ToArray(),fire.ToArray(),s.Targets==2?"À 2 cibles, Giga Glace ; à 3+, Gel. En glace : Infect / Foudre pour attendre Transposition.":"En glace : Infect / Foudre de zone (ou Gel) pour attendre Transposition.");
        }
        if(l==1)return new([new(142,"Répéter")],[],"Feu se débloque au niveau 2.");
        if(l<35) {
            var ice=new List<Step>();var fire=new List<Step>();
            if(l>=4)ice.Add(new(149));ice.Add(new(142,"Jusqu’aux PM pleins"));
            if(l>=4)fire.Add(new(149));fire.Add(new(141,"Jusqu’aux PM bas"));
            return new(ice.ToArray(),fire.ToArray(),l<4?"Alterner feu et glace. Transposition arrive au niveau 4.":"Feu dépense les PM ; la glace les restaure. Recommence.");
        }
        var i=new List<Step>{new(154),new(l>=58?3576u:142u)};
        if(l>=90)i.Add(new(25797,"Disponible en retour de phase feu"));
        var f=new List<Step>{new(152)};
        if(l>=90){f.Add(new(3577,"",3));f.Add(new(25797));f.Add(new(3577,"",3));}
        else if(l>=60)f.Add(new(3577,"",7));else f.Add(new(141,"Jusqu’aux PM bas"));
        if(l is >=50 and <60)f.Add(new(162,"Fin de phase"));
        if(l==100)f.Add(new(36989));
        if(l>=72)f.Add(new(16505));
        return new(i.ToArray(),f.ToArray(),l>=90?"Le Paradoxe de glace manque au premier départ. Foudre et Polyglotte s’insèrent selon leurs conditions.":"Foudre et Polyglotte s’insèrent selon leurs conditions, hors de cette boucle de base.");
    }
    public static List<Reminder> Reminders(GuideContext s)
    {
        if(s.Job==GuideJob.WhiteMage)return WhiteMage.Reminders(s);
        var r=new List<Reminder>();
        if(s.Level>=6)r.Add(new(Thunder(s.Level,AreaThunder(s)),"Foudre : renouveler à moins de 3 s, avec Électrifié. Éviter si la cible va mourir."+(s.Targets==2&&!AreaThunder(s)?" Alterner entre les deux cibles.":""),AreaThunder(s)&&s.Level>=26?"2+ cibles":"Par cible"));
        if(s.Level>=70)r.Add(new(Poly(s.Level,s.Targets),s.Level>=80?"Xénoglossie en mono ; Infect dès 2 cibles. Éviter de remplir Polyglotte. Garder des instants pour bouger.":"Infect : dépenser Polyglotte sans laisser la jauge déborder.",s.Level>=80&&s.Targets>=2?"2+ cibles":"1+ cible"));
        if(s.Level==100)r.Add(new(36989,"Astre flamboyant : 6 charges astrales nécessaires. À lancer avant de quitter le feu.","1+ cible"));
        if(s.Level>=90&&!Area(s))r.Add(new(25797,"Paradoxe : déplacer dans la phase feu si utile pour bouger ou insérer une aptitude.","Instantané"));
        if(s.Level>=35)r.Add(new(16506,"Sans cible : préparer la prochaine reprise avec Âme ombrale en glace.","Entre les combats"));
        if(s.Level>=35&&s.Level<90&&!Area(s))r.Add(new(152,"Pyromane : utiliser sans interrompre une incantation.","Proc disponible"));
        if(s.Level>=90&&!Area(s))r.Add(new(152,"Pyromane : après la glace, Transposition puis Méga Feu instantané pour une reprise renforcée.","Proc disponible"));
        if(s.Level>=30)r.Add(new(158,"Vasque de mana : prolonger le feu quand les PM sont épuisés.","Aptitude disponible"));
        if(s.Level>=52)r.Add(new(3573,"Manalignements : utiliser quand tu peux profiter de la zone ; prévoir les déplacements.","Aptitude disponible"));
        if(s.Level>=86)r.Add(new(25796,"Amplificateur : utiliser dès que possible sans dépasser le maximum de Polyglotte.","Aptitude disponible"));
        return r;
    }
    public static List<Step> Opener(int level,int targets)
    {
        var s=new GuideContext(level,targets);
        if(level==100&&targets==1) {
            uint[] ids=[152,36986,3577,3577,3577,3577,3577,16507,3577,36989,3577,3577,36986,3577,3577,3577,3577,36989,16505,154,3576,25797,25797,152];
            var notes=new Dictionary<int,string>{[0]="Précast : env. 4 s avant le pull",[1]="Insère Magie prompte + Amplificateur",[2]="Insère potion + Manalignements",[7]="Insère Vasque de mana",[12]="Foudre : adapter au DoT et aux buffs",[18]="Insère Transposition + Triple sort",[21]="Insère Transposition",[22]="Paradoxe de feu",[23]="Méga Feu sous Pyromane"};
            var weaves=new Dictionary<int,uint[]>{[1]=[7561,25796],[2]=[3573],[7]=[158],[18]=[149,7421],[21]=[149]};
            return ids.Select((id,index)=>new Step(id,notes.GetValueOrDefault(index,""),Weaves:weaves.GetValueOrDefault(index))).ToList();
        }
        var cycle=Cycle(s);var result=new List<Step>();
        if(Area(s)) {
            result.Add(new(level>=35?154u:25793u,"Préparation initiale en glace"));
            var sequence=level is >=40 and <50?cycle.Ice:cycle.Ice.Concat(cycle.Fire);
            foreach(var step in sequence)for(int k=0;k<step.Count;k++)result.Add(step with{Count=1});
        } else {
            foreach(var step in cycle.Ice.Where(x=>x.Action!=25797 && x.Action!=149))for(int k=0;k<step.Count;k++)result.Add(step with{Count=1});
            if(level>=6)result.Add(new(Thunder(level,AreaThunder(s)),"Si Électrifié"));
            foreach(var step in cycle.Fire)for(int k=0;k<step.Count;k++)result.Add(step with{Count=1});
        }
        return result.Where(x=>Spells.Get(x.Action).Level<=level).ToList();
    }
    public static string OpenerName(GuideContext s)=>s.Job==GuideJob.WhiteMage?WhiteMage.OpenerName(s):Area(s)?"DÉPART MULTICIBLE":s.Level==100&&s.Targets==1?"OUVERTURE 5 + 7":"DÉPART SIMPLE";
}
public sealed class SyncPromptPolicy
{
    private int? accepted,candidate;private DateTime since;
    public int? Pending{get;private set;}
    public int? Observe(int? level,DateTime now,bool busy,bool guideVisible,bool enabled,bool inDuty)
    {
        // Leaving an instance cancels even a proposal deferred by loading or combat.
        if(!inDuty||!enabled||guideVisible)Pending=null;
        if(level==null){candidate=null;Pending=null;return null;}
        if(candidate!=level){candidate=level;since=now;Pending=null;}
        if(busy||now-since<TimeSpan.FromSeconds(2))return null;
        if(!inDuty){accepted=level;return null;}
        if(accepted==null){accepted=level;return null;}
        if(accepted!=level){var lower=level<accepted;accepted=level;Pending=lower&&enabled&&!guideVisible?level:null;}
        return Pending;
    }
    public void Dismiss()=>Pending=null;
    public void Reset(){accepted=null;candidate=null;Pending=null;}
}
