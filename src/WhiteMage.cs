namespace CycleOpener;
public static class WhiteMage {
 public static uint Filler(int l)=>l>=82?25859u:l>=72?16533u:l>=64?7431u:l>=54?3568u:l>=18?127u:119u;
 public static uint Dot(int l)=>l>=72?16532u:l>=46?132u:121u;
 public static uint Holy(int l)=>l>=82?25860u:139u;
 public static int AreaThreshold(int l)=>l<72?2:3;
 public static bool Area(GuideContext s)=>s.Level>=45&&s.Targets>=AreaThreshold(s.Level);
 public static string Threshold(GuideContext s)=>s.Level<45?"ZONE : Miracle arrive au niveau 45. Avant : dégâts monocibles.":$"ZONE : {Spells.Get(Holy(s.Level)).Name} dès {AreaThreshold(s.Level)} cibles regroupées.";
 public static CyclePlan Cycle(GuideContext s){
  var l=s.Level;var basic=new List<Step>();var burst=new List<Step>();
  if(!Area(s)&&l>=4)basic.Add(new(Dot(l),s.Targets==1?"À entretenir sur une cible durable":"Sur chaque cible durable"));
  basic.Add(new(Area(s)?Holy(l):Filler(l),"Répéter entre les soins nécessaires"));
  if(l>=74)burst.Add(new(16535,"Seulement si le Lys de sang est prêt"));
  if(l>=92)burst.Add(new(37009,"Après Présence d'esprit ; avant expiration",3));
  return new(basic.ToArray(),burst.ToArray(),"La survie du groupe passe avant les dégâts. Les soins répondent aux dégâts reçus ou prévus ; ce schéma ne les détecte pas.");
 }
 public static List<Reminder> Reminders(GuideContext s){
  var l=s.Level;var r=new List<Reminder>();
  if(l>=52)r.Add(new(l>=76?16534u:16531u,"Soins : employer les aptitudes adaptées, puis les Lys selon le besoin. Conserver de quoi répondre aux prochains dégâts.","Groupe / tank"));
  else if(l>=2)r.Add(new(l>=30?135u:120u,"Soigner avant qu’un équipier soit en danger ; reprendre les dégâts quand le groupe est en sécurité.","Selon les dégâts"));
  if(l>=74)r.Add(new(16535,"Offrande de misère : dépenser le Lys de sang avant de le saturer. Préférer les buffs ou les déplacements quand c’est possible.","1+ cible"));
  if(l>=4)r.Add(new(Dot(l),Area(s)?$"Sur un pack regroupé : privilégier {Spells.Get(Holy(l)).Name}. Le DoT sert surtout pendant que le tank rassemble les ennemis.":"Entretenir le DoT ; renouveler près de son expiration. Ne pas le poser sur une cible qui va mourir.",s.Targets>=2?"Par cible durable":"Monocible"));
  if(l>=30)r.Add(new(136,"Présence d'esprit : prévoir une fenêtre où tu peux lancer des sorts ; après l’ouverture, aligner avec les buffs si possible.","Aptitude disponible"));
  if(l>=56)r.Add(new(3571,"Assises : utiliser régulièrement pour les dégâts, les soins et les PM. Peut attendre brièvement un groupe de cibles.","1+ cible"));
  if(l>=92)r.Add(new(37009,"Giga Chatoiement : consommer les trois utilisations obtenues par Présence d'esprit avant leur expiration.","1+ cible"));
  if(l>=14)r.Add(new(7562,"Rêve lucide : utiliser régulièrement quand il manque des PM, sans attendre d’être à sec.","Gestion des PM"));
  if(l>=30)r.Add(new(135,"Extra Soin remplace généralement Soin quand un soin direct est nécessaire. Ne pas attendre un effet gratuit.","Soins monocibles"));
  if(l>=35)r.Add(new(137,"Récup : entretien du tank quand il reçoit des dégâts réguliers.","Soin sur la durée"));
  if(l>=50)r.Add(new(140,"Bénédiction : remonter rapidement une cible en danger ; préparer son usage sur le tank.","Urgence / soin planifié"));
  if(l>=52)r.Add(new(3569,"Asile : préparer les soins de zone là où le groupe peut rester.","Groupe / tank"));
  if(l>=80)r.Add(new(16536,"Tempérance : anticiper les dégâts de groupe importants.","Réduction des dégâts"));
  if(l>=100)r.Add(new(37011,"Caresse divine : après Tempérance, placer le bouclier avant les prochains dégâts de groupe.","Disponible après Tempérance"));
  if(l>=12)r.Add(new(125,l>=58?"Vie : utiliser Magie prompte si disponible ; Sponte peut économiser les PM.":l>=18?"Vie : utiliser Magie prompte si disponible et si une résurrection est nécessaire.":"Vie : ressusciter lorsqu’il est possible de terminer l’incantation en sécurité.","Résurrection"));
  return r;
 }
 public static string OpenerName(GuideContext s)=>Area(s)?"DÉPART DE PACK":s.Level>=92&&s.Targets==1?"OUVERTURE · LYS DE SANG PRÊT":"DÉPART SIMPLE";
 public static List<Step> Opener(GuideContext s){
  var l=s.Level;var f=Filler(l);
  if(l>=92&&s.Targets==1)return [new(f,"Précast avant le pull ; potion au pull si prévue"),new(Dot(l)),new(f),new(f,"Insérer Présence d'esprit après ce sort"),new(37009,"Insérer Assises après ce sort"),new(16535,"Lys de sang prêt requis ; sinon utiliser le sort de dégâts habituel"),new(37009),new(37009),new(f),new(f),new(f),new(f),new(f),new(Dot(l),"Renouvellement anticipé sous buffs ; ensuite reprendre les priorités")];
  var r=new List<Step>();
  if(Area(s)){
   r.Add(new(Holy(l),"Quand le tank a regroupé les ennemis"));
   if(l>=30)r.Add(new(Holy(l),l>=56?"Insérer Présence d'esprit puis Assises dans les fenêtres disponibles":"Insérer Présence d'esprit"));
   if(l>=74)r.Add(new(16535,"Si le Lys de sang est prêt ; sinon passer cette étape"));
   if(l>=92)for(var i=0;i<3;i++)r.Add(new(37009,"Si Présence d'esprit a été utilisée"));
   r.Add(new(Holy(l),"Continuer en adaptant les soins"));
  }else{
   r.Add(new(f,"Précast si le départ du combat est annoncé"));
   if(l>=4)r.Add(new(Dot(l)));
   r.Add(new(f,l>=30?"Insérer Présence d'esprit":"Reprendre les dégâts entre les soins"));
   r.Add(new(f,l>=56?"Insérer Assises":"Répéter selon le besoin"));
   if(l>=74)r.Add(new(16535,"Uniquement si le Lys de sang est prêt"));
   if(l>=92)for(var i=0;i<3;i++)r.Add(new(37009,"Si Présence d'esprit a été utilisée"));
  }
  return r;
 }
}
