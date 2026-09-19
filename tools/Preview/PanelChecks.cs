using System.Numerics;
using System.Text.Json;
using CycleOpener;
using Dalamud.Bindings.ImGui;

internal static unsafe class PanelChecks {
 public static void Run(){
  var count=0;
  void Check(bool condition,string message){if(!condition)throw new Exception(message);count++;}
  foreach(var version in new[]{1,2})foreach(var mode in Enum.GetValues<Layout>())foreach(var hud in new[]{false,true})foreach(var opening in new[]{false,true}){
   var old=new Configuration{Version=version,Layout=mode,ShowHud=hud,ShowOpener=opening,Targets=3,HudScale=1.5f,Locked=true};old.Normalize();
   var expectedVisible=version==1?hud:hud||opening;
   var hasCycle=hud&&mode!=Layout.Ouverture;
   var hasOpening=(version==1?hud&&opening:opening)||(hud&&mode==Layout.Ouverture);
   var expectedView=hasCycle&&hasOpening?GuideView.Both:hasOpening?GuideView.Opening:hasCycle?GuideView.Cycle:GuideView.Both;
   Check(old.ShowGuide==expectedVisible&&old.View==expectedView,"Migration preserves visible content in one window");
   Check(old.Layout==(mode==Layout.Ouverture?Layout.Focus:mode)&&old.Targets==3&&old.HudScale==1.5f&&old.Locked,"Migration preserves preferences");
   var saved=JsonSerializer.Serialize(old,new JsonSerializerOptions{IncludeFields=true});
   var restored=JsonSerializer.Deserialize<Configuration>(saved,new JsonSerializerOptions{IncludeFields=true})!;restored.Normalize();
   Check(restored.Version==3&&restored.ShowGuide==old.ShowGuide&&restored.View==old.View,"Reload does not repeat migration");
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
   var json=JsonSerializer.Serialize(config,new JsonSerializerOptions{IncludeFields=true});
   var saved=JsonSerializer.Deserialize<Configuration>(json,new JsonSerializerOptions{IncludeFields=true})!;saved.Normalize();
   Check(saved.Resolve(new(Available:false)).Job==job&&saved.Targets==8,"Each imported manual job and exact target count survives reload");
  }
  config.ManualLevel=false;
  Check(!config.Resolve(new(Available:false)).Available,"Unknown actual level is not a usable level 100");
  foreach(var level in new[]{-50,101,500}){config.PreviewLevel=level;config.Normalize();Check(config.PreviewLevel==Math.Clamp(level,1,100),"Clamp saved level");}

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
   Vector2 origin=default;float width=0,row=0;
   void Frame(){
    ImGui.NewFrame();ImGui.SetNextWindowPos(new(16,16));ImGui.SetNextWindowSize(new(490,700));Panels.PushTheme();
    ImGui.Begin("Native controls",ImGuiWindowFlags.NoTitleBar|ImGuiWindowFlags.NoMove|ImGuiWindowFlags.NoResize);
    origin=ImGui.GetCursorScreenPos();width=ImGui.GetContentRegionAvail().X;row=ImGui.GetFrameHeightWithSpacing();
    if(section=="level")Panels.LevelSelector(cfg,state,ref step);
    else if(section=="header")GuidePanel.Header(cfg,state,ui,_=>null,()=>settingsOpened=true);
    else if(section=="controls")GuidePanel.Controls(cfg,state,ref step);
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
   step=6;Click(width*.84f,row*2+row*.4f);Check(cfg.View==GuideView.Opening&&step==6,"Opening-only view preserves selected reading step");
   Click(width*.16f,row*2+row*.4f);Check(cfg.View==GuideView.Both,"Restore both contents");
   cfg.ManualLevel=true;cfg.ManualJob=GuideJob.BlackMage;cfg.PreviewLevel=99;Frame();
   Click(112+8+140-14,12);Check(cfg.PreviewLevel==100,"Integrated manual plus button");
   Click(112+8+140-14,12);Check(cfg.PreviewLevel==100,"Integrated manual level clamp");
   cfg.Targets=1;section="opener";step=0;Frame();Click(12+112+20,70);Check(step==1,"Native opener step click");
   Click(12+20,70);Check(step==0,"Native opener returns to first step");
   section="level";cfg.ManualLevel=true;cfg.ManualJob=GuideJob.BlackMage;Frame();Frame();Frame();
   Click(width*.5f,row+row*.4f);
   Check(ImGui.IsPopupOpen("",ImGuiPopupFlags.AnyPopupId|ImGuiPopupFlags.AnyPopupLevel),"Native job combo opens");
   Click(width*.5f,row*2+ImGui.GetTextLineHeightWithSpacing()*2.5f);
   Check(cfg.ManualJob==GuideJob.Paladin,$"Native job list selects an imported job (selected {cfg.ManualJob})");
   section="controls";cfg.ManualLevel=false;cfg.Targets=3;step=7;Frame();
   Click(width*.84f,row+row*.4f);
   Check(ImGui.IsPopupOpen("",ImGuiPopupFlags.AnyPopupId|ImGuiPopupFlags.AnyPopupLevel),"Native exact-target popup opens");
   Click(width*.84f+20,row+row*.4f+8+ImGui.GetTextLineHeightWithSpacing()*5.5f);
   Check(cfg.Targets==8&&step==0,$"Native exact-target popup selects eight and resets reading step (selected {cfg.Targets})");
   Check((Panels.GuideFlags(true)&ImGuiWindowFlags.NoInputs)==0,"Lock never removes input handling");
   Check((Panels.GuideFlags(true)&ImGuiWindowFlags.NoTitleBar)!=0,"Generic titlebar replaced by integrated header");
  }finally{ImGui.DestroyContext(context);}
  Console.WriteLine($"PASS {count} native panel checks: migration, manual levels, unified guide, locked close and opening controls.");
 }
}
