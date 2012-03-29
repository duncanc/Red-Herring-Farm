
jQuery(function($){

    var engine = ags.Engine.prototype;
    
    engine.compat_audio = true;
    
    engine.PlayMusic = function(musicNumber) {
        $(this).trigger('playAudioClip', [{def:this.game.def.audio.compat_music[musicNumber]}]);
    };
    engine.PlayMusicQueued = function(musicNumber) { /* TODO */ };
    engine.PlaySilentMIDI = function(musicNumber) { /* TODO */ };
    engine.PlayMP3File = function(filename) { /* TODO */ };
    engine.PlaySound = function(soundNumber) { /* TODO */ return 0; };
    engine.PlaySoundEx = function(soundNumber, channelNumber) { /* TODO */ };
    engine.PlayAmbientSound = function(channelNumber, soundNumber, volume, x, y) { /* TODO */ };
    engine.StopAmbientSound = function(channelNumber) { /* TODO */ };
    engine.GetCurrentMusic = function() { /* TODO */ return 0; };
    engine.SetMusicRepeat = function(repeat) { /* TODO */
        $(this).trigger('setMusicRepeat', [!!repeat]);
    };
    engine.SetMusicVolume = function(volume) { /* TODO */ };
    engine.SetSoundVolume = function(volume) { /* TODO */ };
    engine.SetMusicMasterVolume = function(volume) { /* TODO */ };
    engine.SetDigitalMasterVolume = function(volume) { /* TODO */ };
    engine.SeekMODPattern = function(pattern) { /* TODO */ };
    engine.IsChannelPlaying = function(channelNumber) { /* TODO */ return 0; };
    engine.IsSoundPlaying = function() { /* TODO */ return 0; };
    engine.IsMusicPlaying = function() { /* TODO */ return 0; };
    engine.GetMIDIPosition = function() { /* TODO */ return 0; };
    engine.SeekMIDIPosition = function() { /* TODO */ return 0; };
    engine.GetMP3PosMillis = function() {
        var buffer = {value:0};
        $(this).trigger('getMp3PosMillis', [buffer]);
        return buffer.value | 0;
    };
    engine.SeekMP3PosMillis = function(offset) {
        $(this).trigger('seekMp3PosMillis', [offset]);
    };
    engine.SetChannelVolume = function(channelNumber, volume) { /* TODO */ };
    engine.StopChannel = function(channelNumber) { /* TODO */ };
    engine.StopMusic = function() { /* TODO */ };
    engine.Game$$StopSound = function(includeAmbient) { /* TODO */ };
    
});
