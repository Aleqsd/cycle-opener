using System.Numerics;
using System.Runtime.InteropServices;
using System.Text.Json;
using Dalamud.Bindings.ImGui;
using HexaGen.Runtime;
using CycleOpener;

unsafe class Program
{
 static void Main(string[] args)
 {
    var nativeDir=args[0];var root=args[1];var output=Path.Combine(root,"preview","renders");Directory.CreateDirectory(output);
    using var native=new NativeContext(Path.Combine(nativeDir,"cimgui.dll"));
    NativeLibrary.SetDllImportResolver(typeof(ImGui).Assembly,(n,_,_)=>n=="cimgui"?native.Module:nint.Zero);ImGui.InitApi(native);
    PanelChecks.Run();
    var textures=new Dictionary<ulong,(byte[] Pixels,int Width,int Height)>();
    foreach(var file in Directory.EnumerateFiles(Path.Combine(root,".artifacts","icons"),"*.rgba")) {
        using var reader=new BinaryReader(File.OpenRead(file));var w=reader.ReadInt32();var h=reader.ReadInt32();textures[ulong.Parse(Path.GetFileNameWithoutExtension(file))]=(reader.ReadBytes(w*h*4),w,h);
    }
    var cases=new List<(string Name, Layout Mode, GuideContext State,float Scale,int Width,int Step,string Panel)>();
    foreach(var job in Enum.GetValues<GuideJob>()) foreach(var level in new[]{20,50,72,90,100}) foreach(var targets in new[]{1,2,3}) foreach(var mode in Enum.GetValues<Layout>())
        cases.Add(($"{job}-{level}-{targets}-{(int)mode}",mode,new(level,targets,Job:job),1,(int)Hud.Preferred(mode).X,0,"hud"));
    foreach(var job in Enum.GetValues<GuideJob>()) foreach(var scale in new[]{1f,1.5f,2f}) {
        cases.Add(($"guide-{job}-{scale*100:0}",Layout.Focus,new(Job:job),scale,1040,0,"guide"));
        cases.Add(($"guide-minimum-{job}-{scale*100:0}",Layout.Focus,new(50,3,Job:job),scale,320,0,"guide-manual"));
        cases.Add(($"guide-minimum-english-{job}-{scale*100:0}",Layout.Focus,new(50,3,Job:job),scale,320,0,"guide-manual-english"));
        cases.Add(($"guide-minheight-{job}-{scale*100:0}",Layout.Focus,new(50,3,Job:job),scale,320,0,"guide-manual-english-short"));
        cases.Add(($"guide-english-{job}-{scale*100:0}",Layout.Focus,new(Job:job),scale,1040,0,"guide-english"));
        cases.Add(($"guide-healing-{job}-{scale*100:0}",Layout.Focus,new(Job:job),scale,780,0,"guide-healing"));
        cases.Add(($"guide-folded-{job}-{scale*100:0}",Layout.Focus,new(Job:job),scale,500,0,"guide-folded"));
        cases.Add(($"settings-{job}-{scale*100:0}",Layout.Focus,new(Job:job),scale,490,0,"settings"));
        cases.Add(($"settings-minimum-{job}-{scale*100:0}",Layout.Focus,new(Job:job),scale,360,0,"settings-manual"));
        cases.Add(($"settings-appearance-{job}-{scale*100:0}",Layout.Focus,new(Job:job),scale,360,0,"settings-appearance"));
        cases.Add(($"popup-{job}-{scale*100:0}",Layout.Focus,new(50,Job:job),scale,410,0,"popup"));
    }
    cases.Add(("guide-whm-area",Layout.Priorites,new(100,3,Job:GuideJob.WhiteMage),1,1040,4,"guide-english"));
    cases.Add(("guide-whm-healing-detail",Layout.Focus,new(Job:GuideJob.WhiteMage),1,780,0,"guide-healing-detail"));
    cases.Add(("guide-footer-minimum",Layout.Focus,new(50,3,Job:GuideJob.WhiteMage),2,320,0,"guide-footer"));
    cases.Add(("settings-offline",Layout.Focus,new(Available:false),1,360,0,"settings"));
    cases.Add(("settings-offline-manual",Layout.Focus,new(Available:false),1,360,0,"settings-manual"));
    foreach(var scale in new[]{1f,1.5f,2f})foreach(var panel in new[]{"picker","picker-favorites","picker-empty"})
        cases.Add(($"{panel}-{scale*100:0}",Layout.Focus,new(Job:GuideJob.Paladin),scale,320,0,panel));
    foreach(var scale in new[]{1f,1.5f,2f})cases.Add(($"guide-locked-WhiteMage-{scale*100:0}",Layout.Focus,new(Job:GuideJob.WhiteMage),scale,500,0,"guide-locked"));
    var filter=args.ElementAtOrDefault(2);

    if(filter!=null)cases=cases.Where(c=>c.Name.StartsWith(filter,StringComparison.Ordinal)).ToList();
    var metrics=new List<object>();
    foreach(var c in cases)
    {
        Spells.ConfigureNames(id=>c.Panel.Contains("english")?Spells.Get(id).EnglishName:Spells.Get(id).Name);
        var context=ImGui.CreateContext();
        try {
            ImGui.StyleColorsDark();ImGui.GetStyle().ScaleAllSizes(c.Scale);
            ImGui.GetStyle().WindowPadding=new Vector2(8,8)*c.Scale;ImGui.GetStyle().WindowRounding=3*c.Scale;
            ImGui.GetStyle().Colors[(int)ImGuiCol.WindowBg]=new(0.045f,.039f,.052f,.86f);
            var io=ImGui.GetIO();io.IniFilename=null;io.DeltaTime=1f/60;var pw=(int)((c.Width+32)*c.Scale);var ph=(int)(2600*c.Scale);io.DisplaySize=new(pw,ph);
            var fc=ImGui.ImFontConfig();fc.SizePixels=17*c.Scale;
            ushort[] ranges=[0x20,0x17f,0x2000,0x206f,0x2190,0x21ff,0x2260,0x2265,0];
            fixed(ushort* glyphs=ranges){io.Fonts.AddFontFromFileTTF(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts),"segoeui.ttf"),17*c.Scale,fc,glyphs);io.Fonts.Build();}fc.Destroy();
            byte* atlas;int aw,ah;io.Fonts.GetTexDataAsRGBA32(0,&atlas,&aw,&ah);io.Fonts.SetTexID(0,new ImTextureID(1UL));
            void UploadFontTextures(){
                for(int index=0;index<io.Fonts.Textures.Size;index++) {
                    byte* pixels;int tw,th;io.Fonts.GetTexDataAsRGBA32(index,&pixels,&tw,&th);
                    var bytes=new byte[tw*th*4];Marshal.Copy((nint)pixels,bytes,0,bytes.Length);
                    var id=900000UL+(ulong)index;io.Fonts.SetTexID(index,new ImTextureID(id));textures[id]=(bytes,tw,th);
                }
            }
            float height=0;float overflow=0;
            var guideState=new GuidePanelState{Folded=c.Panel=="guide-folded",Healing=c.Panel.Contains("healing")};
            var pickerConfig=Configuration.Create();pickerConfig.ManualJob=GuideJob.Paladin;
            if(c.Panel=="picker-favorites")pickerConfig.FavoriteJobs=[GuideJob.BlackMage,GuideJob.Sage,GuideJob.Paladin];
            Vector2 pickerPosition=default;
            for(var frame=0;frame<(c.Panel.StartsWith("picker")?6:4);frame++) {
                overflow=0;
                if(c.Panel.StartsWith("picker")&&frame==1)io.AddMousePosEvent(pickerPosition.X+40*c.Scale,pickerPosition.Y+14*c.Scale);
                if(c.Panel.StartsWith("picker")&&frame==2)io.AddMouseButtonEvent(0,true);
                if(c.Panel.StartsWith("picker")&&frame==3)io.AddMouseButtonEvent(0,false);
                UploadFontTextures();
                ImGui.NewFrame();ImGui.SetNextWindowPos(new(16*c.Scale,16*c.Scale));ImGui.SetNextWindowSize(new(c.Width*c.Scale,c.Panel.StartsWith("guide")?840*c.Scale:0));
                if(c.Panel.StartsWith("guide"))GuidePanel.PushStyle(Configuration.Create());else if(c.Panel!="hud")Panels.PushTheme();
                var windowOpen=true;
                if(c.Panel=="guide-folded")ImGui.SetNextWindowSize(new(c.Width*c.Scale,62*c.Scale));
                if(c.Panel.EndsWith("short"))ImGui.SetNextWindowSize(new(c.Width*c.Scale,560*c.Scale));
                ImGui.Begin(c.Panel=="hud"?"Cycle & Opener###Preview":c.Panel=="popup"?"Cycle & Opener · Niveau adapté":c.Panel.StartsWith("guide")?"Cycle & Opener · Guide":"Cycle & Opener · Réglages",ref windowOpen,(c.Panel.StartsWith("guide")?Panels.GuideFlags(false):ImGuiWindowFlags.AlwaysAutoResize)|(c.Panel=="hud"?ImGuiWindowFlags.NoTitleBar:0));
                if(c.Panel=="hud") Hud.Draw(c.Mode,c.State,id=>textures.ContainsKey(id)?new ImTextureID(id):null,new(c.Scale),c.Step,true);
                else if(c.Panel.StartsWith("guide")) {
                    var cfg=Configuration.Create();cfg.ShowGuide=true;cfg.Locked=c.Panel.Contains("locked");cfg.Layout=c.Mode;cfg.Targets=c.State.Targets;cfg.ManualLevel=c.Panel.Contains("manual");cfg.PreviewLevel=c.State.Level;cfg.ManualJob=c.State.Job;var step=c.Step;
                    var ui=guideState;
                    ImTextureID? Icon(uint id)=>textures.ContainsKey(id)?new ImTextureID(id):null;
                    GuidePanel.Header(cfg,c.State,ui,Icon,()=>{});
                    if(!ui.Folded){
                        GuidePanel.Controls(cfg,c.State,ref step);
                        ImGui.BeginChild("Lecture du guide",Vector2.Zero,false);
                        GuidePanel.Content(cfg,cfg.Resolve(c.State),ui,Icon,new(c.Scale),ref step);
                        if(c.Panel=="guide-footer"||c.Panel.Contains("healing"))ImGui.SetScrollY(ImGui.GetScrollMaxY());
                        overflow=Math.Max(overflow,ImGui.GetScrollMaxX());ImGui.EndChild();
                    }
                }
                else if(c.Panel.StartsWith("settings")) {var cfg=Configuration.Create();cfg.ShowGuide=true;cfg.ManualJob=c.State.Job;cfg.ManualLevel=c.Panel=="settings-manual";var step=0;if(c.Panel=="settings-appearance")ImGui.GetStateStorage().SetInt(ImGui.GetID("Apparence du guide"),1);Panels.Settings(cfg,c.State,false,"CycleOpener.dll",ref step,()=>{},()=>{});}
                else if(c.Panel.StartsWith("picker")){
                    ImGui.Text("Job à consulter");pickerPosition=ImGui.GetCursorScreenPos();
                    ImGui.PushID("Demo");ImGui.GetStateStorage().SetInt(ImGui.GetID("Filtre de rôle"),c.Panel=="picker"?2:1);ImGui.PopID();
                    JobPicker.Draw(pickerConfig,"Demo",ImGui.GetContentRegionAvail().X);
                    if(frame==5&&!ImGui.IsPopupOpen("",ImGuiPopupFlags.AnyPopupId|ImGuiPopupFlags.AnyPopupLevel))throw new Exception($"Picker popup did not open: {c.Name}");
                }
                else {var disabled=false;Panels.Prompt(c.State,c.Mode,ref disabled);}
                height=ImGui.GetWindowSize().Y;overflow=Math.Max(overflow,ImGui.GetScrollMaxX());ImGui.End();if(c.Panel.StartsWith("guide"))GuidePanel.PopStyle();else if(c.Panel!="hud")Panels.PopTheme();ImGui.Render();
            }
            // ImGui may rebake fonts at requested draw-list sizes during a frame.
            // Always sample the final atlas, never its pre-frame pointer.
            io.Fonts.GetTexDataAsRGBA32(0,&atlas,&aw,&ah);
            UploadFontTextures();
            var h=(int)Math.Ceiling(height+32*c.Scale);
            if(c.Panel.StartsWith("picker"))h=(int)(570*c.Scale);
            CpuRenderer.Save(ImGui.GetDrawData(),pw,h,atlas,aw,ah,Path.Combine(output,c.Name+".png"),textures);
            if(overflow>0.5f)throw new Exception($"Horizontal overflow {c.Name}: {overflow}");
            metrics.Add(new {c.Name,Width=pw,Height=h,HorizontalScroll=overflow});
        }finally {ImGui.DestroyContext(context);}
    }
    File.WriteAllText(Path.Combine(root,".artifacts",filter==null?"render-metrics.json":"render-metrics-partial.json"),JsonSerializer.Serialize(metrics,new JsonSerializerOptions{WriteIndented=true}));
    Console.WriteLine($"Rendered {metrics.Count} native ImGui views. No horizontal scrolling.");
 }
 private sealed class NativeContext(string path):INativeContext,IDisposable {
    public nint Module{get;}=NativeLibrary.Load(path);
    public nint GetProcAddress(string name)=>NativeLibrary.GetExport(Module,name);
    public bool TryGetProcAddress(string name,out nint address)=>NativeLibrary.TryGetExport(Module,name,out address);
    public bool IsExtensionSupported(string name)=>false;
    public void Dispose()=>NativeLibrary.Free(Module);
 }
}
