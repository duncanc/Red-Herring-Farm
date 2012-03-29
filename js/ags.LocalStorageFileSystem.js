
jQuery(function($) {

    ags.LocalStorageFileSystem = function(prefix) {
        this.prefix = prefix || "";
    };
    ags.LocalStorageFileSystem.prototype = {
        prefix: null,
        clear: function() {
            var paths = [];
            var prefix = this.prefix;
            var i;
            for (i = 0; i < window.localStorage.length; i++) {
                var path = window.localStorage.getKey(i);
                if (path.slice(0, prefix.length) === prefix) {
                    paths.push(path);
                }
            }
            for (i = 0; i < paths.length; i++) {
                delete window.localStorage[paths[i]];
            }
        },
        normalizePath: function(path) {
            path = path.replace('\\', '/');
            path = this.prefix + path;
            return path;
        },
        deleteFile: function(path) {
            path = this.normalizePath(path);
            if (!window.localStorage[path]) {
                return false;
            }
            else {
                delete window.localStorage[path];
                return true;
            }
        },
        fileExists: function(path) {
            path = this.normalizePath(path);
            return !!window.localStorage[path];
        },
        openFile: function(path, mode) {
            path = this.normalizePath(path);
            var file;
            switch(mode) {
                case "read":
                    if (!window.localStorage[path]) {
                        return null;
                    }
                    return new ags.ReadStringFile(window.localStorage[path]);
                case "write":
                    file = new ags.WriteStringFile();
                    $(file).bind('closing', function(e, data) {
                        window.localStorage[path] = data;
                    });
                    return file;
                case "append":
                    if (!window.localStorage[path]) {
                        file = new ags.WriteStringFile();
                    }
                    else {
                        file = new ags.WriteStringFile(window.localStorage[path]);
                    }
                    $(file).bind('closing', function(e, data) {
                        window.localStorage[path] = data;
                    });
                    return file;
            }
            return null;
        }
    };
    
    ags.LocalStorageFileSystem.detectCompatibility = function() {
        return typeof window.localStorage !== 'undefined';
    };

});
