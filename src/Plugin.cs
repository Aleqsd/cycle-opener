using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Configuration;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Command;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Interface.FontIdentifier;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace CycleOpener;

[Serializable]
public sealed class Configuration : IPluginConfiguration {
 public int Version {get;set;}=1;
 public Layout Layout=Layout.Focus;
 public int Targets=1;
 // Legacy visibility fields are retained only to migrate existing configurations.
 public bool ShowHud;
 public bool ShowOpener=true;
 public bool ShowGuide;
 public GuideView View=GuideView.Both;
 public List<GuideJob> FavoriteJobs=[];
 public bool Locked;
 public int PreviewLevel=100;
 public bool ManualLevel;
 public GuideJob ManualJob=GuideJob.BlackMage;
 public bool SuggestOnSync=true;
 public float HudScale=1;
 public float BackgroundOpacity=.86f;
 public bool TextOutline=true;
 public bool Expressway=true;
 public static Configuration Create()=>new(){Version=4};
 public GuideContext Resolve(GuideContext actual)=>ManualLevel?new(PreviewLevel,Targets,true,ManualJob):actual with{Targets=Targets};
 public void Normalize(){
  if(Version<3){
   ShowGuide=Version<2?ShowHud:ShowHud||ShowOpener;
   if(Layout==Layout.Ouverture)Layout=Layout.Focus;
  }
  // View is retained only to read old configurations. Both sections are now mandatory.
  View=GuideView.Both;Version=4;
  FavoriteJobs=(FavoriteJobs??[]).Where(j=>Enum.IsDefined(j)).Distinct().ToList();
  Targets=Math.Clamp(Targets,1,8);PreviewLevel=Math.Clamp(PreviewLevel,1,100);
  HudScale=Math.Clamp(float.IsFinite(HudScale)?HudScale:1,.8f,1.8f);
  BackgroundOpacity=Math.Clamp(float.IsFinite(BackgroundOpacity)?BackgroundOpacity:.86f,.15f,1);
  if(!Enum.IsDefined(Layout)||Layout==Layout.Ouverture)Layout=Layout.Focus;
  if(!Enum.IsDefined(ManualJob))ManualJob=GuideJob.BlackMage;
 }
}

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface Pi {get;private set;}=null!;
    [PluginService] internal static ICommandManager Commands {get;private set;}=null!;
    [PluginService] internal static IFramework Framework {get;private set;}=null!;
    [PluginService] internal static IClientState Client {get;private set;}=null!;
    [PluginService] internal static IObjectTable Objects {get;private set;}=null!;
    [PluginService] internal static ICondition Condition {get;private set;}=null!;
    [PluginService] internal static ITextureProvider Textures {get;private set;}=null!;
    [PluginService] internal static IPluginLog Log {get;private set;}=null!;
    [PluginService] internal static IDataManager Data {get;private set;}=null!;
    private readonly Configuration config;
    private readonly WindowSystem windows=new("CycleOpener");
    private readonly SyncPromptPolicy sync=new();
    private readonly GuidePanelState guideUi=new();
    private readonly Dictionary<uint,ISharedImmediateTexture> icons=new();
    private readonly HudWindow hud;
    private readonly SettingsWindow settings;
    private readonly PromptWindow prompt;
    private GuideContext state=new(Available:false);
    private bool dirty, resetPosition, fontDirty;
    private DateTime nextUpdate;
    private IFontHandle? font;
    private readonly string? expresswayPath;
    private int openerStep;
    private int lastLevel;
    private GuideJob? lastJob;
    private bool busy;
    private string? runtimeError;
    private bool disposed;

    public Plugin()
    {
        config=Pi.GetPluginConfig() as Configuration ?? Configuration.Create();var oldVersion=config.Version;config.Normalize();dirty=oldVersion!=config.Version;
        var sheet=Data.GetExcelSheet<Lumina.Excel.Sheets.Action>();
        foreach(var spell in Spells.All.Values) {
            var icon=sheet?.GetRowOrDefault(spell.Id)?.Icon ?? spell.Icon;
            icons[spell.Id]=Textures.GetFromGameIcon(new GameIconLookup(icon));
        }
        foreach(var job in Enum.GetValues<GuideJob>()){var id=Guide.JobIcon(job);icons[id]=Textures.GetFromGameIcon(new GameIconLookup(id));}
        Spells.ConfigureNames(id=>sheet?.GetRowOrDefault(id)?.Name.ToString());
        expresswayPath=FindExpressway();RefreshFont();
        hud=new(this);settings=new(this);prompt=new(this);
        windows.AddWindow(hud);windows.AddWindow(settings);windows.AddWindow(prompt);
        Commands.AddHandler("/cycle",new CommandInfo(OnCommand){HelpMessage="/cycle : ouvrir le guide ; /cycle config|cfg|setup : réglages ; /cycle show|hide ; /cycle next|prev : fiche d’ouverture."});
        Pi.UiBuilder.Draw+=Draw;Pi.UiBuilder.OpenConfigUi+=OpenSettings;Pi.UiBuilder.OpenMainUi+=OpenGuide;Framework.Update+=Update;Client.Logout+=Logout;
        Log.Information($"Cycle & Opener 1.1.0 — {Pi.AssemblyLocation.FullName}");
    }
    private static string? FindExpressway()
    {
        foreach(var path in new[]{Environment.GetFolderPath(Environment.SpecialFolder.Fonts),Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Microsoft","Windows","Fonts")})
            try {if(Directory.Exists(path)) {var file=Directory.EnumerateFiles(path).FirstOrDefault(p=>Path.GetFileName(p).Contains("expressway",StringComparison.OrdinalIgnoreCase)&&Path.GetExtension(p).Equals(".ttf",StringComparison.OrdinalIgnoreCase));if(file!=null)return file;}} catch(IOException){} catch(UnauthorizedAccessException){}
        return null;
    }
    private void RefreshFont()
    {
        font?.Dispose();font=null;
        if(!config.Expressway || expresswayPath==null)return;
        font=Pi.UiBuilder.FontAtlas.NewDelegateFontHandle(build=>build.OnPreBuild(tk=>{
            var face=tk.AddFontFromFile(expresswayPath,new SafeFontConfig{SizePx=17});
            var fallback=Pi.UiBuilder.DefaultFontSpec;
            if(fallback is SingleFontSpec single)(single with {SizePx=17}).AddToBuildToolkit(tk,face);
            else fallback.AddToBuildToolkit(tk,face);
            tk.Font=face;
        }));
    }
    private void OpenSettings()=>settings.IsOpen=true;
    private void OpenGuide(){
        config.ShowGuide=true;dirty=true;
        if(guideUi.Folded){guideUi.Folded=false;guideUi.RestoreSize=true;}
    }
    private static bool InGuideDuty=>!Client.IsPvP&&(Condition[ConditionFlag.BoundByDuty]||Condition[ConditionFlag.BoundByDuty56]||Condition[ConditionFlag.BoundByDuty95]);
    private void Logout(int type,int code)=>sync.Reset();
    private void OnCommand(string command,string args)
    {
        switch(args.Trim().ToLowerInvariant()) {
            case "config":case "cfg":case "setup":OpenSettings();break;
            case "":case "show":OpenGuide();break;
            case "hide":config.ShowGuide=false;dirty=true;break;
            case "next":openerStep=Math.Min(openerStep+1,Guide.Opener(DisplayState).Count-1);break;
            case "prev":openerStep=Math.Max(0,openerStep-1);break;
            default:OpenGuide();break;
        }
    }
    private void Update(IFramework framework)
    {
        if(disposed)return;
        if(dirty) {Pi.SavePluginConfig(config);dirty=false;}
        if(fontDirty){RefreshFont();fontDirty=false;}
        var now=DateTime.UtcNow;if(now<nextUpdate)return;nextUpdate=now.AddMilliseconds(100);
        busy=Condition[ConditionFlag.InCombat]||Condition[ConditionFlag.BetweenAreas]||Condition[ConditionFlag.BetweenAreas51]||Condition[ConditionFlag.WatchingCutscene]||Condition[ConditionFlag.OccupiedInCutSceneEvent];
        try {
            var player=Objects.LocalPlayer;
            if(player==null){state=new(Available:false);sync.Observe(null,now,true,config.ShowGuide,config.SuggestOnSync,InGuideDuty);prompt.IsOpen=false;return;}
            var job=Guide.JobFromId(player.ClassJob.RowId);
            if(job==null){lastJob=null;state=new(Available:false);sync.Reset();prompt.IsOpen=false;return;}
            if(lastJob!=job){sync.Reset();if(!config.ManualLevel)openerStep=0;lastJob=job;}
            state=new(player.Level,config.Targets,true,job.Value);
            if(lastLevel!=state.Level){if(!config.ManualLevel)openerStep=0;lastLevel=state.Level;}
            var pending=sync.Observe(state.Level,now,busy,config.ShowGuide,config.SuggestOnSync&&!config.ManualLevel,InGuideDuty);
            prompt.IsOpen=pending!=null && !busy;
            runtimeError=null;
        } catch(Exception e) {
            if(runtimeError==null)Log.Error(e,"Impossible de lire le job ou le niveau ; aide suspendue.");
            runtimeError="Niveau indisponible ; la fiche reste consultable en mode manuel.";state=new(Available:false);prompt.IsOpen=false;
        }
    }
    private GuideContext DisplayState=>config.Resolve(state);
    private ImTextureID? Icon(uint id)=>icons.GetValueOrDefault(id)?.GetWrapOrDefault()?.Handle;
    private void Draw()
    {
        if(disposed)return;
        var available=DisplayState.Available && !Condition[ConditionFlag.BetweenAreas] && !Condition[ConditionFlag.BetweenAreas51] && !Condition[ConditionFlag.WatchingCutscene] && !Condition[ConditionFlag.OccupiedInCutSceneEvent];
        if(config.ManualLevel||!InGuideDuty)prompt.IsOpen=false;
        hud.IsOpen=config.ShowGuide&&available;
        windows.Draw();
    }
    public void Dispose()
    {
        disposed=true;Framework.Update-=Update;Client.Logout-=Logout;Pi.UiBuilder.Draw-=Draw;Pi.UiBuilder.OpenConfigUi-=OpenSettings;Pi.UiBuilder.OpenMainUi-=OpenGuide;Commands.RemoveHandler("/cycle");windows.RemoveAllWindows();font?.Dispose();
        if(dirty)Pi.SavePluginConfig(config);
    }
    private sealed class HudWindow : Window {
        private readonly Plugin p;
        public HudWindow(Plugin p):base("Cycle & Opener · Guide##Guide",ImGuiWindowFlags.NoFocusOnAppearing){this.p=p;RespectCloseHotkey=true;}
        public override void OnClose(){p.config.ShowGuide=false;p.dirty=true;}
        public override void PreDraw(){
            // Lock placement only. Closing and all guide controls always remain interactive.
            Flags=Panels.GuideFlags(p.config.Locked);
            ImGui.SetNextWindowBgAlpha(p.config.BackgroundOpacity);
            var sc=ImGuiHelpers.GlobalScale*p.config.HudScale;
            var maximum=ImGui.GetMainViewport().WorkSize-new Vector2(20);
            var minimum=new Vector2(320,560)*ImGuiHelpers.GlobalScale*Math.Max(1,p.config.HudScale);
            if(p.guideUi.Folded)minimum.Y=p.guideUi.HeaderHeight;
            ImGui.SetNextWindowSizeConstraints(Vector2.Min(minimum,maximum),maximum);
            var size=Vector2.Min(new Vector2(900,780)*ImGuiHelpers.GlobalScale,maximum);
            ImGui.SetNextWindowSize(size,ImGuiCond.FirstUseEver);
            if(p.guideUi.Folded)ImGui.SetNextWindowSize(new(0,p.guideUi.HeaderHeight));
            else if(p.guideUi.RestoreSize){ImGui.SetNextWindowSize(Vector2.Min(p.guideUi.ExpandedSize,maximum));p.guideUi.RestoreSize=false;}
            var position=ImGui.GetMainViewport().WorkPos+Vector2.Max(Vector2.Zero,Vector2.Min(new Vector2(60,80)*ImGuiHelpers.GlobalScale,maximum-size));
            ImGui.SetNextWindowPos(position,ImGuiCond.FirstUseEver);
            if(p.resetPosition){ImGui.SetNextWindowPos(position);p.resetPosition=false;}
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,new Vector2(8)*sc);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding,3*sc);
            GuidePanel.PushStyle(p.config);
        }
        public override void PostDraw(){GuidePanel.PopStyle();ImGui.PopStyleVar(2);}
        public override void Draw(){
            if(GuidePanel.Header(p.config,p.state,p.guideUi,p.Icon,p.OpenSettings))p.dirty=true;
            if(p.guideUi.Folded)return;
            if(GuidePanel.Controls(p.config,p.state,ref p.openerStep))p.dirty=true;
            ImGui.BeginChild("Lecture du guide",Vector2.Zero,false);
            try {
                using var pushed=p.font is {Available:true}?p.font.Push():null;
                GuidePanel.Content(p.config,p.DisplayState,p.guideUi,p.Icon,new(ImGuiHelpers.GlobalScale*p.config.HudScale,p.config.BackgroundOpacity,p.config.TextOutline),ref p.openerStep);
            }finally{ImGui.EndChild();}
        }
    }
    private sealed class SettingsWindow : Window
    {
        private readonly Plugin p;
        public SettingsWindow(Plugin p):base("Cycle & Opener · Réglages###CycleOpenerSettings") {this.p=p;Size=new(520,680);SizeCondition=ImGuiCond.FirstUseEver;SizeConstraints=new(){MinimumSize=new(360,320),MaximumSize=new(float.MaxValue,float.MaxValue)};}
        public override void PreDraw()=>Panels.PushTheme();
        public override void PostDraw()=>Panels.PopTheme();
        public override void Draw()
        {
            if(Panels.Settings(p.config,p.state,p.expresswayPath!=null,Pi.AssemblyLocation.FullName,ref p.openerStep,()=>p.resetPosition=true,()=>p.fontDirty=true,p.runtimeError)) p.dirty=true;
        }
    }
    private sealed class PromptWindow : Window
    {
        private readonly Plugin p;
        public PromptWindow(Plugin p):base("Cycle & Opener · Niveau adapté###CycleOpenerSync",ImGuiWindowFlags.AlwaysAutoResize|ImGuiWindowFlags.NoCollapse|ImGuiWindowFlags.NoFocusOnAppearing) {this.p=p;RespectCloseHotkey=true;}
        public override void PreDraw(){Panels.PushTheme();ImGui.SetNextWindowSize(new(410*ImGuiHelpers.GlobalScale,0));}
        public override void PostDraw()=>Panels.PopTheme();
        public override void OnClose()=>p.sync.Dismiss();
        public override void Draw()
        {
            var disabled=!p.config.SuggestOnSync;
            var action=Panels.Prompt(p.state,p.config.Layout,ref disabled);
            if(action==1){p.config.ShowGuide=true;p.dirty=true;}
            if(action==3){p.config.SuggestOnSync=!disabled;p.dirty=true;}
            if(action!=0){p.sync.Dismiss();IsOpen=false;}
        }
    }
}
