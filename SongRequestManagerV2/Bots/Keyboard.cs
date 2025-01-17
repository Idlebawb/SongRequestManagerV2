﻿using BeatSaberMarkupLanguage;
using SongRequestManagerV2.Interfaces;
using SongRequestManagerV2.Models;
using SongRequestManagerV2.Statics;
using SongRequestManagerV2.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Image = UnityEngine.UI.Image;

namespace SongRequestManagerV2.Bots
{
    // Experimental chat console
    public class Keyboard
    {
        public List<KEY> keys = new List<KEY>();
        private bool _sabotageState = false;
        private bool _enableInputField = true;
        private bool _shift = false;
        private bool _caps = false;
        private RectTransform _container;
        private Vector2 _currentposition;
        private Vector2 _baseposition;
        private float _scale = 0.5f; // BUG: Effect of changing this has NOT beed tested. assume changing it doesn't work.
        private readonly float _padding = 0.5f;
        private readonly float _buttonwidth = 12f;
        private TextMeshProUGUI _keyboardCursor;
        public TextMeshProUGUI KeyboardText { get; private set; }
        public Button BaseButton { get; private set; }
        [Inject]
        private readonly IRequestBot _bot;
        [Inject]
        private readonly CommandManager _commandManager;
        private readonly KEY _dummy = new KEY(); // This allows for some lazy programming, since unfound key searches will point to this instead of null. It still logs an error though

        // Keyboard spaces and CR/LF are significicant.
        // A slash following a space or CR/LF alters the width of the space
        // CR on an empty line results in a half life advance

        public const string QWERTY =
@"[CLEAR]/20
(`~) (1!) (2@) (3#) (4$) (5%) (6^) (7&) (8*) (9() (0)) (-_) (=+) [<--]/15
[TAB]/15'\t' (qQ) (wW) (eE) (rR) (tT) (yY) (uU) (iI) (oO) (pP) ([{) (]}) (\|)
[CAPS]/20 (aA) (sS) (dD) (fF) (gG) (hH) (jJ) (kK) (lL) (;:) ('"") [ENTER]/20,22#20A0D0
[SHIFT]/25 (zZ) (xX) (cC) (vV) (bB) (nN) (mM) (,<) (.>) (/?)  
/23 (!!) (@@) [SPACE]/40' ' (##) (__)";

        public const string FKEYROW =
@"
[Esc] /2 [F1] [F2] [F3] [F4] /2 [F5] [F6] [F7] [F8] /2 [F9] [F10] [F11] [F12]
";

        public const string NUMPAD =
@"
[NUM] (//) (**) (--)
(77) (88) (99) [+]/10,22
(44) (55) (66)
(11) (22) (33) [ENTER]/10,22
[0]/22 [.]
";

        public const string DVORAK =
@"
(`~) (1!) (2@) (3#) (4$) (5%) (6^) (7&) (8*) (9() (0)) ([{) (]}) [<--]/15
[TAB]/15 ('"") (,<) (.>) (pP) (yY) (fF) (gG) (cC) (rR) (lL) (/?) (=+) (\|)
[CAPS]/20 (aA) (oO) (eE) (uU) (iI) (dD) (hH) (tT) (nN) (sS) (-_) [ENTER]/20
[SHIFT] (;:) (qQ) (jJ) (kK) (xX) (bB) (mM) (wW) (vV) (zZ) [CLEAR]/28
/23 (!!) (@@) [SPACE]/40 (##) (__)";

        public KEY this[string index]
        {
            get
            {
                foreach (var key in this.keys) {
                    if (key.name == index) {
                        return key;
                    }
                }
                Logger.Debug($"Keyboard: Unable to set property of Key  [{index}]");

                return this._dummy;
            }
        }

        public void SetButtonType(string ButtonName = "A")
        {
            this.BaseButton = Resources.FindObjectsOfTypeAll<Button>().FirstOrDefault(x => (x.name == ButtonName));
            if (this.BaseButton == null) {
                this.BaseButton = Resources.FindObjectsOfTypeAll<Button>().FirstOrDefault(x => (x.name == "KeyboardButton"));
            }
        }

        public void SetValue(string keylabel, string value)
        {
            var found = false;
            foreach (var key in this.keys) {
                if (key.name == keylabel) {
                    found = true;
                    key.value = value;
                    //key.shifted = value;
                }
            }

            if (!found) {
                Logger.Debug($"Keyboard: Unable to set property of Key  [{keylabel}]");
            }
        }

        public void SetAction(string keyname, Action<KEY> action)
        {
            var found = false;
            foreach (var key in this.keys) {
                if (key.name == keyname) {
                    found = true;
                    key.keyaction = action;
                }
            }

            // BUG: This message was annoying if the keyboard didn't include those keys.
            if (!found) {
                Logger.Debug($"Keyboard: Unable to set action of Key  [{keyname}]");
            }
        }

        private KEY AddKey(string keylabel, float width = 12, float height = 10, int color = 0xffffff)
        {
            var position = this._currentposition;
            //position.x += width / 4;

            var co = Color.white;

            co.r = (float)(color & 0xff) / 255;
            co.g = (float)((color >> 8) & 0xff) / 255;
            co.b = (float)((color >> 16) & 0xff) / 255;
            var key = new KEY(this, position, keylabel, width, height, co);
            this.keys.Add(key);
            //currentposition.x += width / 2 + padding;
            return key;
        }

        private KEY AddKey(string keylabel, string Shifted, float width = 12, float height = 10)
        {
            var key = this.AddKey(keylabel, width, height);
            key.shifted = Shifted;
            return key;
        }

        // BUG: Refactor this within a keybard parser subclass once everything works.
        private void EmitKey(ref float spacing, ref float Width, ref string Label, ref string Key, ref bool space, ref string newvalue, ref float height, ref int color)
        {
            this._currentposition.x += spacing;

            if (Label != "") {
                this.AddKey(Label, Width, height, color).Set(newvalue);
            }
            else if (Key != "") {
                this.AddKey(Key[0].ToString(), Key[1].ToString()).Set(newvalue);
            }

            spacing = 0;
            Width = this._buttonwidth;
            height = 10f;
            Label = "";
            Key = "";
            newvalue = "";
            color = 0xffffff;
            space = false;
            return;
        }

        private bool ReadFloat(ref string data, ref int Position, ref float result)
        {
            if (Position >= data.Length) {
                return false;
            }

            var start = Position;
            while (Position < data.Length) {
                var c = data[Position];
                if (!((c >= '0' && c <= '9') || c == '+' || c == '-' || c == '.')) {
                    break;
                }

                Position++;
            }

            if (float.TryParse(data.Substring(start, Position - start), out result)) {
                return true;
            }

            Position = start;
            return false;
        }

        // Very basic parser for the keyboard grammar - no doubt can be improved. Tricky to implement because of special characters.
        // It might possible to make grep do this, but it would be even harder to read than this!
        public Keyboard AddKeys(string Keyboard, float scale = 0.5f)
        {
            this._scale = scale;
            var space = true;
            var spacing = this._padding;
            var width = this._buttonwidth;
            var height = 10f;
            var Label = "";
            var Key = "";
            var newvalue = "";
            var color = 0xffffff;
            var p = 0; // P is for parser

            try {

                while (p < Keyboard.Length) {

                    switch (Keyboard[p]) {
                        case '@': // Position key
                            this.EmitKey(ref spacing, ref width, ref Label, ref Key, ref space, ref newvalue, ref height, ref color);
                            p++;
                            if (this.ReadFloat(ref Keyboard, ref p, ref this._currentposition.x)) {
                                this._baseposition.x = this._currentposition.x;
                                if (p < Keyboard.Length && Keyboard[p] == ',') {
                                    p++;
                                    this.ReadFloat(ref Keyboard, ref p, ref this._currentposition.y);
                                    this._baseposition.y = this._currentposition.y;
                                }
                            }
                            continue;

                        case 'S': // Scale
                            {
                                this.EmitKey(ref spacing, ref width, ref Label, ref Key, ref space, ref newvalue, ref height, ref color);
                                p++;
                                this.ReadFloat(ref Keyboard, ref p, ref this._scale);
                                continue;
                            }

                        case '\r':
                            space = true;
                            break;

                        case '\n':
                            this.EmitKey(ref spacing, ref width, ref Label, ref Key, ref space, ref newvalue, ref height, ref color);
                            space = true;
                            this.NextRow();
                            break;

                        case ' ':
                            space = true;
                            //spacing += padding;
                            break;

                        case '[':
                            this.EmitKey(ref spacing, ref width, ref Label, ref Key, ref space, ref newvalue, ref height, ref color);

                            space = false;
                            p++;
                            var label = p;
                            while (p < Keyboard.Length && Keyboard[p] != ']') {
                                p++;
                            }

                            Label = Keyboard.Substring(label, p - label);
                            break;

                        case '(':
                            this.EmitKey(ref spacing, ref width, ref Label, ref Key, ref space, ref newvalue, ref height, ref color);

                            p++;
                            Key = Keyboard.Substring(p, 2);
                            p += 2;
                            space = false;
                            break;

                        case '#':
                            // BUG: Make this support alpha and 6/8 digit forms
                            p++;
                            color = int.Parse(Keyboard.Substring(p, 6), System.Globalization.NumberStyles.HexNumber);
                            p += 6;
                            continue;

                        case '/':

                            p++;
                            float number = 0;
                            if (this.ReadFloat(ref Keyboard, ref p, ref number)) {
                                if (p < Keyboard.Length && Keyboard[p] == ',') {
                                    p++;
                                    this.ReadFloat(ref Keyboard, ref p, ref height);
                                }

                                if (space) {
                                    if (Label != "" || Key != "") {
                                        this.EmitKey(ref spacing, ref width, ref Label, ref Key, ref space, ref newvalue, ref height, ref color);
                                    }

                                    spacing = number;
                                }
                                else {
                                    width = number;
                                }

                                continue;
                            }

                            break;

                        case '\'':
                            p++;
                            var newvaluep = p;
                            while (p < Keyboard.Length && Keyboard[p] != '\'') {
                                p++;
                            }

                            newvalue = Keyboard.Substring(newvaluep, p - newvaluep);
                            break;


                        default:
                            Logger.Debug($"Unable to parse keyboard at position {p} char [{Keyboard[p]}]: [{Keyboard}]");
                            return this;
                    }

                    p++;
                }

                this.EmitKey(ref spacing, ref width, ref Label, ref Key, ref space, ref newvalue, ref height, ref color);

            }
            catch (Exception ex) {
                Logger.Error($"Unable to parse keyboard at position {p} : [{Keyboard}]");
                Logger.Error(ex);
            }

            return this;
        }

        public void AddKeyboard(string keyboardname, float scale = 0.5f)
        {
            try {
                var fileContent = File.ReadAllText(Path.Combine(Plugin.DataPath, keyboardname));
                if (fileContent.Length > 0) {
                    this.AddKeys(fileContent, scale);
                }
            }
            catch {
                // This is a silent fail since custom keyboards are optional
            }
        }

        // Default actions may be called more than once. Make sure to only set any overrides that replace these AFTER all keys have been added
        public Keyboard DefaultActions()
        {
            this.SetAction("SABOTAGE", this.SABOTAGE);
            this.SetAction("CLEAR", this.Clear);
            this.SetAction("ENTER", this.Enter);
            this.SetAction("<--", this.Backspace);
            this.SetAction("SHIFT", this.SHIFT);
            this.SetAction("CAPS", this.CAPS);
            return this;
        }

        public Keyboard()
        {

        }

        public Keyboard Setup(RectTransform container, string DefaultKeyboard = QWERTY, bool EnableInputField = true, float x = 0, float y = 0)
        {
            this._enableInputField = EnableInputField;
            this._container = container;
            this._baseposition = new Vector2(-50 + x, 23 + y);
            this._currentposition = this._baseposition;
            //bool addhint = true;

            this.SetButtonType();

            // BUG: Make this an input field maybe

            this.KeyboardText = BeatSaberUI.CreateText(container, "", new Vector2(0, 30f));
            this.KeyboardText.fontSize = 6f;
            this.KeyboardText.color = Color.white;
            this.KeyboardText.alignment = TextAlignmentOptions.Center;
            this.KeyboardText.enableWordWrapping = false;
            this.KeyboardText.text = "";
            this.KeyboardText.enabled = this._enableInputField;
            //KeyboardText

            this._keyboardCursor = BeatSaberUI.CreateText(container, "|", new Vector2(0, 0));
            this._keyboardCursor.fontSize = 6f;
            this._keyboardCursor.color = Color.cyan;
            this._keyboardCursor.alignment = TextAlignmentOptions.Left;
            this._keyboardCursor.enableWordWrapping = false;
            this._keyboardCursor.enabled = this._enableInputField;

            this.DrawCursor(); // BUG: Doesn't handle trailing spaces.. seriously, wtf.

            // We protect this since setting nonexistent keys will throw.

            // BUG: These are here on a temporary basis, they will be moving out as soon as API is finished

            if (DefaultKeyboard != "") {
                this.AddKeys(DefaultKeyboard);
                this.DefaultActions();
            }

            return this;
        }

        public Keyboard NextRow(float _ = 0)
        {
            this._currentposition.y -= this._currentposition.x == this._baseposition.x ? 3 : 6; // Next row on an empty row only results in a half row advance
            this._currentposition.x = this._baseposition.x;
            return this;
        }

        public Keyboard SetScale(float scale)
        {
            this._scale = scale;
            return this;
        }

        public void Clear(KEY key)
        {
            key.kb.KeyboardText.text = "";
        }

        public void Enter(KEY key)
        {
            var typedtext = key.kb.KeyboardText.text;
            if (typedtext != "") {
                if (this._commandManager.Aliases.ContainsKey(ParseState.GetCommand(ref typedtext))) {
                    this._bot.Parse(this._bot.GetLoginUser(), typedtext, CmdFlags.Local);
                }
                else {
                    //ToDo
                    //TwitchWebSocketClient.SendCommand(typedtext);
                }

                key.kb.KeyboardText.text = "";
            }
        }

        private void Backspace(KEY key)
        {
            // BUG: This is terribly long winded... 
            if (key.kb.KeyboardText.text.Length > 0) {
                key.kb.KeyboardText.text = key.kb.KeyboardText.text.Substring(0, key.kb.KeyboardText.text.Length - 1); // Is there a cleaner way to say this?
            }
        }

        private void SHIFT(KEY key)
        {
            key.kb._shift = !key.kb._shift;

            foreach (var k in key.kb.keys) {
                var x = key.kb._shift ? k.shifted : k.value;
                //if (key.kb.Caps) x = k.value.ToUpper();
                if (k.shifted != "") {
                    k.mybutton.SetButtonText(x);
                }

                if (k.name == "SHIFT") {
                    k.mybutton.GetComponentInChildren<Image>().color = key.kb._shift ? Color.green : Color.white;
                }
            }
        }

        private void CAPS(KEY key)
        {
            key.kb._caps = !key.kb._caps;
            key.mybutton.GetComponentInChildren<Image>().color = key.kb._caps ? Color.green : Color.white;
        }

        private void SABOTAGE(KEY key)
        {
            this._sabotageState = !this._sabotageState;
            key.mybutton.GetComponentInChildren<Image>().color = this._sabotageState ? Color.green : Color.red;
            var text = "!sabotage " + (this._sabotageState ? "on" : "off");
            this._bot.Parse(this._bot.GetLoginUser(), text, CmdFlags.Local);
        }

        private void DrawCursor()
        {
            if (!this._enableInputField) {
                return;
            }

            var v = this.KeyboardText.GetPreferredValues(this.KeyboardText.text + "|");

            v.y = 30f; // BUG: This needs to be derived from the text position
            // BUG: I do not know why that 30f is here, It makes things work, but I can't understand WHY! Me stupid.
            v.x = v.x / 2 + 30f - 0.5f; // BUG: The .5 gets rid of the trailing |, but technically, we need to calculate its width and store it
            (this._keyboardCursor.transform as RectTransform).anchoredPosition = v;
        }
#if false
        private void Newest(KEY _)
        {
            this.ClearSearches();


            this._bot.Parse(this._bot.GetLoginUser(), $"!addnew/top", CmdFlags.Local);
        }

        private void Search(KEY key)
        {
            if (key.kb.KeyboardText.text.StartsWith("!")) {
                this.Enter(key);
            }
        }

        private void ClearSearch(KEY _)
        {
            this.ClearSearches();

            this._bot.UpdateRequestUI();
            this._bot.RefreshSongQuere();
        }

        private void ClearSearches()
        {
            foreach (var item in RequestManager.RequestSongs.GetConsumingEnumerable()) {
                if (item.Status == RequestStatus.SongSearch) {
                    this._bot.DequeueRequest(item, false);
                }
            }
        }
#endif
        public class KEY
        {
            public string name = "";
            public string value = "";
            public string shifted = "";
            public Button mybutton;
            public Keyboard kb;
            public Action<KEY> keyaction = null;

            public KEY Set(string Value)
            {
                if (Value != "") {
                    this.value = Value;
                    //this.shifted = Value;
                }
                return this;
            }

            public KEY()
            {
                // This key is not intialized at all
            }

            public KEY(Keyboard kb, Vector2 position, string text, float width, float height, Color color)
            {
                this.value = text;
                this.kb = kb;

                this.name = text;
                this.mybutton = Button.Instantiate(kb.BaseButton, kb._container, false);

                (this.mybutton.transform as RectTransform).anchorMin = new Vector2(0.5f, 0.5f);
                (this.mybutton.transform as RectTransform).anchorMax = new Vector2(0.5f, 0.5f);

                var txt = this.mybutton.GetComponentInChildren<TMP_Text>();
                this.mybutton.ToggleWordWrapping(false);

                this.mybutton.transform.localScale = new Vector3(kb._scale, kb._scale, 1.0f);
                this.mybutton.SetButtonTextSize(5f);
                this.mybutton.SetButtonText(text);
                this.mybutton.GetComponentInChildren<Image>().color = color;

                if (width == 0) {
                    var v = txt.GetPreferredValues(text);
                    v.x += 10f;
                    v.y += 2f;
                    width = v.x;
                }

                // Adjust starting position so button aligns to upper left of current drawing position

                position.x += kb._scale * width / 2;
                position.y -= kb._scale * height / 2;
                (this.mybutton.transform as RectTransform).anchoredPosition = position;

                (this.mybutton.transform as RectTransform).sizeDelta = new Vector2(width, height);

                kb._currentposition.x += width * kb._scale + kb._padding;

                this.mybutton.onClick.RemoveAllListeners();

                this.mybutton.onClick.AddListener(delegate ()
                {

                    if (this.keyaction != null) {
                        this.keyaction(this);
                        kb.DrawCursor();
                        return;
                    }

                    if (this.value.EndsWith("%CR%")) {
                        kb.KeyboardText.text += this.value.Substring(0, this.value.Length - 4);
                        kb.Enter(this);
                        kb.DrawCursor();

                        return;
                    }

                    {
                        var x = kb._shift ? this.shifted : this.value;
                        if (x == "") {
                            x = this.value;
                        }

                        if (kb._caps) {
                            x = this.value.ToUpper();
                        }

                        kb.KeyboardText.text += x;
                        kb.DrawCursor();

                    }
                });
                var myHintText = UIHelper.AddHintText(this.mybutton.transform as RectTransform, this.value);
            }
        }

        public class KEYBOARDFactiry : PlaceholderFactory<Keyboard>
        {

        }
    }
}
