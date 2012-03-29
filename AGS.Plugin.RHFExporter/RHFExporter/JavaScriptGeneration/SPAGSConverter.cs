using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using SPAGS.SimSynch;

namespace RedHerringFarm.JavaScriptGeneration
{
    public class SPAGSConverter
    {
        public Dictionary<SPAGS.Function, string> SPAGSFunctionNames
            = new Dictionary<SPAGS.Function,string>();

        public delegate Expression MutateExpression(SpecialFunctionCall input);

        public FunctionDefinition FromSPAGS(SPAGS.Function spagsFunc)
        {
            FunctionDefinition jsFunc = new FunctionDefinition();
            for (int i = 0; i < spagsFunc.ParameterVariables.Count; i++)
            {
                SPAGS.Parameter spagsParam = spagsFunc.ParameterVariables[i];
                Variable p = new Variable(spagsParam.Name, GetValueTypes(spagsParam.Type));
                AddReference(spagsParam, p);
                jsFunc.Parameters.Add(p);
            }
            foreach (SPAGS.Statement statement in spagsFunc.Body.ChildStatements)
            {
                jsFunc.Body.Add(FromSPAGS(spagsFunc, statement, jsFunc.Body));
            }
            return jsFunc;
        }

        public ScopedBlock FunctionBodyFromSPAGS(SPAGS.Function spagsFunc)
        {
            ScopedBlock body = new ScopedBlock();
            foreach (SPAGS.Statement statement in spagsFunc.Body.ChildStatements)
            {
                body.Add(FromSPAGS(spagsFunc, statement, body));
            }
            if (spagsFunc.Signature.ReturnType.Category != SPAGS.ValueTypeCategory.Void
                && !spagsFunc.Body.MustReturn())
            {
                body.Add(FromSPAGS(spagsFunc, new SPAGS.Statement.Return(spagsFunc.Signature.ReturnType.CreateDefaultValueExpression()), body));
            }
            return body;
        }

        public class SpecialFunctionCall : Expression
        {
            internal SpecialFunctionCall(SpecialFunctionExpression expr, List<Expression> parameters)
            {
                funcExpr = expr;
                this.parameters = parameters;
            }
            public List<Expression> parameters;
            private SpecialFunctionExpression funcExpr;
            public override PossibleValueTypes ValueTypes
            {
                get { return funcExpr.ReturnValueTypes; }
            }
            public override void WriteTo(Writer writer)
            {
                int start = 0;
                foreach (Match match in Regex.Matches(funcExpr.Pattern, @"\{(\d+)(\:([^\}]+))?\}"))
                {
                    writer.Write(funcExpr.Pattern.Substring(start, match.Index - start));
                    start = match.Index + match.Length;
                    int num = int.Parse(match.Groups[1].Value);
                    if (num > parameters.Count)
                    {
                        throw new Exception("Not enough parameters to " + funcExpr.SPAGSFunction.Name + " (got " + parameters.Count + ")");
                    }
                    Expression newExpr = parameters[num];
                    PossibleValueTypes castTypes;
                    if (match.Groups[3].Success)
                    {
                        castTypes = (PossibleValueTypes)Enum.Parse(typeof(PossibleValueTypes), match.Groups[3].Value);
                    }
                    else
                    {
                        castTypes = funcExpr.convert.GetValueTypes(funcExpr.SPAGSFunction.Signature.Parameters[num].Type);
                    }
                    newExpr = newExpr.Cast(castTypes);
                    newExpr.WriteTo(writer);
                }
                writer.Write(funcExpr.Pattern.Substring(start));
            }
        }

        public class SpecialFunctionExpression : Expression
        {
            public SpecialFunctionExpression(SPAGSConverter convert, SPAGS.Function spagsFunc, string pattern)
                : this(convert, spagsFunc, pattern, null)
            {
            }
            public SpecialFunctionExpression(SPAGSConverter convert, SPAGS.Function spagsFunc, string pattern, MutateExpression mutate)
            {
                Match returnType = Regex.Match(pattern, @"^(.*?)(\s*\-\>\s*(\S+)\s*)$");
                if (returnType.Success)
                {
                    pattern = returnType.Groups[1].Value;
                    ReturnValueTypes = (PossibleValueTypes)Enum.Parse(typeof(PossibleValueTypes), returnType.Groups[3].Value);
                }
                else
                {
                    ReturnValueTypes = convert.GetValueTypes(spagsFunc.Signature.ReturnType);
                }
                Pattern = pattern;
                SPAGSFunction = spagsFunc;
                this.convert = convert;
                Mutate = mutate;
            }
            public readonly SPAGSConverter convert;
            public readonly PossibleValueTypes ReturnValueTypes;
            public readonly string Pattern;
            public readonly SPAGS.Function SPAGSFunction;
            public override Expression Call(IEnumerable<Expression> parameters)
            {
                List<Expression> newParameters = new List<Expression>(parameters);
                for (int i = 0; i < SPAGSFunction.ParameterVariables.Count; i++)
                {
                    if (i < newParameters.Count)
                    {
                        newParameters[i] = newParameters[i].Cast(convert.GetValueTypes(SPAGSFunction.ParameterVariables[i].Type));
                    }
                }
                SpecialFunctionCall call = new SpecialFunctionCall(this, newParameters);
                if (Mutate != null)
                {
                    return Mutate(call);
                }
                return call;
            }
            public MutateExpression Mutate;
            public override void WriteTo(Writer writer)
            {
                writer.Write("<<<" + SPAGSFunction.Name + ">>>");
            }
        }

        public bool GetSpecial(SPAGS.Function func, out Expression output)
        {
            output = null;
            switch (func.Name)
            {
                case "String::Format":
                    return false;

                case "String::IsNullOrEmpty":
                    output = new SpecialFunctionExpression(this, func, "{0} -> Boolean",
                        delegate(SpecialFunctionCall input)
                        {
                            return input.LogicallyNegate();
                        });
                    return true;
                case "String::Append":
                    output = new SpecialFunctionExpression(this, func, "{0} + {1}",
                        delegate(SpecialFunctionCall input)
                        {
                            return input.parameters[0].BinOp(Infix.Add, input.parameters[1]);
                        });
                    return true;
                case "String::AppendChar":
                    output = new SpecialFunctionExpression(this, func, "(({0}) + String.fromCharCode({1}))",
                        delegate(SpecialFunctionCall input)
                        {
                            return input.parameters[0].BinOp(Infix.Add, StandardLibraries.String.fromCharCode.Call(input.parameters[1]));
                        });
                    return true;
                case "String::CompareTo":
                    return false;
                case "String::Contains":
                    // TODO: Make sure return value type is boolean
                    output = new SpecialFunctionExpression(this, func, "(({0}).indexOf({1}) !== -1)",
                        delegate(SpecialFunctionCall input)
                        {
                            return input.parameters[0].CallMethod("indexOf", input.parameters.GetRange(1,1))
                                .BinOp(Infix.IsEqualTo, (Expression)(-1));
                        });
                    return true;
                case "String::Copy":
                    output = new SpecialFunctionExpression(this, func, "{0}");
                    return true;
                case "String::EndsWith":
                    return false;
                case "String::IndexOf":
                    output = new SpecialFunctionExpression(this, func, "({0}).indexOf({1})",
                        delegate(SpecialFunctionCall input)
                        {
                            return input.parameters[0].CallMethod("indexOf", input.parameters.GetRange(1, 1));
                        });
                    return true;
                case "String::LowerCase":
                    output = new SpecialFunctionExpression(this, func, "({0}).toLowerCase()",
                        delegate(SpecialFunctionCall input)
                        {
                            return input.parameters[0].CallMethod("toLowerCase", new List<Expression>());
                        });
                    return true;
                case "String::Replace":
                    output = new SpecialFunctionExpression(this, func, "({0}).replace(new RegExp(util.regexEscape({1}), {3} ? \"\" : \"i\"), {2})",
                        delegate(SpecialFunctionCall call)
                        {
                            Expression replaceMe;
                            int caseSensitive;
                            bool knowCaseSensitive = call.parameters[3].TryGetIntValue(out caseSensitive);
                            if (knowCaseSensitive && caseSensitive != 0)
                            {
                                replaceMe = call.parameters[1];
                            }
                            else
                            {
                                string pattern;
                                bool knowPattern = call.parameters[1].TryGetStringValue(out pattern);
                                if (knowPattern && knowCaseSensitive)
                                {
                                    replaceMe = new Expression.Custom(Util.EscapedRegexLiteral(pattern, caseSensitive == 0 ? "i" : ""));
                                }
                                else
                                {
                                    Expression.New newRegExp = new Expression.New(StandardLibraries.RegExp);
                                    newRegExp.Parameters.Add(call.parameters[1]);
                                    if (knowCaseSensitive)
                                    {
                                        newRegExp.Parameters.Add(
                                            new Expression.StringLiteral(caseSensitive == 0 ? "i" : ""));
                                    }
                                    else
                                    {
                                        newRegExp.Parameters.Add(
                                            new Expression.TernaryOperation(call.parameters[2],
                                                new Expression.StringLiteral(""),
                                                new Expression.StringLiteral("i")));
                                    }
                                    replaceMe = newRegExp;
                                }
                            }
                            List<Expression> parameters = new List<Expression>();
                            parameters.Add(replaceMe);
                            parameters.Add(call.parameters[2]);
                            return call.parameters[0].CallMethod("replace", parameters);
                        });
                    return true;
                case "String::ReplaceCharAt":
                    return false;
                case "String::StartsWith":
                    return false;
                case "String::Substring":
                    output = new SpecialFunctionExpression(this, func, "({0}).substr({1}, {2})",
                        delegate(SpecialFunctionCall input)
                        {
                            return input.parameters[0].CallMethod("substr", input.parameters.GetRange(1,2));
                        });
                    return true;
                case "String::Truncate":
                    output = new SpecialFunctionExpression(this, func, "({0}).substr(0, {1})",
                        delegate(SpecialFunctionCall input)
                        {
                            List<Expression> parameters = new List<Expression>();
                            parameters.Add((Expression)0);
                            parameters.Add(input.parameters[1]);
                            return input.parameters[0].CallMethod("substr", parameters);
                        });
                    return true;
                case "String::UpperCase":
                    output = new SpecialFunctionExpression(this, func, "({0}).toUpperCase()",
                        delegate(SpecialFunctionCall input)
                        {
                            return input.parameters[0].CallMethod("toUpperCase", new List<Expression>());
                        });
                    return true;
                case "String::get_AsFloat":
                    output = new SpecialFunctionExpression(this, func, "parseFloat({0})",
                        delegate(SpecialFunctionCall input)
                        {
                            return input.BinOp(Infix.LogicalOr, (Expression)0);
                        });
                    return true;
                case "String::get_AsInt":
                    output = new SpecialFunctionExpression(this, func, "parseInt({0}, 10)",
                        delegate(SpecialFunctionCall input)
                        {
                            return input.BinOp(Infix.LogicalOr, (Expression)0);
                        });
                    return true;
                case "String::geti_Chars":
                    output = new SpecialFunctionExpression(this, func, "({0}).charCodeAt({1})",
                        delegate(SpecialFunctionCall input)
                        {
                            return input.parameters[0]
                                .CallMethod("charCodeAt", input.parameters.GetRange(1, 1))
                                .BinOp(Infix.LogicalOr, (Expression)0);
                        });
                    return true;
                case "String::get_Length":
                    output = new SpecialFunctionExpression(this, func, "({0}).length",
                        delegate(SpecialFunctionCall input)
                        {
                            return input.parameters[0]
                                .Index("length", PossibleValueTypes.Int32);
                        });
                    return true;

                case "ViewFrame::get_Frame":
                    return false;
                case "ViewFrame::get_LinkedAudio":
                    return false;
                case "ViewFrame::get_Loop":
                    return false;
                case "ViewFrame::get_View":
                    return false;

                case "DrawingSurface::CreateCopy":
                    return false;
                case "DrawingSurface::Release":
                    return false;

                case "Room::GetTextProperty":
                    return false;
                case "Room::GetDrawingSurfaceForBackground":
                    return false;
                case "Room::get_BottomEdge":
                    return false;
                case "Room::get_ColorDepth":
                    return false;
                case "Room::get_Height":
                    return false;
                case "Room::get_LeftEdge":
                    return false;
                case "Room::geti_Messages":
                    return false;
                case "Room::get_MusicOnLoad":
                    return false;
                case "Room::get_ObjectCount":
                    return false;
                case "Room::get_RightEdge":
                    return false;
                case "Room::get_TopEdge":
                    return false;
                case "Room::get_Width":
                    return false;

                case "Game::ChangeTranslation":
                    return false;
                case "Game::DoOnceOnly":
                    return false;
                case "Game::GetColorFromRGB":
                    return false;
                case "Game::GetFrameCountForLoop":
                    return false;
                case "Game::GetLocationName":
                    return false;
                case "Game::GetLoopCountForView":
                    return false;
                case "Game::GetMODPattern":
                    return false;
                case "Game::GetRunNextSettingForLoop":
                    return false;
                case "Game::GetSaveSlotDescription":
                    return false;
                case "Game::GetViewFrame":
                    return false;
                case "Game::InputBox":
                    return false;
                case "Game::IsAudioPlaying":
                    return false;
                case "Game::SetSaveGameDirectory":
                    return false;
                case "Game::StopAudio":
                    return false;
                case "Game::StopSound":
                    return false;
                case "Game::get_CharacterCount":
                    return false;
                case "Game::get_DialogCount":
                    return false;
                case "Game::get_FileName":
                    return false;
                case "Game::get_FontCount":
                    return false;
                case "Game::geti_GlobalMessages":
                    return false;
                case "Game::geti_GlobalStrings":
                    return false;
                case "Game::get_GUICount":
                    return false;
                case "Game::get_IgnoreUserInputAfterTextTimeoutMs":
                    return false;
                case "Game::set_IgnoreUserInputAfterTextTimeoutMs":
                    return false;
                case "Game::get_InventoryItemCount":
                    return false;
                case "Game::get_MinimumTextDisplayTimeMs":
                    return false;
                case "Game::set_MinimumTextDisplayTimeMs":
                    return false;
                case "Game::get_MouseCursorCount":
                    return false;
                case "Game::get_Name":
                    return false;
                case "Game::set_Name":
                    return false;
                case "Game::get_NormalFont":
                    return false;
                case "Game::set_NormalFont":
                    return false;
                case "Game::get_SpeechFont":
                    return false;
                case "Game::set_SpeechFont":
                    return false;
                case "Game::geti_SpriteHeight":
                    return false;
                case "Game::geti_SpriteWidth":
                    return false;
                case "Game::get_TextReadingSpeech":
                    return false;
                case "Game::set_TextReadingSpeech":
                    return false;
                case "Game::get_TranslationFilename":
                    return false;
                case "Game::get_ViewCount":
                    return false;

                case "Parser::FindWordID":
                    return false;
                case "Parser::ParseText":
                    return false;
                case "Parser::Said":
                    return false;
                case "Parser::SaidUnknownWord":
                    return false;

                case "Display":
                    return false;
                case "DisplayAt":
                    return false;
                case "DisplayAtY":
                    return false;
                case "DisplayMessage":
                    return false;
                case "DisplayMessageAtY":
                    return false;
                case "DisplayTopBar":
                    return false;
                case "DisplayMessageBar":
                    return false;
                case "ResetRoom":
                    return false;
                case "HasPlayerBeenInRoom":
                    return false;
                case "ProcessClick":
                    return false;
                case "AbortGame":
                    return false;
                case "QuitGame":
                    return false;
                case "SetGameSpeed":
                    return false;
                case "GetGameSpeed":
                    output = new SpecialFunctionExpression(this, func, "engine.GetGameSpeed() -> Int16");
                    return true;
                case "SetGameOption":
                    return false;
                case "GetGameOption":
                    return false;
                case "Debug":
                    return false;
                case "CallRoomScript":
                    return false;
                case "RunAGSGame":
                    return false;
                case "GetTranslation":
                    return false;
                case "IsTranslationAvailable":
                    return false;
                case "RestoreGameDialog":
                    return false;
                case "SaveGameDialog":
                    return false;
                case "RestartGame":
                    return false;
                case "SaveGameSlot":
                    return false;
                case "RestoreGameSlot":
                    return false;
                case "DeleteSaveSlot":
                    return false;
                case "SetRestartPoint":
                    return false;
                case "GetLocationType":
                    return false;
                case "GetWalkableAreaAt":
                    return false;
                case "GetScalingAt":
                    return false;
                case "GetRoomProperty":
                    return false;
                case "SetViewport":
                    return false;
                case "ReleaseViewport":
                    return false;
                case "GetViewportX":
                    return false;
                case "GetViewportY":
                    return false;
                case "IsGamePaused":
                    return false;
                case "GetGraphicalVariable":
                    return false;
                case "SetGraphicalVariable":
                    return false;
                case "DisableInterface":
                    return false;
                case "EnableInterface":
                    return false;
                case "IsInterfaceEnabled":
                    return false;

                case "Mouse::ChangeModeGraphic":
                    return false;
                case "Mouse::ChangeModeHotspot":
                    return false;
                case "Mouse::ChangeModeView":
                    return false;
                case "Mouse::DisableMode":
                    return false;
                case "Mouse::EnableMode":
                    return false;
                case "Mouse::GetModeGraphic":
                    return false;
                case "Mouse::IsButtonDown":
                    return false;
                case "Mouse::SaveCursorUntilItLeaves":
                    return false;
                case "Mouse::SelectNextMode":
                    return false;
                case "Mouse::SetBounds":
                    return false;
                case "Mouse::SetPosition":
                    return false;
                case "Mouse::Update":
                    return false;
                case "Mouse::UseDefaultGraphic":
                    return false;
                case "Mouse::UseModeGraphic":
                    return false;
                case "Mouse::get_Mode":
                    return false;
                case "Mouse::set_Mode":
                    return false;

                case "SetGlobalString":
                    return false;
                case "GetGlobalString":
                    return false;
                case "InputBox":
                    return false;
                case "GetTranslationName":
                    return false;
                case "GetSaveSlotDescription":
                    return false;
                case "GetLocationName":
                    return false;
                case "GetRoomPropertyText":
                    return false;
                case "StrCat":
                    return false;
                case "StrCaseComp":
                    return false;
                case "StrComp":
                    return false;
                case "StrCopy":
                    return false;
                case "StrFormat":
                    return false;
                case "StrLen":
                    return false;
                case "StrGetCharAt":
                    return false;
                case "StrSetCharAt":
                    return false;
                case "StrToLowerCase":
                    return false;
                case "StrToUpperCase":
                    return false;
                case "StrContains":
                    return false;
                case "ParseText":
                    return false;
                case "SaidUnknownWord":
                    return false;
                case "GetMessageText":
                    return false;
                case "StringToInt":
                    return false;
                case "Said":
                    return false;

                case "GetHotspotAt":
                    return false;
                case "GetObjectAt":
                    return false;
                case "GetCharacterAt":
                    return false;
                case "GetRegionAt":
                    return false;
                case "GetInvAt":
                    return false;

                case "GetGraphicalOverlay":
                    return false;
                case "CreateTextOverlay":
                    return false;
                case "SetTextOverlay":
                    return false;
                case "RemoveOverlay":
                    return false;
                case "MoveOverlay":
                    return false;
                case "IsOverlayValid":
                    return false;

                case "InventoryScreen":
                    return false;
                case "ChangeCursorGraphic":
                    return false;
                case "ChangeCursorHotspot":
                    return false;
                case "GetCursorMode":
                    return false;
                case "SetCursorMode":
                    return false;
                case "SetNextCursorMode":
                    return false;
                case "SetDefaultCursor":
                    return false;
                case "SetMouseCursor":
                    return false;
                case "SetMouseBounds":
                    return false;
                case "SetMousePosition":
                    return false;
                case "ShowMouseCursor":
                    return false;
                case "HideMouseCursor":
                    return false;
                case "RefreshMouse":
                    return false;
                case "DisableCursorMode":
                    return false;
                case "EnableCursorMode":
                    return false;
                case "SaveCursorForLocationChange":
                    return false;
                case "IsButtonDown":
                    return false;

                case "MergeObject":
                    return false;
                case "SetObjectTint":
                    return false;
                case "RemoveObjectTint":
                    return false;
                case "StopObjectMoving":
                    return false;
                case "RunObjectInteraction":
                    return false;
                case "GetObjectProperty":
                    return false;
                case "GetObjectPropertyText":
                    return false;
                case "AnimateObject":
                    return false;
                case "AnimateObjectEx":
                    return false;
                case "ObjectOff":
                    return false;
                case "ObjectOn":
                    return false;
                case "SetObjectBaseline":
                    return false;
                case "GetObjectBaseline":
                    return false;
                case "SetObjectFrame":
                    return false;
                case "SetObjectGraphic":
                    return false;
                case "SetObjectView":
                    return false;
                case "SetObjectTransparency":
                    return false;
                case "MoveObject":
                    return false;
                case "MoveObjectDirect":
                    return false;
                case "SetObjectPosition":
                    return false;
                case "AreObjectsColliding":
                    return false;
                case "GetObjectName":
                    return false;
                case "GetObjectX":
                    return false;
                case "GetObjectY":
                    return false;
                case "GetObjectGraphic":
                    return false;
                case "IsObjectAnimating":
                    return false;
                case "IsObjectMoving":
                    return false;
                case "IsObjectOn":
                    return false;
                case "SetObjectClickable":
                    return false;
                case "SetObjectIgnoreWalkbehinds":
                    return false;

                case "AddInventory":
                    return false;
                case "LoseInventory":
                    return false;
                case "SetActiveInventory":
                    return false;
                case "NewRoom":
                    return false;
                case "NewRoomEx":
                    return false;
                case "NewRoomNPC":
                    return false;
                case "GetCharacterProperty":
                    return false;
                case "GetCharacterPropertyText":
                    return false;
                case "RunCharacterInteraction":
                    return false;
                case "DisplaySpeech":
                    return false;
                case "DisplaySpeechBackground":
                    return false;
                case "DisplaySpeechAt":
                    return false;
                case "DisplayThought":
                    return false;
                case "FollowCharacter":
                    return false;
                case "FollowCharacterEx":
                    return false;
                case "SetPlayerCharacter":
                    return false;
                case "AddInventoryToCharacter":
                    return false;
                case "LoseInventoryFromCharacter":
                    return false;
                case "AnimateCharacter":
                    return false;
                case "AnimateCharacterEx":
                    return false;
                case "MoveCharacter":
                    return false;
                case "MoveCharacterDirect":
                    return false;
                case "MoveCharacterPath":
                    return false;
                case "MoveCharacterStraight":
                    return false;
                case "MoveCharacterToHotspot":
                    return false;
                case "MoveCharacterToObject":
                    return false;
                case "MoveCharacterBlocking":
                    return false;
                case "MoveToWalkableArea":
                    return false;
                case "FaceCharacter":
                    return false;
                case "FaceLocation":
                    return false;
                case "SetCharacterView":
                    return false;
                case "SetCharacterViewEx":
                    return false;
                case "SetCharacterViewOffset":
                    return false;
                case "SetCharacterFrame":
                    return false;
                case "ReleaseCharacterView":
                    return false;
                case "ChangeCharacterView":
                    return false;
                case "SetCharacterSpeechView":
                    return false;
                case "SetCharacterBlinkView":
                    return false;
                case "SetCharacterIdle":
                    return false;
                case "StopMoving":
                    return false;
                case "AreCharObjColliding":
                    return false;
                case "AreCharactersColliding":
                    return false;
                case "SetCharacterSpeed":
                    return false;
                case "SetCharacterSpeedEx":
                    return false;
                case "SetTalkingColor":
                    return false;
                case "SetCharacterTransparency":
                    return false;
                case "SetCharacterClickable":
                    return false;
                case "SetCharacterBaseline":
                    return false;
                case "SetCharacterIgnoreLight":
                    return false;
                case "SetCharacterIgnoreWalkbehinds":
                    return false;
                case "SetCharacterProperty":
                    return false;
                case "GetPropertyCharacter":
                    return false;

                case "FileOpen":
                    return false;
                case "FileWrite":
                    return false;
                case "FileWriteRawLine":
                    return false;
                case "FileRead":
                    return false;
                case "FileClose":
                    return false;
                case "FileWriteInt":
                    return false;
                case "FileReadInt":
                    return false;
                case "FileReadRawChar":
                    return false;
                case "FileWriteRawChar":
                    return false;
                case "FileReadRawInt":
                    return false;
                case "FileIsEOF":
                    return false;
                case "FileIsError":
                    return false;

                case "DisableHotspot":
                    return false;
                case "EnableHotspot":
                    return false;
                case "GetHotspotName":
                    return false;
                case "GetHotspotPointX":
                    return false;
                case "GetHotspotPointY":
                    return false;
                case "GetHotspotProperty":
                    return false;
                case "GetHotspotPropertyText":
                    return false;
                case "RunHotspotInteraction":
                    return false;
                case "DisableRegion":
                    return false;
                case "EnableRegion":
                    return false;
                case "RunRegionInteraction":
                    return false;
                case "SetAreaLightLevel":
                    return false;
                case "SetRegionTint":
                    return false;

                case "GetInvProperty":
                    return false;
                case "GetInvPropertyText":
                    return false;
                case "GetInvName":
                    return false;
                case "GetInvGraphic":
                    return false;
                case "SetInvItemPic":
                    return false;
                case "SetInvItemName":
                    return false;
                case "IsInventoryInteractionAvailable":
                    return false;
                case "RunInventoryInteraction":
                    return false;

                case "GetTime":
                    return false;
                case "GetRawTime":
                    return false;

                case "LoadSaveSlotDescription":
                    return false;
                case "LoadImageFile":
                    return false;
                case "DeleteSprite":
                    return false;

                case "SetSpeechFont":
                    return false;
                case "SetNormalFont":
                    return false;

                case "GetGameParameter":
                    return false;
                case "SetDialogOption":
                    return false;
                case "GetDialogOption":
                    return false;
                case "RunDialog":
                    return false;

                case "RawClearScreen":
                    return false;
                case "RawDrawCircle":
                    return false;
                case "RawDrawImage":
                    return false;
                case "RawDrawImageOffset":
                    return false;
                case "RawDrawImageResized":
                    return false;
                case "RawDrawImageTransparent":
                    return false;
                case "RawDrawLine":
                    return false;
                case "RawDrawRectangle":
                    return false;
                case "RawDrawTriangle":
                    return false;
                case "RawPrint":
                    return false;
                case "RawPrintMessageWrapped":
                    return false;
                case "RawSetColor":
                    return false;
                case "RawSetColorRGB":
                    return false;
                case "RawDrawFrameTransparent":
                    return false;
                case "RawSaveScreen":
                    return false;
                case "RawRestoreScreen":
                    return false;

                case "GetTextWidth":
                    return false;
                case "GetTextHeight":
                    return false;
                case "GiveScore":
                    return false;
                case "UpdateInventory":
                    return false;
                case "StopDialog":
                    return false;
                case "AreThingsOverlapping":
                    return false;
                case "SetVoiceMode":
                    return false;
                case "SetSkipSpeech":
                    return false;
                case "SetSpeechStyle":
                    return false;
                case "SetTimer":
                    return false;
                case "IsTimerExpired":
                    return false;
                case "SetMultitaskingMode":
                    return false;
                case "FloatToInt":
                    output = new SpecialFunctionExpression(this, func, "engine.FloatToInt({0}, {1})",
                        delegate(SpecialFunctionCall call)
                        {
                            int mode;
                            if (call.parameters[1].TryGetIntValue(out mode))
                            {
                                switch (mode)
                                {
                                    case 0: return call.parameters[0].Cast(PossibleValueTypes.Int32);
                                    case 1: return StandardLibraries.Math.round.Call(call.parameters.GetRange(0,1))
                                        .Cast(PossibleValueTypes.Int32);
                                    case 2: return StandardLibraries.Math.ceil.Call(call.parameters.GetRange(0, 1))
                                        .Cast(PossibleValueTypes.Int32);
                                }
                            }
                            return call;
                        });
                    return true;
                case "IntToFloat":
                    output = new SpecialFunctionExpression(this, func, "{0}");
                    return true;

                case "File::Delete":
                    return false;
                case "File::Exists":
                    return false;
                case "File::Open":
                    return false;
                case "File::Close":
                    return false;
                case "File::ReadInt":
                    return false;
                case "File::ReadRawChar":
                    return false;
                case "File::ReadRawInt":
                    return false;
                case "File::ReadRawLine":
                    return false;
                case "File::ReadString":
                    return false;
                case "File::ReadRawLineBack":
                    return false;
                case "File::ReadStringBack":
                    return false;
                case "File::WriteInt":
                    return false;
                case "File::WriteRawChar":
                    return false;
                case "File::WriteRawLine":
                    return false;
                case "File::WriteString":
                    return false;
                case "Game::get_InSkippableCutscene":
                case "Game::get_SkippingCutscene":
                case "Game::get_UseNativeCoordinates":
                case "Mouse::get_Visible":
                case "Mouse::set_Visible":
                    return false;

                case "InventoryItem::GetAtScreenXY":
                    return false;
                case "InventoryItem::GetProperty":
                    return false;
                case "InventoryItem::GetTextProperty":
                    return false;
                case "InventoryItem::IsInteractionAvailable":
                    return false;
                case "InventoryItem::RunInteraction":
                    return false;
                case "InventoryItem::GetName":
                    return false;
                case "InventoryItem::GetPropertyText":
                    return false;
                case "InventoryItem::SetName":
                    return false;

                case "Overlay::CreateGraphical":
                    return false;
                case "Overlay::CreateTextual":
                    return false;
                case "Overlay::SetText":
                    return false;
                case "Overlay::Remove":
                    return false;

                case "DynamicSprite::Create":
                    return false;
                case "DynamicSprite::CreateFromBackground":
                    return false;
                case "DynamicSprite::CreateFromDrawingSurface":
                    return false;
                case "DynamicSprite::CreateFromExistingSprite":
                    return false;
                case "DynamicSprite::CreateFromFile":
                    return false;
                case "DynamicSprite::CreateFromSaveGame":
                    return false;
                case "DynamicSprite::CreateFromScreenShot":
                    return false;
                case "DynamicSprite::ChangeCanvasSize":
                    return false;
                case "DynamicSprite::CopyTransparencyMask":
                    return false;
                case "DynamicSprite::Crop":
                    return false;
                case "DynamicSprite::Delete":
                    return false;
                case "DynamicSprite::Flip":
                    return false;
                case "DynamicSprite::GetDrawingSurface":
                    return false;
                case "DynamicSprite::SaveToFile":
                    return false;

                case "DynamicSprite::Resize":
                case "DynamicSprite::Rotate":
                case "DynamicSprite::Tint":
                case "DrawingSurface::DrawImage":
                case "DrawingSurface::Clear":
                case "DrawingSurface::DrawCircle":
                case "DrawingSurface::DrawLine":
                case "DrawingSurface::DrawMessageWrapped":
                case "DrawingSurface::DrawPixel":
                case "DrawingSurface::DrawRectangle":
                case "DrawingSurface::DrawString":
                case "DrawingSurface::DrawStringWrapped":
                case "DrawingSurface::DrawSurface":
                case "DrawingSurface::DrawTriangle":
                case "DrawingSurface::GetPixel":
                    {
                        string methodName = Regex.Match(func.Name, @"[^:]+$").Value;
                        methodName = methodName.Substring(0, 1).ToLower() + methodName.Substring(1);
                        string pattern = "{0}." + methodName + "(";
                        for (int i = 1; i < func.Signature.Parameters.Count; i++)
                        {
                            if (i > 1) pattern += ", ";
                            pattern += "{" + i + "}";
                        }
                        pattern += ")";
                        output = new SpecialFunctionExpression(this, func, pattern);
                        return true;
                    }

                // int IDs
                case "Hotspot::get_ID":
                case "Region::get_ID":
                case "InventoryItem::get_ID":
                case "Dialog::get_ID":
                case "GUI::get_ID":
                case "AudioChannel::get_ID":
                case "Object::get_ID":
                case "Character::get_ID":
                case "GUIControl::get_ID":
                    {
                        output = new SpecialFunctionExpression(this, func, "{0}.number");
                        return true;
                    }

                case "Label::get_Text":
                case "DynamicSprite::get_Graphic":
                case "DynamicSprite::get_Height":
                case "DynamicSprite::get_Width":
                case "GUI::get_Height":
                case "ViewFrame::get_Graphic":
                case "ViewFrame::get_Sound":
                case "ViewFrame::get_Speed":
                case "DrawingSurface::get_DrawingColor":
                case "DrawingSurface::get_Height":
                case "DrawingSurface::get_Width":
                case "InventoryItem::get_CursorGraphic":
                case "InventoryItem::get_Graphic":
                case "InventoryItem::get_Name":
                case "Overlay::get_X":
                case "Overlay::get_Y":
                case "DynamicSprite::get_ColorDepth":
                case "GUIControl::get_Height":
                case "GUIControl::get_Width":
                case "GUIControl::get_X":
                case "GUIControl::get_Y":
                case "Label::get_Font":
                case "Label::get_TextColor":
                case "Button::get_Font":
                case "Button::get_Graphic":
                case "Button::get_MouseOverGraphic":
                case "Button::get_NormalGraphic":
                case "Button::get_PushedGraphic":
                case "Button::get_TextColor":
                case "Button::get_Text":
                case "Slider::get_BackgroundGraphic":
                case "Slider::get_HandleGraphic":
                case "Slider::get_HandleOffset":
                case "Slider::get_Max":
                case "Slider::get_Min":
                case "Slider::get_Value":
                case "TextBox::get_Font":
                case "TextBox::get_Text":
                case "TextBox::get_TextColor":
                case "InvWindow::get_CharacterToUse":
                case "InvWindow::get_ItemHeight":
                case "InvWindow::get_ItemWidth":
                case "InvWindow::get_TopItem":
                case "ListBox::get_Font":
                case "ListBox::get_SelectedIndex":
                case "ListBox::get_TopItem":
                case "GUI::get_BackgroundGraphic":
                case "GUI::get_Transparency":
                case "GUI::get_Width":
                case "GUI::get_X":
                case "GUI::get_Y":
                case "GUI:get_ZOrder":
                case "Hotspot::get_Name":
                case "Hotspot::get_WalkToX":
                case "Hotspot::get_WalkToY":
                case "Region::get_LightLevel":
                case "Region::get_TintBlue":
                case "Region::get_TintGreen":
                case "Region::get_TintRed":
                case "Region::get_TintSaturation":
                case "DialogOptionsRenderingInfo::get_ActiveOptionID":
                case "DialogOptionsRenderingInfo::get_DialogToRender":
                case "DialogOptionsRenderingInfo::get_Height":
                case "DialogOptionsRenderingInfo::get_ParserTextBoxWidth":
                case "DialogOptionsRenderingInfo::get_ParserTextBoxX":
                case "DialogOptionsRenderingInfo::get_ParserTextBoxY":
                case "DialogOptionsRenderingInfo::get_Surface":
                case "DialogOptionsRenderingInfo::get_Width":
                case "DialogOptionsRenderingInfo::get_X":
                case "DialogOptionsRenderingInfo::get_Y":
                case "AudioChannel::get_Panning":
                case "AudioChannel::get_PlayingClip":
                case "AudioClip::get_FileType":
                case "AudioClip::get_Type":
                case "Object::get_Baseline":
                case "Object::get_BlockingHeight":
                case "Object::get_Frame":
                case "Object::get_Graphic":
                case "Object::get_Loop":
                case "Object::get_Name":
                case "Object::get_Transparency":
                case "Object::get_View":
                case "Object::get_X":
                case "Object::get_Y":
                case "Character::get_ActiveInventory":
                case "Character::get_AnimationSpeed":
                case "Character::get_Baseline":
                case "Character::get_BlinkInterval":
                case "Character::get_BlinkView":
                case "Character::get_BlockingHeight":
                case "Character::get_BlockingWidth":
                case "Character::get_Frame":
                case "Character::get_IdleView":
                case "Character::get_Loop":
                case "Character::get_Name":
                case "Character::get_NormalView":
                case "Character::get_PreviousRoom":
                case "Character::get_Room":
                case "Character::get_ScaleMoveSpeed":
                case "Character::get_Scaling":
                case "Character::get_SpeakingFrame":
                case "Character::get_SpeechAnimationDelay":
                case "Character::get_SpeechColor":
                case "Character::get_SpeechView":
                case "Character::get_ThinkView":
                case "Character::get_Transparency":
                case "Character::get_View":
                case "Character::get_WalkSpeedX":
                case "Character::get_WalkSpeedY":
                case "Character::get_x":
                case "Character::get_y":
                case "Character::get_z":
                    {
                        string getAttrName = Regex.Match(func.Name, @"::get_(.+)$").Groups[1].Value;
                        getAttrName = getAttrName.Substring(0, 1).ToLower() + getAttrName.Substring(1);
                        output = new SpecialFunctionExpression(this, func, "{0}." + getAttrName);
                        return true;
                    }

                case "ViewFrame::set_Graphic":
                case "ViewFrame::set_LinkedAudio":
                case "ViewFrame::set_Sound":
                case "DrawingSurface::set_DrawingColor":
                case "InventoryItem::set_CursorGraphic":
                case "InventoryItem::set_Graphic":
                case "InventoryItem::set_Name":
                case "Overlay::set_X":
                case "Overlay::set_Y":
                case "Label::set_Text":
                case "GUIControl::set_Height":
                case "GUIControl::set_Width":
                case "GUIControl::set_X":
                case "GUIControl::set_Y":
                case "Label::set_Font":
                case "Label::set_TextColor":
                case "Character::set_Name":
                case "Object::set_X":
                case "Object::set_Transparency":
                case "DialogOptionsRenderingInfo::set_Height":
                case "GUI::set_Y":
                case "GUI::set_X":
                case "ListBox::set_TopItem":
                case "Slider::set_BackgroundGraphic":
                case "Button::set_NormalGraphic":
                case "Button::set_Font":
                case "Button::set_MouseOverGraphic":
                case "Button::set_PushedGraphic":
                case "Button::set_TextColor":
                case "Button::set_Text":
                case "Slider::set_HandleGraphic":
                case "Slider::set_HandleOffset":
                case "Slider::set_Max":
                case "Slider::set_Min":
                case "Slider::set_Value":
                case "TextBox::set_Font":
                case "TextBox::set_Text":
                case "TextBox::set_TextColor":
                case "InvWindow::set_CharacterToUse":
                case "InvWindow::set_ItemHeight":
                case "InvWindow::set_ItemWidth":
                case "InvWindow::set_TopItem":
                case "ListBox::set_Font":
                case "ListBox::set_SelectedIndex":
                case "GUI::set_BackgroundGraphic":
                case "GUI::set_Transparency":
                case "GUI::set_Width":
                case "GUI::set_ZOrder":
                case "Region::set_LightLevel":
                case "DialogOptionsRenderingInfo::set_ActiveOptionID":
                case "DialogOptionsRenderingInfo::set_ParserTextBoxWidth":
                case "DialogOptionsRenderingInfo::set_ParserTextBoxX":
                case "DialogOptionsRenderingInfo::set_ParserTextBoxY":
                case "DialogOptionsRenderingInfo::set_Width":
                case "DialogOptionsRenderingInfo::set_X":
                case "DialogOptionsRenderingInfo::set_Y":
                case "AudioChannel::set_Panning":
                case "AudioChannel::set_Volume":
                case "Object::set_Baseline":
                case "Object::set_BlockingHeight":
                case "Object::set_Graphic":
                case "Object::set_Y":
                case "Character::set_ActiveInventory":
                case "Character::set_AnimationSpeed":
                case "Character::set_Baseline":
                case "Character::set_BlinkInterval":
                case "Character::set_BlinkView":
                case "Character::set_BlockingHeight":
                case "Character::set_BlockingWidth":
                case "Character::set_Frame":
                case "Character::set_ScaleMoveSpeed":
                case "Character::set_Scaling":
                case "Character::set_SpeechAnimationDelay":
                case "Character::set_SpeechColor":
                case "Character::set_SpeechView":
                case "Character::set_ThinkView":
                case "Character::set_Transparency":
                case "Character::set_x":
                case "Character::set_y":
                case "Character::set_z":
                    {
                        string setAttrName = Regex.Match(func.Name, @"::set_(.+)$").Groups[1].Value;
                        setAttrName = setAttrName.Substring(0, 1).ToLower() + setAttrName.Substring(1);
                        output = new SpecialFunctionExpression(this, func, "{0}.set(\"" + setAttrName + "\", {1})");
                        return true;
                    }


                case "FadeIn":
                    return false;
                case "FadeOut":
                    return false;
                case "CyclePalette":
                    return false;
                case "SetPalRGB":
                    return false;
                case "UpdatePalette":
                    return false;
                case "TintScreen":
                    return false;
                case "SetAmbientTint":
                    return false;

                case "Random":
                    output = new SpecialFunctionExpression(this, func, "(Math.random()*({0}+1)|0)",
                        delegate(SpecialFunctionCall call)
                        {
                            return StandardLibraries.Math.random.Call().BinOp(Infix.Multiply,
                                call.parameters[0].BinOp(Infix.Add, (Expression)1)
                                    .Cast(PossibleValueTypes.Int32))
                                        .Cast(PossibleValueTypes.Int32);
                        });
                    return true;

                case "SetBackgroundFrame":
                    return false;
                case "GetBackgroundFrame":
                    return false;
                case "ShakeScreen":
                    return false;
                case "ShakeScreenBackground":
                    return false;
                case "SetScreenTransition":
                    return false;
                case "SetNextScreenTransition":
                    return false;
                case "SetFadeColor":
                    return false;
                case "IsInteractionAvailable":
                    return false;
                case "RemoveWalkableArea":
                    return false;
                case "RestoreWalkableArea":
                    return false;
                case "SetAreaScaling":
                    return false;
                case "DisableGroundLevelAreas":
                    return false;
                case "EnableGroundLevelAreas":
                    return false;
                case "SetWalkBehindBase":
                    return false;
                case "CDAudio":
                    return false;
                case "PlayFlic":
                    return false;
                case "PlayVideo":
                    return false;

                case "PlayMusic":
                    return false;
                case "PlayMusicQueued":
                    return false;
                case "PlaySilentMIDI":
                    return false;
                case "PlayMP3File":
                    return false;
                case "PlaySound":
                    return false;
                case "PlaySoundEx":
                    return false;
                case "PlayAmbientSound":
                    return false;
                case "StopAmbientSound":
                    return false;
                case "GetCurrentMusic":
                    return false;
                case "SetMusicRepeat":
                    return false;
                case "SetMusicVolume":
                    return false;
                case "SetSoundVolume":
                    return false;
                case "SetMusicMasterVolume":
                    return false;
                case "SetDigitalMasterVolume":
                    return false;
                case "SeekMODPattern":
                    return false;
                case "IsChannelPlaying":
                    return false;
                case "IsSoundPlaying":
                    return false;
                case "IsMusicPlaying":
                    return false;
                case "GetMIDIPosition":
                    return false;
                case "SeekMIDIPosition":
                    return false;
                case "GetMP3PosMillis":
                    return false;
                case "SeekMP3PosMillis":
                    return false;
                case "SetChannelVolume":
                    return false;
                case "StopChannel":
                    return false;
                case "StopMusic":
                    return false;

                case "IsVoxAvailable":
                    return false;
                case "SetSpeechVolume":
                    return false;
                case "IsMusicVoxAvailable":
                    return false;
                case "SaveScreenshot":
                    return false;
                case "PauseGame":
                    return false;
                case "UnPauseGame":
                    return false;

                case "Wait":
                    return false;
                case "WaitKey":
                    return false;
                case "WaitMouseKey":
                    return false;
                case "IsKeyPressed":
                    return false;
                case "SetGlobalInt":
                    return false;
                case "GetGlobalInt":
                    return false;
                case "FlipScreen":
                    return false;
                case "SkipUntilCharacterStops":
                    return false;
                case "StartCutscene":
                    return false;
                case "EndCutscene":
                    return false;
                case "ClaimEvent":
                    return false;
                case "SetTextWindowGUI":
                    return false;
                case "FindGUIID":
                    return false;
                    
                case "SetInvDimensions":
                    return false;
                case "GetGUIAt":
                    return false;
                case "GetGUIObjectAt":
                    return false;
                case "InterfaceOn":
                    return false;
                case "InterfaceOff":
                    return false;
                case "SetGUIPosition":
                    return false;
                case "SetGUISize":
                    return false;
                case "CentreGUI":
                    return false;
                case "IsGUIOn":
                    return false;
                case "SetGUIBackgroundPic":
                    return false;
                case "SetGUITransparency":
                    return false;
                case "SetGUIClickable":
                    return false;
                case "SetGUIZOrder":
                    return false;

                case "SetGUIObjectEnabled":
                    return false;
                case "SetGUIObjectPosition":
                    return false;
                case "SetGUIObjectSize":
                    return false;
                case "SetLabelColor":
                    return false;
                case "SetLabelText":
                    return false;
                case "SetLabelFont":
                    return false;
                case "SetButtonText":
                    return false;
                case "SetButtonPic":
                    return false;
                case "GetButtonPic":
                    return false;
                case "AnimateButton":
                    return false;
                case "SetSliderValue":
                    return false;
                case "GetSliderValue":
                    return false;
                case "SetTextBoxFont":
                    return false;
                case "GetTextBoxText":
                    return false;
                case "SetTextBoxText":
                    return false;
                case "ListBoxClear":
                    return false;
                case "ListBoxAdd":
                    return false;
                case "ListBoxGetSelected":
                    return false;
                case "ListBoxGetItemText":
                    return false;
                case "ListBoxSetSelected":
                    return false;
                case "ListBoxSetTopItem":
                    return false;
                case "ListBoxDirList":
                    return false;
                case "ListBoxGetNumItems":
                    return false;
                case "ListBoxSaveGameList":
                    return false;
                case "ListBoxRemove":
                    return false;

                case "SetFrameSound":
                    return false;
                    
                case "GUIControl::BringToFront":
                    return false;
                case "GUIControl::GetAtScreenXY":
                    return false;
                case "GUIControl::SendToBack":
                    return false;
                case "GUIControl::SetPosition":
                    return false;
                case "GUIControl::SetSize":
                    return false;
                case "GUIControl::get_AsButton":
                    return false;
                case "GUIControl::get_AsWindow":
                    return false;
                case "GUIControl::get_AsLabel":
                    return false;
                case "GUIControl::get_AsListBox":
                    return false;
                case "GUIControl::get_AsSlider":
                    return false;
                case "GUIControl::get_AsTextBox":
                    return false;
                case "GUIControl::get_OwningGUI":
                    return false;

                case "Label::GetText":
                    return false;
                case "Label::SetText":
                    return false;

                case "Button::Animate":
                    return false;
                case "Button::GetText":
                    return false;
                case "Button::SetText":
                    return false;

                case "TextBox::GetText":
                    return false;
                case "TextBox::SetText":
                    return false;

                case "InvWindow::ScrollUp":
                    return false;
                case "InvWindow::ScrollDown":
                    return false;
                case "InvWindow::geti_ItemAtIndex":
                    return false;
                case "InvWindow::get_ItemsPerRow":
                case "InvWindow::get_RowCount":
                case "InvWindow::get_ItemCount":
                    return false;

                case "ListBox::AddItem":
                    return false;
                case "ListBox::Clear":
                    return false;
                case "ListBox::FillDirList":
                    return false;
                case "ListBox::FillSaveGameList":
                    return false;
                case "ListBox::GetItemAtLocation":
                    return false;
                case "ListBox::GetItemText":
                    return false;
                case "ListBox::SetItemText":
                    return false;
                case "ListBox::InsertItemAt":
                    return false;
                case "ListBox::RemoveItem":
                    return false;
                case "ListBox::ScrollDown":
                    return false;
                case "ListBox::ScrollUp":
                    return false;
                case "ListBox::get_ItemCount":
                    return false;
                case "ListBox::geti_Items":
                    return false;
                case "ListBox::seti_Items":
                    return false;
                case "ListBox::get_RowCount":
                    return false;
                case "ListBox::geti_SaveGameSlots":
                    return false;

                case "GUI::Centre":
                    return false;
                case "GUI::GetAtScreenXY":
                    return false;
                case "GUI::SetPosition":
                    return false;
                case "GUI::SetSize":
                    return false;
                case "GUI::geti_Controls":
                    return false;
                case "GUI::get_ControlCount":
                    return false;

                case "Hotspot::GetAtScreenXY":
                    return false;
                case "Hotspot::GetName":
                    return false;
                case "Hotspot::GetPropertyText":
                    return false;
                case "Hotspot::GetProperty":
                    return false;
                case "Hotspot::GetTextProperty":
                    return false;
                case "Hotspot::RunInteraction":
                    return false;

                case "Region::GetAtRoomXY":
                    return false;
                case "Region::RunInteraction":
                    return false;
                case "Region::Tint":
                    return false;

                case "Dialog::DisplayOptions":
                    return false;
                case "Dialog::GetOptionState":
                    return false;
                case "Dialog::GetOptionText":
                    return false;
                case "Dialog::HasOptionBeenChosen":
                    return false;
                case "Dialog::SetOptionState":
                    return false;
                case "Dialog::Start":
                    return false;
                case "Dialog::get_OptionCount":
                    return false;

                case "Maths::ArcCos":
                    output = new SpecialFunctionExpression(this, func, "Math.acos({0})");
                    return true;
                case "Maths::ArcSin":
                    output = new SpecialFunctionExpression(this, func, "Math.asin({0})");
                    return true;
                case "Maths::ArcTan":
                    output = new SpecialFunctionExpression(this, func, "Math.atan({0})");
                    return true;
                case "Maths::ArcTan2":
                    output = new SpecialFunctionExpression(this, func, "Math.atan2({0}, {1})");
                    return true;
                case "Maths::Cos":
                    output = new SpecialFunctionExpression(this, func, "Math.cos({0})");
                    return true;
                case "Maths::DegreesToRadians":
                    output = new SpecialFunctionExpression(this, func, "{0}",
                        delegate(SpecialFunctionCall input)
                        {
                            return input.BinOp(Infix.Divide, (Expression)180).BinOp(Infix.Multiply, StandardLibraries.Math.PI);
                        });
                    return true;
                case "Maths::Exp":
                    output = new SpecialFunctionExpression(this, func, "Math.exp({0})");
                    return true;
                case "Maths::Log":
                    output = new SpecialFunctionExpression(this, func, "Math.log({0})");
                    return true;
                case "Maths::Log10":
                    output = new SpecialFunctionExpression(this, func, "Math.log({0})",
                        delegate(SpecialFunctionCall input)
                        {
                            return input.BinOp(Infix.Divide, StandardLibraries.Math.LN10);
                        });
                    return true;
                case "Maths::RadiansToDegrees":
                    output = new SpecialFunctionExpression(this, func, "{0}",
                        delegate(SpecialFunctionCall input)
                        {
                            return input.BinOp(Infix.Divide, StandardLibraries.Math.PI).BinOp(Infix.Multiply, (Expression)180);
                        });
                    return true;
                case "Maths::RaiseToPower":
                    output = new SpecialFunctionExpression(this, func, "Math.pow({0}, {1})");
                    return true;
                case "Maths::Sin":
                    output = new SpecialFunctionExpression(this, func, "Math.sin({0})");
                    return true;
                case "Maths::Sqrt":
                    output = new SpecialFunctionExpression(this, func, "Math.sqrt({0})");
                    return true;
                case "Maths::Tan":
                    output = new SpecialFunctionExpression(this, func, "Math.tan({0})");
                    return true;
                case "Maths::get_Pi":
                    output = new SpecialFunctionExpression(this, func, "Math.PI");
                    return true;

                case "Maths::Tanh":
                    return false;
                case "Maths::Sinh":
                    return false;
                case "Maths::Cosh":
                    return false;

                case "DateTime::get_Now":
                    output = new SpecialFunctionExpression(this, func, "{0}",
                        delegate(SpecialFunctionCall input)
                        {
                            return new Expression.New(StandardLibraries.Date);
                        });
                    return true;
                case "DateTime::get_Year":
                    output = new SpecialFunctionExpression(this, func, "{0}",
                        delegate(SpecialFunctionCall input)
                        {
                            return StandardLibraries.Date.getFullYear(input.parameters[0]);
                        });
                    return true;
                case "DateTime::get_Month":
                    output = new SpecialFunctionExpression(this, func, "{0}",
                        delegate(SpecialFunctionCall input)
                        {
                            return StandardLibraries.Date.getMonth(input.parameters[0])
                                .BinOp(Infix.Add, (Expression)1);
                        });
                    return true;
                case "DateTime::get_DayOfMonth":
                    output = new SpecialFunctionExpression(this, func, "{0}",
                        delegate(SpecialFunctionCall input)
                        {
                            return StandardLibraries.Date.getDate(input.parameters[0]);
                        });
                    return true;
                case "DateTime::get_Hour":
                    output = new SpecialFunctionExpression(this, func, "{0}",
                        delegate(SpecialFunctionCall input)
                        {
                            return StandardLibraries.Date.getHours(input.parameters[0]);
                        });
                    return true;
                case "DateTime::get_Minute":
                    output = new SpecialFunctionExpression(this, func, "{0}",
                        delegate(SpecialFunctionCall input)
                        {
                            return StandardLibraries.Date.getMinutes(input.parameters[0]);
                        });
                    return true;
                case "DateTime::get_Second":
                    output = new SpecialFunctionExpression(this, func, "{0}",
                        delegate(SpecialFunctionCall input)
                        {
                            return StandardLibraries.Date.getSeconds(input.parameters[0]);
                        });
                    return true;
                case "DateTime::get_RawTime":
                    output = new SpecialFunctionExpression(this, func, "{0}",
                        delegate(SpecialFunctionCall input)
                        {
                            return StandardLibraries.Date.valueOf(input.parameters[0])
                                .BinOp(Infix.Divide, (Expression)1000.0)
                                .Cast(PossibleValueTypes.Int32);
                        });
                    return true;

                case "AudioChannel::get_LengthMs":
                    return false;
                case "AudioChannel::get_Position":
                    return false;
                case "AudioChannel::get_PositionMs":
                    return false;
                case "AudioChannel::get_Volume":
                    return false;

                case "AudioClip::Play":
                    return false;
                case "AudioClip::PlayFrom":
                    return false;
                case "AudioClip::PlayQueued":
                    return false;
                case "AudioClip::Stop":
                    return false;

                case "System::get_CapsLock":
                    return false;
                case "System::geti_AudioChannels":
                    return false;
                case "System::get_AudioChannelCount":
                    return false;
                case "System::get_ColorDepth":
                    return false;
                case "System::get_Gamma":
                    return false;
                case "System::set_Gamma":
                    return false;
                case "System::get_HardwareAcceleration":
                    return false;
                case "System::get_NumLock":
                    return false;
                case "System::get_OperatingSystem":
                    return false;
                case "System::get_ScreenHeight":
                    return false;
                case "System::get_ScreenWidth":
                    return false;
                case "System::get_ScrollLock":
                    return false;
                case "System::get_SupportsGammaControl":
                    return false;
                case "System::get_Version":
                    return false;
                case "System::get_ViewportHeight":
                    return false;
                case "System::get_ViewportWidth":
                    return false;
                case "System::get_Volume":
                    return false;
                case "System::set_Volume":
                    return false;
                case "System::get_VSync":
                    return false;
                case "System::set_VSync":
                    return false;
                case "System::get_Windowed":
                    return false;

                case "Object::Animate":
                    return false;
                case "Object::GetAtScreenXY":
                    return false;
                case "Object::GetName":
                    return false;
                case "Object::GetPropertyText":
                    return false;
                case "Object::GetProperty":
                    return false;
                case "Object::GetTextProperty":
                    return false;
                case "Object::IsCollidingWithObject":
                    return false;
                case "Object::MergeIntoBackground":
                    return false;
                case "Object::Move":
                    return false;
                case "Object::RemoveTint":
                    return false;
                case "Object::RunInteraction":
                    return false;
                case "Object::SetPosition":
                    return false;
                case "Object::SetView":
                    return false;
                case "Object::StopAnimating":
                    return false;
                case "Object::StopMoving":
                    return false;
                case "Object::Tint":
                    return false;

                // Cast to Boolean
                case "DrawingSurface::set_UseHighResCoordinates":
                case "GUIControl::set_Clickable":
                case "GUIControl::set_Enabled":
                case "GUIControl::set_Visible":
                case "Button::set_ClipImage":
                case "ListBox::set_HideBorder":
                case "ListBox::set_HideScrollArrows":
                case "GUI::set_Clickable":
                case "GUI::set_Visible":
                case "Hotspot::set_Enabled":
                case "Region::set_Enabled":
                case "Object::set_Clickable":
                case "Object::set_IgnoreScaling":
                case "Object::set_IgnoreWalkbehinds":
                case "Object::set_Solid":
                case "Object::set_Visible":
                case "Character::set_BlinkWhileThinking":
                case "Character::set_Clickable":
                case "Character::set_DiagonalLoops":
                case "Character::set_IgnoreLighting":
                case "Character::set_IgnoreScaling":
                case "Character::set_IgnoreWalkbehinds":
                case "Character::set_ManualScaling":
                case "Character::set_MovementLinkedToAnimation":
                case "Character::set_ScaleVolume":
                case "Character::set_Solid":
                case "Character::set_TurnBeforeWalking":
                    {
                        string setAttrName = Regex.Match(func.Name, @"::set_(.+)$").Groups[1].Value;
                        setAttrName = setAttrName.Substring(0, 1).ToLower() + setAttrName.Substring(1);
                        output = new SpecialFunctionExpression(this, func, "{0}.set(\"" + setAttrName + "\", {1:Boolean})");
                        return true;
                    }
                case "ViewFrame::get_Flipped":
                case "DrawingSurface::get_UseHighResCoordinates":
                case "File::get_EOF":
                case "File::get_Error":
                case "Overlay::get_Valid":
                case "GUIControl::get_Clickable":
                case "GUIControl::get_Enabled":
                case "GUIControl::get_Visible":
                case "Button::get_ClipImage":
                case "ListBox::get_HideBorder":
                case "ListBox::get_HideScrollArrows":
                case "GUI::get_Clickable":
                case "GUI::get_Visible":
                case "Hotspot::get_Enabled":
                case "Region::get_Enabled":
                case "Region::get_TintEnabled":
                case "AudioChannel::get_IsPlaying":
                case "AudioClip::get_IsAvailable":
                case "Object::get_Animating":
                case "Object::get_Clickable":
                case "Object::get_IgnoreWalkbehinds":
                case "Object::get_IgnoreScaling":
                case "Object::get_Moving":
                case "Object::get_Solid":
                case "Object::get_Visible":
                case "Character::get_Animating":
                case "Character::get_BlinkWhileThinking":
                case "Character::get_Clickable":
                case "Character::get_DiagonalLoops":
                case "Character::get_HasExplicitTint":
                case "Character::get_IgnoreLighting":
                case "Character::get_IgnoreScaling":
                case "Character::get_IgnoreWalkbehinds":
                case "Character::get_ManualScaling":
                case "Character::get_MovementLinkedToAnimation":
                case "Character::get_Moving":
                case "Character::get_ScaleVolume":
                case "Character::get_Solid":
                case "Character::get_Speaking":
                case "Character::get_TurnBeforeWalking":
                    {
                        string getAttrName = Regex.Match(func.Name, @"::get_(.+)$").Groups[1].Value;
                        getAttrName = getAttrName.Substring(0, 1).ToLower() + getAttrName.Substring(1);
                        output = new SpecialFunctionExpression(this, func, "{0}." + getAttrName + " -> Boolean");
                        return true;
                    }

                case "Character::AddInventory":
                    return false;
                case "Character::AddWaypoint":
                    return false;
                case "Character::Animate":
                    return false;
                case "Character::ChangeRoom":
                    return false;
                case "Character::ChangeRoomAutoPosition":
                    return false;
                case "Character::ChangeView":
                    return false;
                case "Character::FaceCharacter":
                    return false;
                case "Character::FaceLocation":
                    return false;
                case "Character::FaceObject":
                    return false;
                case "Character::FollowCharacter":
                    return false;
                case "Character::GetAtScreenXY":
                    return false;
                case "Character::geti_InventoryQuantity":
                    return false;
                case "Character::GetProperty":
                    return false;
                case "Character::GetTextProperty":
                    return false;
                case "Character::HasInventory":
                    return false;
                case "Character::IsCollidingWithChar":
                    return false;
                case "Character::IsCollidingWithObj":
                    return false;
                case "Character::LockView":
                    return false;
                case "Character::LockViewAligned":
                    return false;
                case "Character::LockViewFrame":
                    return false;
                case "Character::LockViewOffset":
                    return false;
                case "Character::LoseInventory":
                    return false;
                case "Character::Move":
                    return false;
                case "Character::PlaceOnWalkableArea":
                    return false;
                case "Character::RemoveTint":
                    return false;
                case "Character::RunInteraction":
                    return false;
                case "Character::Say":
                    return false;
                case "Character::SayAt":
                    return false;
                case "Character::SayBackground":
                    return false;
                case "Character::SetAsPlayer":
                    return false;
                case "Character::SetIdleView":
                    return false;
                case "Character::SetWalkSpeed":
                    return false;
                case "Character::StopMoving":
                    return false;
                case "Character::Think":
                    return false;
                case "Character::Tint":
                    return false;
                case "Character::UnlockView":
                    return false;
                case "Character::Walk":
                    return false;
                case "Character::WalkStraight":
                    return false;
            }
            return false;
        }

        public PossibleValueTypes GetValueTypes(SPAGS.ValueType spagsVT)
        {
            switch (spagsVT.Category)
            {
                case SPAGS.ValueTypeCategory.Int:
                    switch (spagsVT.IntType)
                    {
                        case "uint8":
                            return PossibleValueTypes.UInt8;
                        case "int16":
                            return PossibleValueTypes.Int16;
                        default:
                            return PossibleValueTypes.Int32;
                    }
                case SPAGS.ValueTypeCategory.Float:
                    return PossibleValueTypes.Number;
                case SPAGS.ValueTypeCategory.StringValue:
                    return PossibleValueTypes.String;
                default:
                    return PossibleValueTypes.Any;
            }
        }

        public Statement FromSPAGS(SPAGS.Function spagsFunc, SPAGS.Statement spagsStatement, ScopedBlock jsScope)
        {
            SPAGS.Function callFunction;
            List<SPAGS.Expression> callParams;
            List<SPAGS.Expression> callVarargs;
            if (spagsStatement.TryGetSimpleCall(out callFunction, out callParams, out callVarargs))
            {
                return (Statement)FromSPAGS(callFunction, callParams, callVarargs);
            }
            switch (spagsStatement.Type)
            {
                case SPAGS.StatementType.Block:
                    SPAGS.Statement.Block spagsBlock = (SPAGS.Statement.Block)spagsStatement;
                    Statement.GenericBlock jsBlock = new Statement.GenericBlock();
                    foreach (SPAGS.Statement statement in spagsBlock.ChildStatements)
                    {
                        jsBlock.Block.Add(FromSPAGS(spagsFunc, statement, jsScope));
                    }
                    return jsBlock;
                case SPAGS.StatementType.Assign:
                    SPAGS.Statement.Assign spagsAssign = (SPAGS.Statement.Assign)spagsStatement;
                    PossibleValueTypes assignType = GetValueTypes(spagsAssign.Target.GetValueType());
                    Expression jsAssign = new Expression.InfixOperation(
                        FromSPAGS(spagsAssign.Target),
                        Infix.Assign,
                        FromSPAGS(spagsAssign.SimpleAssignValue()).Cast(assignType));
                    return (Statement)jsAssign;
                case SPAGS.StatementType.If:
                    SPAGS.Statement.If spagsIf = (SPAGS.Statement.If)spagsStatement;
                    Statement.If jsIf = new Statement.If(
                        FromSPAGS(spagsIf.IfThisIsTrue).Cast(PossibleValueTypes.Boolean));
                    if (spagsIf.ThenDoThis.Type == SPAGS.StatementType.Block)
                    {
                        SPAGS.Statement.Block thenBlock = (SPAGS.Statement.Block)spagsIf.ThenDoThis;
                        foreach (SPAGS.Statement statement in thenBlock.ChildStatements)
                        {
                            jsIf.ThenDoThis.Add(FromSPAGS(spagsFunc, statement, jsScope));
                        }
                    }
                    else
                    {
                        jsIf.ThenDoThis.Add(FromSPAGS(spagsFunc, spagsIf.ThenDoThis, jsScope));
                    }
                    if (spagsIf.ElseDoThis != null)
                    {
                        if (spagsIf.ElseDoThis.Type == SPAGS.StatementType.Block)
                        {
                            SPAGS.Statement.Block elseBlock = (SPAGS.Statement.Block)spagsIf.ElseDoThis;
                            foreach (SPAGS.Statement statement in elseBlock.ChildStatements)
                            {
                                jsIf.ElseDoThis.Add(FromSPAGS(spagsFunc, statement, jsScope));
                            }
                        }
                        else
                        {
                            jsIf.ElseDoThis.Add(FromSPAGS(spagsFunc, spagsIf.ElseDoThis, jsScope));
                        }
                    }
                    return jsIf;
                case SPAGS.StatementType.Return:
                    SPAGS.Statement.Return spagsReturn = (SPAGS.Statement.Return)spagsStatement;
                    if (spagsReturn.Value == null)
                    {
                        if (spagsFunc.Signature.ReturnType.Category == SPAGS.ValueTypeCategory.Void)
                        {
                            return new Statement.Return();
                        }
                        else
                        {
                            return new Statement.Return(FromSPAGS(spagsFunc.Signature.ReturnType.CreateDefaultValueExpression()));
                        }
                    }
                    else
                    {
                        PossibleValueTypes returnType = GetValueTypes(spagsFunc.Signature.ReturnType);
                        return new Statement.Return(FromSPAGS(spagsReturn.Value).Cast(returnType));
                    }
                case SPAGS.StatementType.VariableDeclaration:
                    SPAGS.Statement.VariableDeclaration varDef = (SPAGS.Statement.VariableDeclaration)spagsStatement;
                    Statement.InitVariables assignment = new Statement.InitVariables();
                    foreach (SPAGS.Variable variable in varDef.Variables)
                    {
                        Variable v;
                        if (jsScope.Variables.ContainsKey(variable.Name))
                        {
                            v = jsScope.Variables[variable.Name];
                        }
                        else
                        {
                            v = new Variable(variable.Name, GetValueTypes(variable.Type));
                            jsScope.Variables.Add(variable.Name, v);
                        }
                        SetReference(variable, v);
                        SPAGS.Expression val = variable.InitialValue ?? variable.Type.CreateDefaultValueExpression();
                        PossibleValueTypes variableType = GetValueTypes(variable.Type);
                        if (val.IsConstant())
                        {
                            v.InitialValue = FromSPAGS(val).Cast(variableType);
                        }
                        else
                        {
                            assignment.Add(v, FromSPAGS(val).Cast(variableType));
                        }
                    }
                    return assignment;
                case SPAGS.StatementType.While:
                    SPAGS.Statement.While spagsLoop = (SPAGS.Statement.While)spagsStatement;
                    Statement.While jsLoop = new Statement.While(
                        FromSPAGS(spagsLoop.WhileThisIsTrue).Cast(PossibleValueTypes.Boolean));
                    if (spagsLoop.KeepDoingThis.Type == SPAGS.StatementType.Block)
                    {
                        SPAGS.Statement.Block body = (SPAGS.Statement.Block)spagsLoop.KeepDoingThis;
                        foreach (SPAGS.Statement statement in body.ChildStatements)
                        {
                            jsLoop.KeepDoingThis.Add(FromSPAGS(spagsFunc, statement, jsScope));
                        }
                    }
                    else
                    {
                        jsLoop.KeepDoingThis.Add(FromSPAGS(spagsFunc, spagsLoop.KeepDoingThis, jsScope));
                    }
                    return jsLoop;
            }
            return (Statement)(new Expression.Custom("(" + spagsStatement.Type + ")"));
        }

        private Dictionary<SPAGS.Function, Expression> functionExpressions
            = new Dictionary<SPAGS.Function,Expression>();
        private Dictionary<SPAGS.Variable, Expression> variableExpressions
            = new Dictionary<SPAGS.Variable,Expression>();
        private Dictionary<SPAGS.ValueType.Struct, Expression> structConstructors
            = new Dictionary<SPAGS.ValueType.Struct,Expression>();

        public void AddReference(SPAGS.Function func, Expression expr)
        {
            functionExpressions.Add(func, expr);
        }

        public void AddReference(SPAGS.Variable variable, Expression expr)
        {
            variableExpressions.Add(variable, expr);
        }

        public void AddReference(SPAGS.ValueType.Struct structType, Expression expr)
        {
            structConstructors.Add(structType, expr);
        }

        public void SetReference(SPAGS.Function func, Expression expr)
        {
            functionExpressions[func] = expr;
        }

        public void SetReference(SPAGS.Variable variable, Expression expr)
        {
            variableExpressions[variable] = expr;
        }

        public void SetReference(SPAGS.ValueType.Struct structType, Expression expr)
        {
            structConstructors[structType] = expr;
        }

        private Expression GetReference(SPAGS.Function func)
        {
            if (!functionExpressions.ContainsKey(func))
            {
                return new Expression.Custom("(UNKNOWN FUNC: " + func.Name + ")");
            }
            return functionExpressions[func];
        }

        private Expression GetReference(SPAGS.Variable variable)
        {
            if (!variableExpressions.ContainsKey(variable))
            {
                return new Expression.Custom("(UNKNOWN VARIBALE: " + variable.Name + ")");
            }
            return variableExpressions[variable];
        }

        private Expression GetReference(SPAGS.ValueType.Struct structType)
        {
            if (!structConstructors.ContainsKey(structType))
            {
                return new Expression.Custom("(UNKNOWN STRUCT: " + structType.Name + ")");
            }
            return structConstructors[structType];
        }

        public Expression FromSPAGS(
            SPAGS.Function func,
            List<SPAGS.Expression> callParameters,
            List<SPAGS.Expression> callVarargs)
        {
            Expression funcRef = GetReference(func);
            List<Expression> parameters = new List<Expression>();
            for (int i = 0; i < callParameters.Count; i++)
            {
                PossibleValueTypes paramVT = GetValueTypes(func.Signature.Parameters[i].Type);
                parameters.Add(FromSPAGS(callParameters[i]).Cast(paramVT));
            }
            if (callVarargs != null && callVarargs.Count != 0)
            {
                Expression.ArrayLiteral arr = new Expression.ArrayLiteral();
                foreach (SPAGS.Expression callVararg in callVarargs)
                {
                    arr.Entries.Add(FromSPAGS(callVararg));
                }
                parameters.Add(arr);
            }
            Expression call = funcRef.Call(parameters);
            return call;
        }

        public class AllocateStringBufferExpression : Expression
        {
            public override void WriteTo(Writer writer)
            {
                writer.Write("new util.StringBuffer()");
            }
        }

        public class FillArrayExpression : Expression
        {
            public Expression Value;
            public Expression Length;
            public FillArrayExpression(Expression value, Expression length)
            {
                Value = value;
                Length = length;
            }
            public override void WriteTo(Writer writer)
            {
                writer.Write("util.fillArray(");
                Value.WriteTo(writer);
                writer.Write(", ");
                Length.WriteTo(writer);
                writer.Write(")");
            }
        }

        public class MultiplyIntExpression : Expression
        {
            public Expression Left, Right;
            public MultiplyIntExpression(Expression left, Expression right)
            {
                Left = left;
                Right = right;
            }
            public override void WriteTo(Writer writer)
            {
                writer.Write("util.imul(");
                Left.WriteTo(writer);
                writer.Write(", ");
                Right.WriteTo(writer);
                writer.Write(")");
            }
            public override PossibleValueTypes ValueTypes
            {
                get { return PossibleValueTypes.Int32; }
            }
        }

        public class StructDefinitionExpression : Expression
        {
            public ObjectLiteral InitialValues = new ObjectLiteral();
            public override void WriteTo(Writer writer)
            {
                writer.Write("new util.StructDefinition(");
                InitialValues.WriteTo(writer);
                writer.Write(")");
            }
        }

        public StructDefinitionExpression FromSPAGS(SPAGS.ValueType.Struct spagsStruct)
        {
            StructDefinitionExpression structDef = new StructDefinitionExpression();
            foreach (SPAGS.StructMember.Field field
                in spagsStruct.Members.EachOf<SPAGS.StructMember.Field>())
            {
                structDef.InitialValues.Add(
                    field.Name,
                    FromSPAGS(field.Type.CreateDefaultValueExpression()));
            }
            return structDef;
        }

        public Expression FromSPAGS(SPAGS.Expression spagsExpr)
        {
            SPAGS.Function callFunction;
            List<SPAGS.Expression> callParameters, callVarargs;
            if (spagsExpr.TryGetSimpleCall(out callFunction, out callParameters, out callVarargs))
            {
                return FromSPAGS(callFunction, callParameters, callVarargs);
            }
            switch (spagsExpr.Type)
            {
                case SPAGS.ExpressionType.AllocateArray:
                    SPAGS.Expression.AllocateArray arr = (SPAGS.Expression.AllocateArray)spagsExpr;
                    if (arr.ElementType.Category == SPAGS.ValueTypeCategory.Struct
                        && !((SPAGS.ValueType.Struct)arr.ElementType).IsManaged)
                    {
                        List<Expression> arguments = new List<Expression>();
                        arguments.Add(GetReference((SPAGS.ValueType.Struct)arr.ElementType));
                        arguments.Add(FromSPAGS(arr.Length));
                        return OtherLibraries.Util.structArray.Call(arguments);
                    }
                    else
                    {
                        return new FillArrayExpression(
                            FromSPAGS(arr.ElementType.CreateDefaultValueExpression()),
                            FromSPAGS(arr.Length));
                    }
                case SPAGS.ExpressionType.AllocStringBuffer:
                    return new AllocateStringBufferExpression();
                case SPAGS.ExpressionType.AllocStruct:
                    SPAGS.Expression.AllocateStruct allocStruct = (SPAGS.Expression.AllocateStruct)spagsExpr;
                    Expression structCtor = GetReference(allocStruct.TheStructType);
                    return new Expression.New(structCtor);
                case SPAGS.ExpressionType.ArrayIndex:
                    SPAGS.Expression.ArrayIndex arrayIndex = (SPAGS.Expression.ArrayIndex)spagsExpr;
                    return FromSPAGS(arrayIndex.Target).Index(FromSPAGS(arrayIndex.Index));
                case SPAGS.ExpressionType.BinaryOperator:
                    SPAGS.Expression.BinaryOperator spagsBinOp = (SPAGS.Expression.BinaryOperator)spagsExpr;
                    Expression left = FromSPAGS(spagsBinOp.Left);
                    Expression right = FromSPAGS(spagsBinOp.Right);
                    PossibleValueTypes mathCast;
                    if (spagsBinOp.Left.GetValueType().Category == SPAGS.ValueTypeCategory.Int)
                    {
                        mathCast = PossibleValueTypes.Int32;
                    }
                    else
                    {
                        mathCast = PossibleValueTypes.Any;
                    }
                    switch (spagsBinOp.Token.Type)
                    {
                        case SPAGS.TokenType.Add:
                            return left.BinOp(Infix.Add, right).Cast(mathCast);
                        case SPAGS.TokenType.BitwiseAnd:
                            return left.BinOp(Infix.BitwiseAnd, right);
                        case SPAGS.TokenType.BitwiseLeftShift:
                            return left.BinOp(Infix.BitwiseLeftShift, right);
                        case SPAGS.TokenType.BitwiseOr:
                            return left.BinOp(Infix.BitwiseOr, right);
                        case SPAGS.TokenType.BitwiseRightShift:
                            return left.BinOp(Infix.BitwiseSignedRightShift, right);
                        case SPAGS.TokenType.BitwiseXor:
                            return left.BinOp(Infix.BitwiseXor, right);
                        case SPAGS.TokenType.Divide:
                            return left.BinOp(Infix.Divide, right).Cast(mathCast);
                        case SPAGS.TokenType.IsEqualTo:
                            return left.BinOp(Infix.IsEqualTo, right).Cast(PossibleValueTypes.UInt8);
                        case SPAGS.TokenType.IsGreaterThan:
                            return left.BinOp(Infix.IsGreaterThan, right).Cast(PossibleValueTypes.UInt8);
                        case SPAGS.TokenType.IsGreaterThanOrEqualTo:
                            return left.BinOp(Infix.IsGreaterThanOrEqualTo, right).Cast(PossibleValueTypes.UInt8);
                        case SPAGS.TokenType.IsLessThan:
                            return left.BinOp(Infix.IsLessThan, right).Cast(PossibleValueTypes.UInt8);
                        case SPAGS.TokenType.IsLessThanOrEqualTo:
                            return left.BinOp(Infix.IsLessThanOrEqualTo, right).Cast(PossibleValueTypes.UInt8);
                        case SPAGS.TokenType.IsNotEqualTo:
                            return left.BinOp(Infix.IsNotEqualTo, right).Cast(PossibleValueTypes.UInt8);
                        case SPAGS.TokenType.LogicalAnd:
                            return left.BinOp(Infix.LogicalAnd, right);
                        case SPAGS.TokenType.LogicalOr:
                            return left.BinOp(Infix.LogicalOr, right);
                        case SPAGS.TokenType.Modulus:
                            return left.BinOp(Infix.Modulus, right);
                        case SPAGS.TokenType.Multiply:
                            if (mathCast == PossibleValueTypes.Int32)
                            {
                                bool useSpecialFunction = true;
                                SPAGS.ValueType leftType = spagsBinOp.Left.GetValueType();
                                SPAGS.ValueType rightType = spagsBinOp.Right.GetValueType();
                                if (leftType.IntType != "int32" || rightType.IntType != "int32"
                                    || left.ValueTypes == PossibleValueTypes.Int16
                                    || right.ValueTypes == PossibleValueTypes.Int16
                                    || left.ValueTypes == PossibleValueTypes.UInt8
                                    || right.ValueTypes == PossibleValueTypes.UInt8)
                                {
                                    useSpecialFunction = false;
                                }
                                else
                                {
                                    int leftVal, rightVal;
                                    if (spagsBinOp.Left.TryGetIntValue(out leftVal)
                                        && (leftVal > -1000000) && (leftVal < 1000000))
                                    {
                                        useSpecialFunction = false;
                                    }
                                    if (spagsBinOp.Right.TryGetIntValue(out rightVal)
                                        && (rightVal > -1000000) && (rightVal < 1000000))
                                    {
                                        useSpecialFunction = false;
                                    }
                                }
                                if (useSpecialFunction)
                                {
                                    return new MultiplyIntExpression(left, right);
                                }
                                else
                                {
                                    return left.BinOp(Infix.Multiply, right).Cast(PossibleValueTypes.Int32);
                                }
                            }
                            return left.BinOp(Infix.Multiply, right);
                        case SPAGS.TokenType.Subtract:
                            return left.BinOp(Infix.Subtract, right.Cast(mathCast));
                        default:
                            throw new Exception("Unknown binop: " + spagsBinOp.Token);
                    }
                case SPAGS.ExpressionType.CharLiteral:
                    return (Expression)(int)((SPAGS.Expression.CharLiteral)spagsExpr).Value;
                case SPAGS.ExpressionType.Constant:
                    return FromSPAGS(((SPAGS.Expression.Constant)spagsExpr).TheConstant.TheExpression);
                case SPAGS.ExpressionType.EnumValue:
                    return (Expression)((SPAGS.Expression.EnumValue)spagsExpr).TheValue.Value;
                case SPAGS.ExpressionType.Field:
                    SPAGS.Expression.Field field = (SPAGS.Expression.Field)spagsExpr;
                    return FromSPAGS(field.Target).Index(new Expression.StringLiteral(field.TheField.Name));
                case SPAGS.ExpressionType.FloatLiteral:
                    return (Expression)((SPAGS.Expression.FloatLiteral)spagsExpr).Value;
                case SPAGS.ExpressionType.IntegerLiteral:
                    return (Expression)((SPAGS.Expression.IntegerLiteral)spagsExpr).Value;
                case SPAGS.ExpressionType.Null:
                    return Expression.Null;
                case SPAGS.ExpressionType.StringLiteral:
                    //return new Expression.StringLiteral(((SPAGS.Expression.StringLiteral)spagsExpr).Value);
                    return new Expression.ObfuscatedStringLiteral(((SPAGS.Expression.StringLiteral)spagsExpr).Value);
                case SPAGS.ExpressionType.UnaryOperator:
                    SPAGS.Expression.UnaryOperator spagsUnOp = (SPAGS.Expression.UnaryOperator)spagsExpr;
                    Expression operand = FromSPAGS(spagsUnOp.Operand);
                    switch(spagsUnOp.Token.Type)
                    {
                        case SPAGS.TokenType.Subtract:
                            if (spagsUnOp.Operand.GetValueType().Category == SPAGS.ValueTypeCategory.Int)
                            {
                                return operand.UnOp(Prefix.Negative).Cast(PossibleValueTypes.Int32);
                            }
                            else
                            {
                                return operand.UnOp(Prefix.Negative);
                            }
                        case SPAGS.TokenType.LogicalNot:
                            return operand.BinOp(Infix.IsEqualTo, (Expression)0).Cast(PossibleValueTypes.UInt8);
                        default:
                            throw new Exception("Unknown unop: " + spagsUnOp.Token.Type);
                    }
                case SPAGS.ExpressionType.Variable:
                    SPAGS.Expression.Variable spagsVar = (SPAGS.Expression.Variable)spagsExpr;
                    Expression expr = GetReference(spagsVar.TheVariable);
                    // TODO: string buffers...
                    return expr;
            }
            return new Expression.Custom("(" + spagsExpr.Type + ")");
        }

    }
}
