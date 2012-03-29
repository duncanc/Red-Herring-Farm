if (typeof ags.games["f3b6a6fcc30c4ec09285e5c42e48be15"] === 'undefined') {
    ags.games["f3b6a6fcc30c4ec09285e5c42e48be15"] = {};
}
ags.games["f3b6a6fcc30c4ec09285e5c42e48be15"].Room3Script = function(engine) {
    var util = ags.util;
    var game = engine.game;
    
    var scripts = game.globalScripts.scripts;
    
    var roomScript = {
          "room_FirstLoad": util.blockingFunction([], function($ctx, $stk, $vars) {
              engine.SetMusicRepeat(0);
              engine.PlayMusic(9);
              $ctx.queueCall(engine.Wait, [10]);
              $ctx.queueCall(engine.Character$$Say, [game.cCount, "! was designed, written, illustrated and thrown together by Ben Chandler, with help from the following people:"]);
              $ctx.queueCall(engine.Wait, [10]);
              $ctx.queueCall(engine.Character$$Say, [game.cCount, "Sebastian Pfaller, musician, advisor, tester and all around source of additional creativity."]);
              $ctx.queueCall(engine.Wait, [10]);
              $ctx.queueCall(engine.Character$$Say, [game.cCount, "Chris Jones, creator of the AGS engine - the solid and wonderful foundation that we've desecrated with this nonsense."]);
              $ctx.queueCall(engine.Wait, [10]);
              $ctx.queueCall(engine.Character$$Say, [game.cCount, "Ryan Timothy, Crimson Wizard and Arjon, who gave up their free time to help me with testing."]);
              $ctx.queueCall(engine.Wait, [10]);
              $ctx.queueCall(engine.Character$$Say, [game.cCount, "Sylvr, who was there when I originally came up with this concept more than half a year ago, and demanded I make a game from it."]);
              $ctx.queueCall(engine.Wait, [10]);
              $ctx.queueCall(engine.Character$$Say, [game.cCount, "Monkey_05_06 who taught me some cool scripting without too much nagging on my behalf."]);
              $ctx.queueCall(engine.Wait, [10]);
              $ctx.queueCall(engine.Character$$Say, [game.cCount, "Peder Johnsen, whose webspace I soil with the presence of my games without even giving him any money."]);
              $ctx.queueCall(engine.Wait, [10]);
              $ctx.queueCall(engine.Character$$Say, [game.cCount, "Craig Kroeger and Matthew Welch, who have never heard of me, but did make the lovely fonts you've been reading through this game, and let me use them for free."]);
              $ctx.queueCall(engine.Wait, [10]);
              $ctx.queueCall(engine.Character$$Say, [game.cCount, "David Pfaller, who programmed !,Robot's voice during this rap battle."]);
              $ctx.queueCall(engine.Wait, [10]);
              $ctx.queueCall(engine.Character$$Say, [game.cCount, "Music was composed with the aid of the following:"]);
              $ctx.queueCall(engine.Character$$Say, [game.cCount, "Element of Surprise! synth by ugo http://www.ugoaudio.com/ - HaHaHa DS-01 by pethu audio http://pethu.se/music/instruments.html"]);
              $ctx.queueCall(engine.Wait, [10]);
              $ctx.queueCall(engine.Character$$Say, [game.cCount, "AGS engine 3.1.2 Copyright (c) 2006-2009 Chris Jones."]);
              $ctx.queueCall(engine.Wait, [10]);
              $ctx.queueCall(engine.Character$$Say, [game.cCount, "Audio content and Erdbeertelefon logo Copyright (c) 2009 Sebastian Pfaller."]);
              $ctx.queueCall(engine.Wait, [10]);
              $ctx.queueCall(engine.Character$$Say, [game.cCount, "Speech font Copyright (c) 2009 Craig Kroeger, large gui font Copyright (c) 2009 Matthew Welch."]);
              $ctx.queueCall(engine.Wait, [10]);
              $ctx.queueCall(engine.Character$$Say, [game.cCount, "Everything else Copyright (c) 2009 Ben Chandler."]);
              $ctx.queueCall(engine.Wait, [10]);
              $ctx.queueCall(engine.Character$$Say, [game.cCount, "Thank you for playing!"]);
              $ctx.queueCall(engine.Wait, [20]);
              $ctx.queueCall(engine.QuitGame, [0]);
              return $ctx.finish(0);
          }),
          "room_Load": function() {
              engine.Character$$set_Transparency(game.player, 100);
              return 0;
          }
    };
    
    this.script = roomScript;
    this.enterRoomAfterFadeIn = null;
    this.enterRoomBeforeFadeIn = roomScript.room_Load;
    this.firstTimeEntersRoom = roomScript.room_FirstLoad;
    this.leavesRoom = null;
    this.repeatedlyExecute = null;
    this.walksOffBottomEdge = null;
    this.walksOffLeftEdge = null;
    this.walksOffRightEdge = null;
    this.walksOffTopEdge = null;
    
};
