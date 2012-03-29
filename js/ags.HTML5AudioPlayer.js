
jQuery(function($) {

    var testAudio = document.createElement("AUDIO");
    
    if (testAudio && typeof testAudio.play === 'undefined') {
        testAudio = null;
    }
    
    var repeatMusic = true;
    
    // hack for browsers that don't support loop yet (Firefox)
    if (testAudio && typeof testAudio.loop !== 'boolean') {
        window.setInterval(function() {
            if (!repeatMusic) return;
            $("audio.looping").each(function() {
                if (this.ended) {
                    this.currentTime = 0;
                }
            });
        }, 250);
    }

    var FADE_UP_SECONDS = 1;
    var FADE_UP_GRANULARITY = 10;
    var FADE_DOWN_SECONDS = 1;
    var FADE_DOWN_GRANULARITY = 10;
    
    var preloadAudio = function(url) {
        var existing = $("audio[src=" + url + "]");
        if (existing.length > 0) {
            return existing[0];
        }
        var newAudio = document.createElement("AUDIO");
        newAudio.muted = true;
        newAudio.volume = 0;
        newAudio.loop = repeatMusic;
        newAudio.autoplay = true;
        newAudio.controls = false;
        newAudio.playingLocation = url;
        newAudio.src = url;
        $(newAudio).addClass("looping").appendTo("body");
        return newAudio;
    };
    
    var clearAudioInterval = function(aud) {
        if (typeof aud.interval !== 'undefined') {
            window.clearInterval(aud.interval);
            delete aud.interval;
        }
    };
    
    var setAudioInterval = function(aud, callback, ms) {
        clearAudioInterval(aud);
        aud.interval = window.setInterval(callback, ms);
    };
    
    var fadeDownAudio = function(newAudio, callback) {
        var fadeDownVolume = newAudio.volume;
        setAudioInterval(newAudio, function() {
            fadeDownVolume = Math.max(0, fadeDownVolume - FADE_DOWN_SECONDS/FADE_DOWN_GRANULARITY);
            newAudio.volume = fadeDownVolume;
            if (fadeDownVolume == 0) {
                newAudio.muted = true;
                newAudio.pause();
                clearAudioInterval(newAudio);
                if (callback) {
                    callback();
                }
            }
        }, 1000/FADE_DOWN_GRANULARITY);
    };
    
    var fadeUpAudio = function(newAudio) {
        newAudio.muted = false;
        newAudio.volume = 0;
        var fadeUpVolume = 0;
        
        setAudioInterval(newAudio, function() {
            if (newAudio.readyState < 3 /* HAVE_FUTURE_DATA */) {
                return;
            }
            fadeUpVolume = Math.min(1, fadeUpVolume + FADE_UP_SECONDS/FADE_UP_GRANULARITY);
            newAudio.volume = fadeUpVolume;
            if (fadeUpVolume == 1) {
                clearAudioInterval(newAudio);
            }
        }, 1000/FADE_UP_GRANULARITY);
    };
    
    var playAudio = function(url) {
        var newAudio = preloadAudio(url);
        var currentTrack = $("audio.currentTrack").removeClass("currentTrack");
        var oldTrack = (currentTrack.length === 0) ? null : currentTrack[0];
        if (oldTrack) {
            if (oldTrack.playingLocation == url) {
                $(oldTrack).addClass("currentTrack");
                return;
            }
            oldTrack.autoplay = false;
            clearAudioInterval(oldTrack);
            fadeDownAudio(oldTrack);
        }
        $(newAudio).addClass("currentTrack");
        setAudioInterval(newAudio, function() {
            if (newAudio.seeking || newAudio.readyState < 1 /* HAVE_METADATA */) {
                return;
            }
            if (!newAudio.paused) {
                if (typeof newAudio.seekStart === 'number') {
                    newAudio.pause();
                    var offsetTime = newAudio.seekStart + (new Date().valueOf() - newAudio.seekStartBase);
                    offsetTime = offsetTime / 1000;
                    while (offsetTime < 0) {
                        offsetTime += newAudio.duration;
                    }
                    while (offsetTime > newAudio.duration) {
                        offsetTime -= newAudio.duration;
                    }
                    newAudio.currentTime = offsetTime;
                }
                else {
                    newAudio.pause();
                    newAudio.startTime = 0;
                }
            }
            if (newAudio.paused && newAudio.readyState >= 3 /* HAVE_FUTURE_DATA */) {
            
                // do SeekStart again because the time may have moved on, we want to get it right
                if (typeof newAudio.seekStart === 'number') {
                    var offsetTime = newAudio.seekStart + (new Date().valueOf() - newAudio.seekStartBase);
                    offsetTime = offsetTime / 1000;
                    while (offsetTime < 0) {
                        offsetTime += newAudio.duration;
                    }
                    while (offsetTime > newAudio.duration) {
                        offsetTime -= newAudio.duration;
                    }
                    newAudio.currentTime = offsetTime;
                    delete newAudio.seekStart;
                    delete newAudio.seekStartBase;
                }
                
                if (oldTrack) {
                    fadeDownAudio(oldTrack);
                }
                
                newAudio.play();
                fadeUpAudio(newAudio);
            }
        }, 250);
    }
    
    var getAudioPosition = function() {
        var track = $("audio.currentTrack");
        if (track.length === 0) return 0;
        track = track[0];
        if (track.readyState < 1 /* HAVE_METADATA */) return 0;
        return Math.floor(track.currentTime * 1000);
    };
    
    var setAudioPosition = function(pos) {
        var track = $("audio.currentTrack");
        if (track.length === 0) {
            return;
        }
        track = track[0];
        if (track.readyState >= 3 /* HAVE_FUTURE_DATA */) {
            pos = pos / 1000;
            while (pos < 0) pos += track.duration;
            while (pos > track.duration) pos -= track.duration;
            track.currentTime = pos;
        }
        else if (pos !== 0) {
            track.seekStart = pos;
            track.seekStartBase = new Date().valueOf();
        }
    };

    ags.HTML5AudioPlayer = function(engine) {
        $(engine).bind('playAudioClip', function(e, clip) {
            if (!testAudio || typeof testAudio.canPlayType !== 'function') {
                $(this).unbind(e);
                return;
            }
            var canPlay = false;
            switch(clip.def.fileType) {
                case "ogg":
                    canPlay = testAudio.canPlayType("audio/ogg");
                    break;
                case "mp3":
                    canPlay = testAudio.canPlayType("audio/mpeg");
                    break;
                case "mid":
                    canPlay = testAudio.canPlayType("audio/midi");
                    break;
                default:
                    canPlay = testAudio.canPlayType("audio/x-" + clip.def.fileType);
                    break;
            }
            if (canPlay) {
                playAudio(engine.game.getResourceLocation("audioClip", {fileName:clip.def.fileName}));
                e.stopImmediatePropagation();
            }
        });
        $(engine).bind('getMp3PosMillis', function(e, buffer) {
            buffer.value = getAudioPosition();
            e.stopImmediatePropagation();
        });
        $(engine).bind('seekMp3PosMillis', function(e, offset) {
            setAudioPosition(offset);
            e.stopImmediatePropagation();
        });
        $(engine).bind('setMusicRepeat', function(e, repeat) {
            repeatMusic = repeat;
            if (!repeat) {
                $("audio.looping").removeClass('looping').each(function(){ this.loop = false; });
            }
        });
    };
    
    ags.HTML5AudioPlayer.prototype = {
    };
    
    ags.HTML5AudioPlayer.detectCompatibility = function() {
        return !!testAudio;
    };
    
});
