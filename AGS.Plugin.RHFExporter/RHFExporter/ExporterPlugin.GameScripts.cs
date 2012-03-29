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
        private void ExportGlobalScripts()
        {
            SPAGS.ScriptCollection scripts = new SPAGS.ScriptCollection(editor.Version);
            scripts.SetStandardConstants(editor.CurrentGame.Settings);
            scripts.AddStandardHeaders(editor);
            List<SPAGS.Script> globalScripts = GetGlobalScripts(scripts);

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

                    JS.Variable ctorVar_util = ctor.NewVar("util");
                    ctorVar_util.InitialValue = ags_util;

                    JS.Variable ctorVar_self = ctor.NewVar("globalScripts");
                    ctorVar_self.InitialValue = ctor.This;

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

                    foreach (SPAGS.Variable globalVar in scripts.GlobalNamespace.EachOf<SPAGS.Variable>())
                    {
                        if (globalVar.OwnerScript == null)
                        {
                            convert.AddReference(globalVar, ctorParam_game.Index(globalVar.Name, convert.GetValueTypes(globalVar.Type)));
                        }
                    }


                    Dictionary<SPAGS.Function, JS.FunctionDefinition> funcDefs
                        = new Dictionary<SPAGS.Function, JS.FunctionDefinition>();

                    foreach (SPAGS.Script globalScript in globalScripts)
                    {
                        string scriptName = Regex.Replace(globalScript.Name, @"\..*$", "");
                        scriptName = Regex.Replace(scriptName, "[^a-zA-Z_0-9]", "_");
                        if (!Regex.IsMatch(scriptName, "^[a-zA-Z_]"))
                        {
                            if (globalScript.Name.EndsWith(".ash"))
                            {
                                scriptName = "h$" + scriptName;
                            }
                            else
                            {
                                scriptName = "s$" + scriptName;
                            }
                        }
                        foreach (SPAGS.ValueType.Struct structType in globalScript.DefinedStructs)
                        {
                            if (structType.IsManaged)
                            {
                                continue;
                            }
                            string structName = scriptName + "$" + structType.Name;

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
                        foreach (SPAGS.Variable spagsVar in globalScript.DefinedVariables)
                        {
                            JS.Expression jsVar;
                            if (scripts.GlobalNamespace.ContainsKey(spagsVar.Name)
                                && scripts.GlobalNamespace[spagsVar.Name] == spagsVar)
                            {
                                jsVar = ctorVar_self.Index(spagsVar.Name, convert.GetValueTypes(spagsVar.Type));
                                SPAGS.Expression value = spagsVar.InitialValue ?? spagsVar.Type.CreateDefaultValueExpression();
                                if (value.IsConstant())
                                {
                                    ctorProto.Entries.Add(spagsVar.Name, convert.FromSPAGS(value));
                                }
                                else
                                {
                                    ctor.Body.Add(jsVar.Assign(convert.FromSPAGS(value)));
                                }
                            }
                            else
                            {
                                string varName = scriptName + "$" + spagsVar.Name;
                                jsVar = ctor.NewVar(varName, convert.GetValueTypes(spagsVar.Type));
                                if (spagsVar.InitialValue == null)
                                {
                                    ctor.Body.Add(jsVar.Assign(convert.FromSPAGS(spagsVar.Type.CreateDefaultValueExpression())));
                                }
                                else
                                {
                                    ctor.Body.Add(jsVar.Assign(convert.FromSPAGS(spagsVar.InitialValue)));
                                }
                            }
                            convert.AddReference(spagsVar, jsVar);
                        }
                        foreach (SPAGS.Function scriptFunc in globalScript.DefinedFunctions)
                        {
                            string funcName = scriptName + "$" + scriptFunc.Name.Replace("::", "$$");
                            JS.Variable funcVar = ctor.NewVar(funcName);
                            JS.FunctionDefinition funcDef = new JS.FunctionDefinition();
                            convert.AddReference(scriptFunc, funcVar);
                            funcDefs.Add(scriptFunc, funcDef);
                            ctor.Body.Add(funcVar.Assign(funcDef));
                            foreach (SPAGS.Parameter param in scriptFunc.ParameterVariables)
                            {
                                string paramName = "p$" + param.Name;
                                JS.Variable jsParam = funcDef.NewParam(paramName);
                                jsParam.VariableType = convert.GetValueTypes(param.Type);
                                convert.AddReference(param, jsParam);
                            }
                            ctor.Body.Add(ctorVar_self.Index(scriptFunc.Name.Replace("::","$$")).Assign(funcVar));
                        }
                    }
                    // process function bodies separately
                    foreach (SPAGS.Script globalScript in globalScripts)
                    {
                        foreach (SPAGS.Function scriptFunc in globalScript.DefinedFunctions)
                        {
                            JS.FunctionDefinition funcDef = funcDefs[scriptFunc];
                            funcDef.Body = convert.FunctionBodyFromSPAGS(scriptFunc);
                        }
                    }
                }
                initFunc.Body.Add(ags_games_guid.Index("GlobalScripts").Assign(ctor));

                if (ctorProto.Entries.Count > 0)
                {
                    initFunc.Body.Add(ags_games_guid.Index("GlobalScripts", "prototype").Assign(ctorProto));
                }
            }
            script.Add((JS.Statement)jQuery.Call(initFunc));

            Dictionary<SPAGS.Function, bool> blockingFunctions = new Dictionary<SPAGS.Function,bool>();
            foreach (SPAGS.Function func in scripts.GlobalNamespace.EachOf<SPAGS.Function>())
            {
                switch (func.Name)
                {
                    case "Game::InputBox":
                    case "Display":
                    case "DisplayAt":
                    case "DisplayMessage":
                    case "DisplayMessageAtY":
                    case "DisplayTopBar":
                    case "DisplayMessageBar":
                    case "ProcessClick":
                    case "QuitGame":
                    case "AbortGame":
                    case "RestartGame":
                    case "InputBox":
                    case "InventoryScreen":
                    case "RunObjectInteraction":
                    case "AnimateObject":
                    case "AnimateObjectEx":
                    case "MoveObject":
                    case "MoveObjectDirect":
                    case "RunCharacterInteraction":
                    case "DisplaySpeech":
                    case "DisplayThought":
                    case "AnimateCharacter":
                    case "AnimateCharacterEx":
                    case "MoveCharacter":
                    case "MoveCharacterDirect":
                    case "MoveCharacterPath":
                    case "MoveCharacterStraight":
                    case "MoveCharacterToHotspot":
                    case "MoveCharacterToObject":
                    case "MoveCharacterBlocking":
                    case "FaceCharacter":
                    case "FaceLocation":
                    case "RunHotspotInteraction":
                    case "RunRegionInteraction":
                    case "RunInventoryInteraction":
                    case "InventoryItem::RunInteraction":
                    case "FadeIn":
                    case "FadeOut":
                    case "ShakeScreen":
                    case "PlayFlic":
                    case "PlayVideo":
                    case "Wait":
                    case "WaitKey":
                    case "WaitMouseKey":
                    case "Hotspot::RunInteraction":
                    case "Region::RunInteraction":
                    case "Dialog::DisplayOptions":
                    case "Object::Animate":
                    case "Object::Move":
                    case "Object::RunInteraction":
                    case "Character::Animate":
                    case "Character::FaceCharacter":
                    case "Character::FaceLocation":
                    case "Character::FaceObject":
                    case "Character::Move":
                    case "Character::RunInteraction":
                    case "Character::Say":
                    case "Character::SayAt":
                    case "Character::Think":
                    case "Character::Walk":
                    case "Character::WalkStraight":
                        func.AddSelfAndAllCallersTo(blockingFunctions);
                        break;
                }
            }

            using (JS.Writer output = JS.Writer.Create(InExportFolder(GLOBAL_SCRIPTS_FILENAME)))
            {
                foreach (SPAGS.Function func in scripts.Exported.EachOf<SPAGS.Function>())
                {
                    if (func.Body == null || !blockingFunctions.ContainsKey(func)) continue;
                    SPAGS.SimSynch.SimSynchFunction ssf = SPAGS.SimSynch.SimSynchFunction.Create(func, blockingFunctions);
                    output.WriteLine(ssf.ToString());
                }
                script.WriteTo(output);
            }
        }
        private List<SPAGS.Script> GetGlobalScripts(SPAGS.ScriptCollection scripts)
        {
            List<SPAGS.Script> globalScripts = new List<SPAGS.Script>();
            globalScripts.AddRange(scripts.Headers);
            SPAGS.Script globVarsScript = scripts.CompileGlobalVariablesScript(editor);
            if (globVarsScript != null)
            {
                globalScripts.Add(globVarsScript);
            }
            foreach (AGS.Types.Script script in editor.CurrentGame.Scripts)
            {
                if (!script.IsHeader)
                {
                    globalScripts.Add(scripts.CompileScript(script.FileName, HacksAndKludges.GetScriptText(script)));
                }
            }
            globalScripts.Add(scripts.CompileDialogScript(editor));
            return globalScripts;
        }
    }
}
