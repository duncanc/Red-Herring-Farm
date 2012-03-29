if (typeof ags.games["f3b6a6fcc30c4ec09285e5c42e48be15"] === 'undefined') {
    ags.games["f3b6a6fcc30c4ec09285e5c42e48be15"] = {};
}
ags.games["f3b6a6fcc30c4ec09285e5c42e48be15"].Room1Script = function(engine) {
    var util = ags.util;
    var game = engine.game;
    
    var scripts = game.globalScripts.scripts;
    
    var roomScript = {
      userInput: util.fillArray(4, 0),
      actual: util.fillArray(4, 0),
      index: 0,
      "room_Load": function() {
          var i;
          engine.AudioClip$$Play(game.aMusic1, 31998 /* SCR_NO_VALUE */, 31998 /* SCR_NO_VALUE */);
          engine.Character$$set_Transparency(game.cPhone, 100);
          i = 8;
          for (; i < 14; i = (i + 1) | 0) {
            engine.Character$$set_Transparency(game.character[i], 100);
            engine.Character$$set_Baseline(game.character[i], 241);
          }
          i = 1;
          for (; i < 8; i = (i + 1) | 0) {
            engine.Character$$set_Baseline(game.character[i], 241);
          }
          roomScript.actual[0] = 1;
          roomScript.actual[1] = 2;
          roomScript.actual[2] = 3;
          roomScript.actual[3] = 4;
          engine.GUI$$set_Visible(game.gHotspots, 0 /* false */);
          return 0;
      },
      "CheckInputs": util.blockingFunction([], function($ctx, $stk, $vars) {
          switch ($ctx.entryPoint) {
            case 0:
              $vars["i"] = 0;
              for (; $vars["i"] < roomScript.index; $vars["i"] = ($vars["i"] + 1) | 0) {
                if (roomScript.userInput[$vars["i"]] !== roomScript.actual[$vars["i"]]) {
                  roomScript.index = 0;
                }
              }
              if (roomScript.index === 4) {
                $ctx.queueCall(engine.Wait, [40]);
                $ctx.queueCall(engine.Character$$Say, [game.cPicnic, "Hey! You've found the natural order of things."]);
                $ctx.queueCall(engine.Wait, [20]);
                $ctx.queueCall(engine.Character$$Say, [game.cPicnic, "Go to the city rubbish tip. That's where Elder Steve spends most of his time."]);
                $ctx.queueCall(engine.Wait, [10]);
                $ctx.queueCall(engine.Character$$Say, [game.cPicnic, "I doubt he'll talk to you until you can prove you are willing to listen to him, though."]);
                $ctx.queueCall(engine.Character$$Say, [game.cRobot, "Alright, thank you for your help."]);
                return $ctx.nextEntryPoint(1);
              }
              else {
                return $ctx.finish(0);
              }
            case 1:
              roomScript.index = 0;
              scripts["_GlobalVariables.asc"].progress = 6;
              return $ctx.finish(0);
          }
      }),
      "hCity_Interact": util.blockingFunction([], function($ctx, $stk, $vars) {
          switch ($ctx.entryPoint) {
            case 0:
              $ctx.queueCall(engine.Character$$Say, [game.cRobot, "Ah yes, the city."]);
              return $ctx.nextEntryPoint(1);
            case 1:
              if (scripts["_GlobalVariables.asc"].progress === 5) {
                $ctx.queueCall(engine.Character$$Say, [game.cPicnic, "Such a busy place. Things change every second."]);
                return $ctx.nextEntryPoint(2);
              }
              else {
                return $ctx.finish(0);
              }
            case 2:
              roomScript.userInput[roomScript.index] = 2;
              roomScript.index = (roomScript.index + 1) | 0;
              $ctx.queueCall(roomScript.CheckInputs);
              return $ctx.finish(0);
          }
      }),
      "hBlanket_Interact": util.blockingFunction([], function($ctx, $stk, $vars) {
          switch ($ctx.entryPoint) {
            case 0:
              $ctx.queueCall(engine.Character$$Say, [game.cRobot, "Nice blanket."]);
              return $ctx.nextEntryPoint(1);
            case 1:
              if (scripts["_GlobalVariables.asc"].progress === 5) {
                $ctx.queueCall(engine.Character$$Say, [game.cPicnic, "Ah yes, it was a prize that I won."]);
                return $ctx.nextEntryPoint(2);
              }
              else {
                return $ctx.finish(0);
              }
            case 2:
              roomScript.userInput[roomScript.index] = 1;
              roomScript.index = (roomScript.index + 1) | 0;
              $ctx.queueCall(roomScript.CheckInputs);
              return $ctx.finish(0);
          }
      }),
      "hTree_Interact": util.blockingFunction([], function($ctx, $stk, $vars) {
          switch ($ctx.entryPoint) {
            case 0:
              $ctx.queueCall(engine.Character$$Say, [game.cRobot, "One of many trees in the park."]);
              return $ctx.nextEntryPoint(1);
            case 1:
              if (scripts["_GlobalVariables.asc"].progress === 5) {
                $ctx.queueCall(engine.Character$$Say, [game.cPicnic, "I do love to sit beneath a tree or three."]);
                return $ctx.nextEntryPoint(2);
              }
              else {
                return $ctx.finish(0);
              }
            case 2:
              roomScript.userInput[roomScript.index] = 3;
              roomScript.index = (roomScript.index + 1) | 0;
              $ctx.queueCall(roomScript.CheckInputs);
              return $ctx.finish(0);
          }
      }),
      "hBasket_Interact": util.blockingFunction([], function($ctx, $stk, $vars) {
          switch ($ctx.entryPoint) {
            case 0:
              $ctx.queueCall(engine.Character$$Say, [game.cRobot, "A basket of some sort."]);
              return $ctx.nextEntryPoint(1);
            case 1:
              if (scripts["_GlobalVariables.asc"].progress === 5) {
                $ctx.queueCall(engine.Character$$Say, [game.cPicnic, "I'm sure you know what that is for."]);
                return $ctx.nextEntryPoint(2);
              }
              else {
                return $ctx.finish(0);
              }
            case 2:
              roomScript.userInput[roomScript.index] = 4;
              roomScript.index = (roomScript.index + 1) | 0;
              $ctx.queueCall(roomScript.CheckInputs);
              return $ctx.finish(0);
          }
      }),
      "room_AfterFadeIn": util.blockingFunction([], function($ctx, $stk, $vars) {
          switch ($ctx.entryPoint) {
            case 0:
              engine.StartCutscene(1 /* eSkipESCOnly */);
              $ctx.queueCall(engine.Wait, [120]);
              $ctx.queueCall(engine.Character$$Walk, [game.cCount, 165, 153, 919 /* eBlock */, 304 /* eAnywhere */]);
              $ctx.queueCall(engine.Wait, [40]);
              $ctx.queueCall(engine.Character$$Animate, [game.cCount, 4, 15, 0 /* eOnce */, 919 /* eBlock */]);
              $ctx.queueCall(engine.Wait, [10]);
              $ctx.queueCall(engine.Character$$Say, [game.cCount, "Pssst, you there, chap with the sign..."]);
              $ctx.queueCall(engine.Wait, [10]);
              $ctx.queueCall(engine.Character$$set_Loop, [game.cSign, 1]);
              $ctx.queueCall(engine.Wait, [10]);
              $ctx.queueCall(engine.Character$$Say, [game.cSign, "Who, me?"]);
              $ctx.queueCall(engine.Character$$Say, [game.cCount, "Yes, yes, look..."]);
              $ctx.queueCall(engine.Wait, [20]);
              $ctx.queueCall(engine.Character$$Say, [game.cCount, "...I couldn't possibly convince you to perhaps turn a blind eye whilst I go and... examine... this monument, could I?"]);
              $ctx.queueCall(engine.Wait, [40]);
              $ctx.queueCall(engine.Character$$Say, [game.cSign, "Whatever. You can take the thing for all I care."]);
              $ctx.queueCall(engine.Wait, [20]);
              $ctx.queueCall(engine.Character$$Say, [game.cCount, "Ah, most wonderful. You see..."]);
              $ctx.queueCall(engine.Character$$Animate, [game.cCount, 4, 15, 0 /* eOnce */, 919 /* eBlock */]);
              $ctx.queueCall(engine.Character$$Say, [game.cCount, "That is exactly what I, the nefarious Count Can't, am planning to do."]);
              $ctx.queueCall(engine.Wait, [20]);
              $ctx.queueCall(engine.Character$$Walk, [game.cCount, 147, 152, 919 /* eBlock */, 304 /* eAnywhere */]);
              $ctx.queueCall(engine.Wait, [20]);
              $ctx.queueCall(engine.Character$$Say, [game.cCount, "This monument, the Giant Cardboard Exclamation Mark, is perfect, see."]);
              $ctx.queueCall(engine.Wait, [20]);
              $ctx.queueCall(engine.Character$$Say, [game.cCount, "I have spent years perfecting my evil laugh to the point where it is the fourth most powerful thing in the northern hemisphere..."]);
              $ctx.queueCall(engine.Wait, [20]);
              $ctx.queueCall(engine.Character$$Say, [game.cCount, "...however, it still lacks that extra zest, the spice, the punchiness it requires."]);
              $ctx.queueCall(engine.Wait, [20]);
              $ctx.queueCall(engine.Character$$Say, [game.cCount, "With this enormous exclamation mark at the end, it will gain a whole new level of potency."]);
              $ctx.queueCall(engine.Wait, [20]);
              $ctx.queueCall(engine.Character$$Walk, [game.cCount, 165, 153, 919 /* eBlock */, 304 /* eAnywhere */]);
              $ctx.queueCall(engine.Wait, [10]);
              $ctx.queueCall(engine.Character$$Say, [game.cCount, "I have a plan, you see. A goal... nay, a *vision*."]);
              $ctx.queueCall(engine.Wait, [20]);
              $ctx.queueCall(engine.Character$$Say, [game.cCount, "And with the aid of this large cardboard item of punctuation, that dream can become REALITY."]);
              $ctx.queueCall(engine.Wait, [40]);
              $ctx.queueCall(engine.Character$$Say, [game.cSign, "...I don't see why you're telling me all this..."]);
              $ctx.queueCall(engine.Wait, [30]);
              $ctx.queueCall(engine.Character$$Say, [game.cCount, "...errr, yes, right. Ahem. I do so get carried away."]);
              $ctx.queueCall(engine.Wait, [20]);
              $ctx.queueCall(engine.Character$$Say, [game.cCount, "Forgive me for rambling, sir, and I shall bid you a good day..."]);
              $ctx.queueCall(engine.Wait, [20]);
              $ctx.queueCall(engine.Character$$Say, [game.cCount, "...and I shall look forward to utterly destroying you when I return to the city with my all-powerful evil laugh."]);
              $ctx.queueCall(engine.Wait, [40]);
              $ctx.queueCall(engine.Character$$Say, [game.cCount, "MUAHAHAHAHAHAHAHAHAHAHA HAAA HA...."]);
              $ctx.queueCall(engine.Wait, [20]);
              $ctx.queueCall(engine.Character$$Say, [game.cCount, "Ah yes, still a bit lacking."]);
              $ctx.queueCall(engine.Wait, [10]);
              $ctx.queueCall(engine.Character$$Walk, [game.cCount, 136, 151, 919 /* eBlock */, 304 /* eAnywhere */]);
              $ctx.queueCall(engine.Wait, [10]);
              $ctx.queueCall(engine.Character$$set_Frame, [game.cCount, 0]);
              $ctx.queueCall(engine.Character$$set_Loop, [game.cCount, 5]);
              $ctx.queueCall(engine.Wait, [20]);
              $ctx.queueCall(engine.Character$$Animate, [game.cCount, 5, 3, 0 /* eOnce */, 919 /* eBlock */]);
              $ctx.queueCall(engine.Wait, [20]);
              return $ctx.nextEntryPoint(1);
            case 1:
              engine.Object$$set_Visible(game.object[0], 0 /* false */);
              engine.Character$$ChangeView(game.cCount, 11);
              $ctx.queueCall(engine.Wait, [10]);
              return $ctx.nextEntryPoint(2);
            case 2:
              $ctx.queueCall(engine.Character$$Walk, [game.cCount, (game.cCount.x - 60) | 0, game.cCount.y, 919 /* eBlock */, 304 /* eAnywhere */]);
              $ctx.queueCall(engine.Character$$ChangeRoom, [game.cCount, 0]);
              $ctx.queueCall(engine.Wait, [20]);
              $ctx.queueCall(engine.Character$$set_Loop, [game.cSign, 0]);
              $ctx.queueCall(engine.Wait, [20]);
              $ctx.queueCall(engine.Character$$Say, [game.cSign, "...hang on a minute..."]);
              $ctx.queueCall(engine.Wait, [40]);
              $ctx.queueCall(engine.Character$$Say, [game.cSign, "...did he say 'utterly destroy'?"]);
              $ctx.queueCall(engine.Wait, [20]);
              $ctx.queueCall(engine.Character$$Animate, [game.cSign, 4, 2, 0 /* eOnce */, 919 /* eBlock */]);
              $ctx.queueCall(engine.Character$$set_Loop, [game.cSign, 2]);
              $ctx.queueCall(engine.Wait, [20]);
              $ctx.queueCall(engine.Character$$Say, [game.cSign, "I don't want to be utterly destroyed!"]);
              $ctx.queueCall(engine.PlayMusic, [10]);
              $ctx.queueCall(engine.Wait, [20]);
              $ctx.queueCall(engine.Character$$Say, [game.cSign, "POLICE! HELP!!!"]);
              $ctx.queueCall(engine.Wait, [40]);
              $ctx.queueCall(engine.Character$$set_Loop, [game.cOfficer, 1]);
              $ctx.queueCall(engine.Wait, [10]);
              $ctx.queueCall(engine.Character$$Say, [game.cOfficer, "Yes, honest citizen?"]);
              $ctx.queueCall(engine.Wait, [20]);
              $ctx.queueCall(engine.Character$$set_Loop, [game.cSign, 5]);
              $ctx.queueCall(engine.Wait, [40]);
              $ctx.queueCall(engine.Character$$Say, [game.cSign, "I have a theft to report!"]);
              $ctx.queueCall(engine.Wait, [20]);
              $ctx.queueCall(engine.Character$$Say, [game.cOfficer, "Where are you and what was stolen?"]);
              $ctx.queueCall(engine.Wait, [20]);
              $ctx.queueCall(engine.Character$$Say, [game.cSign, "I'm by the town monument. The Giant Cardboard Exclamation Mark was stolen!"]);
              $ctx.queueCall(engine.Wait, [20]);
              $ctx.queueCall(engine.Character$$Say, [game.cOfficer, "And did you see the thief?"]);
              $ctx.queueCall(engine.Wait, [20]);
              $ctx.queueCall(engine.Character$$Say, [game.cSign, "Yes, it was a man in a top hat who called himself Count Can't."]);
              $ctx.queueCall(engine.Wait, [20]);
              $ctx.queueCall(engine.Character$$Say, [game.cOfficer, "Ah, the nefarious Count makes his move. Thank you for your report, citizen. A detective will be there shortly."]);
              $ctx.queueCall(engine.Wait, [10]);
              $ctx.queueCall(engine.Character$$set_Loop, [game.cSign, 2]);
              $ctx.queueCall(engine.Character$$set_Loop, [game.cOfficer, 0]);
              $ctx.queueCall(engine.Wait, [20]);
              $ctx.queueCall(engine.Character$$Say, [game.cOfficer, "I'd best inform the mayor, seeing as Can't is involved..."]);
              $ctx.queueCall(engine.Wait, [10]);
              $ctx.queueCall(engine.Character$$Animate, [game.cOfficer, 2, 3, 0 /* eOnce */, 919 /* eBlock */]);
              $ctx.queueCall(engine.Wait, [20]);
              $ctx.queueCall(engine.Character$$SayAt, [game.cRobot, 58, 61, 50, "*ring ring*"]);
              $ctx.queueCall(engine.Wait, [5]);
              $ctx.queueCall(engine.Character$$set_Loop, [game.cMayor, 1]);
              $ctx.queueCall(engine.Wait, [5]);
              $ctx.queueCall(engine.Character$$Say, [game.cMayor, "Yo, DJ The Earl, is that the mayoral telephone I hear?"]);
              $ctx.queueCall(engine.Wait, [10]);
              $ctx.queueCall(engine.Character$$SayAt, [game.cRobot, 58, 61, 50, "*ring ring*"]);
              $ctx.queueCall(engine.Wait, [10]);
              $ctx.queueCall(engine.Character$$Say, [game.cEarl, "Verily, MC Mista Maya! I shall answer it at once!"]);
              $ctx.queueCall(engine.Wait, [5]);
              $ctx.queueCall(engine.Character$$set_Loop, [game.cMayor, 0]);
              $ctx.queueCall(engine.Wait, [5]);
              $ctx.queueCall(engine.Character$$Animate, [game.cEarl, 1, 3, 0 /* eOnce */, 919 /* eBlock */]);
              $ctx.queueCall(engine.Wait, [10]);
              $ctx.queueCall(engine.Character$$Say, [game.cEarl, "Hello, you have contacted DJ The Earl, answering on behalf of my homeboy, His Royal Flyness, MC Mista Maya."]);
              $ctx.queueCall(engine.Wait, [20]);
              $ctx.queueCall(engine.Character$$Say, [game.cEarl, "Let me hear ya holler, yo."]);
              $ctx.queueCall(engine.Wait, [20]);
              $ctx.queueCall(engine.Character$$Say, [game.cOfficer, "DJ The Earl! The mayor must be notified immediately! We have a code 3 on our hands!"]);
              $ctx.queueCall(engine.Wait, [20]);
              $ctx.queueCall(engine.Character$$Say, [game.cEarl, "Are you saying that the nefarious Count Can't has taken The Giant Cardboard Exclamation Mark and plans to use it with his laugh for evil?"]);
              $ctx.queueCall(engine.Wait, [20]);
              $ctx.queueCall(engine.Character$$Say, [game.cOfficer, "Precisely, DJ The Earl!"]);
              $ctx.queueCall(engine.Wait, [20]);
              $ctx.queueCall(engine.Character$$Say, [game.cEarl, "Then remain calm, officer, for DJ The Earl and MC Mista Maya are now in the house."]);
              $ctx.queueCall(engine.Wait, [20]);
              $ctx.queueCall(engine.Character$$Say, [game.cEarl, "Remain where you are - we shall give this case our urgent attention."]);
              $ctx.queueCall(engine.Wait, [10]);
              $ctx.queueCall(engine.Character$$Animate, [game.cEarl, 1, 3, 0 /* eOnce */, 919 /* eBlock */, 1063 /* eBackwards */]);
              $ctx.queueCall(engine.Character$$set_Loop, [game.cEarl, 0]);
              $ctx.queueCall(engine.Character$$Animate, [game.cOfficer, 2, 3, 0 /* eOnce */, 919 /* eBlock */, 1063 /* eBackwards */]);
              $ctx.queueCall(engine.Character$$set_Loop, [game.cOfficer, 0]);
              $ctx.queueCall(engine.Wait, [10]);
              $ctx.queueCall(engine.Character$$set_Loop, [game.cMayor, 1]);
              $ctx.queueCall(engine.Wait, [10]);
              $ctx.queueCall(engine.Character$$Say, [game.cMayor, "This is bad news, DJ The Earl."]);
              $ctx.queueCall(engine.Wait, [20]);
              $ctx.queueCall(engine.Character$$Say, [game.cEarl, "Verily, MC Mista Maya. We are going to need a phat dawg to conduct this investigation."]);
              $ctx.queueCall(engine.Wait, [20]);
              $ctx.queueCall(engine.Character$$Say, [game.cMayor, "Very true, but I think I have just the candidate."]);
              $ctx.queueCall(engine.Wait, [20]);
              $ctx.queueCall(engine.Character$$Say, [game.cEarl, "One of our top class police investigators?"]);
              $ctx.queueCall(engine.Wait, [20]);
              $ctx.queueCall(engine.Character$$Say, [game.cMayor, "Sadly they're all off on secret missions. Our only remaining playa from that department is !, Robot."]);
              $ctx.queueCall(engine.Wait, [40]);
              $ctx.queueCall(engine.Character$$Say, [game.cEarl, "But MC Mista Maya, he is a machine of fact, not of tact. His people skills are limited at best."]);
              $ctx.queueCall(engine.Wait, [20]);
              $ctx.queueCall(engine.Character$$Say, [game.cMayor, "I am afraid he is our only candidate, DJ The Earl. Do call for him, will you?"]);
              $ctx.queueCall(engine.Wait, [20]);
              $ctx.queueCall(engine.SetWalkBehindBase, [1, 0]);
              $ctx.queueCall(engine.Character$$Walk, [game.cRobot, 34, 80, 919 /* eBlock */, 304 /* eAnywhere */]);
              $ctx.queueCall(engine.Wait, [10]);
              $ctx.queueCall(engine.Character$$Say, [game.cRobot, "Actually, MC Mista Maya, I'm right here."]);
              $ctx.queueCall(engine.Wait, [10]);
              $ctx.queueCall(engine.Character$$set_Loop, [game.cMayor, 0]);
              $ctx.queueCall(engine.Wait, [10]);
              $ctx.queueCall(engine.Character$$Say, [game.cRobot, "I'm your caddie today, remember?"]);
              $ctx.queueCall(engine.Wait, [20]);
              $ctx.queueCall(engine.Character$$Say, [game.cMayor, "Ah yes, silly of me to forget. You heard the situation, I take it, so best head off to that town monument to see what you can find."]);
              $ctx.queueCall(engine.Wait, [20]);
              $ctx.queueCall(engine.Character$$Say, [game.cRobot, "As you wish, Mc Mista Maya."]);
              $ctx.queueCall(engine.EndCutscene);
              $ctx.queueCall(scripts["GlobalScript.asc"].MusicTracker, [4]);
              $ctx.queueCall(engine.GUI$$set_Visible, [game.gHotspots, 1 /* true */]);
              return $ctx.finish(0);
          }
      }),
      "hPhone_Interact": util.blockingFunction([], function($ctx, $stk, $vars) {
          if (scripts["_GlobalVariables.asc"].progress === 2) {
            scripts["GlobalScript.asc"].MusicTracker(10);
            engine.Character$$set_Baseline(game.cSoda, 1);
            $ctx.queueCall(engine.Character$$Walk, [game.player, 74, 155, 919 /* eBlock */, 304 /* eAnywhere */]);
            $ctx.queueCall(engine.Wait, [10]);
            $ctx.queueCall(engine.Character$$Animate, [game.player, 4, 3, 0 /* eOnce */, 919 /* eBlock */]);
            $ctx.queueCall(engine.Dialog$$Start, [game.dPhone]);
            return $ctx.finish(0);
          }
          else {
            $ctx.queueCall(engine.Character$$Say, [game.player, "I don't have any calls to make."]);
            return $ctx.finish(0);
          }
      }),
      "hFlag_Interact": util.blockingFunction([], function($ctx, $stk, $vars) {
          $ctx.queueCall(engine.Character$$Say, [game.cRobot, "It shows the golfers where to hit their ball."]);
          return $ctx.finish(0);
      }),
      "hSand_Interact": util.blockingFunction([], function($ctx, $stk, $vars) {
          $ctx.queueCall(engine.Character$$Say, [game.cRobot, "Until Can't showed up today, this counted as MC Mista Maya's number one nemesis."]);
          return $ctx.finish(0);
      }),
      "hBench_Interact": util.blockingFunction([], function($ctx, $stk, $vars) {
          $ctx.queueCall(engine.Character$$Say, [game.cRobot, "Before I worked for the local government, my primary application was as a bench handler."]);
          return $ctx.finish(0);
      }),
      "hHouse_Interact": util.blockingFunction([], function($ctx, $stk, $vars) {
          $ctx.queueCall(engine.Character$$Say, [game.cRobot, "It's like a shed for people."]);
          return $ctx.finish(0);
      }),
      "hSoda_Interact": util.blockingFunction([], function($ctx, $stk, $vars) {
          $ctx.queueCall(engine.Character$$Say, [game.cRobot, "Ugh, soda. Why can't they have oil stands around the city?"]);
          return $ctx.finish(0);
      }),
      "hMonument_Interact": util.blockingFunction([], function($ctx, $stk, $vars) {
          $ctx.queueCall(engine.Character$$Say, [game.cRobot, "That's where The Giant Cardboard Exclamation Mark is supposed to be."]);
          return $ctx.finish(0);
      }),
      "hBush_Interact": util.blockingFunction([], function($ctx, $stk, $vars) {
          $ctx.queueCall(engine.Character$$Say, [game.cRobot, "Shrubbery is never a reliable witness."]);
          return $ctx.finish(0);
      }),
      "hTV_Interact": util.blockingFunction([], function($ctx, $stk, $vars) {
          $ctx.queueCall(engine.Character$$Say, [game.cRobot, "It's just junk."]);
          return $ctx.finish(0);
      }),
      "hTyre_Interact": util.blockingFunction([], function($ctx, $stk, $vars) {
          $ctx.queueCall(engine.Character$$Say, [game.cRobot, "That's busted."]);
          return $ctx.finish(0);
      }),
      "hChair_Interact": util.blockingFunction([], function($ctx, $stk, $vars) {
          $ctx.queueCall(engine.Character$$Say, [game.cRobot, "Filthy and probably rickety as well."]);
          return $ctx.finish(0);
      }),
      "oSave_Interact": util.blockingFunction([], function($ctx, $stk, $vars) {
          engine.SaveGameSlot(1, "1");
          $ctx.queueCall(engine.Character$$Say, [game.cRobot, "Game Saved!"]);
          return $ctx.finish(0);
      }),
      "oLoad_Interact": util.blockingFunction([], function($ctx, $stk, $vars) {
          if (engine.Game$$GetSaveSlotDescription(1) !== null) {
            $ctx.queueCall(engine.Character$$Say, [game.cRobot, "Loading Game!"]);
            $ctx.queueCall(engine.RestoreGameSlot, [1]);
            return $ctx.finish(0);
          }
          else {
            $ctx.queueCall(engine.Character$$Say, [game.cRobot, "No save found!"]);
            return $ctx.finish(0);
          }
      }),
      "oQuit_Interact": util.blockingFunction([], function($ctx, $stk, $vars) {
          $ctx.queueCall(engine.Character$$Say, [game.cRobot, "Goodbye!"]);
          $ctx.queueCall(engine.QuitGame, [0]);
          return $ctx.finish(0);
      })
    };
    
    this.script = roomScript;
    this.enterRoomAfterFadeIn = roomScript.room_AfterFadeIn;
    this.enterRoomBeforeFadeIn = roomScript.room_Load;
    this.firstTimeEntersRoom = null;
    this.leavesRoom = null;
    this.repeatedlyExecute = null;
    this.walksOffBottomEdge = null;
    this.walksOffLeftEdge = null;
    this.walksOffRightEdge = null;
    this.walksOffTopEdge = null;
    
};
