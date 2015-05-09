﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CCT.NUI.HandTracking.Gesture
{
    public interface IGesture
    {
        void process(HandCollection handData, ref IGesture gestureState);
        void cleanup();

        String Name { get;}
    }

    public abstract class GestureBase : IGesture
    {
        public GestureBase(String name)
        {
            this.name = name;
        }
        private String name ;
        public String Name
        {
            get { return this.name; }
        }

        public abstract void process(HandCollection handData, ref IGesture gestureState);
        public abstract void cleanup(); 
    }
}
