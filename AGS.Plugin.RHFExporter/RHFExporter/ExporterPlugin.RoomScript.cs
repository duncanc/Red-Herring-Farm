using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using JS = RedHerringFarm.JavaScriptGeneration;

namespace RedHerringFarm
{
    partial class ExporterPlugin
    {
        private void ExportCurrentRoomScript()
        {
            SPAGS.ScriptCollection scripts = new SPAGS.ScriptCollection(editor.Version);
            scripts.SetStandardConstants(editor.CurrentGame.Settings);
            scripts.AddStandardHeaders(editor);
            List<SPAGS.Script> globalScripts = GetGlobalScripts(scripts);

            SPAGS.ValueType.Struct hotspotType, objectType;
            if (!scripts.GlobalNamespace.TryGetValue2<SPAGS.ValueType.Struct>("Hotspot", out hotspotType))
            {
                throw new Exception("Unable to find \"Hotspot\" type!");
            }
            if (!scripts.GlobalNamespace.TryGetValue2<SPAGS.ValueType.Struct>("Object", out objectType))
            {
                throw new Exception("Unable to find \"Object\" type!");
            }
            StringBuilder roomHeaderSB = new StringBuilder();
            foreach (AGS.Types.RoomHotspot hs in editor.RoomController.CurrentRoom.Hotspots)
            {
                roomHeaderSB.AppendLine("Hotspot " + hs.Name + ";");
            }
            foreach (AGS.Types.RoomObject obj in editor.RoomController.CurrentRoom.Objects)
            {
                roomHeaderSB.AppendLine("Object " + obj.Name + ";");
            }
            SPAGS.Script roomHeader = scripts.AddHeader("___CurrentRoomScript__.ash", roomHeaderSB.ToString());

            SPAGS.Script roomScript = null;

            foreach (AGS.Types.IRoom iroom in editor.CurrentGame.Rooms)
            {
                if (iroom.Number == editor.RoomController.CurrentRoom.Number)
                {
                    roomScript = scripts.CompileScript("___CurrentRoomScript__.asc", iroom.Script.Text);
                }
            }

            if (roomScript == null)
            {
                throw new Exception("Room #" + editor.RoomController.CurrentRoom.Number + " not found!");
            }

            JS.SPAGSConverter convert = new JS.SPAGSConverter();

            JS.Script script = new JS.Script();

            JS.Expression ags = script.GetExternalGlobal("ags");
            JS.Expression jQuery = script.GetExternalGlobal("jQuery");

            JS.Expression ags_util = ags.Index("util");
            JS.Expression ags_games_guid = ags.Index("games", GetCurrentGameGuid());

            JS.FunctionDefinition initFunc = new JS.FunctionDefinition();
            {
                JS.Variable initializerParam_jQuery = initFunc.NewParam("$");

                JS.FunctionDefinition ctor = new JS.FunctionDefinition();
                JS.Expression.ObjectLiteral ctorProto = new JS.Expression.ObjectLiteral();
                {
                    JS.Variable ctorParam_engine = ctor.NewParam("engine");
                    JS.Variable ctorParam_game = ctor.NewParam("game");
                    JS.Variable ctorParam_room = ctor.NewParam("room");

                    JS.Variable ctorVar_util = ctor.NewVar("util");
                    ctorVar_util.InitialValue = ags_util;

                    JS.Variable ctorVar_self = ctor.NewVar("roomScript");
                    ctorVar_self.InitialValue = ctor.This;

                    JS.Variable ctorVar_globalScripts = ctor.NewVar("globalScripts");
                    ctorVar_globalScripts.InitialValue = ctorParam_game.Index("globalScripts");

                    foreach (SPAGS.Function globalFunc in scripts.GlobalNamespace.EachOf<SPAGS.Function>())
                    {
                        if (globalFunc.OwnerScript == null)
                        {
                            JS.Expression specialExpr;
                            if (convert.GetSpecial(globalFunc, out specialExpr))
                            {
                                convert.AddReference(globalFunc, specialExpr);
                            }
                            else
                            {
                                string funcName = globalFunc.Name.Replace("::", "$$");
                                convert.AddReference(globalFunc, ctorParam_engine.Index(funcName));
                            }
                        }
                    }

                    foreach (SPAGS.Function exportedFunc in scripts.Exported.EachOf<SPAGS.Function>())
                    {
                        if (exportedFunc.OwnerScript != null && exportedFunc.OwnerScript != roomScript)
                        {
                            convert.AddReference(exportedFunc, ctorVar_globalScripts.Index(exportedFunc.Name.Replace("::", "$$")));
                        }
                    }

                    foreach (SPAGS.Variable globalVar in scripts.GlobalNamespace.EachOf<SPAGS.Variable>())
                    {
                        if (globalVar.OwnerScript == null)
                        {
                            convert.AddReference(globalVar, ctorParam_game.Index(globalVar.Name, convert.GetValueTypes(globalVar.Type)));
                        }
                        else
                        {
                            if (globalVar.OwnerScript == roomHeader)
                            {
                                convert.AddReference(globalVar, ctorParam_room.Index(globalVar.Name, convert.GetValueTypes(globalVar.Type)));
                            }
                            else
                            {
                                convert.AddReference(globalVar, ctorVar_globalScripts.Index(globalVar.Name, convert.GetValueTypes(globalVar.Type)));
                            }
                        }
                    }

                    foreach (SPAGS.ValueType.Struct structType in roomScript.DefinedStructs)
                    {
                        if (structType.IsManaged)
                        {
                            continue;
                        }
                        string structName = "t$" + structType.Name;

                        JS.Variable structVar = ctor.NewVar(structName);

                        convert.AddReference(structType, structVar);

                        JS.FunctionDefinition structCtor = new JS.FunctionDefinition();
                        ctor.Body.Add(structVar.Assign(structCtor));

                        JS.Expression.ObjectLiteral structProto = new JS.Expression.ObjectLiteral();
                        foreach (SPAGS.StructMember.Field field in structType.Members.EachOf<SPAGS.StructMember.Field>())
                        {
                            SPAGS.Expression initValue = field.Type.CreateDefaultValueExpression();
                            if (initValue.IsConstant())
                            {
                                structProto.Add(field.Name, convert.FromSPAGS(initValue));
                            }
                            else
                            {
                                structProto.Add(field.Name, JS.Expression.Null);
                                structCtor.Body.Add(structCtor.This.Index(field.Name).Assign(convert.FromSPAGS(initValue)));
                            }
                        }
                        if (structProto.Entries.Count > 0)
                        {
                            ctor.Body.Add(structVar.Index("prototype").Assign(structProto));
                        }

                        if (scripts.GlobalNamespace.ContainsKey(structType.Name)
                            && scripts.GlobalNamespace[structType.Name] == structType)
                        {
                            ctor.Body.Add(ctorVar_self.Index(structType.Name).Assign(structVar));
                        }
                    }

                    foreach (SPAGS.Variable roomVar in roomScript.DefinedVariables)
                    {
                        string varName = "v$" + roomVar.Name;
                        JS.Variable jsVar = ctor.NewVar(varName, convert.GetValueTypes(roomVar.Type));
                        convert.AddReference(roomVar, jsVar);
                        SPAGS.Expression value = roomVar.InitialValue ?? roomVar.Type.CreateDefaultValueExpression();
                        if (value.IsConstant())
                        {
                            jsVar.InitialValue = convert.FromSPAGS(value);
                        }
                        else
                        {
                            ctor.Body.Add(jsVar.Assign(convert.FromSPAGS(value)));
                        }
                    }

                    Dictionary<SPAGS.Function, JS.FunctionDefinition> funcDefs
                        = new Dictionary<SPAGS.Function, JS.FunctionDefinition>();

                    foreach (SPAGS.Function func in roomScript.DefinedFunctions)
                    {
                        string funcName = "f$" + func.Name.Replace("::","$$");
                        JS.Variable funcVar = ctor.NewVar(funcName);
                        JS.FunctionDefinition funcDef = new JS.FunctionDefinition();
                        foreach (SPAGS.Parameter spagsParam in func.ParameterVariables)
                        {
                            string paramName = "p$" + spagsParam.Name;
                            JS.Variable jsParam = funcDef.NewParam(paramName);
                            jsParam.VariableType = convert.GetValueTypes(spagsParam.Type);
                            convert.SetReference(spagsParam, jsParam);
                        }
                        funcDefs[func] = funcDef;
                        ctor.Body.Add(funcVar.Assign(funcDef));
                        ctor.Body.Add(ctorVar_self.Index(func.Name).Assign(funcVar));
                        convert.AddReference(func, funcVar);
                    }

                    foreach (SPAGS.Function func in roomScript.DefinedFunctions)
                    {
                        JS.FunctionDefinition funcDef = funcDefs[func];
                        funcDef.Body = convert.FunctionBodyFromSPAGS(func);
                    }
                }
                initFunc.Body.Add(ags_games_guid.Index("Room" + editor.RoomController.CurrentRoom.Number + "Script").Assign(ctor));

                if (ctorProto.Entries.Count > 0)
                {
                    initFunc.Body.Add(ags_games_guid.Index("Room" + editor.RoomController.CurrentRoom.Number + "Script", "prototype").Assign(ctorProto));
                }
            }
            script.Add((JS.Statement)jQuery.Call(initFunc));

            using (JS.Writer output = JS.Writer.Create(InExportFolder(ROOM_SCRIPT_FILENAME, editor.RoomController.CurrentRoom.Number)))
            {
                script.WriteTo(output);
            }
        }
    }
}
