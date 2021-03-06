﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheEngine.GUI
{
    public interface IHwndHost
    {
        void Bind(Engine engine);
        IntPtr Handle { get; }
        float Aspect { get; }
        float WindowWidth { get; }
        float WindowHeight { get; }
    }
}
