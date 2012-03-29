
jQuery(function($){

    ags.FlashOggVorbisPlayer = function(engine, swfUrl) {
        var player = this;
        var replaceElemId = "playMe";
        
        var repeatMusic = true;
        
        var restartTimeout = null;
        
        window.onOggState = function(state) {
            player.state = state;
            if (state === "loaded") {
                player.loaded = true;
                if (player.url) {
                    player.swf.playURL(player.url);
                }
            }
            if (state === "streamstop" && repeatMusic) {
                restartTimeout = window.setTimeout(function() {
                    restartTimeout = null;
                    player.swf.playURL(player.url);
                }, 500);
            }
        }
        window.onOggBuffer = function(val) {
        }
        
        $("<div>").attr("id", replaceElemId).appendTo("body");
        swfobject.embedSWF(
            /* swfUrl */ swfUrl,
            /* replaceElemId */ replaceElemId,
            /* width */ 1,
            /* height */ 1,
            /* swfVersion */ "10.0.0",
            /* xiSwfUrl */ null, // "swfobject/expressInstall.swf",
            /* flashVars */ {},
            /* params */ {allowScriptAccess:"always", wmode:"opaque"},
            /* attributes */ {"class":"offscreen"},
            function/* callbackFn */(e){
                if (e.success) {
                    player.swf = e.ref;
                }
            });
        $(engine).bind('playAudioClip', function(e, clip) {
            if (clip.def.fileType === "ogg" || clip.def.fileType === "mp3") {
                if (restartTimeout !== null) {
                    window.clearTimeout(restartTimeout);
                    restartTimeout = null;
                }
                var url = engine.game.getResourceLocation("audioClip", {fileName:clip.def.fileName});
                if (url !== player.url) {
                    player.url = url;
                    if (player.loaded) {
                        player.swf.stopPlaying();
                        player.swf.playURL(url);
                    }
                }
                e.stopImmediatePropagation();
            }
        });
        $(engine).bind('seekMp3PosMillis', function(e, offset) {
        });
        $(engine).bind('getMp3PosMillis', function(e, buffer) {
        });
        $(engine).bind('setMusicRepeat', function(e, repeat) {
            repeatMusic = repeat;
        });
    };
    ags.FlashOggVorbisPlayer.prototype = {
        state: null,
        url: null,
        loaded: false
    };
    
});
