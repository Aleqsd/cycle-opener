using System.Numerics;
using System.Text.Json;
using System.Reflection;
using System.Runtime.CompilerServices;
using CycleOpener;
using Dalamud.Bindings.ImGui;

internal static unsafe class PanelChecks {
 public static void Run(){
  var count=0;
  void Check(bool condition,string message){if(!condition)throw new Exception(message);count++;}
  foreach(var version in new[]{1,2})foreach(var mode in Enum.GetValues<Layout>())foreach(var hud in new[]{false,true})foreach(var opening in new[]{false,true}){
   var old=new Configuration{Version=version,Layout=mode,ShowHud=hud,ShowOpener=opening,Targets=3,HudScale=1.5f,Locked=true};old.Normalize();
   var expectedVisible=version==1?hud:hud||opening;
   Check(old.ShowGuide==expectedVisible&&old.View==GuideView.Both,"Migration keeps window visibility and unifies content");
   Check(old.Layout==(mode==Layout.Ouverture?Layout.Focus:mode)&&old.Targets==3&&old.HudScale==1.5f&&old.Locked,"Migration preserves preferences");
   var saved=JsonSerializer.Serialize(old,new JsonSerializerOptions{IncludeFields=true});
   var restored=JsonSerializer.Deserialize<Configuration>(saved,new JsonSerializerOptions{IncludeFields=true})!;restored.Normalize();
   Check(restored.Version==4&&restored.ShowGuide==old.ShowGuide&&restored.View==GuideView.Both,"Reload keeps both sections");
  }
  foreach(var view in Enum.GetValues<GuideView>()){
   var old=new Configuration{Version=3,View=view,ShowGuide=false,ManualLevel=true,ManualJob=GuideJob.Sage,PreviewLevel=72,Locked=true,Targets=8,HudScale=1.5f};old.Normalize();
   Check(old.Version==4&&old.View==GuideView.Both&&!old.ShowGuide&&old.ManualLevel&&old.ManualJob==GuideJob.Sage&&old.PreviewLevel==72&&old.Targets==8&&old.Locked&&old.HudScale==1.5f,"V3 view preference migrates without opening window or losing settings");
  }
  var config=Configuration.Create();config.ManualLevel=true;config.PreviewLevel=64;config.ManualJob=GuideJob.WhiteMage;config.Targets=3;
  Check(config.Resolve(new(50,Job:GuideJob.BlackMage))==new GuideContext(64,3,true,GuideJob.WhiteMage),"Manual job and level ignore actual sync");
  Check(config.Resolve(new(Available:false)).Available,"Manual guide works offline");
  var serialized=JsonSerializer.Serialize(config,new JsonSerializerOptions{IncludeFields=true});
  var reloaded=JsonSerializer.Deserialize<Configuration>(serialized,new JsonSerializerOptions{IncludeFields=true})!;reloaded.Normalize();
  Check(reloaded.ManualLevel&&reloaded.ManualJob==GuideJob.WhiteMage&&reloaded.PreviewLevel==64,"Manual selection persists");
  config.ManualLevel=false;Check(config.Resolve(new(50,Job:GuideJob.BlackMage)).Level==50,"Auto follows sync");
  foreach(var job in Enum.GetValues<GuideJob>()){
   config.ManualLevel=true;config.ManualJob=job;config.Targets=8;
   config.FavoriteJobs=[job,job,(GuideJob)999];config.Normalize();
   var json=JsonSerializer.Serialize(config,new JsonSerializerOptions{IncludeFields=true});
   var saved=JsonSerializer.Deserialize<Configuration>(json,new JsonSerializerOptions{IncludeFields=true})!;saved.Normalize();
   Check(saved.Resolve(new(Available:false)).Job==job&&saved.Targets==8&&saved.FavoriteJobs.SequenceEqual(new[]{job}),"Manual job, exact targets and favourites survive reload with invalid values removed");
  }
  config.FavoriteJobs=[];Check(!JobPicker.Choices(config,1).Any(),"Empty favourites are not silently filled");
  config.FavoriteJobs=[GuideJob.WhiteMage,GuideJob.Gunbreaker];
  Check(JobPicker.Choices(config,1).Select(j=>j.Job).ToHashSet().SetEquals(config.FavoriteJobs),"Favourite filter only contains selected jobs");
  Check(JobPicker.Choices(config,2).Count()==4&&JobPicker.Choices(config,2).All(j=>j.Role=="tanks"),"Tank role filter");
  Check(Enumerable.Range(2,5).SelectMany(f=>JobPicker.Choices(config,f)).Select(j=>j.Job).Distinct().Count()==21,"Every job is accessible through a role");
  config.ManualLevel=false;
  Check(!config.Resolve(new(Available:false)).Available,"Unknown actual level is not a usable level 100");
  foreach(var level in new[]{-50,101,500}){config.PreviewLevel=level;config.Normalize();Check(config.PreviewLevel==Math.Clamp(level,1,100),"Clamp saved level");}

  // Exercise the actual command callback without starting Dalamud services.
  var plugin=(Plugin)RuntimeHelpers.GetUninitializedObject(typeof(Plugin));
  var binding=BindingFlags.Instance|BindingFlags.NonPublic;
  var commandConfig=Configuration.Create();commandConfig.ManualLevel=true;commandConfig.PreviewLevel=72;
  var commandUi=new GuidePanelState{Folded=true,ExpandedSize=new(900,780)};
  typeof(Plugin).GetField("config",binding)!.SetValue(plugin,commandConfig);
  typeof(Plugin).GetField("guideUi",binding)!.SetValue(plugin,commandUi);
  var windowType=typeof(Plugin).GetNestedType("SettingsWindow",BindingFlags.NonPublic)!;
  var settingsWindow=(Dalamud.Interface.Windowing.Window)Activator.CreateInstance(windowType,plugin)!;
  typeof(Plugin).GetField("settings",binding)!.SetValue(plugin,settingsWindow);
  var callback=typeof(Plugin).GetMethod("OnCommand",binding)!;
  void Command(string args)=>callback.Invoke(plugin,new object[]{"/cycle",args});
  foreach(var args in new[]{"","  ","show"}){
   commandConfig.ShowGuide=false;settingsWindow.IsOpen=false;Command(args);
   Check(commandConfig.ShowGuide&&!settingsWindow.IsOpen,"Cycle opens the guide, not settings");
  }
  Check(!commandUi.Folded&&commandUi.RestoreSize&&commandConfig.ManualLevel&&commandConfig.PreviewLevel==72,"Opening unfolds the guide without losing level preferences");
  Command("hide");Check(!commandConfig.ShowGuide,"Hide closes the guide");
  foreach(var args in new[]{"config","cfg","setup"," CFG ","SETUP"}){
   settingsWindow.IsOpen=false;Command(args);
   Check(settingsWindow.IsOpen&&!commandConfig.ShowGuide,"Configuration aliases open settings without changing guide visibility");
  }

  var context=ImGui.CreateContext();
  try{
   ImGui.StyleColorsDark();ImGui.GetStyle().WindowPadding=new(8,8);
   var beforeColor=ImGui.GetStyle().Colors[(int)ImGuiCol.WindowBg];var beforePadding=ImGui.GetStyle().FramePadding;var beforeBorder=ImGui.GetStyle().WindowBorderSize;
   GuidePanel.PushStyle(new Configuration{BackgroundOpacity=.15f,HudScale=1.8f});GuidePanel.PopStyle();
   Check(ImGui.GetStyle().Colors[(int)ImGuiCol.WindowBg]==beforeColor&&ImGui.GetStyle().FramePadding==beforePadding&&ImGui.GetStyle().WindowBorderSize==beforeBorder,"Guide styling leaves other windows unchanged");
   var io=ImGui.GetIO();io.IniFilename=null;io.DeltaTime=1f/60;io.DisplaySize=new(800,1200);io.ConfigFlags|=ImGuiConfigFlags.NavEnableKeyboard;
   var fc=ImGui.ImFontConfig();fc.SizePixels=17;
   ushort[] ranges=[0x20,0x17f,0x2000,0x206f,0x2190,0x21ff,0];
   fixed(ushort* glyphs=ranges){io.Fonts.AddFontFromFileTTF(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts),"segoeui.ttf"),17,fc,glyphs);io.Fonts.Build();}fc.Destroy();
   io.Fonts.SetTexID(0,new ImTextureID(1UL));
   var cfg=Configuration.Create();cfg.ShowGuide=true;cfg.Locked=true;var step=5;var state=new GuideContext(50,Job:GuideJob.WhiteMage);var section="level";var settingsOpened=false;var ui=new GuidePanelState();
   Vector2 origin=default;float width=0,row=0;var pickerFilter=0;
   void Frame(){
    ImGui.NewFrame();ImGui.SetNextWindowPos(new(16,16));ImGui.SetNextWindowSize(new(490,700));Panels.PushTheme();
    ImGui.Begin("Native controls",ImGuiWindowFlags.NoTitleBar|ImGuiWindowFlags.NoMove|ImGuiWindowFlags.NoResize);
    origin=ImGui.GetCursorScreenPos();width=ImGui.GetContentRegionAvail().X;row=ImGui.GetFrameHeightWithSpacing();
    if(section=="level")Panels.LevelSelector(cfg,state,ref step);
    else if(section=="header")GuidePanel.Header(cfg,state,ui,_=>null,()=>settingsOpened=true);
    else if(section=="controls")GuidePanel.Controls(cfg,state,ref step);
    else if(section=="picker"){
     JobPicker.Draw(cfg,"Checks",304);
     ImGui.PushID("Checks");pickerFilter=ImGui.GetStateStorage().GetInt(ImGui.GetID("Filtre de rôle"),0);ImGui.PopID();
    }
    else Hud.Draw(Layout.Ouverture,cfg.Resolve(state),_=>null,new(),step,selectStep:n=>step=n,embedded:true);
    ImGui.End();Panels.PopTheme();ImGui.Render();
   }
   void Click(float x,float y){io.AddMousePosEvent(origin.X+x,origin.Y+y);Frame();io.AddMouseButtonEvent(0,true);Frame();io.AddMouseButtonEvent(0,false);Frame();}
   for(var i=0;i<3;i++)Frame();
   Click(width*.75f,row*.4f);Check(cfg.ManualLevel&&cfg.ManualJob==GuideJob.WhiteMage&&step==0,"Manual mode picks actual job and resets reading step");
   var presetWidth=ImGui.CalcTextSize("Paliers").X+20+29;
   cfg.PreviewLevel=99;Frame();Click(width-presetWidth-8-14,row*2+12);Check(cfg.PreviewLevel==100,"Native level plus button");
   Click(width-presetWidth-8-14,row*2+12);Check(cfg.PreviewLevel==100,"Native level plus stays at 100");
   Click(width*.25f,row*.4f);Check(!cfg.ManualLevel&&cfg.Resolve(state).Level==50,"Auto mode restores synchronized level");
   state=new(Available:false);Frame();Click(width*.75f,row*.4f);Check(cfg.ManualLevel,"Manual mode available offline");
   Click(width*.25f,row*.4f);Check(cfg.ManualLevel,"Unavailable auto mode stays disabled");
   state=new();cfg.ManualLevel=false;section="header";Frame();
   Click(width-14,18);Check(!cfg.ShowGuide,"Locked guide can close without settings");
   Click(width-47,18);Check(settingsOpened,"Locked guide retains settings access");
   Click(width-113,18);Check(ui.Folded&&ui.ExpandedSize.Y==700,"Collapse preserves expanded size");
   cfg.ShowGuide=true;Click(width-14,18);Check(!cfg.ShowGuide&&ui.Folded,"Folded locked guide can still close");
   Click(width-113,18);Check(!ui.Folded&&ui.RestoreSize,"Expand requests saved size");
   Click(width-80,18);Check(!cfg.Locked,"Header unlock button");
   section="controls";Frame();
   Click(width*.5f,row+row*.4f);Check(cfg.Targets==2,"Target choice in unified guide");
   cfg.ManualLevel=true;cfg.ManualJob=GuideJob.BlackMage;cfg.PreviewLevel=99;Frame();
   Click(112+8+140-14,12);Check(cfg.PreviewLevel==100,"Integrated manual plus button");
   Click(112+8+140-14,12);Check(cfg.PreviewLevel==100,"Integrated manual level clamp");
   cfg.Targets=1;section="opener";step=0;Frame();Frame();Frame();Click(12+(width-24)/Math.Max(2,(int)((width-24)/110))+20,70);Check(step==1,$"Native opener step click (step={step}, width={width})");
   Click(12+20,70);Check(step==0,"Native opener returns to first step");
   section="level";cfg.ManualLevel=true;cfg.ManualJob=GuideJob.BlackMage;Frame();Frame();Frame();
   Click(width*.5f,row+row*.4f);
   Check(ImGui.IsPopupOpen("",ImGuiPopupFlags.AnyPopupId|ImGuiPopupFlags.AnyPopupLevel),"Native job combo opens");
   // Close with an outside click; picker rows and favourite controls have their own checks below.
   Click(5,500);
   section="controls";cfg.ManualLevel=false;cfg.Targets=3;step=7;Frame();
   Click(width*.84f,row+row*.4f);
   Check(ImGui.IsPopupOpen("",ImGuiPopupFlags.AnyPopupId|ImGuiPopupFlags.AnyPopupLevel),"Native exact-target popup opens");
   Click(width*.84f+20,row+row*.4f+8+ImGui.GetTextLineHeightWithSpacing()*5.5f);
   Check(cfg.Targets==8&&step==0,$"Native exact-target popup selects eight and resets reading step (selected {cfg.Targets})");
   section="picker";cfg.FavoriteJobs=[];cfg.ManualJob=GuideJob.BlackMage;Frame();Frame();Frame();
   Click(100,14);Frame();Frame();
   Click(155,43);Check(pickerFilter==2,$"Native tank filter (filter={pickerFilter})");
   Click(25,171);Check(cfg.FavoriteJobs.SequenceEqual(new[]{GuideJob.Paladin}),"Native star adds Paladin to favourites");
   Check(ImGui.IsPopupOpen("",ImGuiPopupFlags.AnyPopupId|ImGuiPopupFlags.AnyPopupLevel),"Starring keeps the picker open");
   Click(95,43);Check(pickerFilter==1,"Native favourites filter");
   Click(25,171);Check(cfg.FavoriteJobs.Count==0,"Native star removes a favourite");
   Click(155,43);Click(25,171);Click(95,43);Click(100,171);
   Check(cfg.ManualJob==GuideJob.Paladin,"Native favourite job selection");
   Check(!ImGui.IsPopupOpen("",ImGuiPopupFlags.AnyPopupId|ImGuiPopupFlags.AnyPopupLevel),"Selecting a job closes the picker");
   Check((Panels.GuideFlags(true)&ImGuiWindowFlags.NoInputs)==0,"Lock never removes input handling");
   Check((Panels.GuideFlags(true)&ImGuiWindowFlags.NoTitleBar)!=0,"Generic titlebar replaced by integrated header");
  }finally{ImGui.DestroyContext(context);}
  Console.WriteLine($"PASS {count} native panel checks: migration, manual levels, unified guide, locked close and opening controls.");
 }
}
