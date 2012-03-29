
jQuery(function() {

    // image-rendering: -moz-crisp-edges introduced in Gecko 1.9.2
    // https://developer.mozilla.org/en/css/image-rendering

    var geckoVersion = null;

    if (typeof window.navigator !== 'undefined' && typeof navigator.userAgent === 'string') {
        var parts = navigator.userAgent.match(/Gecko/) && navigator.userAgent.match(/rv:\s*(\d+)(\.(\d+))?(\.(\d+))?/);
        if (parts) {
            geckoVersion = [parseInt(parts[1]), parseInt(parts[3] || '0'), parseInt(parts[5] || '0')];
        }
    }
    
    ags.CanvasRenderer.CSSScaling = {
        detectCompatibility: function() {
            if (!geckoVersion || geckoVersion[0] < 1 || geckoVersion[1] < 9 || geckoVersion[2] < 2) {
                return false;
            }
            return true;
        }
    };

});
