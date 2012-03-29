if (typeof ags.games["f3b6a6fcc30c4ec09285e5c42e48be15"] === 'undefined') {
    ags.games["f3b6a6fcc30c4ec09285e5c42e48be15"] = {};
}
ags.games["f3b6a6fcc30c4ec09285e5c42e48be15"].Room2Script = function(engine) {
    var util = ags.util;
    var game = engine.game;
    
    var scripts = game.globalScripts.scripts;
    
    var roomScript = {
        room_Load: function() {
            engine.AudioClip$$Play(game.aMusic1, 31998 /* SCR_NO_VALUE */, 31998 /* SCR_NO_VALUE */);
            engine.SetBackgroundFrame(0);
            return 0;
        },
        room_AfterFadeIn: util.blockingFunction([], function($ctx, $stk, $vars) {
            engine.StartCutscene(1 /* eSkipESCOnly */);
            $ctx.queueCall(engine.Wait, [120]);
            $ctx.queueCall(engine.EndCutscene);
            $ctx.queueCall(engine.StartCutscene, [1 /* eSkipESCOnly */]);
            $ctx.queueCall(engine.SetBackgroundFrame, [1]);
            $ctx.queueCall(engine.Wait, [120]);
            $ctx.queueCall(engine.EndCutscene);
            $ctx.queueCall(engine.Character$$ChangeRoom, [game.player, 1, 1, 80]);
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
