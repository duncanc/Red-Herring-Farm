
jQuery(function($){

    ags.FlashNativeAudioPlayer = function(engine) {
        var player = this;
        var replaceElemId = 'flashnative';
        var swfUrl = 'FlashNativeAudioPlayer.swf' + '?'+Math.random();
        $("<div>").attr("id", replaceElemId).appendTo("body");
        
        var prefix = '_' + ags.util.randomString() + '_';
        
        window[prefix+"loaded"] = function() {
            player.isSwfLoaded = true;
        };
        
        swfobject.embedSWF(
            /* swfUrl */ swfUrl,
            /* replaceElemId */ replaceElemId,
            /* width */ 1,
            /* height */ 1,
            /* swfVersion */ "10.0.0",
            /* xiSwfUrl */ null, // "swfobject/expressInstall.swf",
            /* flashVars */ {prefix:prefix},
            /* params */ {allowScriptAccess:"always", wmode:"opaque"},
            /* attributes */ {"class":"offscreen"},
            function/* callbackFn */(e){
                if (e.success) {
                    player.swf = e.ref;
                }
            });
        
        $(engine).bind('playAudioClip', function(e, clip) {
            if (player.isSwfLoaded && clip.def.fileType === 'mp3') {
                var url = engine.game.getResourceLocation("audioClip", {fileName:clip.def.fileName});
                player.swf.startPlaying(url);
                e.stopImmediatePropagation();
            }
        });
        $(engine).bind('seekMp3PosMillis', function(e, offset) {
            if (player.isSwfLoaded) {
                player.swf.seekMp3PosMillis(offset);
                e.stopImmediatePropagation();
            }
        });
        $(engine).bind('getMp3PosMillis', function(e, buffer) {
            if (player.isSwfLoaded) {
                buffer.value = player.swf.getMp3PosMillis();
                e.stopImmediatePropagation();
            }
        });
        $(engine).bind('setMusicRepeat', function(e, repeat) {
            if (player.isSwfLoaded) {
                player.swf.setMusicRepeat(repeat);
                e.stopImmediatePropagation();
            }
        });
        
    };
    
    ags.FlashNativeAudioPlayer.prototype = {
        swf: null,
        isSwfLoaded: false
    };

});
