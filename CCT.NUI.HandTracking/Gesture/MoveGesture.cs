﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//
using CCT.NUI.Core;
using CCT.NUI.HandTracking.Mouse;

namespace CCT.NUI.HandTracking.Gesture
{
    public class MoveGesture : GestureBase
    {

        public MoveGesture(int w, int h):
            base("Move", w, h)
        {
        }

        public override void process(HandCollection handData, ref IGesture gestureState)
        {
            var fingerCount = handData.Hands.First().FingerCount;

            if ( fingerCount != (int)Gestures.Move_Write ) // not valid handData for MoveGesture
            {
                if (!this.InAbnormal())
                {
                    this.BeginAbnormal();
                }
                else // in abnormal
                {
                    if (this.TimeToQuitGesture())
                        gestureState = null;
                }
            }
            else
            {
                if (this.InAbnormal())
                {
                    this.LeaveAbnormal();
                }

                gestureState = this;

                var pointOnScreen = this.MapToScreen(this.cursorMode.GetPoint(handData));

                this.MoveToScreen(pointOnScreen);
            }
        }

        public  override void cleanup()
        {
            if (this.InAbnormal())
                this.LeaveAbnormal();
        }
    }
}
