
jQuery(function($) {

    ags.QuicktimeAudioPlayer = function() {
    };
    
    ags.QuicktimeAudioPlayer.prototype = {
    }:
    
    ags.QuicktimeAudioPlayer.detectCompatibility = function() {
        // IE
        if (typeof window.ActiveXObject !== 'undefined') {
            try {
                var _ = new ActiveXObject("Quicktime.Quicktime");
                return true;
            }
            catch (e) {
                return false;
            }
        }
        // Firefox/Chrome/Opera/Safari
        if (typeof window.navigator !== 'undefined' && typeof navigator.plugins !== 'undefined') {
            for (var i = 0; i < navigator.plugins.length; i++) {
                if (/^quicktime plug\-?in/i.test(navigator.plugins[i].name)) {
                    return true;
                }
            }
        }
        // Assume not
        return false;
    };

});

