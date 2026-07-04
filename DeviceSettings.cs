//using Microsoft.Xna.Framework;
//using Sachssoft.Sasogine.Features;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Sachssoft.Sasogine;

//public class DeviceSettings : GameSettings
//{
//    public FrameLimitMode FrameLimitMode
//    {
//        get => GetValue<FrameLimitMode>();
//        set => SetValue<FrameLimitMode>(value: value);
//    }

//    public int UserDefinedFramePerSeconds
//    {
//        get => GetValue<int>();
//        set => SetValue<int>(value: value);
//    }

//    public override void Apply(IGameApplication app)
//    {
//        if (app is not Game game)
//        {
//            throw new ArgumentException("Invalid type of game");
//        }

//        switch (FrameLimitMode)
//        {
//            case FrameLimitMode.Default:
//                game.IsFixedTimeStep = true;
//                game.TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 60); // 60 FPS
//                break;

//            case FrameLimitMode.Unlimited:
//                game.IsFixedTimeStep = false;
//                break;

//            case FrameLimitMode.Custom:
//                game.IsFixedTimeStep = true;
//                game.TargetElapsedTime = TimeSpan.FromSeconds(1.0 / UserDefinedFramePerSeconds);
//                break;
//        }
//    }
//}
