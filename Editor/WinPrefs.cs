//---------------------------------------------------------------------//
//                    GNU GENERAL PUBLIC LICENSE                       //
//                       Version 2, June 1991                          //
//                                                                     //
// Copyright (C) Wells Hsu, wellshsu@outlook.com, All rights reserved. //
// Everyone is permitted to copy and distribute verbatim copies        //
// of this license document, but changing it is not allowed.           //
//                  SEE LICENSE.md FOR MORE DETAILS.                   //
//---------------------------------------------------------------------//
using EP.U3D.EDITOR.BASE;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EP.U3D.EDITOR.PREF
{
    public class WinPrefs : Window
    {
        [MenuItem(Constants.MENU_WIN_PREF, false, 3)]
        public static void Invoke()
        {
            GetWindowWithRect(WindowType, WindowRect, WindowUtility, WindowTitle);
        }

        public static Type WindowType = typeof(WinPrefs);
        public static Rect WindowRect = new Rect(30, 30, 255, 455);
        public static bool WindowUtility = false;
        public static string WindowTitle = "Preferences";
        public static Type TargetType = typeof(Preferences);
        public new Preferences Target;

        public class Preferences : LIBRARY.BASE.Preferences
        {

            [NonSerialized] public string Path = string.Empty;

            public Preferences(string path = null, Action callback = null) : base(path, callback) { Path = path; }

            [Header("Target")]
            [GUI] [NonSerialized] public Action<WinPrefs> OnTargetGUI = (window) => window.OnTargetGUI();

            [Header("Editor")]
            [Field("Developer", 95)] [SerializeField] public new string Developer = string.Empty;
            [Field("ListenLog", 95)] [SerializeField] public new string ListenLog = string.Empty;
            [Field("PushPatch", 95)] [SerializeField] public new string PushPatch = string.Empty;
            [Horizontal]
            [Field("Simulator Width", 95, 50)] [SerializeField] public new int SWidth = 960;
            [Field("Height", 45, 47)] [SerializeField] public new int SHeight = 540;
            [Horizontal]
            [Field("Pauseable", 100)] [SerializeField] public new bool Pauseable;
            [Field("CheckMode", 100)] [SerializeField] public new bool CheckMode;
            [Horizontal]
            [Field("ScriptBundle", 100)] [SerializeField] public new bool ScriptBundle;
            [Field("AssetBundle", 100)] [SerializeField] public new bool AssetBundle;
            [Horizontal]
            [Field("CatVerbose", 100)] [SerializeField] public new bool CatVerbose;
            [Field("CatException", 102)] [SerializeField] public new bool CatException;

            [Header("IDE")]
            [GUI] [NonSerialized] public Action<WinPrefs> OnIDEGUI = (window) => window.OnIDEGUI();

            [Header("Runtime")]
            [Horizontal]
            [Field("ReleaseMode", 100)] [SerializeField] public new bool ReleaseMode;
            [Field("LiveMode", 100)] [SerializeField] public new bool LiveMode;
            [Horizontal]
            [Field("CheckUpdate", 100)] [SerializeField] public new bool CheckUpdate;
            [Field("ForceUpdate", 100)] [SerializeField] public new bool ForceUpdate;
            [Vertical]
            [Field] [SerializeField] public new string LogServer = string.Empty;
            [Field] [SerializeField] public new string PatchServer = string.Empty;
            [GUI] [NonSerialized] public Action<WinPrefs> OnCgiServerGUI = (window) => window.OnCgiServerGUI();
            [GUI] [NonSerialized] public Action<WinPrefs> OnConnServerGUI = (window) => window.OnConnServerGUI();

            [Space(5)]
            [Horizontal]
            [Button("Save")] [NonSerialized] public Action<WinPrefs> OnSave = (window) => window.Save();
            [Button("Apply")] [NonSerialized] public Action<WinPrefs> OnApply = (window) => window.Apply();
        }

        private int currentIndex = -1;
        private bool cgiServerEdit;
        private string cgiServerTemp = "";
        private bool connServerEdit;
        private string connServerTemp = "";

        public override void OnEnable()
        {
            base.Target = new Preferences();
        }

        public virtual void OnTargetGUI()
        {
            List<string> names = new List<string>();
            List<string> files = new List<string>();
            Helper.CollectFiles(Constants.PREF_FILE_PATH, files);
            for (int i = 0; i < files.Count; i++)
            {
                var f = files[i];
                if (f.Contains(Constants.PREF_DEFALUT_FILE))
                {
                    files.RemoveAt(i);
                    files.Insert(0, f);
                    break;
                }
            }
            for (int i = 0; i < files.Count; i++)
            {
                var f = files[i];
                var p = new Preferences(f);
                if (p.Error == null && string.IsNullOrEmpty(p.Name) == false)
                {
                    names.Add(string.Format("{0} ({1})", p.Name, Path.GetFileName(f)));
                }
            }
            if (currentIndex == -1)
            {
                var cpref = Target != null ? Target : new Preferences(Constants.PREF_STEAMING_FILE);
                if (cpref.Error == null && string.IsNullOrEmpty(cpref.Name) == false)
                {
                    for (int i = 0; i < files.Count; i++)
                    {
                        var f = files[i];
                        var p = new Preferences(f);
                        if (p.Error == null && string.IsNullOrEmpty(p.Name) == false && cpref.Name == p.Name)
                        {
                            currentIndex = i;
                            break;
                        }
                    }
                }
            }
            if (Target == null)
            {
                if (currentIndex == -1) currentIndex = 0;
                Target = new Preferences(files[currentIndex]);
            }
            GUILayout.BeginHorizontal();
            var lastIndex = currentIndex;
            currentIndex = EditorGUILayout.Popup(currentIndex, names.ToArray());
            if (lastIndex != currentIndex)
            {
                Target = new Preferences(files[currentIndex]);
            }
            if (GUILayout.Button("Path"))
            {
                Helper.ShowInExplorer(files[currentIndex]);
            }
            if (GUILayout.Button("Delete"))
            {
                if (!files[currentIndex].Contains(Constants.PREF_DEFALUT_FILE))
                {
                    Helper.DeleteFile(files[currentIndex]);
                }
            }
            if (GUILayout.Button("Clone"))
            {
                string path = EditorUtility.SaveFilePanel("Clone Prefs", Constants.PREF_FILE_PATH, "NAME", Constants.PREF_FILE_EXT);
                if (!string.IsNullOrEmpty(path))
                {
                    Helper.CopyFile(files[currentIndex], path);
                    Target = new Preferences(path);
                    Target.Name = Path.GetFileNameWithoutExtension(path);
                    Helper.SaveText(Target.Path, Helper.ObjectToJson(Helper.ObjectToDict(Target)));
                    currentIndex = -1;
                }
            }
            base.Target = Target;
            GUILayout.EndHorizontal();
        }

        public virtual void OnIDEGUI()
        {
            Helper.BeginContents();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Lua", GUILayout.Width(25));
            var luaIDE = EditorPrefs.GetString(Constants.LUA_IDE_KEY);
            var luaIDECurrent = EditorIDE.IDEType.None;
            if (string.IsNullOrEmpty(luaIDE))
            {
                luaIDECurrent = EditorIDE.GetIDEType(luaIDE);
                if (luaIDECurrent == EditorIDE.IDEType.VSCode || luaIDECurrent == EditorIDE.IDEType.IDEA)
                {
                    EditorPrefs.SetString(Constants.LUA_IDE_KEY, luaIDE);
                    EditorIDE.SetIDEPath(luaIDE, luaIDECurrent);
                }
                else
                {
                    luaIDE = string.Empty;
                }
            }

            var tempEnum2 = EditorPrefs.GetInt(Constants.LUA_IDE_CURRENT_KEY);
            if (tempEnum2 <= 0) luaIDECurrent = EditorIDE.GetIDEType(luaIDE);
            var luaIDELast = luaIDECurrent;
            luaIDECurrent = (EditorIDE.IDEType)EditorGUILayout.EnumPopup(luaIDECurrent);
            if (luaIDECurrent != luaIDELast)
            {
                var newIde = EditorIDE.GetIDEPath(luaIDECurrent);
                if (File.Exists(newIde))
                {
                    var newIdeType = EditorIDE.GetIDEType(newIde);
                    if (newIdeType == EditorIDE.IDEType.VSCode || newIdeType == EditorIDE.IDEType.IDEA) EditorPrefs.SetString(Constants.LUA_IDE_KEY, newIde);
                }
            }

            EditorGUILayout.TextField(Helper.SimplifyLabel(luaIDE, 135), GUILayout.Width(145));
            if (GUILayout.Button(".."))
            {
                var temp = string.Empty;
                if (Application.platform == RuntimePlatform.WindowsEditor)
                    temp = EditorUtility.OpenFilePanel("Select Lua IDE", luaIDE, "exe");
                else if (Application.platform == RuntimePlatform.OSXEditor)
                    temp = EditorUtility.OpenFilePanel("Select Lua IDE", luaIDE, "app");
                if (string.IsNullOrEmpty(temp) == false)
                {
                    EditorPrefs.SetString(Constants.LUA_IDE_KEY, temp);
                    luaIDECurrent = EditorIDE.GetIDEType(temp);
                    EditorIDE.SetIDEPath(temp, luaIDECurrent);
                }
            }

            GUILayout.EndHorizontal();

            Helper.EndContents();
        }

        public virtual void OnCgiServerGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("CgiServer", GUILayout.Width(75));
            if (cgiServerEdit)
            {
                cgiServerTemp = EditorGUILayout.TextField(cgiServerTemp, GUILayout.Width(125));
            }
            else
            {
                string[] strs = Target.CgiServer.ToArray();
                for (int i = 0; i < strs.Length; i++) strs[i] = strs[i].Replace('/', '\u2215');
                Target.CgiIndex = EditorGUILayout.Popup(Target.CgiIndex, strs, GUILayout.Width(125));
            }
            if (!cgiServerEdit)
            {
                if (GUILayout.Button("+"))
                {
                    cgiServerEdit = true;
                }
                if (GUILayout.Button("-"))
                {
                    if (Target.CgiServer[Target.CgiIndex] != "NONE")
                    {
                        Target.CgiServer.RemoveAt(Target.CgiIndex);
                        Target.CgiIndex--;
                    }
                }
            }
            else
            {
                if (GUILayout.Button("o"))
                {
                    if (string.IsNullOrEmpty(cgiServerTemp) == false)
                    {
                        var finish = true;
                        if (!string.IsNullOrEmpty(cgiServerTemp) && !(cgiServerTemp.StartsWith("http://") || cgiServerTemp.StartsWith("https://")))
                        {
                            Helper.ShowToast("CgiServer is invalid, name it's prefix with 'http://' or 'https://'");
                            finish = false;
                        }
                        if (finish)
                        {
                            Target.CgiServer.Add(cgiServerTemp);
                            cgiServerTemp = "";
                            cgiServerEdit = false;
                            Target.CgiIndex = Target.CgiServer.Count - 1;
                        }
                    }
                }
                if (GUILayout.Button("x"))
                {
                    cgiServerTemp = "";
                    cgiServerEdit = false;
                }
            }
            GUILayout.EndHorizontal();
        }

        public virtual void OnConnServerGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("ConnServer", GUILayout.Width(75));
            if (connServerEdit)
            {
                connServerTemp = EditorGUILayout.TextField(connServerTemp, GUILayout.Width(125));
            }
            else
            {
                Target.ConnIndex = EditorGUILayout.Popup(Target.ConnIndex, Target.ConnServer.ToArray(), GUILayout.Width(125));
            }
            if (!connServerEdit)
            {
                if (GUILayout.Button("+"))
                {
                    connServerEdit = true;
                }
                if (GUILayout.Button("-"))
                {
                    if (Target.ConnServer[Target.ConnIndex] != "NONE")
                    {
                        Target.ConnServer.RemoveAt(Target.ConnIndex);
                        Target.ConnIndex--;
                    }
                }
            }
            else
            {
                if (GUILayout.Button("o"))
                {
                    if (string.IsNullOrEmpty(connServerTemp) == false)
                    {
                        var strs = connServerTemp.Split(':');
                        var finish = true;
                        if (strs.Length != 2)
                        {
                            Helper.ShowToast("ConnServer is invalid, name it like '127.0.0.1:50000'");
                            finish = false;
                        }
                        if (finish)
                        {
                            Target.ConnServer.Add(connServerTemp);
                            connServerTemp = "";
                            connServerEdit = false;
                            Target.ConnIndex = Target.ConnServer.Count - 1;
                        }
                    }
                }
                if (GUILayout.Button("x"))
                {
                    connServerTemp = "";
                    connServerEdit = false;
                }
            }
            GUILayout.EndHorizontal();
        }

        public virtual void Save()
        {
            if (Validate())
            {
                Action func = new Action(() =>
                {
                    Helper.SaveText(Target.Path, Helper.ObjectToJson(Helper.ObjectToDict(Target)));
                    Helper.Log("[FILE@{0}] Save preferences success.", Target.Path);
                    Helper.ShowToast("Save preferences success.");
                    EditorLogcat.Reset();
                });
                if (Target.Path.EndsWith(Constants.PREF_DEFALUT_FILE))
                {
                    if (EditorUtility.DisplayDialogComplex("Warning", "You are saving the default prefs, please clone it and edit the copy.",
                        "Don't Save", "Dismiss", "Save") == 2)
                    {
                        func.Invoke();
                    }
                }
                else
                {
                    func.Invoke();
                }
            }
        }

        public virtual void Apply()
        {
            if (Validate())
            {
                Action func = new Action(() =>
                {
                    Helper.SaveText(Target.Path, Helper.ObjectToJson(Helper.ObjectToDict(Target)));
                    Helper.Log("[FILE@{0}] Apply preferences success.", Target.Path);
                    Helper.CopyFile(Target.Path, Constants.PREF_STEAMING_FILE);
                    AssetDatabase.Refresh();
                    Preferences.Instance = new Preferences(Constants.PREF_STEAMING_FILE);
                    if (File.Exists(Constants.SIMULATOR_EXE))
                    {
                        var dest = Path.GetDirectoryName(Constants.SIMULATOR_EXE) + "/" +
                        Path.GetFileNameWithoutExtension(Constants.SIMULATOR_EXE) + "_Data/StreamingAssets/" +
                        Path.GetFileName(Constants.PREF_STEAMING_FILE);
                        File.Copy(Target.Path, dest, true);
                        Helper.ShowToast("Apply preferences success.");
                    }
                    EditorLogcat.Reset();
                });
                if (Target.Path.EndsWith(Constants.PREF_DEFALUT_FILE))
                {
                    if (EditorUtility.DisplayDialogComplex("Warning", "You are saving the default prefs, please clone it and edit the copy.",
                    "Don't Save", "Dismiss", "Save") == 2)
                    {
                        func.Invoke();
                    }
                }
                else
                {
                    func.Invoke();
                }
            }
        }

        private bool Validate()
        {
            if (!string.IsNullOrEmpty(Target.ListenLog))
            {
                var strs = Target.ListenLog.Split(':');
                if (strs.Length != 2)
                {
                    Helper.ShowToast("ListenLog is invalid, name it like '127.0.0.1:50000'");
                    return false;
                }
            }

            if (!string.IsNullOrEmpty(Target.PushPatch))
            {
                var strs = Target.PushPatch.Split(':');
                if (strs.Length != 2)
                {
                    Helper.ShowToast("PushPatch is invalid, name it like '127.0.0.1:50000'");
                    return false;
                }
            }

            if (!string.IsNullOrEmpty(Target.PatchServer) && !(Target.PatchServer.StartsWith("http://") || Target.PatchServer.StartsWith("https://")))
            {
                Helper.ShowToast("PatchServer is invalid, name it's prefix with 'http://' or 'https://'");
                return false;
            }

            if (!string.IsNullOrEmpty(Target.LogServer) && !(Target.LogServer.StartsWith("http://") || Target.LogServer.StartsWith("https://")))
            {
                Helper.ShowToast("LogServer is invalid, name it's prefix with 'http://' or 'https://'");
                return false;
            }

            if (connServerEdit && !string.IsNullOrEmpty(connServerTemp))
            {
                var strs = connServerTemp.Split(':');
                if (strs.Length != 2)
                {
                    Helper.ShowToast("ConnServer is invalid, name it like '127.0.0.1:50000'");
                    return false;
                }
                Target.ConnServer.Add(connServerTemp);
                connServerTemp = "";
                connServerEdit = false;
                Target.ConnIndex = Target.ConnServer.Count - 1;
            }

            if (cgiServerEdit && !string.IsNullOrEmpty(cgiServerTemp))
            {
                if (!string.IsNullOrEmpty(cgiServerTemp) && !(cgiServerTemp.StartsWith("http://") || cgiServerTemp.StartsWith("https://")))
                {
                    Helper.ShowToast("CgiServer is invalid, name it's prefix with 'http://' or 'https://'");
                    return false;
                }
                Target.CgiServer.Add(cgiServerTemp);
                cgiServerTemp = "";
                cgiServerEdit = false;
                Target.CgiIndex = Target.CgiServer.Count - 1;
            }
            return true;
        }
    }
}