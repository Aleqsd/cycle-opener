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
  Check(!config.Resolve(new(Available:false)).Available,"Unknown actual level is not a usable level 100");
  foreach(var level in new[]{-50,101,500}){config.PreviewLevel=level;config.Normalize();Check(config.PreviewLevel==Math.Clamp(level,1,100),"Clamp saved level");}

  var context=ImGui.CreateContext();
  try{
   ImGui.StyleColorsDark();ImGui.GetStyle().WindowPadding=new(8,8);
   var io=ImGui.GetIO();io.IniFilename=null;io.DeltaTime=1f/60;io.DisplaySize=new(800,1200);
   var fc=ImGui.ImFontConfig();fc.SizePixels=17;
   ushort[] ranges=[0x20,0x17f,0x2000,0x206f,0x2190,0x21ff,0];
   fixed(ushort* glyphs=ranges){io.Fonts.AddFontFromFileTTF(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts),"segoeui.ttf"),17,fc,glyphs);io.Fonts.Build();}fc.Destroy();
   io.Fonts.SetTexID(0,new ImTextureID(1UL));
   var cfg=Configuration.Create();cfg.ShowGuide=true;cfg.Locked=true;var step=5;var state=new GuideContext(50,Job:GuideJob.WhiteMage);var section="level";var settingsOpened=false;
   Vector2 origin=default;float width=0,row=0;
   void Frame(){
    ImGui.NewFrame();ImGui.SetNextWindowPos(new(16,16));ImGui.SetNextWindowSize(new(490,700));Panels.PushTheme();
    ImGui.Begin("Native controls",ImGuiWindowFlags.NoTitleBar|ImGuiWindowFlags.NoMove|ImGuiWindowFlags.NoResize);
    origin=ImGui.GetCursorScreenPos();width=ImGui.GetContentRegionAvail().X;row=ImGui.GetFrameHeightWithSpacing();
    if(section=="level")Panels.LevelSelector(cfg,state,ref step);
    else if(section=="guide")Panels.GuideToolbar(cfg,()=>settingsOpened=true,ref step,state);
    else Panels.OpenerToolbar(ref step,cfg.Resolve(state));
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
   state=new();cfg.ManualLevel=false;section="guide";Frame();
   Click(width*.75f,row*.4f);Check(!cfg.ShowGuide,"Locked guide can close without settings");
   Click(width*.25f,row*.4f);Check(settingsOpened,"Locked guide retains settings access");
   Click(width*.5f,row*2+row*.4f);Check(cfg.Targets==2,"Target choice in unified guide");
   Click(width*.84f,row*3+row*.4f);Check(cfg.View==GuideView.Opening,"Opening-only view without a second window");
   Click(width*.16f,row*3+row*.4f);Check(cfg.View==GuideView.Both,"Restore both contents");
   section="opener";step=0;Frame();Click(width*.84f,row*.4f);Check(step==1,"Next opening step");
   Click(width*.5f,row*.4f);Check(step==0,"Restart opening");
   step=Guide.Opener(cfg.Resolve(state)).Count-1;Frame();Click(width*.84f,row*.4f);Check(step==Guide.Opener(cfg.Resolve(state)).Count-1,"Final step cannot advance");
   Check((Panels.GuideFlags(true)&ImGuiWindowFlags.NoInputs)==0,"Lock never removes input handling");
   var nativeOpen=true;
   void CloseFrame(){ImGui.NewFrame();ImGui.SetNextWindowPos(new(16,16));ImGui.SetNextWindowSize(new(490,200));ImGui.Begin("Locked window",ref nativeOpen,Panels.GuideFlags(true));ImGui.Text("Guide");ImGui.End();ImGui.Render();}
   for(var i=0;i<3;i++)CloseFrame();
   io.AddMousePosEvent(491,28);CloseFrame();io.AddMouseButtonEvent(0,true);CloseFrame();io.AddMouseButtonEvent(0,false);CloseFrame();
   Check(!nativeOpen,"Native titlebar close remains usable while locked");
  }finally{ImGui.DestroyContext(context);}
  Console.WriteLine($"PASS {count} native panel checks: migration, manual levels, unified guide, locked close and opening controls.");
 }
}
