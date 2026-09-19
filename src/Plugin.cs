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
public sealed class Configuration : IPluginConfiguration
{
    // Keep the legacy default for deserialization; new configurations use Create().
    public int Version {get;set;}=1;
    public Layout Layout=Layout.Focus;
    public int Targets=1;
    public bool ShowHud;
    public bool ShowOpener=true;
    public bool Locked;
    public int PreviewLevel=100;
    public bool SuggestOnSync=true;
    public float HudScale=1;
    public float BackgroundOpacity=.86f;
    public bool TextOutline=true;
    public bool Expressway=true;
    public static Configuration Create()=>new(){Version=2,ShowOpener=false};
    public bool AnyPanel=>ShowHud||ShowOpener;
    public bool CycleVisible=>ShowHud&&Layout!=Layout.Ouverture;
    public bool OpeningVisible=>ShowOpener||(ShowHud&&Layout==Layout.Ouverture);
    public bool CompanionVisible=>ShowOpener&&!(ShowHud&&Layout==Layout.Ouverture);
    public void OpenBoth(){if(Layout==Layout.Ouverture)Layout=Layout.Focus;ShowHud=true;ShowOpener=true;}
    public void OpenCycle(){if(Layout==Layout.Ouverture){ShowOpener|=ShowHud;Layout=Layout.Focus;}ShowHud=true;}
    public void OpenOpening(){if(!(ShowHud&&Layout==Layout.Ouverture))ShowOpener=true;}
    public void HideOpening(){ShowOpener=false;if(Layout==Layout.Ouverture)ShowHud=false;}
    public void HideAll(){ShowHud=false;ShowOpener=false;}
    public void Normalize() {
        if(Version<2){ShowOpener=ShowHud&&ShowOpener&&Layout!=Layout.Ouverture;Version=2;}
        Targets=Math.Clamp(Targets,1,8);PreviewLevel=Math.Clamp(PreviewLevel,1,100);HudScale=Math.Clamp(float.IsFinite(HudScale)?HudScale:1,.8f,1.8f);BackgroundOpacity=Math.Clamp(float.IsFinite(BackgroundOpacity)?BackgroundOpacity:.86f,.15f,1);if(!Enum.IsDefined(Layout))Layout=Layout.Focus;
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
    private readonly Dictionary<uint,ISharedImmediateTexture> icons=new();
    private readonly HudWindow hud;
    private readonly HudWindow opener;
    private readonly SettingsWindow settings;
    private readonly PromptWindow prompt;
    private GuideContext state=new(Available:false);
    private bool dirty, demo, resetPosition, resetOpenerPosition, fontDirty;
    private DateTime nextUpdate;
    private IFontHandle? font;
    private readonly string? expresswayPath;
    private int openerStep;
    private int lastLevel;
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
        expresswayPath=FindExpressway();RefreshFont();
        hud=new(this);opener=new(this,true);settings=new(this);prompt=new(this);
        windows.AddWindow(hud);windows.AddWindow(opener);windows.AddWindow(settings);windows.AddWindow(prompt);
        Commands.AddHandler("/cycle",new CommandInfo(OnCommand){HelpMessage="Guide Mage noir. /cycle : réglages ; /cycle show|hide ; /cycle next|prev : fiche d’ouverture."});
        Pi.UiBuilder.Draw+=Draw;Pi.UiBuilder.OpenConfigUi+=OpenSettings;Pi.UiBuilder.OpenMainUi+=OpenSettings;Framework.Update+=Update;Client.Logout+=Logout;
        Log.Information($"Cycle & Opener 0.1.2 — {Pi.AssemblyLocation.FullName}");
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
    private void Logout(int type,int code)=>sync.Reset();
    private void OnCommand(string command,string args)
    {
        switch(args.Trim().ToLowerInvariant()) {
            case "show":config.OpenBoth();dirty=true;break;
            case "hide":config.HideAll();dirty=true;break;
            case "next":openerStep=Math.Min(openerStep+1,Guide.Opener(DisplayState.Level,DisplayState.Targets).Count-1);break;
            case "prev":openerStep=Math.Max(0,openerStep-1);break;
            default:settings.IsOpen=true;break;
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
            if(player==null){state=new(Available:false);sync.Observe(null,now,true,config.AnyPanel,config.SuggestOnSync);prompt.IsOpen=false;return;}
            if(player.ClassJob.RowId is not (7 or 25)){state=new(Available:false);sync.Reset();prompt.IsOpen=false;return;}
            state=new(player.Level,config.Targets);
            if(lastLevel!=state.Level){openerStep=0;lastLevel=state.Level;}
            var pending=sync.Observe(state.Level,now,busy,config.AnyPanel,config.SuggestOnSync);
            prompt.IsOpen=pending!=null && !busy;
            runtimeError=null;
        } catch(Exception e) {
            if(runtimeError==null)Log.Error(e,"Impossible de lire l’état Mage noir ; aide suspendue.");
            runtimeError="Niveau indisponible ; la fiche reste consultable en mode manuel.";state=new(Available:false);prompt.IsOpen=false;
        }
    }
    private GuideContext DisplayState=>demo?new(config.PreviewLevel,config.Targets):state with{Targets=config.Targets};
    private ImTextureID? Icon(uint id)=>icons.GetValueOrDefault(id)?.GetWrapOrDefault()?.Handle;
    private void Draw()
    {
        if(disposed)return;
        var available=(demo||state.Available) && !Condition[ConditionFlag.BetweenAreas] && !Condition[ConditionFlag.BetweenAreas51] && !Condition[ConditionFlag.WatchingCutscene] && !Condition[ConditionFlag.OccupiedInCutSceneEvent];
        hud.IsOpen=config.ShowHud&&available;
        opener.IsOpen=config.CompanionVisible&&available;
        windows.Draw();
    }
    public void Dispose()
    {
        disposed=true;Framework.Update-=Update;Client.Logout-=Logout;Pi.UiBuilder.Draw-=Draw;Pi.UiBuilder.OpenConfigUi-=OpenSettings;Pi.UiBuilder.OpenMainUi-=OpenSettings;Commands.RemoveHandler("/cycle");windows.RemoveAllWindows();font?.Dispose();
        if(dirty)Pi.SavePluginConfig(config);
    }
    private sealed class HudWindow : Window
    {
        private readonly Plugin p;
        private readonly bool isOpener;
        public HudWindow(Plugin p,bool isOpener=false):base(isOpener?"Cycle & Opener · Ouverture##Opening":"Cycle & Opener · Cycle##Guide",ImGuiWindowFlags.NoFocusOnAppearing) {this.p=p;this.isOpener=isOpener;RespectCloseHotkey=true;}
        public override void OnClose(){if(isOpener)p.config.ShowOpener=false;else p.config.ShowHud=false;p.dirty=true;}
        public override void PreDraw()
        {
            Flags=ImGuiWindowFlags.NoFocusOnAppearing|(p.config.Locked?ImGuiWindowFlags.NoInputs:0);
            ImGui.SetNextWindowBgAlpha(p.config.BackgroundOpacity);
            var sc=ImGuiHelpers.GlobalScale*p.config.HudScale;
            var maximum=ImGui.GetMainViewport().WorkSize-new Vector2(20);
            var mode=isOpener?Layout.Ouverture:p.config.Layout;
            ImGui.SetNextWindowSizeConstraints(Vector2.Min(new Vector2(300,240)*sc,maximum),maximum);
            var size=Vector2.Min(new(Hud.Preferred(mode).X*sc,740*ImGuiHelpers.GlobalScale),maximum);
            ImGui.SetNextWindowSize(size,ImGuiCond.FirstUseEver);
            var offset=new Vector2(60+(isOpener?Hud.Preferred(p.config.Layout).X*p.config.HudScale+20:0),100)*ImGuiHelpers.GlobalScale;
            var position=ImGui.GetMainViewport().WorkPos+Vector2.Max(Vector2.Zero,Vector2.Min(offset,maximum-size));
            ImGui.SetNextWindowPos(position,ImGuiCond.FirstUseEver);
            if(isOpener?p.resetOpenerPosition:p.resetPosition){ImGui.SetNextWindowPos(position);if(isOpener)p.resetOpenerPosition=false;else p.resetPosition=false;}
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,new Vector2(8,8)*sc);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding,3*sc);
        }
        public override void PostDraw()=>ImGui.PopStyleVar(2);
        public override void Draw()
        {
            if(isOpener){if(Panels.OpeningPanelToolbar(p.config,p.OpenSettings,ref p.openerStep,p.DisplayState.Level))p.dirty=true;}
            else if(Panels.GuideToolbar(p.config,p.OpenSettings,ref p.openerStep,p.DisplayState.Level))p.dirty=true;
            using var pushed=p.font is {Available:true}?p.font.Push():null;
            Hud.Draw(isOpener?Layout.Ouverture:p.config.Layout,p.DisplayState,p.Icon,new(ImGuiHelpers.GlobalScale*p.config.HudScale,p.config.BackgroundOpacity,p.config.TextOutline),p.openerStep,p.demo,step=>p.openerStep=step);
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
            if(Panels.Settings(p.config,p.state,p.expresswayPath!=null,Pi.AssemblyLocation.FullName,ref p.demo,ref p.openerStep,()=>{p.resetPosition=true;p.resetOpenerPosition=true;},()=>p.fontDirty=true,p.runtimeError)) p.dirty=true;
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
            if(action==1){p.config.OpenBoth();p.dirty=true;}
            if(action==3){p.config.SuggestOnSync=!disabled;p.dirty=true;}
            if(action!=0){p.sync.Dismiss();IsOpen=false;}
        }
    }
}
