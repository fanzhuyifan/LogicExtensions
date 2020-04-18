﻿using InternalModding.Blocks;
using Modding;
using Modding.Blocks;
using Modding.Common;
using Modding.Levels;
using Selectors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;
using Logic.Blocks;
using Logic.Script;
using System.Text.RegularExpressions;
using Logic.BlockScripts;

namespace Logic
{
    public class Logic : SingleInstance<Logic>
    {
        public bool DrawSensorDebug = false;
        Material LineMaterial;
        public Dictionary<Type, Action<BlockBehaviour, KeyInputController>> Registers;
        public Dictionary<Type, Action<BlockBehaviour>> Unregisters;
        Dictionary<Type, Type> AdditionScripts = new Dictionary<Type, Type>
        {
            { typeof(FlyingController), typeof(FlyingScript) },
            { typeof(SteeringWheel), typeof(SteeringScript) },
            { typeof(CogMotorControllerHinge), typeof(CogScript) },
            { typeof(SliderCompress), typeof(PistonScript) },
            { typeof(WaterCannonController), typeof(WaterCannonScript) }
        };

        Dictionary<Machine, MachineHandler> MachineHandlers = new Dictionary<Machine, MachineHandler>();
        public MessageType CpuInfoMessage;
        public MessageType CpuLogMessage;
        public void Awake()
        {
            // Loading
            var logic = new Interpreter();
            Registers = new Dictionary<Type, Action<BlockBehaviour, KeyInputController>>();
            Unregisters = new Dictionary<Type, Action<BlockBehaviour>>();

            ModConsole.RegisterCommand("script", args => {
                var text = string.Join(" ", args);
                var func = logic.PrepareScript(text);
                logic.AddExtFunc(func, "log", (ctx, x) => { ModConsole.Log(x[0]?.ToString()); return null; }, true);
                logic.SetScript(func);
                var res = logic.ContinueScript(1000);
                ModConsole.Log(res?.ToString());
            }, "exec script");

            ModConsole.RegisterCommand("cpuapi", args =>
            {
                foreach (var line in SingleInstance<CpuApi>.Instance.GetHelp())
                    ModConsole.Log(line);
            }, "print cpu api list");

            ModConsole.RegisterCommand("sensordbg", args =>
            {
                DrawSensorDebug = args.Length < 1 ? false : args[0] == "true";
            }, "print sensor debug points");

            CpuBlock.Create(this);
            ExtLogicGate.Create(this);
            ExtAltimeterBlock.Create(this);
            ExtSpeedometerBlock.Create(this);
            ExtAnglometerBlock.Create(this);
            ExtSensorBlock.Create(this);
            ModConsole.Log($"Logic mod Awake");

            Events.OnMachineSimulationToggle += Events_OnMachineSimulationToggle;
            LineMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));
            LineMaterial.hideFlags = HideFlags.HideAndDontSave;
            LineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            LineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            LineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            LineMaterial.SetInt("_ZWrite", 0);
            Camera.onPostRender += DrawConnectingLines;

            Events.OnBlockInit += InitBlock;

            CpuInfoMessage = ModNetworking.CreateMessageType(new DataType[]
            {
                DataType.Block,
                DataType.ByteArray
            });

            CpuLogMessage = ModNetworking.CreateMessageType(new DataType[] 
            {
                DataType.Block,
                DataType.String
            });

            ModNetworking.Callbacks[CpuInfoMessage] += (Action<Message>)((msg) =>
            {
                Player localPlayer = Player.GetLocalPlayer();
                if (msg == null || localPlayer == null || !localPlayer.IsHost)
                    return;

                var block = msg.GetData(0) as Modding.Blocks.Block;
                if (!(block?.BlockScript is CpuBlock cpu))
                    return;

                if (block.Machine == localPlayer.Machine)
                    return; // don't read updates for MY machine!

                cpu.AfterEdit_ServerRecv((byte[])msg.GetData(1));
            });

            ModNetworking.Callbacks[CpuLogMessage] += (Action<Message>)((msg) =>
            {
                if (msg == null)
                    return;

                var block = msg.GetData(0) as Modding.Blocks.Block;
                if (!(block?.BlockScript is CpuBlock cpu))
                    return;

                cpu.LogMessage((string)msg.GetData(1));
            });
        }

        private void InitBlock(Modding.Blocks.Block block)
        {
            var machine = block.Machine?.InternalObject;
            if (machine != null && !MachineHandlers.ContainsKey(machine))
                MachineHandlers[machine] = new MachineHandler(machine);

            foreach (var kp in AdditionScripts)
            {
                if (kp.Key.IsInstanceOfType(block.InternalObject) && block.GameObject.GetComponent(kp.Value) == null)
                    block.GameObject.AddComponent(kp.Value);
            }
        }

        public void AddKeyRegistrer(Type moddedType, Action<BlockBehaviour, KeyInputController> register, Action<BlockBehaviour> unregister)
        {
            Registers[moddedType] = register;
            Unregisters[moddedType] = unregister;
        }

        public void PlaceAdditionScripts(BlockBehaviour block)
        {
            foreach (var kp in AdditionScripts)
            {
                if (kp.Key.IsInstanceOfType(block))
                    (block.GetComponent(kp.Value) as AdditionScript)?.Reset();
            }
        }

        private void Events_OnMachineSimulationToggle(PlayerMachine pmachine, bool simulating)
        {
            if (pmachine == null)
                return;

            var machine = pmachine.InternalObject;
            if (!MachineHandlers.ContainsKey(machine))
                return;

            if (simulating)
                MachineHandlers[machine].Start();
            else
                MachineHandlers[machine].Stop();
        }

        public void OnDestroy()
        {

        }

        // -----------------------------------------------

        public List<KeyValuePair<Vector3, Vector3>> IncomingLines = new List<KeyValuePair<Vector3, Vector3>>();
        public List<KeyValuePair<Vector3, Vector3>> OutgoingLines = new List<KeyValuePair<Vector3, Vector3>>();

        void DrawConnectingLines(Camera cam)
        {
            if (Camera.main != cam)
                return;
            //Debug.Log($"{DateTime.Now} {IncomingLines.Count}, {OutgoingLines.Count}");
            LineMaterial.SetPass(0);
            foreach (var point in IncomingLines)
            {
                GL.Begin(GL.LINES);
                GL.Color(Color.green);
                GL.Vertex(point.Key);
                GL.Vertex(point.Value);
                GL.End();
            }
            foreach (var point in OutgoingLines)
            {
                GL.Begin(GL.LINES);
                GL.Color(Color.red);
                GL.Vertex(point.Key);
                GL.Vertex(point.Value);
                GL.End();
            }
        }

        DateTime lastGc = DateTime.Now;
        public void Update()
        {
            if (Camera.current == null)
                return;

            foreach (var mh in MachineHandlers.Values)
                mh.Collect();

            IncomingLines = new List<KeyValuePair<Vector3, Vector3>>();
            OutgoingLines = new List<KeyValuePair<Vector3, Vector3>>();
            if (Game.IsSimulating || !Input.GetKey(KeyCode.BackQuote))
                return;

            var machine = Machine.Active();
            if (machine == null || !MachineHandlers.ContainsKey(machine))
                return;

            var selectedBlocks = machine.BuildingBlocks.Where(x => x.IsSelected).ToList();
            if (selectedBlocks.Count == 0)
                return;

            var machineHandler = MachineHandlers[machine];
            var keys = machineHandler.GetKeys(machine.BuildingBlocks).Concat(machineHandler.GetCpuKeys())
                        .GroupBy(x => x.IOMode)
                        .ToDictionary(x => x.Key, x => x.SelectMany(y => machineHandler.GetMKeys(y.key).Select(z => new { y.block, key = z }))
                            .GroupBy(y => y.key).ToDictionary(y => y.Key, y => y.Select(z => z.block).ToList())
                        );
            var inputs = keys.ContainsKey(0) ? keys[0] : new Dictionary<uint, List<BlockBehaviour>>();
            var outputs = keys.ContainsKey(1) ? keys[1] : new Dictionary<uint, List<BlockBehaviour>>();
            var fullIO = keys.ContainsKey(2) ? keys[2] : new Dictionary<uint, List<BlockBehaviour>>();

            inputs = inputs.Union(fullIO).GroupBy(x => x.Key).ToDictionary(x => x.Key, x => x.SelectMany(y => y.Value).Distinct().ToList());
            outputs = outputs.Union(fullIO).GroupBy(x => x.Key).ToDictionary(x => x.Key, x => x.SelectMany(y => y.Value).Distinct().ToList());

            foreach (var block in selectedBlocks)
            {
                if (block.Rigidbody == null)
                    continue;

                var cpu = machineHandler.GetCpuBlock(block);
                if (cpu != null)
                {
                    foreach (var mkey in cpu.PIO.Values)
                    {
                        foreach (var target in machineHandler.GetMKeys(mkey).Where(x => inputs.ContainsKey(x)).SelectMany(x => inputs[x]).Where(x => x != block)
                                .Where(x => x.Rigidbody != null))
                            OutgoingLines.Add(new KeyValuePair<Vector3, Vector3>(block.Rigidbody.position, target.Rigidbody.position));
                        foreach (var source in machineHandler.GetMKeys(mkey).Where(x => outputs.ContainsKey(x)).SelectMany(x => outputs[x]).Where(x => x != block)
                                .Where(x => x.Rigidbody != null))
                            IncomingLines.Add(new KeyValuePair<Vector3, Vector3>(source.Rigidbody.position, block.Rigidbody.position));
                    }
                }
                else
                {
                    foreach (var mkey in block.MapperTypes.Where(y => y is MKey).Select(y => y as MKey))
                    {

                        if (mkey.isEmulator) // Draw outgoing lines
                        {
                            foreach (var target in machineHandler.GetMKeys(mkey).Where(x => inputs.ContainsKey(x)).SelectMany(x => inputs[x]).Where(x => x != block)
                                    .Where(x => x.Rigidbody != null))
                                OutgoingLines.Add(new KeyValuePair<Vector3, Vector3>(block.Rigidbody.position, target.Rigidbody.position));
                        }
                        else // Draw incoming lines
                        {
                            foreach (var source in machineHandler.GetMKeys(mkey).Where(x => outputs.ContainsKey(x)).SelectMany(x => outputs[x]).Where(x => x != block)
                                    .Where(x => x.Rigidbody != null))
                                IncomingLines.Add(new KeyValuePair<Vector3, Vector3>(source.Rigidbody.position, block.Rigidbody.position));
                        }
                    }
                }
            }
        }

        public void Simulate()
        {

        }

        public void FixedUpdate()
        {
        }

        public override string Name => "LogicExtensions";

        CpuBlock SelectedCpu = null;
        CpuBlock PrevCpu = null;
        string lastScript = "";
        string savedScript = "";
        int windowId;
        Rect uiRect;
        GUIStyle textStyle;
        Vector2 scroll = Vector2.zero;
        string statusText = "";

        void InitFont()
        {
            windowId = ModUtility.GetWindowId();
            uiRect = new Rect(Screen.width - 750, Screen.height - 800, 0f, 0f);
            // load console font
            textStyle = new GUIStyle(GUI.skin.textArea);
            textStyle.wordWrap = false;
            //textStyle.richText = true;
            try
            {
                var text = GameObject.Find(@"_PERSISTENT/Canvas/ConsoleView/ConsoleViewContainer/Content/Scroll View/Viewport/Content/LogText")?.GetComponent<UnityEngine.UI.Text>();
                textStyle.font = text.font;
            }
            catch (Exception e)
            {
                Debug.LogWarning("Load console font failed");
            }
        }

        public MachineHandler GetMachineHandler(BlockBehaviour block)
        {
            var machine = block.ParentMachine;
            if (MachineHandlers.ContainsKey(machine))
                return MachineHandlers[machine];
            return null;
        }

        IEnumerable<IEnumerable<T>> Batch<T>(IEnumerable<T> obj, int batch)
        {
            int pos = 0;
            var bat = new List<T>();
            foreach (var x in obj)
            {
                bat.Add(x);
                ++pos;
                if (pos >= batch)
                {
                    pos = 0;
                    yield return bat;
                    bat = new List<T>();
                }
            }
            if (bat.Count > 0)
                yield return bat;
        }

        Dictionary<MExtKey, string> KeyTexts = new Dictionary<MExtKey, string>();

        public void OnGUI()
        {
            if (Game.IsSimulating)
                return;

            if (textStyle == null)
                InitFont();

            var mach = Machine.Active();
            if (mach == null || !MachineHandlers.ContainsKey(mach))
                return;

            SelectedCpu = MachineHandlers[mach].GetCpus().Where(x => x.Key.IsModifying).Select(x => x.Value).FirstOrDefault();
            if (SelectedCpu != null)
            {
                if (PrevCpu != SelectedCpu)
                {
                    PrevCpu = SelectedCpu;
                    savedScript = lastScript = SelectedCpu.Script.Script;
                }
                uiRect = GUILayout.Window(windowId, new Rect(uiRect.position, Vector2.zero), GuiFunc, "CPU edit");
            }
            else
            {
                if (KeyTexts.Count > 0)
                    KeyTexts = new Dictionary<MExtKey, string>();
            }
        }

        bool AddPIOLine(MExtKey key)
        {
            GUILayout.Label(key.DisplayName, GUILayout.ExpandWidth(false));
            if (!KeyTexts.ContainsKey(key))
                KeyTexts[key] = key.GenerateText();
            KeyTexts[key] = GUILayout.TextField(KeyTexts[key], GUILayout.Width(80));
            return GUILayout.Button("-", GUILayout.ExpandWidth(false));
        }

        void UpdateScriptStatus(Exception e)
        {
            if (e is Parser.ParserException pe)
                statusText = pe.Message + " " + pe.Pos.ToString();
            else if (e != null)
                statusText = e.Message;
            else
                statusText = "";
        }

        void GuiFunc(int id)
        {
            if (SelectedCpu == null)
                return;

            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.Label("PIO", GUILayout.ExpandWidth(false));
            bool addPio = GUILayout.Button("+", GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            if (addPio && SelectedCpu.PIO.Count < 100)
                SelectedCpu.AddPIO();

            List<MExtKey> toDelete = new List<MExtKey>();
            foreach (var line in Batch(SelectedCpu.PIO.OrderBy(x => x.Key).Select(x => x.Value), 4))
            {
                GUILayout.BeginHorizontal();
                foreach (var key in line)
                    if (AddPIOLine(key))
                        toDelete.Add(key);
                GUILayout.EndHorizontal();
            }

            foreach (var d in toDelete)
                KeyTexts.Remove(d);
            SelectedCpu.RemovePIO(toDelete);

            scroll = GUILayout.BeginScrollView(scroll, GUILayout.Width(700), GUILayout.Height(600));
            lastScript = GUILayout.TextArea(lastScript, textStyle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            GUILayout.EndScrollView();
            //lastScript = StaticSettings.SanatizeString(newText);
            if (savedScript != lastScript)
            {
                savedScript = lastScript;
                var e = SelectedCpu.CheckScript(savedScript);
                UpdateScriptStatus(e);
            }

            if (GUILayout.Button("Save"))
            {
                foreach (var k in KeyTexts.Keys.ToArray())
                {
                    k.Text_TextChanged(KeyTexts[k]);
                    KeyTexts[k] = k.GenerateText();
                    SelectedCpu.AfterEdit(k);
                }

                var e = SelectedCpu.ApplyScript(savedScript);
                if (e != null)
                {
                    UpdateScriptStatus(e);
                }
                else
                {
                    statusText = "Saved";
                }
            }
            GUILayout.Label(statusText);
            GUILayout.EndVertical();
            GUI.DragWindow();

            /*int controlID = GUIUtility.GetControlID(FocusType.Keyboard);
            var gUIContent = ((GUIUtility.keyboardControl == controlID) ? new GUIContent(newText + Input.compositionString) : new GUIContent(newText));
            var rect = GUILayoutUtility.GetRect(gUIContent, textStyle, GUILayout.Width(300), GUILayout.Height(500));
            newText = GUI.TextArea(rect, newText, textStyle);
            var editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);

            var tags = TagRegex.Matches(newText);
            var ci = editor.cursorIndex;
            // if inside of match - move to the end of match
            for (int i = 0; i < tags.Count; ++i)
            {
                var tag = tags[i];
                if (tag.Index <= ci && tag.Index + tag.Length > ci)
                {
                    for (int j = 0; j < tag.Length - ci; ++j)
                        editor.MoveRight();
                    break;
                }
            }

            ci = editor.cursorIndex;
            for (int i = 0; i < tags.Count; ++i)
            {
                var tag = tags[i];
                if (tag.Index <= ci)
                {
                    for (int j = 0; j < tag.Length; ++j)
                        editor.MoveLeft();
                }
            }*/
        }

    }
}