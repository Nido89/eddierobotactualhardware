//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.296
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

[assembly: global::System.Reflection.AssemblyVersionAttribute("0.0.0.0")]
[assembly: global::Microsoft.Dss.Core.Attributes.ServiceDeclarationAttribute(global::Microsoft.Dss.Core.Attributes.DssServiceDeclaration.Transform, SourceAssemblyKey="User.ColorSegment.Y2007.M07, Version=0.0.0.0, Culture=neutral, PublicKeyToken=7f9" +
    "074033fd3dcf7")]
[assembly: global::System.Security.SecurityTransparentAttribute()]
[assembly: global::System.Security.SecurityRulesAttribute(global::System.Security.SecurityRuleSet.Level1)]

namespace Dss.Transforms.TransformUser {
    
    
    public class Transforms : global::Microsoft.Dss.Core.Transforms.TransformBase {
        
        static Transforms() {
            Register();
        }
        
        public static void Register() {
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddProxyTransform(typeof(global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.ColorSegmentState), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Robotics_Services_Sample_ColorSegment_Proxy_ColorSegmentState_TO_Microsoft_Robotics_Services_Sample_ColorSegment_ColorSegmentState));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddSourceTransform(typeof(global::Microsoft.Robotics.Services.Sample.ColorSegment.ColorSegmentState), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Robotics_Services_Sample_ColorSegment_ColorSegmentState_TO_Microsoft_Robotics_Services_Sample_ColorSegment_Proxy_ColorSegmentState));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddProxyTransform(typeof(global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.Settings), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Robotics_Services_Sample_ColorSegment_Proxy_Settings_TO_Microsoft_Robotics_Services_Sample_ColorSegment_Settings));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddSourceTransform(typeof(global::Microsoft.Robotics.Services.Sample.ColorSegment.Settings), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Robotics_Services_Sample_ColorSegment_Settings_TO_Microsoft_Robotics_Services_Sample_ColorSegment_Proxy_Settings));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddProxyTransform(typeof(global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.ColorSet), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Robotics_Services_Sample_ColorSegment_Proxy_ColorSet_TO_Microsoft_Robotics_Services_Sample_ColorSegment_ColorSet));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddSourceTransform(typeof(global::Microsoft.Robotics.Services.Sample.ColorSegment.ColorSet), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Robotics_Services_Sample_ColorSegment_ColorSet_TO_Microsoft_Robotics_Services_Sample_ColorSegment_Proxy_ColorSet));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddProxyTransform(typeof(global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.ColorDefinition), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Robotics_Services_Sample_ColorSegment_Proxy_ColorDefinition_TO_Microsoft_Robotics_Services_Sample_ColorSegment_ColorDefinition));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddSourceTransform(typeof(global::Microsoft.Robotics.Services.Sample.ColorSegment.ColorDefinition), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Robotics_Services_Sample_ColorSegment_ColorDefinition_TO_Microsoft_Robotics_Services_Sample_ColorSegment_Proxy_ColorDefinition));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddProxyTransform(typeof(global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.SegmentedImage), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Robotics_Services_Sample_ColorSegment_Proxy_SegmentedImage_TO_Microsoft_Robotics_Services_Sample_ColorSegment_SegmentedImage));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddSourceTransform(typeof(global::Microsoft.Robotics.Services.Sample.ColorSegment.SegmentedImage), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Robotics_Services_Sample_ColorSegment_SegmentedImage_TO_Microsoft_Robotics_Services_Sample_ColorSegment_Proxy_SegmentedImage));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddProxyTransform(typeof(global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.FoundColorAreas), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Robotics_Services_Sample_ColorSegment_Proxy_FoundColorAreas_TO_Microsoft_Robotics_Services_Sample_ColorSegment_FoundColorAreas));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddSourceTransform(typeof(global::Microsoft.Robotics.Services.Sample.ColorSegment.FoundColorAreas), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Robotics_Services_Sample_ColorSegment_FoundColorAreas_TO_Microsoft_Robotics_Services_Sample_ColorSegment_Proxy_FoundColorAreas));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddProxyTransform(typeof(global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.ColorArea), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Robotics_Services_Sample_ColorSegment_Proxy_ColorArea_TO_Microsoft_Robotics_Services_Sample_ColorSegment_ColorArea));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddSourceTransform(typeof(global::Microsoft.Robotics.Services.Sample.ColorSegment.ColorArea), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Robotics_Services_Sample_ColorSegment_ColorArea_TO_Microsoft_Robotics_Services_Sample_ColorSegment_Proxy_ColorArea));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddProxyTransform(typeof(global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.ProcessFrameRequest), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Robotics_Services_Sample_ColorSegment_Proxy_ProcessFrameRequest_TO_Microsoft_Robotics_Services_Sample_ColorSegment_ProcessFrameRequest));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddSourceTransform(typeof(global::Microsoft.Robotics.Services.Sample.ColorSegment.ProcessFrameRequest), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Robotics_Services_Sample_ColorSegment_ProcessFrameRequest_TO_Microsoft_Robotics_Services_Sample_ColorSegment_Proxy_ProcessFrameRequest));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddProxyTransform(typeof(global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.FilteredSubscribeRequest), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Robotics_Services_Sample_ColorSegment_Proxy_FilteredSubscribeRequest_TO_Microsoft_Robotics_Services_Sample_ColorSegment_FilteredSubscribeRequest));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddSourceTransform(typeof(global::Microsoft.Robotics.Services.Sample.ColorSegment.FilteredSubscribeRequest), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Robotics_Services_Sample_ColorSegment_FilteredSubscribeRequest_TO_Microsoft_Robotics_Services_Sample_ColorSegment_Proxy_FilteredSubscribeRequest));
        }
        
        public static object Microsoft_Robotics_Services_Sample_ColorSegment_Proxy_ColorSegmentState_TO_Microsoft_Robotics_Services_Sample_ColorSegment_ColorSegmentState(object transformFrom) {
            global::Microsoft.Robotics.Services.Sample.ColorSegment.ColorSegmentState target = new global::Microsoft.Robotics.Services.Sample.ColorSegment.ColorSegmentState();
            global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.ColorSegmentState from = ((global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.ColorSegmentState)(transformFrom));
            target.Processing = from.Processing;
            target.FrameCount = from.FrameCount;
            target.DroppedFrames = from.DroppedFrames;
            target.ImageSource = from.ImageSource;
            if ((from.Settings != null)) {
                target.Settings = ((global::Microsoft.Robotics.Services.Sample.ColorSegment.Settings)(Microsoft_Robotics_Services_Sample_ColorSegment_Proxy_Settings_TO_Microsoft_Robotics_Services_Sample_ColorSegment_Settings(from.Settings)));
            }
            else {
                target.Settings = null;
            }
            if ((from.Colors != null)) {
                int count = from.Colors.Count;
                global::System.Collections.Generic.List<global::Microsoft.Robotics.Services.Sample.ColorSegment.ColorSet> tmp0 = new global::System.Collections.Generic.List<global::Microsoft.Robotics.Services.Sample.ColorSegment.ColorSet>(count);
                for (int index = 0; (index < count); index = (index + 1)) {
                    global::Microsoft.Robotics.Services.Sample.ColorSegment.ColorSet tmp1 = default(global::Microsoft.Robotics.Services.Sample.ColorSegment.ColorSet);
                    if ((from.Colors[index] != null)) {
                        tmp1 = ((global::Microsoft.Robotics.Services.Sample.ColorSegment.ColorSet)(Microsoft_Robotics_Services_Sample_ColorSegment_Proxy_ColorSet_TO_Microsoft_Robotics_Services_Sample_ColorSegment_ColorSet(from.Colors[index])));
                    }
                    else {
                        tmp1 = null;
                    }
                    tmp0.Add(tmp1);
                }
                target.Colors = tmp0;
            }
            else {
                target.Colors = null;
            }
            if ((from.SegmentedImage != null)) {
                target.SegmentedImage = ((global::Microsoft.Robotics.Services.Sample.ColorSegment.SegmentedImage)(Microsoft_Robotics_Services_Sample_ColorSegment_Proxy_SegmentedImage_TO_Microsoft_Robotics_Services_Sample_ColorSegment_SegmentedImage(from.SegmentedImage)));
            }
            else {
                target.SegmentedImage = null;
            }
            if ((from.FoundColorAreas != null)) {
                target.FoundColorAreas = ((global::Microsoft.Robotics.Services.Sample.ColorSegment.FoundColorAreas)(Microsoft_Robotics_Services_Sample_ColorSegment_Proxy_FoundColorAreas_TO_Microsoft_Robotics_Services_Sample_ColorSegment_FoundColorAreas(from.FoundColorAreas)));
            }
            else {
                target.FoundColorAreas = null;
            }
            return target;
        }
        
        public static object Microsoft_Robotics_Services_Sample_ColorSegment_ColorSegmentState_TO_Microsoft_Robotics_Services_Sample_ColorSegment_Proxy_ColorSegmentState(object transformFrom) {
            global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.ColorSegmentState target = new global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.ColorSegmentState();
            global::Microsoft.Robotics.Services.Sample.ColorSegment.ColorSegmentState from = ((global::Microsoft.Robotics.Services.Sample.ColorSegment.ColorSegmentState)(transformFrom));
            target.Processing = from.Processing;
            target.FrameCount = from.FrameCount;
            target.DroppedFrames = from.DroppedFrames;
            global::System.Uri tmp = from.ImageSource;
            target.ImageSource = tmp;
            global::Microsoft.Robotics.Services.Sample.ColorSegment.Settings tmp1 = from.Settings;
            if ((tmp1 != null)) {
                target.Settings = ((global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.Settings)(Microsoft_Robotics_Services_Sample_ColorSegment_Settings_TO_Microsoft_Robotics_Services_Sample_ColorSegment_Proxy_Settings(tmp1)));
            }
            global::System.Collections.Generic.List<global::Microsoft.Robotics.Services.Sample.ColorSegment.ColorSet> tmp2 = from.Colors;
            if ((tmp2 != null)) {
                int count = tmp2.Count;
                global::System.Collections.Generic.List<global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.ColorSet> tmp3 = new global::System.Collections.Generic.List<global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.ColorSet>(count);
                for (int index = 0; (index < count); index = (index + 1)) {
                    global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.ColorSet tmp4 = default(global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.ColorSet);
                    global::Microsoft.Robotics.Services.Sample.ColorSegment.ColorSet tmp5 = tmp2[index];
                    if ((tmp5 != null)) {
                        tmp4 = ((global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.ColorSet)(Microsoft_Robotics_Services_Sample_ColorSegment_ColorSet_TO_Microsoft_Robotics_Services_Sample_ColorSegment_Proxy_ColorSet(tmp5)));
                    }
                    tmp3.Add(tmp4);
                }
                target.Colors = tmp3;
            }
            global::Microsoft.Robotics.Services.Sample.ColorSegment.SegmentedImage tmp6 = from.SegmentedImage;
            if ((tmp6 != null)) {
                target.SegmentedImage = ((global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.SegmentedImage)(Microsoft_Robotics_Services_Sample_ColorSegment_SegmentedImage_TO_Microsoft_Robotics_Services_Sample_ColorSegment_Proxy_SegmentedImage(tmp6)));
            }
            global::Microsoft.Robotics.Services.Sample.ColorSegment.FoundColorAreas tmp7 = from.FoundColorAreas;
            if ((tmp7 != null)) {
                target.FoundColorAreas = ((global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.FoundColorAreas)(Microsoft_Robotics_Services_Sample_ColorSegment_FoundColorAreas_TO_Microsoft_Robotics_Services_Sample_ColorSegment_Proxy_FoundColorAreas(tmp7)));
            }
            return target;
        }
        
        public static object Microsoft_Robotics_Services_Sample_ColorSegment_Proxy_Settings_TO_Microsoft_Robotics_Services_Sample_ColorSegment_Settings(object transformFrom) {
            global::Microsoft.Robotics.Services.Sample.ColorSegment.Settings target = new global::Microsoft.Robotics.Services.Sample.ColorSegment.Settings();
            global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.Settings from = ((global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.Settings)(transformFrom));
            target.Threshold = from.Threshold;
            target.ShowPartialMatches = from.ShowPartialMatches;
            target.Despeckle = from.Despeckle;
            target.MinBlobSize = from.MinBlobSize;
            return target;
        }
        
        public static object Microsoft_Robotics_Services_Sample_ColorSegment_Settings_TO_Microsoft_Robotics_Services_Sample_ColorSegment_Proxy_Settings(object transformFrom) {
            global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.Settings target = new global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.Settings();
            global::Microsoft.Robotics.Services.Sample.ColorSegment.Settings from = ((global::Microsoft.Robotics.Services.Sample.ColorSegment.Settings)(transformFrom));
            target.Threshold = from.Threshold;
            target.ShowPartialMatches = from.ShowPartialMatches;
            target.Despeckle = from.Despeckle;
            target.MinBlobSize = from.MinBlobSize;
            return target;
        }
        
        public static object Microsoft_Robotics_Services_Sample_ColorSegment_Proxy_ColorSet_TO_Microsoft_Robotics_Services_Sample_ColorSegment_ColorSet(object transformFrom) {
            global::Microsoft.Robotics.Services.Sample.ColorSegment.ColorSet target = new global::Microsoft.Robotics.Services.Sample.ColorSegment.ColorSet();
            global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.ColorSet from = ((global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.ColorSet)(transformFrom));
            target.Name = from.Name;
            if ((from.Colors != null)) {
                int count = from.Colors.Count;
                global::System.Collections.Generic.List<global::Microsoft.Robotics.Services.Sample.ColorSegment.ColorDefinition> tmp = new global::System.Collections.Generic.List<global::Microsoft.Robotics.Services.Sample.ColorSegment.ColorDefinition>(count);
                for (int index = 0; (index < count); index = (index + 1)) {
                    global::Microsoft.Robotics.Services.Sample.ColorSegment.ColorDefinition tmp0 = default(global::Microsoft.Robotics.Services.Sample.ColorSegment.ColorDefinition);
                    if ((from.Colors[index] != null)) {
                        tmp0 = ((global::Microsoft.Robotics.Services.Sample.ColorSegment.ColorDefinition)(Microsoft_Robotics_Services_Sample_ColorSegment_Proxy_ColorDefinition_TO_Microsoft_Robotics_Services_Sample_ColorSegment_ColorDefinition(from.Colors[index])));
                    }
                    else {
                        tmp0 = null;
                    }
                    tmp.Add(tmp0);
                }
                target.Colors = tmp;
            }
            else {
                target.Colors = null;
            }
            return target;
        }
        
        public static object Microsoft_Robotics_Services_Sample_ColorSegment_ColorSet_TO_Microsoft_Robotics_Services_Sample_ColorSegment_Proxy_ColorSet(object transformFrom) {
            global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.ColorSet target = new global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.ColorSet();
            global::Microsoft.Robotics.Services.Sample.ColorSegment.ColorSet from = ((global::Microsoft.Robotics.Services.Sample.ColorSegment.ColorSet)(transformFrom));
            target.Name = from.Name;
            global::System.Collections.Generic.List<global::Microsoft.Robotics.Services.Sample.ColorSegment.ColorDefinition> tmp = from.Colors;
            if ((tmp != null)) {
                int count = tmp.Count;
                global::System.Collections.Generic.List<global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.ColorDefinition> tmp0 = new global::System.Collections.Generic.List<global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.ColorDefinition>(count);
                for (int index = 0; (index < count); index = (index + 1)) {
                    global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.ColorDefinition tmp1 = default(global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.ColorDefinition);
                    global::Microsoft.Robotics.Services.Sample.ColorSegment.ColorDefinition tmp2 = tmp[index];
                    if ((tmp2 != null)) {
                        tmp1 = ((global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.ColorDefinition)(Microsoft_Robotics_Services_Sample_ColorSegment_ColorDefinition_TO_Microsoft_Robotics_Services_Sample_ColorSegment_Proxy_ColorDefinition(tmp2)));
                    }
                    tmp0.Add(tmp1);
                }
                target.Colors = tmp0;
            }
            return target;
        }
        
        public static object Microsoft_Robotics_Services_Sample_ColorSegment_Proxy_ColorDefinition_TO_Microsoft_Robotics_Services_Sample_ColorSegment_ColorDefinition(object transformFrom) {
            global::Microsoft.Robotics.Services.Sample.ColorSegment.ColorDefinition target = new global::Microsoft.Robotics.Services.Sample.ColorSegment.ColorDefinition();
            global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.ColorDefinition from = ((global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.ColorDefinition)(transformFrom));
            target.Name = from.Name;
            target.Y = from.Y;
            target.Cb = from.Cb;
            target.Cr = from.Cr;
            target.SigmaY = from.SigmaY;
            target.SigmaCb = from.SigmaCb;
            target.SigmaCr = from.SigmaCr;
            target.R = from.R;
            target.G = from.G;
            target.B = from.B;
            return target;
        }
        
        public static object Microsoft_Robotics_Services_Sample_ColorSegment_ColorDefinition_TO_Microsoft_Robotics_Services_Sample_ColorSegment_Proxy_ColorDefinition(object transformFrom) {
            global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.ColorDefinition target = new global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.ColorDefinition();
            global::Microsoft.Robotics.Services.Sample.ColorSegment.ColorDefinition from = ((global::Microsoft.Robotics.Services.Sample.ColorSegment.ColorDefinition)(transformFrom));
            target.Name = from.Name;
            target.Y = from.Y;
            target.Cb = from.Cb;
            target.Cr = from.Cr;
            target.SigmaY = from.SigmaY;
            target.SigmaCb = from.SigmaCb;
            target.SigmaCr = from.SigmaCr;
            target.R = from.R;
            target.G = from.G;
            target.B = from.B;
            return target;
        }
        
        public static object Microsoft_Robotics_Services_Sample_ColorSegment_Proxy_SegmentedImage_TO_Microsoft_Robotics_Services_Sample_ColorSegment_SegmentedImage(object transformFrom) {
            global::Microsoft.Robotics.Services.Sample.ColorSegment.SegmentedImage target = new global::Microsoft.Robotics.Services.Sample.ColorSegment.SegmentedImage();
            global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.SegmentedImage from = ((global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.SegmentedImage)(transformFrom));
            target.TimeStamp = from.TimeStamp;
            target.Width = from.Width;
            target.height = from.height;
            if ((from.Segmented != null)) {
                int count = from.Segmented.Length;
                byte[] tmp = new byte[count];
                global::System.Buffer.BlockCopy(from.Segmented, 0, tmp, 0, global::System.Buffer.ByteLength(from.Segmented));
                target.Segmented = tmp;
            }
            else {
                target.Segmented = null;
            }
            return target;
        }
        
        public static object Microsoft_Robotics_Services_Sample_ColorSegment_SegmentedImage_TO_Microsoft_Robotics_Services_Sample_ColorSegment_Proxy_SegmentedImage(object transformFrom) {
            global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.SegmentedImage target = new global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.SegmentedImage();
            global::Microsoft.Robotics.Services.Sample.ColorSegment.SegmentedImage from = ((global::Microsoft.Robotics.Services.Sample.ColorSegment.SegmentedImage)(transformFrom));
            target.TimeStamp = from.TimeStamp;
            target.Width = from.Width;
            target.height = from.height;
            byte[] tmp = from.Segmented;
            if ((tmp != null)) {
                int count = tmp.Length;
                byte[] tmp0 = new byte[count];
                global::System.Buffer.BlockCopy(tmp, 0, tmp0, 0, global::System.Buffer.ByteLength(tmp));
                target.Segmented = tmp0;
            }
            return target;
        }
        
        public static object Microsoft_Robotics_Services_Sample_ColorSegment_Proxy_FoundColorAreas_TO_Microsoft_Robotics_Services_Sample_ColorSegment_FoundColorAreas(object transformFrom) {
            global::Microsoft.Robotics.Services.Sample.ColorSegment.FoundColorAreas target = new global::Microsoft.Robotics.Services.Sample.ColorSegment.FoundColorAreas();
            global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.FoundColorAreas from = ((global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.FoundColorAreas)(transformFrom));
            target.TimeStamp = from.TimeStamp;
            if ((from.Areas != null)) {
                int count = from.Areas.Count;
                global::System.Collections.Generic.List<global::Microsoft.Robotics.Services.Sample.ColorSegment.ColorArea> tmp = new global::System.Collections.Generic.List<global::Microsoft.Robotics.Services.Sample.ColorSegment.ColorArea>(count);
                for (int index = 0; (index < count); index = (index + 1)) {
                    global::Microsoft.Robotics.Services.Sample.ColorSegment.ColorArea tmp0 = default(global::Microsoft.Robotics.Services.Sample.ColorSegment.ColorArea);
                    if ((from.Areas[index] != null)) {
                        tmp0 = ((global::Microsoft.Robotics.Services.Sample.ColorSegment.ColorArea)(Microsoft_Robotics_Services_Sample_ColorSegment_Proxy_ColorArea_TO_Microsoft_Robotics_Services_Sample_ColorSegment_ColorArea(from.Areas[index])));
                    }
                    else {
                        tmp0 = null;
                    }
                    tmp.Add(tmp0);
                }
                target.Areas = tmp;
            }
            else {
                target.Areas = null;
            }
            return target;
        }
        
        public static object Microsoft_Robotics_Services_Sample_ColorSegment_FoundColorAreas_TO_Microsoft_Robotics_Services_Sample_ColorSegment_Proxy_FoundColorAreas(object transformFrom) {
            global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.FoundColorAreas target = new global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.FoundColorAreas();
            global::Microsoft.Robotics.Services.Sample.ColorSegment.FoundColorAreas from = ((global::Microsoft.Robotics.Services.Sample.ColorSegment.FoundColorAreas)(transformFrom));
            target.TimeStamp = from.TimeStamp;
            global::System.Collections.Generic.List<global::Microsoft.Robotics.Services.Sample.ColorSegment.ColorArea> tmp = from.Areas;
            if ((tmp != null)) {
                int count = tmp.Count;
                global::System.Collections.Generic.List<global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.ColorArea> tmp0 = new global::System.Collections.Generic.List<global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.ColorArea>(count);
                for (int index = 0; (index < count); index = (index + 1)) {
                    global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.ColorArea tmp1 = default(global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.ColorArea);
                    global::Microsoft.Robotics.Services.Sample.ColorSegment.ColorArea tmp2 = tmp[index];
                    if ((tmp2 != null)) {
                        tmp1 = ((global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.ColorArea)(Microsoft_Robotics_Services_Sample_ColorSegment_ColorArea_TO_Microsoft_Robotics_Services_Sample_ColorSegment_Proxy_ColorArea(tmp2)));
                    }
                    tmp0.Add(tmp1);
                }
                target.Areas = tmp0;
            }
            return target;
        }
        
        public static object Microsoft_Robotics_Services_Sample_ColorSegment_Proxy_ColorArea_TO_Microsoft_Robotics_Services_Sample_ColorSegment_ColorArea(object transformFrom) {
            global::Microsoft.Robotics.Services.Sample.ColorSegment.ColorArea target = new global::Microsoft.Robotics.Services.Sample.ColorSegment.ColorArea();
            global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.ColorArea from = ((global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.ColorArea)(transformFrom));
            target.Name = from.Name;
            target.CenterX = from.CenterX;
            target.CenterY = from.CenterY;
            target.MinX = from.MinX;
            target.MaxX = from.MaxX;
            target.MinY = from.MinY;
            target.MaxY = from.MaxY;
            target.Area = from.Area;
            return target;
        }
        
        public static object Microsoft_Robotics_Services_Sample_ColorSegment_ColorArea_TO_Microsoft_Robotics_Services_Sample_ColorSegment_Proxy_ColorArea(object transformFrom) {
            global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.ColorArea target = new global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.ColorArea();
            global::Microsoft.Robotics.Services.Sample.ColorSegment.ColorArea from = ((global::Microsoft.Robotics.Services.Sample.ColorSegment.ColorArea)(transformFrom));
            target.Name = from.Name;
            target.CenterX = from.CenterX;
            target.CenterY = from.CenterY;
            target.MinX = from.MinX;
            target.MaxX = from.MaxX;
            target.MinY = from.MinY;
            target.MaxY = from.MaxY;
            target.Area = from.Area;
            return target;
        }
        
        public static object Microsoft_Robotics_Services_Sample_ColorSegment_Proxy_ProcessFrameRequest_TO_Microsoft_Robotics_Services_Sample_ColorSegment_ProcessFrameRequest(object transformFrom) {
            global::Microsoft.Robotics.Services.Sample.ColorSegment.ProcessFrameRequest target = new global::Microsoft.Robotics.Services.Sample.ColorSegment.ProcessFrameRequest();
            global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.ProcessFrameRequest from = ((global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.ProcessFrameRequest)(transformFrom));
            target.Process = from.Process;
            return target;
        }
        
        public static object Microsoft_Robotics_Services_Sample_ColorSegment_ProcessFrameRequest_TO_Microsoft_Robotics_Services_Sample_ColorSegment_Proxy_ProcessFrameRequest(object transformFrom) {
            global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.ProcessFrameRequest target = new global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.ProcessFrameRequest();
            global::Microsoft.Robotics.Services.Sample.ColorSegment.ProcessFrameRequest from = ((global::Microsoft.Robotics.Services.Sample.ColorSegment.ProcessFrameRequest)(transformFrom));
            target.Process = from.Process;
            return target;
        }
        
        public static object Microsoft_Robotics_Services_Sample_ColorSegment_Proxy_FilteredSubscribeRequest_TO_Microsoft_Robotics_Services_Sample_ColorSegment_FilteredSubscribeRequest(object transformFrom) {
            global::Microsoft.Robotics.Services.Sample.ColorSegment.FilteredSubscribeRequest target = new global::Microsoft.Robotics.Services.Sample.ColorSegment.FilteredSubscribeRequest();
            global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.FilteredSubscribeRequest from = ((global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.FilteredSubscribeRequest)(transformFrom));
            target.Filter = ((global::Microsoft.Robotics.Services.Sample.ColorSegment.Filter)(((int)(from.Filter))));
            target.Subscriber = from.Subscriber;
            target.Expiration = from.Expiration;
            target.NotificationCount = from.NotificationCount;
            if ((from.TypeFilter != null)) {
                int count = from.TypeFilter.Length;
                string[] tmp = new string[count];
                from.TypeFilter.CopyTo(tmp, 0);
                target.TypeFilter = tmp;
            }
            else {
                target.TypeFilter = null;
            }
            return target;
        }
        
        public static object Microsoft_Robotics_Services_Sample_ColorSegment_FilteredSubscribeRequest_TO_Microsoft_Robotics_Services_Sample_ColorSegment_Proxy_FilteredSubscribeRequest(object transformFrom) {
            global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.FilteredSubscribeRequest target = new global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.FilteredSubscribeRequest();
            global::Microsoft.Robotics.Services.Sample.ColorSegment.FilteredSubscribeRequest from = ((global::Microsoft.Robotics.Services.Sample.ColorSegment.FilteredSubscribeRequest)(transformFrom));
            target.Filter = ((global::Microsoft.Robotics.Services.Sample.ColorSegment.Proxy.Filter)(((int)(from.Filter))));
            target.Subscriber = from.Subscriber;
            target.Expiration = from.Expiration;
            target.NotificationCount = from.NotificationCount;
            string[] tmp = from.TypeFilter;
            if ((tmp != null)) {
                int count = tmp.Length;
                string[] tmp0 = new string[count];
                tmp.CopyTo(tmp0, 0);
                target.TypeFilter = tmp0;
            }
            return target;
        }
    }
}
