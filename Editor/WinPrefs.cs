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
        public LIBRARY.BASE.Preferences TTarget;

        public class Preferences : LIBRARY.BASE.Preferences
        {
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
            Target = CreateObject();
        }

        public virtual object CreateObject(string path = null)
        {
            return Activator.CreateInstance(TargetType, new object[] { path, null });
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
                var p = CreateObject(f) as LIBRARY.BASE.Preferences;
                if (p.Error == null && string.IsNullOrEmpty(p.Name) == false)
                {
                    names.Add(string.Format("{0} ({1})", p.Name, Path.GetFileName(f)));
                }
            }
            if (currentIndex == -1)
            {
                var cpref = TTarget != null ? TTarget : CreateObject(Constants.PREF_STEAMING_FILE) as LIBRARY.BASE.Preferences;
                if (cpref.Error == null && string.IsNullOrEmpty(cpref.Name) == false)
                {
                    for (int i = 0; i < files.Count; i++)
                    {
                        var f = files[i];
                        var p = CreateObject(f) as LIBRARY.BASE.Preferences;
                        if (p.Error == null && string.IsNullOrEmpty(p.Name) == false && cpref.Name == p.Name)
                        {
                            currentIndex = i;
                            break;
                        }
                    }
                }
            }
            if (TTarget == null)
            {
                if (currentIndex == -1) currentIndex = 0;
                Target = CreateObject(files[currentIndex]);
                TTarget = Target as LIBRARY.BASE.Preferences;
            }
            GUILayout.BeginHorizontal();
            var lastIndex = currentIndex;
            currentIndex = EditorGUILayout.Popup(currentIndex, names.ToArray());
            if (lastIndex != currentIndex)
            {
                Target = CreateObject(files[currentIndex]);
                TTarget = Target as LIBRARY.BASE.Preferences;
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
                    Target = CreateObject(path);
                    TTarget = Target as LIBRARY.BASE.Preferences;
                    TTarget.Name = Path.GetFileNameWithoutExtension(path);
                    Helper.SaveText(TTarget.Path, Helper.ObjectToJson(Helper.ObjectToDict(TTarget)));
                    currentIndex = -1;
                }
            }
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
                string[] strs = TTarget.CgiServer.ToArray();
                for (int i = 0; i < strs.Length; i++) strs[i] = strs[i].Replace('/', '\u2215');
                TTarget.CgiIndex = EditorGUILayout.Popup(TTarget.CgiIndex, strs, GUILayout.Width(125));
            }
            if (!cgiServerEdit)
            {
                if (GUILayout.Button("+"))
                {
                    cgiServerEdit = true;
                }
                if (GUILayout.Button("-"))
                {
                    if (TTarget.CgiServer[TTarget.CgiIndex] != "NONE")
                    {
                        TTarget.CgiServer.RemoveAt(TTarget.CgiIndex);
                        TTarget.CgiIndex--;
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
                            TTarget.CgiServer.Add(cgiServerTemp);
                            cgiServerTemp = "";
                            cgiServerEdit = false;
                            TTarget.CgiIndex = TTarget.CgiServer.Count - 1;
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
                TTarget.ConnIndex = EditorGUILayout.Popup(TTarget.ConnIndex, TTarget.ConnServer.ToArray(), GUILayout.Width(125));
            }
            if (!connServerEdit)
            {
                if (GUILayout.Button("+"))
                {
                    connServerEdit = true;
                }
                if (GUILayout.Button("-"))
                {
                    if (TTarget.ConnServer[TTarget.ConnIndex] != "NONE")
                    {
                        TTarget.ConnServer.RemoveAt(TTarget.ConnIndex);
                        TTarget.ConnIndex--;
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
                            TTarget.ConnServer.Add(connServerTemp);
                            connServerTemp = "";
                            connServerEdit = false;
                            TTarget.ConnIndex = TTarget.ConnServer.Count - 1;
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
                    Helper.SaveText(TTarget.Path, Helper.ObjectToJson(Helper.ObjectToDict(TTarget)));
                    Helper.Log("[FILE@{0}] Save preferences success.", TTarget.Path);
                    Helper.ShowToast("Save preferences success.");
                    EditorLogcat.Reset();
                });
                if (TTarget.Path.EndsWith(Constants.PREF_DEFALUT_FILE))
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
                    Helper.SaveText(TTarget.Path, Helper.ObjectToJson(Helper.ObjectToDict(TTarget)));
                    Helper.Log("[FILE@{0}] Apply preferences success.", TTarget.Path);
                    Helper.CopyFile(TTarget.Path, Constants.PREF_STEAMING_FILE);
                    AssetDatabase.Refresh();
                    Preferences.Instance = CreateObject(Constants.PREF_STEAMING_FILE) as Preferences;
                    if (File.Exists(Constants.SIMULATOR_EXE))
                    {
                        var dest = Path.GetDirectoryName(Constants.SIMULATOR_EXE) + "/" +
                        Path.GetFileNameWithoutExtension(Constants.SIMULATOR_EXE) + "_Data/StreamingAssets/" +
                        Path.GetFileName(Constants.PREF_STEAMING_FILE);
                        File.Copy(TTarget.Path, dest, true);
                        Helper.ShowToast("Apply preferences success.");
                    }
                    EditorLogcat.Reset();
                });
                if (TTarget.Path.EndsWith(Constants.PREF_DEFALUT_FILE))
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
            if (!string.IsNullOrEmpty(TTarget.ListenLog))
            {
                var strs = TTarget.ListenLog.Split(':');
                if (strs.Length != 2)
                {
                    Helper.ShowToast("ListenLog is invalid, name it like '127.0.0.1:50000'");
                    return false;
                }
            }

            if (!string.IsNullOrEmpty(TTarget.PushPatch))
            {
                var strs = TTarget.PushPatch.Split(':');
                if (strs.Length != 2)
                {
                    Helper.ShowToast("PushPatch is invalid, name it like '127.0.0.1:50000'");
                    return false;
                }
            }

            if (!string.IsNullOrEmpty(TTarget.PatchServer) && !(TTarget.PatchServer.StartsWith("http://") || TTarget.PatchServer.StartsWith("https://")))
            {
                Helper.ShowToast("PatchServer is invalid, name it's prefix with 'http://' or 'https://'");
                return false;
            }

            if (!string.IsNullOrEmpty(TTarget.LogServer) && !(TTarget.LogServer.StartsWith("http://") || TTarget.LogServer.StartsWith("https://")))
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
                TTarget.ConnServer.Add(connServerTemp);
                connServerTemp = "";
                connServerEdit = false;
                TTarget.ConnIndex = TTarget.ConnServer.Count - 1;
            }

            if (cgiServerEdit && !string.IsNullOrEmpty(cgiServerTemp))
            {
                if (!string.IsNullOrEmpty(cgiServerTemp) && !(cgiServerTemp.StartsWith("http://") || cgiServerTemp.StartsWith("https://")))
                {
                    Helper.ShowToast("CgiServer is invalid, name it's prefix with 'http://' or 'https://'");
                    return false;
                }
                TTarget.CgiServer.Add(cgiServerTemp);
                cgiServerTemp = "";
                cgiServerEdit = false;
                TTarget.CgiIndex = TTarget.CgiServer.Count - 1;
            }
            return true;
        }
    }
}