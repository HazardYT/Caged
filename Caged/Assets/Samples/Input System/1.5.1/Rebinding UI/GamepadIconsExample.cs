using System;
using UnityEngine.UI;

////TODO: have updateBindingUIEvent receive a control path string, too (in addition to the device layout name)

namespace UnityEngine.InputSystem.Samples.RebindUI
{
    /// <summary>
    /// This is an example for how to override the default display behavior of bindings. The component
    /// hooks into <see cref="RebindActionUI.updateBindingUIEvent"/> which is triggered when UI display
    /// of a binding should be refreshed. It then checks whether we have an icon for the current binding
    /// and if so, replaces the default text display with an icon.
    /// </summary>
    public class GamepadIconsExample : MonoBehaviour
    {
        public GamepadIcons xbox;
        public GamepadIcons ps4;
        public KeyboardIcons Keyboard;
        public MouseIcons Mouse;

        protected void OnEnable()
        {
            // Hook into all updateBindingUIEvents on all RebindActionUI components in our hierarchy.
            var rebindUIComponents = transform.GetComponentsInChildren<RebindActionUI>();
            foreach (var component in rebindUIComponents)
            {
                component.updateBindingUIEvent.AddListener(OnUpdateBindingDisplay);
                component.UpdateBindingDisplay();
            }
        }

        protected void OnUpdateBindingDisplay(RebindActionUI component, string bindingDisplayString, string deviceLayoutName, string controlPath)
        {
            if (string.IsNullOrEmpty(deviceLayoutName) || string.IsNullOrEmpty(controlPath))
                return;

            var icon = default(Sprite);
            if (InputSystem.IsFirstLayoutBasedOnSecond(deviceLayoutName, "DualShockGamepad"))
                icon = ps4.GetSprite(controlPath);
            else if (InputSystem.IsFirstLayoutBasedOnSecond(deviceLayoutName, "Gamepad"))
                icon = xbox.GetSprite(controlPath);
            else if (InputSystem.IsFirstLayoutBasedOnSecond(deviceLayoutName, "Keyboard"))
                icon = Keyboard.GetSprite(controlPath);
            else if (InputSystem.IsFirstLayoutBasedOnSecond(deviceLayoutName, "Mouse"))
                icon = Mouse.GetSprite(controlPath);

            var textComponent = component.bindingText;

            // Grab Image component.
            var imageGO = textComponent.transform.parent.Find("ActionBindingIcon");
            var imageComponent = imageGO.GetComponent<Image>();

            if (icon != null)
            {
                textComponent.gameObject.SetActive(false);
                imageComponent.sprite = icon;
                imageComponent.gameObject.SetActive(true);
            }
            else
            {
                textComponent.gameObject.SetActive(true);
                imageComponent.gameObject.SetActive(false);
            }
        }

        [Serializable]
        public struct KeyboardIcons
        {
            public Sprite a;
            public Sprite b;
            public Sprite c;
            public Sprite d;
            public Sprite e;
            public Sprite f;
            public Sprite g;
            public Sprite h;
            public Sprite i;
            public Sprite j;
            public Sprite k;
            public Sprite l;
            public Sprite m;
            public Sprite n;
            public Sprite o;
            public Sprite p;
            public Sprite q;
            public Sprite r;
            public Sprite s;
            public Sprite t;
            public Sprite u;
            public Sprite v;
            public Sprite w;
            public Sprite x;
            public Sprite y;
            public Sprite z;
            public Sprite _0;
            public Sprite _1;
            public Sprite _2;
            public Sprite _3;
            public Sprite _4;
            public Sprite _5;
            public Sprite _6;
            public Sprite _7;
            public Sprite _8;
            public Sprite _9;
            public Sprite escape;
            public Sprite f1;
            public Sprite f2;
            public Sprite f3;
            public Sprite f4;
            public Sprite f5;
            public Sprite f6;
            public Sprite f7;
            public Sprite f8;
            public Sprite f9;
            public Sprite f10;
            public Sprite f11;
            public Sprite f12;
            public Sprite printScreen;
            public Sprite scrollLock;
            public Sprite pause;
            public Sprite insert;
            public Sprite home;
            public Sprite pageUp;
            public Sprite delete;
            public Sprite end;
            public Sprite pageDown;
            public Sprite tab;
            public Sprite capsLock;
            public Sprite leftShift;
            public Sprite rightShift;
            public Sprite leftCtrl;
            public Sprite rightCtrl;
            public Sprite leftAlt;
            public Sprite rightAlt;
            public Sprite leftCommand;
            public Sprite rightCommand;
            public Sprite space;
            public Sprite backspace;
            public Sprite enter;
            public Sprite leftArrow;
            public Sprite rightArrow;
            public Sprite upArrow;
            public Sprite downArrow;
            public Sprite GetSprite(string controlPath)
            {
                switch (controlPath)
                {
                    case "a": return a;
                    case "b": return b;
                    case "c": return c;
                    case "d": return d;
                    case "e": return e;
                    case "f": return f;
                    case "g": return g;
                    case "h": return h;
                    case "i": return i;
                    case "j": return j;
                    case "k": return k;
                    case "l": return l;
                    case "m": return m;
                    case "n": return n;
                    case "o": return o;
                    case "p": return p;
                    case "q": return q;
                    case "r": return r;
                    case "s": return s;
                    case "t": return t;
                    case "u": return u;
                    case "v": return v;
                    case "w": return w;
                    case "x": return x;
                    case "y": return y;
                    case "z": return z;
                    case "0": return _0;
                    case "1": return _1;
                    case "2": return _2;
                    case "3": return _3;
                    case "4": return _4;
                    case "5": return _5;
                    case "6": return _6;
                    case "7": return _7;
                    case "8": return _8;
                    case "9": return _9;
                    case "escape": return escape;
                    case "f1": return f1;
                    case "f2": return f2;
                    case "f3": return f3;
                    case "f4": return f4;
                    case "f5": return f5;
                    case "f6": return f6;
                    case "f7": return f7;
                    case "f8": return f8;
                    case "f9": return f9;
                    case "f10": return f10;
                    case "f11": return f11;
                    case "f12": return f12;
                    case "printScreen": return printScreen;
                    case "scrollLock": return scrollLock;
                    case "pause": return pause;
                    case "insert": return insert;
                    case "home": return home;
                    case "pageUp": return pageUp;
                    case "delete": return delete;
                    case "end": return end;
                    case "pageDown": return pageDown;
                    case "tab": return tab;
                    case "capsLock": return capsLock;
                    case "leftShift": return leftShift;
                    case "rightShift": return rightShift;
                    case "leftCtrl": return leftCtrl;
                    case "rightCtrl": return rightCtrl;
                    case "leftAlt": return leftAlt;
                    case "rightAlt": return rightAlt;
                    case "leftCommand": return leftCommand;
                    case "rightCommand": return rightCommand;
                    case "space": return space;
                    case "backspace": return backspace;
                    case "enter": return enter;
                    case "leftArrow": return leftArrow;
                    case "rightArrow": return rightArrow;
                    case "upArrow": return upArrow;
                    case "downArrow": return downArrow;
                }
                return null;
            }

        }
        [Serializable]
        public struct MouseIcons
        {
            public Sprite leftButton;
            public Sprite rightButton;
            public Sprite middleButton;
            public Sprite forwardButton;
            public Sprite backButton;

            public Sprite GetSprite(string controlPath)
            {
                switch (controlPath)
                {
                    case "leftButton": return leftButton;
                    case "rightButton": return rightButton;
                    case "middleButton": return middleButton;
                    case "forwardButton": return forwardButton;
                    case "backButton": return backButton;
                }
                return null;
            }
        }



        [Serializable]
        public struct GamepadIcons
        {
            public Sprite buttonSouth;
            public Sprite buttonNorth;
            public Sprite buttonEast;
            public Sprite buttonWest;
            public Sprite startButton;
            public Sprite selectButton;
            public Sprite leftTrigger;
            public Sprite rightTrigger;
            public Sprite leftShoulder;
            public Sprite rightShoulder;
            public Sprite dpad;
            public Sprite dpadUp;
            public Sprite dpadDown;
            public Sprite dpadLeft;
            public Sprite dpadRight;
            public Sprite leftStick;
            public Sprite rightStick;
            public Sprite leftStickPress;
            public Sprite rightStickPress;

            public Sprite GetSprite(string controlPath)
            {
                // From the input system, we get the path of the control on device. So we can just
                // map from that to the sprites we have for gamepads.
                switch (controlPath)
                {
                    case "buttonSouth": return buttonSouth;
                    case "buttonNorth": return buttonNorth;
                    case "buttonEast": return buttonEast;
                    case "buttonWest": return buttonWest;
                    case "start": return startButton;
                    case "select": return selectButton;
                    case "leftTrigger": return leftTrigger;
                    case "rightTrigger": return rightTrigger;
                    case "leftShoulder": return leftShoulder;
                    case "rightShoulder": return rightShoulder;
                    case "dpad": return dpad;
                    case "dpad/up": return dpadUp;
                    case "dpad/down": return dpadDown;
                    case "dpad/left": return dpadLeft;
                    case "dpad/right": return dpadRight;
                    case "leftStick": return leftStick;
                    case "rightStick": return rightStick;
                    case "leftStickPress": return leftStickPress;
                    case "rightStickPress": return rightStickPress;
                }
                return null;
            }
        }
    }
}
