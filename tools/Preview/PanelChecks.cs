using System.Numerics;
using System.Text.Json;
using CycleOpener;
using Dalamud.Bindings.ImGui;

internal static unsafe class PanelChecks {
 public static void Run(){
  var count=0;
  void Check(bool condition,string message){if(!condition)throw new Exception(message);count++;}
  foreach(var mode in Enum.GetValues<Layout>())foreach(var hud in new[]{false,true})foreach(var opening in new[]{false,true}){
   var old=new Configuration{Version=1,Layout=mode,ShowHud=hud,ShowOpener=opening,Targets=3,HudScale=1.5f,Locked=true};old.Normalize();
   Check(old.ShowHud==hud&&old.ShowOpener==(hud&&opening&&mode!=Layout.Ouverture),"Migration preserves actual visible panels");
   Check(old.Layout==mode&&old.Targets==3&&old.HudScale==1.5f&&old.Locked,"Migration preserves user preferences");
   var saved=JsonSerializer.Serialize(old,new JsonSerializerOptions{IncludeFields=true});
   var restored=JsonSerializer.Deserialize<Configuration>(saved,new JsonSerializerOptions{IncludeFields=true})!;restored.Normalize();
   Check(restored.Version==2&&restored.ShowHud==old.ShowHud&&restored.ShowOpener==old.ShowOpener,"Reload does not repeat legacy migration");
  }
  var single=Configuration.Create();single.OpenOpening();single.Normalize();
  Check(!single.ShowHud&&single.CompanionVisible,"Opening can be visible without cycle");
  single.Layout=Layout.Ouverture;single.ShowHud=true;single.ShowOpener=false;single.OpenCycle();
  Check(single.CycleVisible&&single.CompanionVisible,"Opening survives switching its main window back to cycle");
  single.Layout=Layout.Ouverture;
  Check(!single.CompanionVisible&&single.OpeningVisible,"No duplicate opening window");
  single.HideOpening();Check(!single.AnyPanel,"Hide opening works for legacy opening layout");

  var context=ImGui.CreateContext();
  try{
   ImGui.StyleColorsDark();ImGui.GetStyle().WindowPadding=new(8,8);
   var io=ImGui.GetIO();io.IniFilename=null;io.DeltaTime=1f/60;io.DisplaySize=new(800,1200);
   var fc=ImGui.ImFontConfig();fc.SizePixels=17;
   ushort[] ranges=[0x20,0x17f,0x2000,0x206f,0x2190,0x21ff,0];
   fixed(ushort* glyphs=ranges){io.Fonts.AddFontFromFileTTF(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts),"segoeui.ttf"),17,fc,glyphs);io.Fonts.Build();}fc.Destroy();
   io.Fonts.SetTexID(0,new ImTextureID(1UL));
   var cfg=Configuration.Create();var demo=false;var step=0;var state=new GuideContext();
   void Frame(){
    ImGui.NewFrame();ImGui.SetNextWindowPos(new(16,16));ImGui.SetNextWindowSize(new(490,1000));Panels.PushTheme();
    ImGui.Begin("Cycle & Opener · Réglages");Panels.Settings(cfg,state,false,"CycleOpener.dll",ref demo,ref step,()=>{},()=>{});ImGui.End();Panels.PopTheme();ImGui.Render();
   }
   void Click(float x,float y){io.AddMousePosEvent(x,y);Frame();io.AddMouseButtonEvent(0,true);Frame();io.AddMouseButtonEvent(0,false);Frame();}
   for(var i=0;i<3;i++)Frame();
   Click(260,113);Check(cfg.CycleVisible&&cfg.OpeningVisible,"Open both button");
   Click(140,145);Check(!cfg.CycleVisible&&cfg.CompanionVisible,"Hide cycle keeps opening");
   Click(380,145);Check(!cfg.AnyPanel,"Hide opening button");
   Click(380,145);Check(!cfg.ShowHud&&cfg.CompanionVisible,"Open only opening button");
   Click(140,145);Check(cfg.CycleVisible&&cfg.OpeningVisible,"Open cycle keeps opening");
   Click(380,552);Check(!cfg.AnyPanel,"Hide all button");
   Click(260,316);Check(cfg.Targets==2,"Two target button");
   Click(380,258);Check(demo,"Manual level button");
   Click(140,258);Check(!demo,"Automatic level button");
   state=new(Available:false);Frame();Click(260,113);Check(!cfg.AnyPanel,"Unavailable player disables panel opening");
   Click(380,258);Click(260,113);Check(demo&&cfg.CycleVisible&&cfg.OpeningVisible,"Manual mode permits offline consultation");
   cfg.HideAll();cfg.OpenOpening();step=0;
   void OpeningFrame(){
    ImGui.NewFrame();ImGui.SetNextWindowPos(new(16,16));ImGui.SetNextWindowSize(new(600,700));Panels.PushTheme();
    ImGui.Begin("Cycle & Opener · Ouverture");Panels.OpeningPanelToolbar(cfg,()=>{},ref step,100);ImGui.End();Panels.PopTheme();ImGui.Render();
   }
   void OpeningClick(float x,float y){io.AddMousePosEvent(x,y);OpeningFrame();io.AddMouseButtonEvent(0,true);OpeningFrame();io.AddMouseButtonEvent(0,false);OpeningFrame();}
   for(var i=0;i<3;i++)OpeningFrame();
   OpeningClick(520,133);Check(step==1,"Opening next step button");
   OpeningClick(120,67);Check(cfg.CycleVisible&&cfg.OpeningVisible&&step==1,"Open cycle preserves reading step");
   OpeningClick(320,133);Check(step==0,"Opening restart button");
   step=5;OpeningClick(320,100);Check(cfg.Targets==2&&step==0,"Opening target buttons reset reading step");
   OpeningClick(120,67);Check(!cfg.CycleVisible&&cfg.OpeningVisible,"Opening hides cycle independently");
  }finally{ImGui.DestroyContext(context);}
  Console.WriteLine($"PASS {count} panel checks: migration, independent visibility and native button clicks.");
 }
}
