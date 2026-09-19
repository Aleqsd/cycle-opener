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
    var textures=new Dictionary<ulong,(byte[] Pixels,int Width,int Height)>();
    foreach(var file in Directory.EnumerateFiles(Path.Combine(root,".artifacts","icons"),"*.rgba")) {
        using var reader=new BinaryReader(File.OpenRead(file));var w=reader.ReadInt32();var h=reader.ReadInt32();textures[ulong.Parse(Path.GetFileNameWithoutExtension(file))]=(reader.ReadBytes(w*h*4),w,h);
    }
    var cases=new List<(string Name, Layout Mode, GuideContext State,float Scale,int Width,int Step,string Panel)>();
    foreach(var level in new[]{20,35,50,60,70,80,90,100}) foreach(var targets in new[]{1,2,3}) foreach(var mode in Enum.GetValues<Layout>())
    {
        cases.Add(($"{level}-{targets}-{(int)mode}",mode,new GuideContext(level,targets),1,(int)Hud.Preferred(mode).X,0,"hud"));
        cases.Add(($"duo-{level}-{targets}-{(int)mode}",mode,new GuideContext(level,targets),1,400,0,"hud"));
    }
    foreach(var mode in Enum.GetValues<Layout>()) foreach(var scale in new[]{1f,1.5f,2f})
        cases.Add(($"minimum-{(int)mode}-{scale*100:0}",mode,new GuideContext(100,3),scale,280,0,"hud"));
    foreach(var mode in Enum.GetValues<Layout>()) foreach(var scale in new[]{1f,1.5f,2f})
        cases.Add(($"window-{(int)mode}-{scale*100:0}",mode,new GuideContext(100,3),scale,300,0,"window"));
    foreach(var scale in new[]{1f,1.5f,2f}) {
        cases.Add(($"settings-{scale*100:0}",Layout.Focus,new(),scale,490,0,"settings"));
        cases.Add(($"popup-{scale*100:0}",Layout.Focus,new(Level:50),scale,410,0,"popup"));
        cases.Add(($"settings-minimum-{scale*100:0}",Layout.Focus,new(),scale,360,0,"settings"));
    }
    // Exact opening step variants for the interactive comparison.
    for(var i=0;i<24;i++)cases.Add(($"opener-{i}",Layout.Ouverture,new(),1,590,i,"hud"));
    for(var i=0;i<24;i++)cases.Add(($"opener-duo-{i}",Layout.Ouverture,new(),1,400,i,"hud"));
    var metrics=new List<object>();
    foreach(var c in cases)
    {
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
            for(var frame=0;frame<3;frame++) {
                UploadFontTextures();
                ImGui.NewFrame();ImGui.SetNextWindowPos(new(16*c.Scale,16*c.Scale));ImGui.SetNextWindowSize(new(c.Width*c.Scale,0));
                if(c.Panel!="hud")Panels.PushTheme();
                ImGui.Begin(c.Panel=="hud"?"Cycle & Opener###Preview":c.Panel=="popup"?"Cycle & Opener · Niveau adapté":c.Panel=="window"?"Cycle & Opener · Cycle":"Cycle & Opener · Réglages",ImGuiWindowFlags.AlwaysAutoResize|(c.Panel=="hud"?ImGuiWindowFlags.NoTitleBar:0));
                if(c.Panel=="hud") Hud.Draw(c.Mode,c.State,id=>textures.ContainsKey(id)?new ImTextureID(id):null,new(c.Scale),c.Step,true);
                else if(c.Panel=="window") {var cfg=new Configuration{Layout=c.Mode,Targets=c.State.Targets};var step=c.Step;Panels.GuideToolbar(cfg,()=>{},ref step,c.State.Level);Hud.Draw(c.Mode,c.State,id=>textures.ContainsKey(id)?new ImTextureID(id):null,new(c.Scale),step,true);}
                else if(c.Panel=="settings") {var cfg=new Configuration();var demo=false;var step=0;Panels.Settings(cfg,c.State,false,"CycleOpener.dll",ref demo,ref step,()=>{},()=>{});}
                else {var disabled=false;Panels.Prompt(c.State,c.Mode,ref disabled);}
                height=ImGui.GetWindowSize().Y;overflow=ImGui.GetScrollMaxX();ImGui.End();if(c.Panel!="hud")Panels.PopTheme();ImGui.Render();
            }
            // ImGui may rebake fonts at requested draw-list sizes during a frame.
            // Always sample the final atlas, never its pre-frame pointer.
            io.Fonts.GetTexDataAsRGBA32(0,&atlas,&aw,&ah);
            UploadFontTextures();
            var h=(int)Math.Ceiling(height+32*c.Scale);
            CpuRenderer.Save(ImGui.GetDrawData(),pw,h,atlas,aw,ah,Path.Combine(output,c.Name+".png"),textures);
            if(overflow>0.5f)throw new Exception($"Horizontal overflow {c.Name}: {overflow}");
            metrics.Add(new {c.Name,Width=pw,Height=h,HorizontalScroll=overflow});
        }finally {ImGui.DestroyContext(context);}
    }
    File.WriteAllText(Path.Combine(root,".artifacts","render-metrics.json"),JsonSerializer.Serialize(metrics,new JsonSerializerOptions{WriteIndented=true}));
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
