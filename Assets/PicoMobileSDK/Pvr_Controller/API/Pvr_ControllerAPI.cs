// Copyright  2015-2020 Pico Technology Co., Ltd. All Rights Reserved.


#if !UNITY_EDITOR && UNITY_ANDROID
#define ANDROID_DEVICE
#endif

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Pvr_UnitySDKAPI
{
    #region Properties
    public class PvrControllerKey
    {
        public bool State;
        public bool PressedDown;
        public bool PressedUp;
        public bool LongPressed;
        public bool Click;
        public bool Touch;
        public bool TouchDown;
        public bool TouchUp;
        public PvrControllerKey()
        {
            State = false;
            PressedDown = false;
            PressedUp = false;
            LongPressed = false;
            Click = false;
            Touch = false;
            TouchDown = false;
            TouchUp = false;
        }
    }

    public class ControllerHand
    {
        public PvrControllerKey App;
        public PvrControllerKey Touch;
        public PvrControllerKey Home;
        public PvrControllerKey VolumeDown;
        public PvrControllerKey VolumeUp;
        public PvrControllerKey Trigger;
        public PvrControllerKey X;
        public PvrControllerKey Y;
        public PvrControllerKey A;
        public PvrControllerKey B;
        public PvrControllerKey Left;
        public PvrControllerKey Right;
        public PvrControllerKey Thumbrest;
        public Vector2 TouchPadPosition;
        public int TriggerNum;
        public int GripValue;
        public Quaternion Rotation;
        public Vector3 Position;
        public int Battery;
        public ControllerState ConnectState;
        public SwipeDirection SwipeDirection;
        public TouchPadClick TouchPadClick;
        public bool isShowBoundary;

        public ControllerHand()
        {
            App = new PvrControllerKey();
            Touch = new PvrControllerKey();
            Home = new PvrControllerKey();
            VolumeDown = new PvrControllerKey();
            VolumeUp = new PvrControllerKey();
            Trigger = new PvrControllerKey();
            A = new PvrControllerKey();
            B = new PvrControllerKey();
            X = new PvrControllerKey();
            Y = new PvrControllerKey();
            Left = new PvrControllerKey();
            Right = new PvrControllerKey();
            Thumbrest = new PvrControllerKey();
            TouchPadPosition = new Vector2();
            Rotation = new Quaternion();
            Position = new Vector3();
            Battery = 0;
            TriggerNum = 0;
            GripValue = 0;
            ConnectState = ControllerState.Error;
            SwipeDirection = SwipeDirection.No;
            TouchPadClick = TouchPadClick.No;
            isShowBoundary = false;
        }
    }

    public enum ControllerState
    {
        Error = -1,
        DisConnected = 0,
        Connected = 1
    }

    /// <summary>
    /// controller key value
    /// </summary>
    public enum Pvr_KeyCode
    {
        None = 0,
        APP = 0x00000001,
        TOUCHPAD = 0x00000002,
        HOME = 0x00000004,
        VOLUMEUP = 0x00000008,
        VOLUMEDOWN = 0x00000010,
        TRIGGER = 0x00000020,
        A = 0x00000040,
        B = 0x00000080,
        X = 0x00000100,
        Y = 0x00000200,
        Left = 0x00000400,
        Right = 0x00000800,
        Thumbrest = 0x00001000,
        Any = ~None,
    }

    /// <summary>
    /// The controller Touchpad slides in the direction.
    /// </summary>
    public enum SwipeDirection
    {
        No = 0,
        SwipeUp = 1,
        SwipeDown = 2,
        SwipeLeft = 3,
        SwipeRight = 4
    }

    /// <summary>
    /// The controller Touchpad click the direction.
    /// </summary>
    public enum TouchPadClick
    {
        No = 0,
        ClickUp = 1,
        ClickDown = 2,
        ClickLeft = 3,
        ClickRight = 4
    }

    #endregion
    public struct Controller
    {
        /**************************** Public Static Funcations *******************************************/
        #region Public Static Funcation  
        private const float JOYSTICK_THRESHOLD = 0.4f;

        /// <summary>
        /// Get the touch pad position data.
        /// </summary>
        /// <param name="hand">0,1</param>
        public static Vector2 UPvr_GetTouchPadPosition(int hand)
        {
            switch (hand)
            {
                case 0:
                    {
                        var postion = Pvr_ControllerManager.controllerlink.Controller0.TouchPadPosition;
                        return postion;
                    }
                case 1:
                    {
                        var postion = Pvr_ControllerManager.controllerlink.Controller1.TouchPadPosition;
                        return postion;
                    }
            }
            return new Vector2(0, 0);
        }

        public static float UPvr_GetAxis1D(int hand, Pvr_KeyCode key)
        {
            switch (hand)
            {
                case 0:
                    {
                        switch (key)
                        {
                            case Pvr_KeyCode.TRIGGER:
                                {
                                    return Pvr_ControllerManager.controllerlink.Controller0.TriggerNum / 255.0f;
                                }
                            case Pvr_KeyCode.Left:
                                {
                                    return Pvr_ControllerManager.controllerlink.Controller0.GripValue / 255.0f;
                                }
                        }
                        return 0.0f;
                    }
                case 1:
                    {
                        switch (key)
                        {
                            case Pvr_KeyCode.TRIGGER:
                                {
                                    return Pvr_ControllerManager.controllerlink.Controller1.TriggerNum / 255.0f;
                                }
                            case Pvr_KeyCode.Right:
                                {
                                    return Pvr_ControllerManager.controllerlink.Controller1.GripValue / 255.0f;
                                }
                        }
                        return 0.0f;
                    }
            }
            return 0.0f;
        }

        /// <summary>
        /// convert coordinate system
        /// </summary>
        /// <param name="hand">0,1</param>
        /// <returns>horizontal :X -1~1 vertical: Y -1~1</returns>
        public static Vector2 UPvr_GetAxis2D(int hand)
        {
            switch (hand)
            {
                case 0:
                    {
                        if (Pvr_ControllerManager.controllerlink.Controller0.TouchPadPosition != Vector2.zero)
                        {
                            var postion = new Vector2(Pvr_ControllerManager.controllerlink.Controller0.TouchPadPosition.x / 128.0f - 1,
                                Pvr_ControllerManager.controllerlink.Controller0.TouchPadPosition.y / 128.0f - 1);
                            if (postion.x > 1.0f || postion.x < -1.0f || postion.y > 1.0f || postion.y < -1.0f)
                            {
                                return Vector2.zero;
                            }
                            return postion;
                        }
                        return Vector2.zero;
                    }
                case 1:
                    {
                        if (Pvr_ControllerManager.controllerlink.Controller1.TouchPadPosition != Vector2.zero)
                        {
                            var postion = new Vector2(Pvr_ControllerManager.controllerlink.Controller1.TouchPadPosition.x / 128.0f - 1,
                                Pvr_ControllerManager.controllerlink.Controller1.TouchPadPosition.y / 128.0f - 1);
                            if (postion.x > 1.0f || postion.x < -1.0f || postion.y > 1.0f || postion.y < -1.0f)
                            {
                                return Vector2.zero;
                            }
                            return postion;
                        }
                        return Vector2.zero;
                    }
            }
            return Vector2.zero;
        }

        /// <summary>
        /// Get the up state of the joystick.
        /// </summary>
        /// <param name="hand">0,1</param>
        public static bool UPvr_GetJoystickUp(int hand)
        {
            switch (hand)
            {
                case 0:
                    {
                        if (Pvr_ControllerManager.controllerlink.Controller0.TouchPadPosition != Vector2.zero)
                        {
                            float x = Pvr_ControllerManager.controllerlink.Controller0.TouchPadPosition.x / 128.0f - 1;
                            float y = Pvr_ControllerManager.controllerlink.Controller0.TouchPadPosition.y / 128.0f - 1;

                            if (y > JOYSTICK_THRESHOLD && y > Math.Abs(x))
                            {
                                return true;
                            }
                        }

                        return false;

                    }
                case 1:
                    {
                        if (Pvr_ControllerManager.controllerlink.Controller1.TouchPadPosition != Vector2.zero)
                        {
                            float x = Pvr_ControllerManager.controllerlink.Controller1.TouchPadPosition.x / 128.0f - 1;
                            float y = Pvr_ControllerManager.controllerlink.Controller1.TouchPadPosition.y / 128.0f - 1;

                            if (y > JOYSTICK_THRESHOLD && y > Math.Abs(x))
                            {
                                return true;
                            }
                        }

                        return false;
                    }
            }
            return false;
        }

        /// <summary>
        /// Get the down state of the joystick.
        /// </summary>
        /// <param name="hand">0,1</param>
        public static bool UPvr_GetJoystickDown(int hand)
        {
            switch (hand)
            {
                case 0:
                    {
                        if (Pvr_ControllerManager.controllerlink.Controller0.TouchPadPosition != Vector2.zero)
                        {
                            float x = Pvr_ControllerManager.controllerlink.Controller0.TouchPadPosition.x / 128.0f - 1;
                            float y = Pvr_ControllerManager.controllerlink.Controller0.TouchPadPosition.y / 128.0f - 1;

                            if (-y > JOYSTICK_THRESHOLD && -y > Math.Abs(x))
                            {
                                return true;
                            }
                        }

                        return false;

                    }
                case 1:
                    {
                        if (Pvr_ControllerManager.controllerlink.Controller1.TouchPadPosition != Vector2.zero)
                        {
                            float x = Pvr_ControllerManager.controllerlink.Controller1.TouchPadPosition.x / 128.0f - 1;
                            float y = Pvr_ControllerManager.controllerlink.Controller1.TouchPadPosition.y / 128.0f - 1;

                            if (-y > JOYSTICK_THRESHOLD && -y > Math.Abs(x))
                            {
                                return true;
                            }
                        }

                        return false;
                    }
            }
            return false;
        }

        /// <summary>
        /// Get the left state of the joystick.
        /// </summary>
        /// <param name="hand">0,1</param>
        public static bool UPvr_GetJoystickLeft(int hand)
        {
            switch (hand)
            {
                case 0:
                    {
                        if (Pvr_ControllerManager.controllerlink.Controller0.TouchPadPosition != Vector2.zero)
                        {
                            float x = Pvr_ControllerManager.controllerlink.Controller0.TouchPadPosition.x / 128.0f - 1;
                            float y = Pvr_ControllerManager.controllerlink.Controller0.TouchPadPosition.y / 128.0f - 1;

                            if (-x > JOYSTICK_THRESHOLD && -x > Math.Abs(y))
                            {
                                return true;
                            }
                        }

                        return false;

                    }
                case 1:
                    {
                        if (Pvr_ControllerManager.controllerlink.Controller1.TouchPadPosition != Vector2.zero)
                        {
                            float x = Pvr_ControllerManager.controllerlink.Controller1.TouchPadPosition.x / 128.0f - 1;
                            float y = Pvr_ControllerManager.controllerlink.Controller1.TouchPadPosition.y / 128.0f - 1;

                            if (-x > JOYSTICK_THRESHOLD && -x > Math.Abs(y))
                            {
                                return true;
                            }
                        }

                        return false;
                    }
            }
            return false;
        }

        /// <summary>
        /// Get the right state of the joystick.
        /// </summary>
        /// <param name="hand">0,1</param>
        public static bool UPvr_GetJoystickRight(int hand)
        {
            switch (hand)
            {
                case 0:
                    {
                        if (Pvr_ControllerManager.controllerlink.Controller0.TouchPadPosition != Vector2.zero)
                        {
                            float x = Pvr_ControllerManager.controllerlink.Controller0.TouchPadPosition.x / 128.0f - 1;
                            float y = Pvr_ControllerManager.controllerlink.Controller0.TouchPadPosition.y / 128.0f - 1;

                            if (x > JOYSTICK_THRESHOLD && x > Math.Abs(y))
                            {
                                return true;
                            }
                        }

                        return false;

                    }
                case 1:
                    {
                        if (Pvr_ControllerManager.controllerlink.Controller1.TouchPadPosition != Vector2.zero)
                        {
                            float x = Pvr_ControllerManager.controllerlink.Controller1.TouchPadPosition.x / 128.0f - 1;
                            float y = Pvr_ControllerManager.controllerlink.Controller1.TouchPadPosition.y / 128.0f - 1;

                            if (x > JOYSTICK_THRESHOLD && x > Math.Abs(y))
                            {
                                return true;
                            }
                        }

                        return false;
                    }
            }
            return false;
        }

        public static ControllerState UPvr_GetControllerState(int hand)
        {
            switch (hand)
            {
                case 0:
                    return (ControllerState)Convert.ToInt16(Pvr_ControllerManager.controllerlink.controller0Connected);
                case 1:
                    return (ControllerState)Convert.ToInt16(Pvr_ControllerManager.controllerlink.controller1Connected);

            }
            return ControllerState.Error;
        }

        /// <summary>
        /// Get the controller rotation data.
        /// </summary>
        /// <param name="hand">0,1</param>
        public static Quaternion UPvr_GetControllerQUA(int hand)
        {
            switch (hand)
            {
                case 0:
                    return Pvr_ControllerManager.controllerlink.Controller0.Rotation;
                case 1:
                    return Pvr_ControllerManager.controllerlink.Controller1.Rotation;
            }
            return new Quaternion(0, 0, 0, 1);
        }

        /// <summary>
        /// Get the controller position data.
        /// </summary>
        /// <param name="hand">0,1</param>
        public static Vector3 UPvr_GetControllerPOS(int hand)
        {
            switch (hand)
            {
                case 0:
                    return Pvr_ControllerManager.controllerlink.Controller0.Position;
                case 1:
                    return Pvr_ControllerManager.controllerlink.Controller1.Position;
            }
            return new Vector3(0, 0, 0);
        }

        /// <summary>
        /// Get the controller predict rotation data.
        /// </summary>
        /// <param name="hand">0,1</param>
        /// <param name="predictTime">ms</param>
        public static Quaternion UPvr_GetControllerPredictRotation(int hand, float predictTime)
        {
            var data = Pvr_ControllerManager.controllerlink.GetControllerPredictSensorData(hand, predictTime);
            return new Quaternion(data[0], data[1], data[2], data[3]);
        }

        /// <summary>
        /// Get the controller predict position data.
        /// </summary>
        /// <param name="hand">0,1</param>
        /// <param name="predictTime">ms</param>
        public static Vector3 UPvr_GetControllerPredictPosition(int hand, float predictTime)
        {
            var data = Pvr_ControllerManager.controllerlink.GetControllerPredictSensorData(hand, predictTime);
            return new Vector3(data[4] / 1000.0f, data[5] / 1000.0f, -data[6] / 1000.0f);
        }

        /// <summary>
        /// Get the value of the trigger key 
        /// </summary>
        /// <param name="hand">0,1</param>
        /// <returns>Neo/Neo2:0-255,G2:0/1</returns>
        public static int UPvr_GetControllerTriggerValue(int hand)
        {
            switch (hand)
            {
                case 0:
                    return Pvr_ControllerManager.controllerlink.Controller0.TriggerNum;
                case 1:
                    return Pvr_ControllerManager.controllerlink.Controller1.TriggerNum;
            }
            return 0;
        }

        /// <summary>
        /// Get the power of the controller, 1-5
        /// </summary>
        /// <param name="hand">0,1</param>
        public static int UPvr_GetControllerPower(int hand)
        {
            switch (hand)
            {
                case 0:
                    return Pvr_ControllerManager.controllerlink.Controller0.Battery;
                case 1:
                    return Pvr_ControllerManager.controllerlink.Controller1.Battery;
            }
            return 0;
        }

        /// <summary>
        /// Get the power of the controller, 1-100
        /// </summary>
        /// <param name="hand">0,1</param>
        public static int UPvr_GetControllerPowerByPercent(int hand)
        {
            switch (hand)
            {
                case 0:
                    return Pvr_ControllerManager.controllerlink.Controller0.Battery * 20;
                case 1:
                    return Pvr_ControllerManager.controllerlink.Controller1.Battery * 20;
            }
            return 0;
        }

        /// <summary>
        /// Get the sliding direction of the touchpad.
        /// </summary>
        /// <param name="hand">0,1</param>
        public static SwipeDirection UPvr_GetSwipeDirection(int hand)
        {
            switch (hand)
            {
                case 0:
                    return Pvr_ControllerManager.controllerlink.Controller0.SwipeDirection;
                case 1:
                    return Pvr_ControllerManager.controllerlink.Controller1.SwipeDirection;
            }
            return SwipeDirection.No;
        }


        /// <summary>
        /// Get the click direction of the touchpad.
        /// </summary>
        /// <param name="hand">0,1</param>
        public static TouchPadClick UPvr_GetTouchPadClick(int hand)
        {
            switch (hand)
            {
                case 0:
                    return Pvr_ControllerManager.controllerlink.Controller0.TouchPadClick;
                case 1:
                    return Pvr_ControllerManager.controllerlink.Controller1.TouchPadClick;
            }
            return TouchPadClick.No;
        }

        /// <summary>
        /// Get the key state
        /// </summary>
        /// <param name="hand">0,1</param>
        /// <param name="key">Pvr_KeyCode</param>
        public static bool UPvr_GetKey(int hand, Pvr_KeyCode key)
        {
            bool isPressed = false;

            if (hand == 0)
            {
                if ((Pvr_KeyCode.APP & key) == Pvr_KeyCode.APP)
                {
                    isPressed |= Pvr_ControllerManager.controllerlink.Controller0.App.State;
                }

                if ((Pvr_KeyCode.HOME & key) == Pvr_KeyCode.HOME)
                {
                    isPressed |= Pvr_ControllerManager.controllerlink.Controller0.Home.State;
                }

                if ((Pvr_KeyCode.TOUCHPAD & key) == Pvr_KeyCode.TOUCHPAD)
                {
                    isPressed |= Pvr_ControllerManager.controllerlink.Controller0.Touch.State;
                }

                if ((Pvr_KeyCode.VOLUMEUP & key) == Pvr_KeyCode.VOLUMEUP)
                {
                    isPressed |= Pvr_ControllerManager.controllerlink.Controller0.VolumeUp.State;
                }

                if ((Pvr_KeyCode.VOLUMEDOWN & key) == Pvr_KeyCode.VOLUMEDOWN)
                {
                    isPressed |= Pvr_ControllerManager.controllerlink.Controller0.VolumeDown.State;
                }

                if ((Pvr_KeyCode.TRIGGER & key) == Pvr_KeyCode.TRIGGER)
                {
                    isPressed |= Pvr_ControllerManager.controllerlink.Controller0.Trigger.State;
                }

                if ((Pvr_KeyCode.X & key) == Pvr_KeyCode.X)
                {
                    isPressed |= Pvr_ControllerManager.controllerlink.Controller0.X.State;
                }

                if ((Pvr_KeyCode.Y & key) == Pvr_KeyCode.Y)
                {
                    isPressed |= Pvr_ControllerManager.controllerlink.Controller0.Y.State;
                }

                if ((Pvr_KeyCode.Left & key) == Pvr_KeyCode.Left)
                {
                    isPressed |= Pvr_ControllerManager.controllerlink.Controller0.Left.State;
                }
            }
            if (hand == 1)
            {
                if ((Pvr_KeyCode.APP & key) == Pvr_KeyCode.APP)
                {
                    isPressed |= Pvr_ControllerManager.controllerlink.Controller1.App.State;
                }

                if ((Pvr_KeyCode.HOME & key) == Pvr_KeyCode.HOME)
                {
                    isPressed |= Pvr_ControllerManager.controllerlink.Controller1.Home.State;
                }

                if ((Pvr_KeyCode.TOUCHPAD & key) == Pvr_KeyCode.TOUCHPAD)
                {
                    isPressed |= Pvr_ControllerManager.controllerlink.Controller1.Touch.State;
                }

                if ((Pvr_KeyCode.VOLUMEUP & key) == Pvr_KeyCode.VOLUMEUP)
                {
                    isPressed |= Pvr_ControllerManager.controllerlink.Controller1.VolumeUp.State;
                }

                if ((Pvr_KeyCode.VOLUMEDOWN & key) == Pvr_KeyCode.VOLUMEDOWN)
                {
                    isPressed |= Pvr_ControllerManager.controllerlink.Controller1.VolumeDown.State;
                }

                if ((Pvr_KeyCode.TRIGGER & key) == Pvr_KeyCode.TRIGGER)
                {
                    isPressed |= Pvr_ControllerManager.controllerlink.Controller1.Trigger.State;
                }

                if ((Pvr_KeyCode.A & key) == Pvr_KeyCode.A)
                {
                    isPressed |= Pvr_ControllerManager.controllerlink.Controller1.A.State;
                }

                if ((Pvr_KeyCode.B & key) == Pvr_KeyCode.B)
                {
                    isPressed |= Pvr_ControllerManager.controllerlink.Controller1.B.State;
                }

                if ((Pvr_KeyCode.Right & key) == Pvr_KeyCode.Right)
                {
                    isPressed |= Pvr_ControllerManager.controllerlink.Controller1.Right.State;
                }
            }

            return isPressed;
        }

        /// <summary>
        /// Get the pressed state of the key
        /// </summary>
        /// <param name="hand">0,1</param>
        /// <param name="key">Pvr_KeyCode</param>
        public static bool UPvr_GetKeyDown(int hand, Pvr_KeyCode key)
        {
            bool isPressedDown = false;

            if (hand == 0)
            {
                if ((Pvr_KeyCode.APP & key) == Pvr_KeyCode.APP)
                {
                    isPressedDown |= Pvr_ControllerManager.controllerlink.Controller0.App.PressedDown;
                }

                if ((Pvr_KeyCode.HOME & key) == Pvr_KeyCode.HOME)
                {
                    isPressedDown |= Pvr_ControllerManager.controllerlink.Controller0.Home.PressedDown;
                }

                if ((Pvr_KeyCode.TOUCHPAD & key) == Pvr_KeyCode.TOUCHPAD)
                {
                    isPressedDown |= Pvr_ControllerManager.controllerlink.Controller0.Touch.PressedDown;
                }

                if ((Pvr_KeyCode.VOLUMEUP & key) == Pvr_KeyCode.VOLUMEUP)
                {
                    isPressedDown |= Pvr_ControllerManager.controllerlink.Controller0.VolumeUp.PressedDown;
                }

                if ((Pvr_KeyCode.VOLUMEDOWN & key) == Pvr_KeyCode.VOLUMEDOWN)
                {
                    isPressedDown |= Pvr_ControllerManager.controllerlink.Controller0.VolumeDown.PressedDown;
                }

                if ((Pvr_KeyCode.TRIGGER & key) == Pvr_KeyCode.TRIGGER)
                {
                    isPressedDown |= Pvr_ControllerManager.controllerlink.Controller0.Trigger.PressedDown;
                }

                if ((Pvr_KeyCode.X & key) == Pvr_KeyCode.X)
                {
                    isPressedDown |= Pvr_ControllerManager.controllerlink.Controller0.X.PressedDown;
                }

                if ((Pvr_KeyCode.Y & key) == Pvr_KeyCode.Y)
                {
                    isPressedDown |= Pvr_ControllerManager.controllerlink.Controller0.Y.PressedDown;
                }

                if ((Pvr_KeyCode.Left & key) == Pvr_KeyCode.Left)
                {
                    isPressedDown |= Pvr_ControllerManager.controllerlink.Controller0.Left.PressedDown;
                }
            }
            if (hand == 1)
            {
                if ((Pvr_KeyCode.APP & key) == Pvr_KeyCode.APP)
                {
                    isPressedDown |= Pvr_ControllerManager.controllerlink.Controller1.App.PressedDown;
                }

                if ((Pvr_KeyCode.HOME & key) == Pvr_KeyCode.HOME)
                {
                    isPressedDown |= Pvr_ControllerManager.controllerlink.Controller1.Home.PressedDown;
                }

                if ((Pvr_KeyCode.TOUCHPAD & key) == Pvr_KeyCode.TOUCHPAD)
                {
                    isPressedDown |= Pvr_ControllerManager.controllerlink.Controller1.Touch.PressedDown;
                }

                if ((Pvr_KeyCode.VOLUMEUP & key) == Pvr_KeyCode.VOLUMEUP)
                {
                    isPressedDown |= Pvr_ControllerManager.controllerlink.Controller1.VolumeUp.PressedDown;
                }

                if ((Pvr_KeyCode.VOLUMEDOWN & key) == Pvr_KeyCode.VOLUMEDOWN)
                {
                    isPressedDown |= Pvr_ControllerManager.controllerlink.Controller1.VolumeDown.PressedDown;
                }

                if ((Pvr_KeyCode.TRIGGER & key) == Pvr_KeyCode.TRIGGER)
                {
                    isPressedDown |= Pvr_ControllerManager.controllerlink.Controller1.Trigger.PressedDown;
                }

                if ((Pvr_KeyCode.A & key) == Pvr_KeyCode.A)
                {
                    isPressedDown |= Pvr_ControllerManager.controllerlink.Controller1.A.PressedDown;
                }

                if ((Pvr_KeyCode.B & key) == Pvr_KeyCode.B)
                {
                    isPressedDown |= Pvr_ControllerManager.controllerlink.Controller1.B.PressedDown;
                }

                if ((Pvr_KeyCode.Right & key) == Pvr_KeyCode.Right)
                {
                    isPressedDown |= Pvr_ControllerManager.controllerlink.Controller1.Right.PressedDown;
                }
            }

            return isPressedDown;
        }

        /// <summary>
        /// Gets the lift state of the key.
        /// </summary>
        /// <param name="hand">0,1</param>
        /// <param name="key">Pvr_KeyCode</param>
        public static bool UPvr_GetKeyUp(int hand, Pvr_KeyCode key)
        {
            bool isPressedUp = false;

            if (hand == 0)
            {
                if ((Pvr_KeyCode.APP & key) == Pvr_KeyCode.APP)
                {
                    isPressedUp |= Pvr_ControllerManager.controllerlink.Controller0.App.PressedUp;
                }

                if ((Pvr_KeyCode.HOME & key) == Pvr_KeyCode.HOME)
                {
                    isPressedUp |= Pvr_ControllerManager.controllerlink.Controller0.Home.PressedUp;
                }

                if ((Pvr_KeyCode.TOUCHPAD & key) == Pvr_KeyCode.TOUCHPAD)
                {
                    isPressedUp |= Pvr_ControllerManager.controllerlink.Controller0.Touch.PressedUp;
                }

                if ((Pvr_KeyCode.VOLUMEUP & key) == Pvr_KeyCode.VOLUMEUP)
                {
                    isPressedUp |= Pvr_ControllerManager.controllerlink.Controller0.VolumeUp.PressedUp;
                }

                if ((Pvr_KeyCode.VOLUMEDOWN & key) == Pvr_KeyCode.VOLUMEDOWN)
                {
                    isPressedUp |= Pvr_ControllerManager.controllerlink.Controller0.VolumeDown.PressedUp;
                }

                if ((Pvr_KeyCode.TRIGGER & key) == Pvr_KeyCode.TRIGGER)
                {
                    isPressedUp |= Pvr_ControllerManager.controllerlink.Controller0.Trigger.PressedUp;
                }

                if ((Pvr_KeyCode.X & key) == Pvr_KeyCode.X)
                {
                    isPressedUp |= Pvr_ControllerManager.controllerlink.Controller0.X.PressedUp;
                }

                if ((Pvr_KeyCode.Y & key) == Pvr_KeyCode.Y)
                {
                    isPressedUp |= Pvr_ControllerManager.controllerlink.Controller0.Y.PressedUp;
                }

                if ((Pvr_KeyCode.Left & key) == Pvr_KeyCode.Left)
                {
                    isPressedUp |= Pvr_ControllerManager.controllerlink.Controller0.Left.PressedUp;
                }
            }
            if (hand == 1)
            {
                if ((Pvr_KeyCode.APP & key) == Pvr_KeyCode.APP)
                {
                    isPressedUp |= Pvr_ControllerManager.controllerlink.Controller1.App.PressedUp;
                }

                if ((Pvr_KeyCode.HOME & key) == Pvr_KeyCode.HOME)
                {
                    isPressedUp |= Pvr_ControllerManager.controllerlink.Controller1.Home.PressedUp;
                }

                if ((Pvr_KeyCode.TOUCHPAD & key) == Pvr_KeyCode.TOUCHPAD)
                {
                    isPressedUp |= Pvr_ControllerManager.controllerlink.Controller1.Touch.PressedUp;
                }

                if ((Pvr_KeyCode.VOLUMEUP & key) == Pvr_KeyCode.VOLUMEUP)
                {
                    isPressedUp |= Pvr_ControllerManager.controllerlink.Controller1.VolumeUp.PressedUp;
                }

                if ((Pvr_KeyCode.VOLUMEDOWN & key) == Pvr_KeyCode.VOLUMEDOWN)
                {
                    isPressedUp |= Pvr_ControllerManager.controllerlink.Controller1.VolumeDown.PressedUp;
                }

                if ((Pvr_KeyCode.TRIGGER & key) == Pvr_KeyCode.TRIGGER)
                {
                    isPressedUp |= Pvr_ControllerManager.controllerlink.Controller1.Trigger.PressedUp;
                }

                if ((Pvr_KeyCode.A & key) == Pvr_KeyCode.A)
                {
                    isPressedUp |= Pvr_ControllerManager.controllerlink.Controller1.A.PressedUp;
                }

                if ((Pvr_KeyCode.B & key) == Pvr_KeyCode.B)
                {
                    isPressedUp |= Pvr_ControllerManager.controllerlink.Controller1.B.PressedUp;
                }

                if ((Pvr_KeyCode.Right & key) == Pvr_KeyCode.Right)
                {
                    isPressedUp |= Pvr_ControllerManager.controllerlink.Controller1.Right.PressedUp;
                }
            }

            return isPressedUp;
        }

        public static bool UPvr_GetTouch(int hand, Pvr_KeyCode key)
        {
            bool isTouch = false;

            if (hand == 0)
            {
                if ((Pvr_KeyCode.TOUCHPAD & key) == Pvr_KeyCode.TOUCHPAD)
                {
                    isTouch |= Pvr_ControllerManager.controllerlink.Controller0.Touch.Touch;
                }

                if ((Pvr_KeyCode.TRIGGER & key) == Pvr_KeyCode.TRIGGER)
                {
                    isTouch |= Pvr_ControllerManager.controllerlink.Controller0.Trigger.Touch;
                }

                if ((Pvr_KeyCode.X & key) == Pvr_KeyCode.X)
                {
                    isTouch |= Pvr_ControllerManager.controllerlink.Controller0.X.Touch;
                }

                if ((Pvr_KeyCode.Y & key) == Pvr_KeyCode.Y)
                {
                    isTouch |= Pvr_ControllerManager.controllerlink.Controller0.Y.Touch;
                }

                if ((Pvr_KeyCode.Thumbrest & key) == Pvr_KeyCode.Thumbrest)
                {
                    isTouch |= Pvr_ControllerManager.controllerlink.Controller0.Thumbrest.Touch;
                }
            }
            if (hand == 1)
            {
                if ((Pvr_KeyCode.TOUCHPAD & key) == Pvr_KeyCode.TOUCHPAD)
                {
                    isTouch |= Pvr_ControllerManager.controllerlink.Controller1.Touch.Touch;
                }

                if ((Pvr_KeyCode.TRIGGER & key) == Pvr_KeyCode.TRIGGER)
                {
                    isTouch |= Pvr_ControllerManager.controllerlink.Controller1.Trigger.Touch;
                }

                if ((Pvr_KeyCode.A & key) == Pvr_KeyCode.A)
                {
                    isTouch |= Pvr_ControllerManager.controllerlink.Controller1.A.Touch;
                }

                if ((Pvr_KeyCode.B & key) == Pvr_KeyCode.B)
                {
                    isTouch |= Pvr_ControllerManager.controllerlink.Controller1.B.Touch;
                }

                if ((Pvr_KeyCode.Thumbrest & key) == Pvr_KeyCode.Thumbrest)
                {
                    isTouch |= Pvr_ControllerManager.controllerlink.Controller1.Thumbrest.Touch;
                }
            }
            return isTouch;
        }

        /// <summary>
        /// Get the pressed state of the touch
        /// </summary>
        /// <param name="hand">0,1</param>
        /// <param name="key">Pvr_KeyCode</param>
        public static bool UPvr_GetTouchDown(int hand, Pvr_KeyCode key)
        {
            bool isTouchDown = false;

            if (hand == 0)
            {
                if ((Pvr_KeyCode.TOUCHPAD & key) == Pvr_KeyCode.TOUCHPAD)
                {
                    isTouchDown |= Pvr_ControllerManager.controllerlink.Controller0.Touch.TouchDown;
                }

                if ((Pvr_KeyCode.TRIGGER & key) == Pvr_KeyCode.TRIGGER)
                {
                    isTouchDown |= Pvr_ControllerManager.controllerlink.Controller0.Trigger.TouchDown;
                }

                if ((Pvr_KeyCode.X & key) == Pvr_KeyCode.X)
                {
                    isTouchDown |= Pvr_ControllerManager.controllerlink.Controller0.X.TouchDown;
                }

                if ((Pvr_KeyCode.Y & key) == Pvr_KeyCode.Y)
                {
                    isTouchDown |= Pvr_ControllerManager.controllerlink.Controller0.Y.TouchDown;
                }

                if ((Pvr_KeyCode.Thumbrest & key) == Pvr_KeyCode.Thumbrest)
                {
                    isTouchDown |= Pvr_ControllerManager.controllerlink.Controller0.Thumbrest.TouchDown;
                }
            }
            if (hand == 1)
            {
                if ((Pvr_KeyCode.TOUCHPAD & key) == Pvr_KeyCode.TOUCHPAD)
                {
                    isTouchDown |= Pvr_ControllerManager.controllerlink.Controller1.Touch.TouchDown;
                }

                if ((Pvr_KeyCode.TRIGGER & key) == Pvr_KeyCode.TRIGGER)
                {
                    isTouchDown |= Pvr_ControllerManager.controllerlink.Controller1.Trigger.TouchDown;
                }

                if ((Pvr_KeyCode.A & key) == Pvr_KeyCode.A)
                {
                    isTouchDown |= Pvr_ControllerManager.controllerlink.Controller1.A.TouchDown;
                }

                if ((Pvr_KeyCode.B & key) == Pvr_KeyCode.B)
                {
                    isTouchDown |= Pvr_ControllerManager.controllerlink.Controller1.B.TouchDown;
                }

                if ((Pvr_KeyCode.Thumbrest & key) == Pvr_KeyCode.Thumbrest)
                {
                    isTouchDown |= Pvr_ControllerManager.controllerlink.Controller1.Thumbrest.TouchDown;
                }
            }
            return isTouchDown;
        }

        /// <summary>
        /// Gets the lift state of the touch.
        /// </summary>
        /// <param name="hand">0,1</param>
        /// <param name="key">Pvr_KeyCode</param>
        public static bool UPvr_GetTouchUp(int hand, Pvr_KeyCode key)
        {
            bool isTouchUp = false;

            if (hand == 0)
            {
                if ((Pvr_KeyCode.TOUCHPAD & key) == Pvr_KeyCode.TOUCHPAD)
                {
                    isTouchUp |= Pvr_ControllerManager.controllerlink.Controller0.Touch.TouchUp;
                }

                if ((Pvr_KeyCode.TRIGGER & key) == Pvr_KeyCode.TRIGGER)
                {
                    isTouchUp |= Pvr_ControllerManager.controllerlink.Controller0.Trigger.TouchUp;
                }

                if ((Pvr_KeyCode.X & key) == Pvr_KeyCode.X)
                {
                    isTouchUp |= Pvr_ControllerManager.controllerlink.Controller0.X.TouchUp;
                }

                if ((Pvr_KeyCode.Y & key) == Pvr_KeyCode.Y)
                {
                    isTouchUp |= Pvr_ControllerManager.controllerlink.Controller0.Y.TouchUp;
                }

                if ((Pvr_KeyCode.Thumbrest & key) == Pvr_KeyCode.Thumbrest)
                {
                    isTouchUp |= Pvr_ControllerManager.controllerlink.Controller0.Thumbrest.TouchUp;
                }
            }
            if (hand == 1)
            {
                if ((Pvr_KeyCode.TOUCHPAD & key) == Pvr_KeyCode.TOUCHPAD)
                {
                    isTouchUp |= Pvr_ControllerManager.controllerlink.Controller1.Touch.TouchUp;
                }

                if ((Pvr_KeyCode.TRIGGER & key) == Pvr_KeyCode.TRIGGER)
                {
                    isTouchUp |= Pvr_ControllerManager.controllerlink.Controller1.Trigger.TouchUp;
                }

                if ((Pvr_KeyCode.A & key) == Pvr_KeyCode.A)
                {
                    isTouchUp |= Pvr_ControllerManager.controllerlink.Controller1.A.TouchUp;
                }

                if ((Pvr_KeyCode.B & key) == Pvr_KeyCode.B)
                {
                    isTouchUp |= Pvr_ControllerManager.controllerlink.Controller1.B.TouchUp;
                }

                if ((Pvr_KeyCode.Thumbrest & key) == Pvr_KeyCode.Thumbrest)
                {
                    isTouchUp |= Pvr_ControllerManager.controllerlink.Controller1.Thumbrest.TouchUp;
                }
            }
            return isTouchUp;
        }

        /// <summary>
        /// Gets the click state of the Key.
        /// </summary>
        /// <param name="hand">0,1</param>
        /// <param name="key">Pvr_KeyCode</param>
        public static bool UPvr_GetKeyClick(int hand, Pvr_KeyCode key)
        {
            bool isClick = false;

            if (hand == 0)
            {
                if ((Pvr_KeyCode.APP & key) == Pvr_KeyCode.APP)
                {
                    isClick |= Pvr_ControllerManager.controllerlink.Controller0.App.Click;
                }

                if ((Pvr_KeyCode.HOME & key) == Pvr_KeyCode.HOME)
                {
                    isClick |= Pvr_ControllerManager.controllerlink.Controller0.Home.Click;
                }

                if ((Pvr_KeyCode.TOUCHPAD & key) == Pvr_KeyCode.TOUCHPAD)
                {
                    isClick |= Pvr_ControllerManager.controllerlink.Controller0.Touch.Click;
                }

                if ((Pvr_KeyCode.VOLUMEUP & key) == Pvr_KeyCode.VOLUMEUP)
                {
                    isClick |= Pvr_ControllerManager.controllerlink.Controller0.VolumeUp.Click;
                }

                if ((Pvr_KeyCode.VOLUMEDOWN & key) == Pvr_KeyCode.VOLUMEDOWN)
                {
                    isClick |= Pvr_ControllerManager.controllerlink.Controller0.VolumeDown.Click;
                }

                if ((Pvr_KeyCode.TRIGGER & key) == Pvr_KeyCode.TRIGGER)
                {
                    isClick |= Pvr_ControllerManager.controllerlink.Controller0.Trigger.Click;
                }

                if ((Pvr_KeyCode.X & key) == Pvr_KeyCode.X)
                {
                    isClick |= Pvr_ControllerManager.controllerlink.Controller0.X.Click;
                }

                if ((Pvr_KeyCode.Y & key) == Pvr_KeyCode.Y)
                {
                    isClick |= Pvr_ControllerManager.controllerlink.Controller0.Y.Click;
                }

                if ((Pvr_KeyCode.Left & key) == Pvr_KeyCode.Left)
                {
                    isClick |= Pvr_ControllerManager.controllerlink.Controller0.Left.Click;
                }
            }
            if (hand == 1)
            {
                if ((Pvr_KeyCode.APP & key) == Pvr_KeyCode.APP)
                {
                    isClick |= Pvr_ControllerManager.controllerlink.Controller1.App.Click;
                }

                if ((Pvr_KeyCode.HOME & key) == Pvr_KeyCode.HOME)
                {
                    isClick |= Pvr_ControllerManager.controllerlink.Controller1.Home.Click;
                }

                if ((Pvr_KeyCode.TOUCHPAD & key) == Pvr_KeyCode.TOUCHPAD)
                {
                    isClick |= Pvr_ControllerManager.controllerlink.Controller1.Touch.Click;
                }

                if ((Pvr_KeyCode.VOLUMEUP & key) == Pvr_KeyCode.VOLUMEUP)
                {
                    isClick |= Pvr_ControllerManager.controllerlink.Controller1.VolumeUp.Click;
                }

                if ((Pvr_KeyCode.VOLUMEDOWN & key) == Pvr_KeyCode.VOLUMEDOWN)
                {
                    isClick |= Pvr_ControllerManager.controllerlink.Controller1.VolumeDown.Click;
                }

                if ((Pvr_KeyCode.TRIGGER & key) == Pvr_KeyCode.TRIGGER)
                {
                    isClick |= Pvr_ControllerManager.controllerlink.Controller1.Trigger.Click;
                }

                if ((Pvr_KeyCode.A & key) == Pvr_KeyCode.A)
                {
                    isClick |= Pvr_ControllerManager.controllerlink.Controller1.A.Click;
                }

                if ((Pvr_KeyCode.B & key) == Pvr_KeyCode.B)
                {
                    isClick |= Pvr_ControllerManager.controllerlink.Controller1.B.Click;
                }

                if ((Pvr_KeyCode.Right & key) == Pvr_KeyCode.Right)
                {
                    isClick |= Pvr_ControllerManager.controllerlink.Controller1.Right.Click;
                }
            }

            return isClick;
        }

        /// <summary>
        /// Gets the long press state of the Key.
        /// </summary>
        /// <param name="hand">0,1</param>
        /// <param name="key">Pvr_KeyCode</param>
        public static bool UPvr_GetKeyLongPressed(int hand, Pvr_KeyCode key)
        {
            bool isLongPressed = false;

            if (hand == 0)
            {
                if ((Pvr_KeyCode.APP & key) == Pvr_KeyCode.APP)
                {
                    isLongPressed |= Pvr_ControllerManager.controllerlink.Controller0.App.LongPressed;
                }

                if ((Pvr_KeyCode.HOME & key) == Pvr_KeyCode.HOME)
                {
                    isLongPressed |= Pvr_ControllerManager.controllerlink.Controller0.Home.LongPressed;
                }

                if ((Pvr_KeyCode.TOUCHPAD & key) == Pvr_KeyCode.TOUCHPAD)
                {
                    isLongPressed |= Pvr_ControllerManager.controllerlink.Controller0.Touch.LongPressed;
                }

                if ((Pvr_KeyCode.VOLUMEUP & key) == Pvr_KeyCode.VOLUMEUP)
                {
                    isLongPressed |= Pvr_ControllerManager.controllerlink.Controller0.VolumeUp.LongPressed;
                }

                if ((Pvr_KeyCode.VOLUMEDOWN & key) == Pvr_KeyCode.VOLUMEDOWN)
                {
                    isLongPressed |= Pvr_ControllerManager.controllerlink.Controller0.VolumeDown.LongPressed;
                }

                if ((Pvr_KeyCode.TRIGGER & key) == Pvr_KeyCode.TRIGGER)
                {
                    isLongPressed |= Pvr_ControllerManager.controllerlink.Controller0.Trigger.LongPressed;
                }

                if ((Pvr_KeyCode.X & key) == Pvr_KeyCode.X)
                {
                    isLongPressed |= Pvr_ControllerManager.controllerlink.Controller0.X.LongPressed;
                }

                if ((Pvr_KeyCode.Y & key) == Pvr_KeyCode.Y)
                {
                    isLongPressed |= Pvr_ControllerManager.controllerlink.Controller0.Y.LongPressed;
                }

                if ((Pvr_KeyCode.Left & key) == Pvr_KeyCode.Left)
                {
                    isLongPressed |= Pvr_ControllerManager.controllerlink.Controller0.Left.LongPressed;
                }
            }
            if (hand == 1)
            {
                if ((Pvr_KeyCode.APP & key) == Pvr_KeyCode.APP)
                {
                    isLongPressed |= Pvr_ControllerManager.controllerlink.Controller1.App.LongPressed;
                }

                if ((Pvr_KeyCode.HOME & key) == Pvr_KeyCode.HOME)
                {
                    isLongPressed |= Pvr_ControllerManager.controllerlink.Controller1.Home.LongPressed;
                }

                if ((Pvr_KeyCode.TOUCHPAD & key) == Pvr_KeyCode.TOUCHPAD)
                {
                    isLongPressed |= Pvr_ControllerManager.controllerlink.Controller1.Touch.LongPressed;
                }

                if ((Pvr_KeyCode.VOLUMEUP & key) == Pvr_KeyCode.VOLUMEUP)
                {
                    isLongPressed |= Pvr_ControllerManager.controllerlink.Controller1.VolumeUp.LongPressed;
                }

                if ((Pvr_KeyCode.VOLUMEDOWN & key) == Pvr_KeyCode.VOLUMEDOWN)
                {
                    isLongPressed |= Pvr_ControllerManager.controllerlink.Controller1.VolumeDown.LongPressed;
                }

                if ((Pvr_KeyCode.TRIGGER & key) == Pvr_KeyCode.TRIGGER)
                {
                    isLongPressed |= Pvr_ControllerManager.controllerlink.Controller1.Trigger.LongPressed;
                }

                if ((Pvr_KeyCode.A & key) == Pvr_KeyCode.A)
                {
                    isLongPressed |= Pvr_ControllerManager.controllerlink.Controller1.A.LongPressed;
                }

                if ((Pvr_KeyCode.B & key) == Pvr_KeyCode.B)
                {
                    isLongPressed |= Pvr_ControllerManager.controllerlink.Controller1.B.LongPressed;
                }

                if ((Pvr_KeyCode.Right & key) == Pvr_KeyCode.Right)
                {
                    isLongPressed |= Pvr_ControllerManager.controllerlink.Controller1.Right.LongPressed;
                }
            }
            return isLongPressed;
        }

        /// <summary>
        /// Determine if you touched the touchpad.
        /// </summary>
        /// <param name="key">Pvr_KeyCode</param>
        public static bool UPvr_IsTouching(int hand)
        {
            const float tolerance = 0;
            switch (hand)
            {
                case 0:
                    {
                        return Math.Abs(Pvr_ControllerManager.controllerlink.Controller0.TouchPadPosition.x) > tolerance ||
                               Math.Abs(Pvr_ControllerManager.controllerlink.Controller0.TouchPadPosition.y) > tolerance;
                    }
                case 1:
                    {
                        return Math.Abs(Pvr_ControllerManager.controllerlink.Controller1.TouchPadPosition.x) > tolerance ||
                               Math.Abs(Pvr_ControllerManager.controllerlink.Controller1.TouchPadPosition.y) > tolerance;
                    }
            }
            return false;
        }

        /// <summary>
        /// Set the handness.
        /// </summary>
        /// <param name="hand">UserHandNess</param>
        public static void UPvr_SetHandNess(Pvr_Controller.UserHandNess hand)
        {
            if (Pvr_ControllerManager.controllerlink.getHandness() != (int)hand)
            {
                Pvr_ControllerManager.controllerlink.setHandness((int)hand);
            }
        }

        /// <summary>
        /// Get the handness.
        /// </summary>
        public static Pvr_Controller.UserHandNess UPvr_GetHandNess()
        {
            return Pvr_ControllerManager.controllerlink.handness;
        }

        /// <summary>
        /// The service type that currently needs bind.
        /// </summary>
        /// <returns>1：Goblin service 2:Neo service </returns>
        public static int UPvr_GetPreferenceDevice()
        {
            var trackingmode = Pvr_ControllerManager.controllerlink.trackingmode;
            var systemproc = Pvr_ControllerManager.controllerlink.systemProp;
            if (trackingmode == 0 || trackingmode == 1 || (trackingmode == 3 || trackingmode == 5 || trackingmode == 6) && (systemproc == 1 || systemproc == 3))
            {
                return 1;
            }
            return 2;
        }

        /// <summary>
        /// Whether the current controller has trigger key
        /// </summary>
        public static bool UPvr_IsEnbleTrigger()
        {
            return Pvr_ControllerManager.controllerlink.IsEnbleTrigger();
        }

        /// <summary>
        ///Gets the controller type of the current connection.
        /// </summary>
        /// <returns>0: no connection 1：goblin1 2:Neo 3:goblin2 4:Neo2 5:Neo3</returns>
        public static int UPvr_GetDeviceType()
        {
            return Pvr_ControllerManager.controllerlink.controllerType;
        }

        /// <summary>
        /// Gets the current master hand for which 0/1.
        /// </summary>
        /// <returns></returns>
        public static int UPvr_GetMainHandNess()
        {
            return Pvr_ControllerManager.controllerlink.mainHandID;
        }

        /// <summary>
        /// Set the current controller as the master controller.
        /// </summary>
        public static void UPvr_SetMainHandNess(int hand)
        {
            Pvr_ControllerManager.controllerlink.SetMainController(hand);
        }

        /// <summary>
        /// Ability to obtain the current controller (3dof/6dof)
        /// </summary>
        /// <param name="hand">0/1</param>
        /// <returns>-1:error 0：6dof  1：3dof 2:6dof </returns>
        public static int UPvr_GetControllerAbility(int hand)
        {
            return Pvr_ControllerManager.controllerlink.GetControllerAbility(hand);
        }

        /// <summary>
        /// Vibrate Neo2 controller 
        /// </summary>
        /// <param name="strength">0-1</param>
        /// <param name="time">ms,0-65535</param>
        /// <param name="hand">0,1</param>
        public static void UPvr_VibrateNeo2Controller(float strength, int time, int hand)
        {
            Pvr_ControllerManager.controllerlink.VibrateNeo2Controller(strength, time, hand);
        }

        /// <summary>
        /// Vibrate controller 
        /// </summary>
        /// <param name="strength">0-1</param>
        /// <param name="time">ms,0-65535</param>
        /// <param name="hand">0,1</param>
        public static void UPvr_VibrateController(float strength, int time, int hand)
        {
            Pvr_ControllerManager.controllerlink.VibrateController(strength, time, hand);
        }

        /// <summary>
        /// get controller binding state
        /// </summary>
        /// <param name="hand">0,1</param>
        /// <returns>-1:error 0:Unbound 1:bind</returns>
        public static int UPvr_GetControllerBindingState(int hand)
        {
            return Pvr_ControllerManager.controllerlink.GetControllerBindingState(hand);
        }

        /// <summary>
        /// Get the controller Velocity, Obtain the controller's pose data.
        /// unit:m/s
        /// </summary>
        public static Vector3 UPvr_GetVelocity(int hand)
        {
            return Pvr_ControllerManager.controllerlink.GetVelocity(hand);
        }

        /// <summary>
        /// Get the controller AngularVelocity, Obtain the controller's gyroscope data.
        /// unit:rad/s
        /// </summary>
        public static Vector3 UPvr_GetAngularVelocity(int hand)
        {
            return Pvr_ControllerManager.controllerlink.GetAngularVelocity(hand);
        }

        /// <summary>
        /// Get the controller Acceleration.
        /// m/s^2
        /// </summary>
        public static Vector3 UPvr_GetAcceleration(int hand)
        {
            return Pvr_ControllerManager.controllerlink.GetAcceleration(hand);
        }
        
        /// <summary>
        /// Get the controller AngularAcceleration.
        /// unit:rad/s^2
        /// </summary>
        public static Vector3 UPvr_GetAngularAcceleration(int hand)
        {
            return Pvr_ControllerManager.controllerlink.GetAngularAcceleration(hand);
        }

        /// <summary>
        /// scan Goblin,G2 controller
        /// </summary>
        public static void UPvr_ScanController()
        {
            Pvr_ControllerManager.controllerlink.StartScan();
        }

        /// <summary>
        /// Stop scan Goblin,G2 controller
        /// </summary>
        public static void UPvr_StopScanController()
        {
            Pvr_ControllerManager.controllerlink.StopScan();
        }

        /// <summary>
        /// Connect controller by mac address.
        /// only fit goblin,g2
        /// </summary>
        /// <param name="mac">mac address of controller</param>
        public static void UPvr_ConnectController(string mac)
        {
            if (mac != "")
            {
                Pvr_ControllerManager.controllerlink.hummingBirdMac = mac;
            }
            Pvr_ControllerManager.controllerlink.ConnectBLE();
        }

        /// <summary>
        /// Disonnect controller.
        /// only fit goblin,g2
        /// </summary>
        public static void UPvr_DisConnectController()
        {
            Pvr_ControllerManager.controllerlink.DisConnectBLE();
        }

        /// <summary>
        /// Reset Controller`s rotation
        /// </summary>
        public static void UPvr_ResetController(int hand)
        {
            Pvr_ControllerManager.controllerlink.ResetController(hand);
        }

        /// <summary>
        /// get controller version
        /// </summary>
        public static string UPvr_GetControllerVersion()
        {
            return Pvr_ControllerManager.controllerlink.GetControllerVersion();
        }

        /// <summary>
        /// Get version number deviceType
        /// </summary>
        /// <param name="deviceType">0-station 1-controller0  2-controller1</param>
        public static void UPvr_GetDeviceVersion(int deviceType)
        {
            Pvr_ControllerManager.controllerlink.GetDeviceVersion(deviceType);
        }

        /// <summary>
        /// Get the controller Sn number controllerSerialNum
        /// </summary>
        /// <param name="controllerSerialNum">0-controller0  1-controller1</param>
        public static void UPvr_GetControllerSnCode(int controllerSerialNum)
        {
            Pvr_ControllerManager.controllerlink.GetControllerSnCode(controllerSerialNum);
        }

        /// <summary>
        /// neo:Unbind the controller: 0- controller 0 1- controller 1
        /// neo2:Unbind the controller: 0- all controller 1- left controller 2- right controller
        /// </summary>
        public static void UPvr_SetControllerUnbind(int controllerSerialNum)
        {
            Pvr_ControllerManager.controllerlink.SetControllerUnbind(controllerSerialNum);
        }

        /// <summary>
        /// Restart the station
        /// </summary>
        public static void UPvr_SetStationRestart()
        {
            Pvr_ControllerManager.controllerlink.SetStationRestart();
        }

        /// <summary>
        /// Launch station OTA upgrade.
        /// </summary>
        public static void UPvr_StartStationOtaUpdate()
        {
            Pvr_ControllerManager.controllerlink.StartStationOtaUpdate();
        }

        /// <summary>
        /// Launch controller ota upgrade mode: 1-rf upgrade communication module 2- upgrade STM32 module;ControllerSerialNum: 0- controller 0 1- controller 1.
        /// </summary>
        public static void UPvr_StartControllerOtaUpdate(int mode, int controllerSerialNum)
        {
            Pvr_ControllerManager.controllerlink.StartControllerOtaUpdate(mode, controllerSerialNum);
        }

        /// <summary>
        /// Enter the pairing mode controllerSerialNum: 0- controller 0 1- controller 1.
        /// </summary>
        public static void UPvr_EnterPairMode(int controllerSerialNum)
        {
            Pvr_ControllerManager.controllerlink.EnterPairMode(controllerSerialNum);
        }

        /// <summary>
        /// controller shutdown controllerSerialNum: 0- controller 0 1- controller 1.
        /// </summary>
        public static void UPvr_SetControllerShutdown(int controllerSerialNum)
        {
            Pvr_ControllerManager.controllerlink.SetControllerShutdown(controllerSerialNum);
        }

        /// <summary>
        /// Retrieves the pairing status of the current station with 0- unpaired state 1- pairing.
        /// </summary>
        public static int UPvr_GetStationPairState()
        {
            return Pvr_ControllerManager.controllerlink.GetStationPairState();
        }

        /// <summary>
        /// Get the upgrade of station ota
        /// </summary>
        public static int UPvr_GetStationOtaUpdateProgress()
        {
            return Pvr_ControllerManager.controllerlink.GetStationOtaUpdateProgress();
        }

        /// <summary>
        /// Get the Controller ota upgrade progress
        /// Normal 0-100
        /// Exception 101: failed to receive a successful upgrade of id 102: the controller did not enter the upgrade status 103: upgrade interrupt exception
        /// </summary>
        public static int UPvr_GetControllerOtaUpdateProgress()
        {
            return Pvr_ControllerManager.controllerlink.GetControllerOtaUpdateProgress();
        }

        /// <summary>
        /// Also get the controller version number and SN number controllerSerialNum: 0- controller 0 1- controller 1
        /// </summary>
        public static void UPvr_GetControllerVersionAndSN(int controllerSerialNum)
        {
            Pvr_ControllerManager.controllerlink.GetControllerVersionAndSN(controllerSerialNum);
        }

        /// <summary>
        /// Gets the unique identifier of the controller
        /// </summary>
        public static void UPvr_GetControllerUniqueID()
        {
            Pvr_ControllerManager.controllerlink.GetControllerUniqueID();
        }

        /// <summary>
        /// Disconnect the station from the current pairing mode
        /// </summary>
        public void UPvr_InterruptStationPairMode()
        {
            Pvr_ControllerManager.controllerlink.InterruptStationPairMode();
        }

        /// <summary>
        /// deviceType: 0：scan both controller；1：scan left controller；2：scan right controller
        /// </summary>
        public void UPvr_StartCV2PairingMode(int deviceType)
        {
            Pvr_ControllerManager.controllerlink.StartCV2PairingMode(deviceType);
        }

        /// <summary>
        /// deviceType: 0：stop scan both controller；1：stop scan left controller；2：stop scan right controller
        /// </summary>
        public void UPvr_StopCV2PairingMode(int deviceType)
        {
            Pvr_ControllerManager.controllerlink.StopCV2PairingMode(deviceType);
        }

        public static void UPvr_SetArmModelParameters(int hand, int gazeType, float elbowHeight, float elbowDepth, float pointerTiltAngle)
        {
#if ANDROID_DEVICE
            Pvr_SetArmModelParameters( hand,  gazeType,  elbowHeight,  elbowDepth,  pointerTiltAngle);
#endif
        }

        public static void UPvr_CalcArmModelParameters(float[] headOrientation, float[] controllerOrientation, float[] controllerPrimary)
        {
#if ANDROID_DEVICE
            Pvr_CalcArmModelParameters( headOrientation,  controllerOrientation, controllerPrimary);
#endif
        }

        public static void UPvr_GetPointerPose(float[] rotation, float[] position)
        {
#if ANDROID_DEVICE
            Pvr_GetPointerPose(  rotation,  position);
#endif
        }

        public static void UPvr_GetElbowPose(float[] rotation, float[] position)
        {
#if ANDROID_DEVICE
            Pvr_GetElbowPose(  rotation,   position);
#endif
        }

        public static void UPvr_GetWristPose(float[] rotation, float[] position)
        {
#if ANDROID_DEVICE
            Pvr_GetWristPose(  rotation,  position);
#endif
        }

        public static void UPvr_GetShoulderPose(float[] rotation, float[] position)
        {
#if ANDROID_DEVICE
            Pvr_GetShoulderPose(  rotation,   position);
#endif
        }
        //Whether key injection
        //true:open injection
        //false:close injection,Unity can get the key value
        public static void UPvr_IsEnbleHomeKey(bool state)
        {
            Pvr_ControllerManager.controllerlink.setIsEnbleHomeKey(state);
        }
        //whether use default home key
        //true:Use the default home key function,Developers cannot operate on the home key
        //false:Developers can operate on the home key
        public static void UPvr_SwitchHomeKey(bool state)
        {
            Pvr_ControllerManager.controllerlink.SwitchHomeKey(state);
        }

        /// <summary>
        /// Determine whether the current controller data is valid
        /// </summary>
        /// <returns>1:valid 0: unvalid -1:fail</returns>
        public static int UPvr_GetControllerSensorStatus(int id)
        {
            return Pvr_ControllerManager.controllerlink.getControllerSensorStatus(id);
        }

        /// <summary>
        /// Set the controller origin offset data.
        /// </summary>
        /// <param name="hand">0,1</param>
        /// <param name="offset">m</param>
        public static void UPvr_SetControllerOriginOffset(int hand, Vector3 offset)
        {
            if (hand == 0)
            {
                Pvr_Controller.originOffsetL = offset;
            }
            else if (hand == 1)
            {
                Pvr_Controller.originOffsetR = offset;
            }
        }

        /**************************** Private Static Funcations *******************************************/
        #region Private Static Funcation
#if ANDROID_DEVICE
        public const string LibFileName = "Pvr_UnitySDK";
        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Pvr_CalcArmModelParameters(float[] headOrientation,float[] controllerOrientation,float[] gyro);
        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Pvr_GetPointerPose( float[] rotation,  float[] position);
        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Pvr_GetElbowPose( float[] rotation,  float[] position);
        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Pvr_GetWristPose( float[] rotation,  float[] position);
        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Pvr_GetShoulderPose( float[] rotation,  float[] position);
        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Pvr_SetArmModelParameters(int hand, int gazeType, float elbowHeight, float elbowDepth, float pointerTiltAngle);
#endif
        #endregion
        #endregion
    }

}
